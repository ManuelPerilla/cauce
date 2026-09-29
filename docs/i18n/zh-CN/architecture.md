# 架构

NgMusic 将终端、音乐提供者、认证和播放器解耦。

- `MusicShell`：命令、解析、队列与历史。
- `YouTubeProvider`：YouTube Data API v3 搜索。
- `GoogleOAuthService`：OAuth 2.0 + PKCE。
- `WindowsCredentialTokenStore`：Credential Manager token 存储。
- `IPlayer`：播放抽象。
- `YouTubeIframePlayer`：可见 IFrame 播放器和 localhost 控制桥。

未来可以替换成 WebView2，而不需要重写 shell。

x64、ARM64、x86 分别独立构建，WiX 的中间目录也按架构隔离，避免交叉复用。