# 配置与 Google OAuth

运行 `login`。如果没有 Client ID，NgMusic 自动启动交互式设置向导。

粘贴 Google OAuth **Desktop app** Client ID 后，它会保存到：

`%LOCALAPPDATA%\NgMusic\config.json`

OAuth access/refresh token 不会写入该 JSON，而是保存在 Windows Credential Manager。

命令：
- `setup`: 配置或替换 Client ID
- `config show`: 查看配置状态
- `config path`: 查看配置文件路径
- `config reset`: 删除本地配置
- `logout`: 删除 OAuth token

环境变量仍然支持，并优先于本地配置。
