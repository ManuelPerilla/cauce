# NgMusic

**面向 Windows、受 PowerShell 启发的终端优先音乐控制器，使用 Google OAuth 和 YouTube 官方支持的嵌入式播放器。**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **项目状态：** 早期阶段，但已可使用。公开版本通过 GitHub Actions 自动构建，支持 Windows x64、ARM64 和 x86。

## NgMusic 是什么

NgMusic 是一个面向 Windows 的个人音乐 shell，适合更喜欢键盘和终端而不是传统媒体播放器界面的用户。它提供类似 PowerShell 的命令，用于搜索、播放、音量、跳转、队列管理和 Google 登录。

播放始终保留在可见的 YouTube 官方 IFrame 播放器中。NgMusic 不提取原始媒体流，不下载音轨，也不运行隐藏的音频抓取层。

## 主要功能

- Google OAuth 2.0 Desktop 登录，支持 PKCE 和 loopback 回调。
- OAuth token 保存在 **Windows Credential Manager**。
- 使用 YouTube Data API v3 搜索。
- 从终端控制可见的 YouTube IFrame 播放器。
- 播放队列和历史记录。
- x64、ARM64、x86 的 MSI 和便携 ZIP。
- Self-contained 发布，用户无需单独安装 .NET。
- GitHub Actions 可复现构建和 SHA-256 校验值。

## 下载

前往 [最新 GitHub Release](https://github.com/ManuelPerilla/ngmusic/releases/latest)。

| 架构 | 安装包 | 便携版 |
| --- | --- | --- |
| x64 | `*-win-x64.msi` | `*-win-x64-portable.zip` |
| ARM64 | `*-win-arm64.msi` | `*-win-arm64-portable.zip` |
| x86 | `*-win-x86.msi` | `*-win-x86-portable.zip` |

MSI 安装到 Program Files，并将 `ngmusic` 加入系统 `PATH`，可能需要管理员批准。便携版不会修改 Program Files 或 `PATH`。

## 初始配置

需要 Google OAuth Desktop 应用客户端和 YouTube Data API v3。

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="your-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="your-client-secret"
$env:NGMUSIC_YOUTUBE_API_KEY="your-api-key"
```

请勿将凭据提交到仓库。

## 文档

- [安装](docs/i18n/zh-CN/installation.md)
- [配置与 OAuth](docs/i18n/zh-CN/configuration.md)
- [命令参考](docs/i18n/zh-CN/commands.md)
- [架构](docs/i18n/zh-CN/architecture.md)
- [安全与隐私](docs/i18n/zh-CN/security.md)
- [故障排除](docs/i18n/zh-CN/troubleshooting.md)
- [构建与发布](docs/i18n/zh-CN/releasing.md)
- [代码签名策略](docs/i18n/zh-CN/code-signing-policy.md)

英文文档是规范来源。若翻译与英文版本存在差异，以英文版本为准。

## 签名状态

SignPath Foundation 审批目前仍在等待中。启用正式签名后：

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**
