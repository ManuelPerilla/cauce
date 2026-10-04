# 偏好设置与账户

在 **Apariencia** 中选择 Sistema、Claro、Oscuro、Bosque 或 Alto contraste，并按需减少动画和透明度。在 **Escuchar** 中选择曲风、同一艺人的间隔及是否允许重复。切换曲风会开始新的聆听会话。

引用、元数据和偏好保存在 `%LOCALAPPDATA%\Cauce`，最多 10,000 条引用、16 MiB。Cauce 不复制或修改音频。播放历史仅保留在当前会话中。导出包含文件路径和 URL，分享前请检查。

账户为可选功能。分发者可以在 `Cauce.exe` 旁部署公开的 `auth.json`，通过可信的 HTTPS OIDC 身份代理连接 Google、Apple、Facebook 和 Microsoft。没有真实配置时，登录保持禁用。客户端使用系统浏览器和 Authorization Code + PKCE，不保存令牌或秘密。账户不会同步曲库，也不授予音乐服务权限。

[账户指南](../../cauce-accounts.md) 说明身份代理注册、回调和真实服务测试。播放本地文件不需要配置任何账户。

[主页](../../../README.zh-CN.md) · [英文指南](../../configuration.md)
