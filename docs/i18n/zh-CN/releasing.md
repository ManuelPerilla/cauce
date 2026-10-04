# 构建与发布

需要 Windows 和 .NET 10 SDK。在仓库根目录运行：

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet build src/Cauce.Desktop -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1 -Runtime win-x64
./build/cauce.ps1 -Runtime win-arm64
./build/release.ps1 -Version 0.6.0-rc.1
```

测试验证曲库、队列、身份及 WPF 状态。合成测试不代表真实账户提供商已经通过验证，也不能保证其他设备上的性能。

脚本准备包含运行时及校验和的便携 ZIP。除非使用经批准的证书签名并验证，包保持未签名状态。没有安装程序或 Store 包。

`build/cauce.ps1` 支持 `-Version`，输出到 `artifacts/cauce/`。`build/release.ps1` 默认构建 x64 和 ARM64，将 ZIP 和 `SHA256SUMS.txt` 汇总到 `artifacts/release/`。

`.github/workflows/cauce.yml` 保留审核用构建产物。`release.yml` 通过手动运行或版本标签触发，准备包并创建**发布草稿**。公开发布前核对版本、架构、校验和、测试结果及说明。签名请求遵循[代码签名政策](code-signing-policy.md)。

[主页](../../../README.zh-CN.md) · [英文指南](../../releasing.md)
