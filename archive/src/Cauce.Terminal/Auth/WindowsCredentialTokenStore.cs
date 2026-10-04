using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Cauce.Terminal.Auth;

public sealed class WindowsCredentialTokenStore(string targetName) : ITokenStore
{
    private const uint CredTypeGeneric = 1;
    private const uint CredPersistLocalMachine = 2;

    public Task<StoredToken?> ReadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows())
            return Task.FromResult<StoredToken?>(null);

        if (!CredRead(targetName, CredTypeGeneric, 0, out var credentialPtr))
        {
            var error = Marshal.GetLastWin32Error();
            const int ErrorNotFound = 1168;
            if (error == ErrorNotFound)
                return Task.FromResult<StoredToken?>(null);

            throw new Win32Exception(error, "Could not read OAuth token from Windows Credential Manager.");
        }

        try
        {
            var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
            if (credential.CredentialBlob == IntPtr.Zero || credential.CredentialBlobSize == 0)
                return Task.FromResult<StoredToken?>(null);

            var bytes = new byte[checked((int)credential.CredentialBlobSize)];
            Marshal.Copy(credential.CredentialBlob, bytes, 0, bytes.Length);
            var json = Encoding.UTF8.GetString(bytes);
            return Task.FromResult(JsonSerializer.Deserialize<StoredToken>(json));
        }
        finally
        {
            CredFree(credentialPtr);
        }
    }

    public Task WriteAsync(StoredToken token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows Credential Manager is required.");

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(token));
        var blob = Marshal.AllocCoTaskMem(bytes.Length);

        try
        {
            Marshal.Copy(bytes, 0, blob, bytes.Length);
            var credential = new CREDENTIAL
            {
                Type = CredTypeGeneric,
                TargetName = targetName,
                CredentialBlobSize = (uint)bytes.Length,
                CredentialBlob = blob,
                Persist = CredPersistLocalMachine,
                UserName = token.Email ?? "Google OAuth"
            };

            if (!CredWrite(ref credential, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not save OAuth token to Windows Credential Manager.");
        }
        finally
        {
            Marshal.FreeCoTaskMem(blob);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows())
            return Task.CompletedTask;

        if (!CredDelete(targetName, CredTypeGeneric, 0))
        {
            var error = Marshal.GetLastWin32Error();
            const int ErrorNotFound = 1168;
            if (error != ErrorNotFound)
                throw new Win32Exception(error, "Could not delete OAuth token from Windows Credential Manager.");
        }

        return Task.CompletedTask;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredRead(string target, uint type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredDelete(string target, uint type, int flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree([In] IntPtr buffer);
}
