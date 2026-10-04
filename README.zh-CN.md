# Cauce

**让音乐保持连贯。**

Cauce 是使用 C#、WPF 和 .NET 10 开发的 Windows 原生音乐播放器。预览版 **0.6.0-rc.1 未签名**，提供 x64 和 ARM64 便携 ZIP。

- 通过 Windows 播放本地 MP3、WAV 和 M4A；具体文件及编码仍需系统支持。
- 使用确定性规则保持所选曲风，控制重复和艺人间隔。队列无法继续时会解释原因。
- 只保存引用和元数据，不复制或修改音频；上限为 10,000 条引用和 16 MiB。
- 区分可播放文件、缺失文件和服务 HTTPS 链接。服务链接在播放器外部打开。
- 提供紧凑模式、通知区域控制、五种主题、减少动画及透明度、教程和可检查的错误报告草稿。

播放本地音乐无需账户。可选 OIDC 客户端在配置真实身份代理及 Google、Apple、Facebook、Microsoft 注册之前保持禁用。当前没有集成流媒体曲库或云同步。界面目前使用西班牙语。

使用 Windows 和 .NET 10 SDK：

```powershell
dotnet run --project src/Cauce.Desktop
```

[安装](docs/i18n/zh-CN/installation.md) · [偏好设置](docs/i18n/zh-CN/configuration.md) · [操作](docs/i18n/zh-CN/commands.md) · [故障排除](docs/i18n/zh-CN/troubleshooting.md)

[架构](docs/i18n/zh-CN/architecture.md) · [安全](docs/i18n/zh-CN/security.md) · [发布](docs/i18n/zh-CN/releasing.md) · [签名](docs/i18n/zh-CN/code-signing-policy.md)

[完整指南](docs/cauce.md) · [账户](docs/cauce-accounts.md) · [代码](https://github.com/ManuelPerilla/cauce)

[English](README.md) · [Español](README.es.md) · [Français](README.fr.md) · [हिन्दी](README.hi.md) · 简体中文
