# Command reference

[Documentation home](../README.md)

NgMusic uses a compact PowerShell-inspired command line.

| Command | Description |
| --- | --- |
| `help`, `?` | Show built-in help |
| `login` | Authenticate with Google |
| `logout` | Remove saved OAuth credentials |
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
| `queue play` | Play the next queued item |
| `now`, `np` | Show current track |
| `clear`, `cls` | Clear terminal |
| `exit`, `quit` | Exit NgMusic |

## Example session

```text
PS Music:\> login
PS Music:\> search "Nujabes Feather"
PS Music:\> queue add 1
PS Music:\> queue add 3
PS Music:\> queue
PS Music:\> queue play
PS Music:\> volume 55
PS Music:\> seek 1:20
PS Music:\> now
```

Quoted search terms are supported:

```text
PS Music:\> search "Boards of Canada Dayvan Cowboy"
```

Pipeline syntax such as `search ... | play 1` is a roadmap item and is not implemented yet.
