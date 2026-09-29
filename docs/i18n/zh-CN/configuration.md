# 配置与 Google OAuth

NgMusic 使用 Google OAuth 2.0 Desktop 流程和 YouTube Data API v3。

| 变量 | 用途 |
| --- | --- |
| `NGMUSIC_GOOGLE_CLIENT_ID` | 登录必需 |
| `NGMUSIC_GOOGLE_CLIENT_SECRET` | Google 提供时使用 |
| `NGMUSIC_YOUTUBE_API_KEY` | 可选，无 OAuth 搜索时使用 |

登录时 NgMusic 会生成 PKCE，监听 `127.0.0.1` 的临时端口，打开系统浏览器，验证回调状态并交换 token。

token 保存在 Windows Credential Manager 的 `NgMusic.GoogleOAuth` 项中。

NgMusic 不会请求或保存 Google 密码。

不要提交 OAuth secret、API key、签名证书或 token。