# 命令参考

| 命令 | 说明 |
| --- | --- |
| `help`, `?` | 显示帮助 |
| `login` | Google 登录 |
| `logout` | 删除保存的 OAuth token |
| `whoami` | 显示当前账户 |
| `search <query>`, `s` | 搜索音乐 |
| `play <n>`, `p <n>` | 播放第 n 个结果 |
| `pause` | 暂停 |
| `resume` | 继续 |
| `stop` | 停止 |
| `next`, `n` | 下一首 |
| `prev` | 上一首 |
| `seek <秒|mm:ss>` | 跳转 |
| `volume <0-100>` | 设置音量 |
| `queue` | 查看队列 |
| `queue add <n>` | 加入队列 |
| `queue clear` | 清空队列 |
| `now` | 当前曲目 |
| `exit` | 退出 |

类似 `search ... | play 1` 的管道语法尚未实现。