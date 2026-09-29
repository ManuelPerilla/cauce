# NgMusic

**面向 Windows 的 PowerShell 风格终端音乐控制器。**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

## 更简单的 OAuth 设置

现在只需要运行：

```text
PS Music:\> login
```

如果没有配置 Google OAuth Client ID，NgMusic 会自动启动设置向导。粘贴一次 **Desktop app** Client ID 即可。

非敏感的 Client ID 保存到：

```text
%LOCALAPPDATA%\NgMusic\config.json
```

OAuth token 仍保存在 Windows Credential Manager。

可用命令：

```text
setup
config show
config path
config reset
```

环境变量仍然支持，并且优先级更高。

[最新 Release](https://github.com/ManuelPerilla/ngmusic/releases/latest)

英文文档是规范来源。
