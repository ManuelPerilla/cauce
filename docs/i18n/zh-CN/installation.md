# 安装

NgMusic 面向现代 Windows 10 和 Windows 11。

## 选择正确的软件包

| 系统 | 推荐包 |
| --- | --- |
| Intel/AMD 64 位 | `NgMusic-<版本>-win-x64.msi` |
| Windows on ARM | `NgMusic-<版本>-win-arm64.msi` |
| 32 位 Windows | `NgMusic-<版本>-win-x86.msi` |

## MSI

下载对应架构的 MSI，按需使用 `SHA256SUMS.txt` 校验，然后运行安装程序。Windows 可能要求管理员权限。安装完成后请打开新的终端并运行 `ngmusic`。

MSI 安装到 Program Files，注册升级/卸载信息并加入系统 `PATH`。当前版本不会创建开始菜单快捷方式。

## 便携版

解压对应的 portable ZIP，然后运行 `ngmusic.exe`。它不会修改 Program Files 或 `PATH`。

OAuth token 仍保存在 Windows Credential Manager。

官方 release 是 self-contained，运行时无需单独安装 .NET。