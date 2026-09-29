# 故障排除

## 找不到 `ngmusic`

MSI 安装后请重新打开终端。便携版请直接从解压目录运行 `ngmusic.exe`。

## OAuth 未配置

设置 `NGMUSIC_GOOGLE_CLIENT_ID`。

## 登录没有完成

确认浏览器可访问 `127.0.0.1`，安全软件未阻止 loopback，并确认 OAuth client 类型为 Desktop。

## 搜索要求认证或 API key

先运行 `login`，或配置 `NGMUSIC_YOUTUBE_API_KEY`。

## SmartScreen / Defender 警告

不要全局禁用防病毒。只从官方 Release 下载，对照 SHA-256，并在签名可用后验证 Authenticode 签名。