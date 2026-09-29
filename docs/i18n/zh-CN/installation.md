# 安装

Setup.exe、MSI 和 Portable ZIP 都是正式支持的安装方式。

交互式 MSI 安装完成后会自动启动 NgMusic 配置器，用于保存 Google OAuth Desktop Client ID 和创建快捷方式，因此最终状态与 Setup.exe 一致。

静默 MSI（/qn）不会弹出配置窗口，适合企业部署。之后可运行 `ngmusic setup`。

Portable 模式不修改 Program Files 或 PATH。
