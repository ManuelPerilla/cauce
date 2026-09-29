# Command reference

[Documentation home](../README.md)

| Command | Description |
| --- | --- |
| `setup`, `configure` | Run interactive Google OAuth configuration |
| `config show` | Show configuration source/status |
| `config path` | Show local configuration file path |
| `config reset` | Remove local JSON configuration |
| `login` | Authenticate with Google; runs setup automatically if needed |
| `logout` | Remove saved OAuth token |
| `whoami` | Show current authenticated account |
| `search <query>`, `s <query>` | Search YouTube music videos |
| `play <n>`, `p <n>` | Play result number `n` |
| `pause` | Pause playback |
| `resume` | Resume playback |
| `stop` | Stop playback |
| `next`, `n` | Play next queued track |
| `prev`, `previous` | Play previous track from history |
| `seek <seconds>` | Seek to an absolute position |
| `seek <mm:ss>` | Seek using minute/second notation |
| `volume <0-100>`, `vol <0-100>` | Set player volume |
| `queue`, `q` | Show current queue |
| `queue add <n>` | Add search result `n` to queue |
| `queue clear` | Clear queue |
| `queue play` | Play next queued item |
| `now`, `np` | Show current track |
| `clear`, `cls` | Clear terminal |
| `exit`, `quit` | Exit NgMusic |

## First login

```text
PS Music:\> login

Google OAuth setup
------------------
 [1] Paste Google OAuth Client ID
 [2] Open step-by-step setup instructions
 [3] Cancel
```

After the Client ID has been saved, future `login` commands skip the wizard unless configuration is reset or replaced.
