# 安全与隐私

NgMusic 是桌面客户端，没有项目自营的账户或遥测后端。

它可能处理搜索词、OAuth token、Google 基本资料以及 YouTube 视频元数据。

项目不会主动收集第一方分析、广告标识或遥测。

Google 登录、token 刷新、YouTube 搜索和播放会产生网络请求。OAuth 和播放器本地服务只监听 `127.0.0.1`。

OAuth token 保存在 Windows Credential Manager 的 `NgMusic.GoogleOAuth`。

NgMusic 不会向项目自营服务器传输数据，因为项目没有此类后端。用户使用登录、搜索或播放时，数据会传递给 Google/YouTube。

敏感安全问题请通过私有渠道联系维护者，不要在公开 issue 中发布凭据或可利用细节。