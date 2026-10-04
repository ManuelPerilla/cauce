# 安装 Cauce

预览版 0.6.0-rc.1 以 Windows x64 和 ARM64 便携 ZIP 发布。在完成外部代码签名要求之前，当前版本**未签名**。请选择适合设备架构的包。

1. 从官方发布页获取 ZIP，并核对 SHA-256。
2. 解压到具有写入权限的文件夹。
3. 运行 `Cauce.exe`，包内已包含 .NET 运行时。
4. 在 **Biblioteca → Añadir archivos** 中选择 MP3、WAV 或 M4A，补充曲风，然后在 **Escuchar** 中开始聆听。

无需账户或管理员权限，也不会修改 PATH。应用数据保存在 `%LOCALAPPDATA%\Cauce`，与程序文件夹分开。卸载时先退出 Cauce，再删除程序文件夹；**Borrar datos locales** 会删除曲库及恢复文件，不会删除音乐。

从源码运行需要 Windows 和 .NET 10 SDK：

```powershell
dotnet run --project src/Cauce.Desktop
```

当前界面使用西班牙语；本指南翻译操作说明，并不表示应用界面已经本地化。

[主页](../../../README.zh-CN.md) · [英文指南](../../installation.md)
