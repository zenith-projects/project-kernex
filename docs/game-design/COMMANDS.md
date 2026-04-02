# PROJECT KERNEX — Shell Commands Reference

> *Reference for all in-game `vsh` shell commands.*
> *This is a design document — commands will be implemented incrementally.*

---

## Navigation & Information

| Command | Description | Example |
|---------|-------------|---------|
| `help` | List all available commands | `help` |
| `man <cmd>` | Detailed manual for a command | `man scan` |
| `status` | Station overview | `status` |
| `log` | View event log | `log --last 20` |
| `clear` | Clear shell screen | `clear` |
| `map` | Display sector map | `map --zoom 2` |
| `whoami` | Display operator info | `whoami` |
| `uptime` | Station uptime | `uptime` |

## Resources & Production

| Command | Description | Example |
|---------|-------------|---------|
| `scan` | Scan sectors for resources | `scan --sector 7` |
| `drone` | Manage drones | `drone deploy --target AST-7A` |
| `inventory` | View stored resources | `inventory --sort quantity` |
| `refinery` | Manage refineries | `refinery start --input iron_ore` |
| `craft` | Craft components | `craft circuit_board --quantity 5` |

## Station Management

| Command | Description | Example |
|---------|-------------|---------|
| `build` | Construct modules | `build module --type solar_array` |
| `upgrade` | Upgrade modules | `upgrade refinery-a --level 3` |
| `repair` | Repair damaged systems | `repair hull --bay 2` |
| `power` | Power grid management | `power status` |
| `systems` | System overview | `systems --detailed` |

## Defense

| Command | Description | Example |
|---------|-------------|---------|
| `defense` | Defense systems | `defense status` |
| `alert` | Manage alerts | `alert set --type proximity` |

## Communication & AI

| Command | Description | Example |
|---------|-------------|---------|
| `axia` | Talk to AXIA AI | `axia "status report"` |
| `comms` | Communications | `comms broadcast --message "..."` |
| `beacon` | Manage beacons | `beacon deploy --type trade` |

## Scripting & Automation

| Command | Description | Example |
|---------|-------------|---------|
| `script` | Run/manage scripts | `script run auto_mine.vsh` |
| `cron` | Schedule tasks | `cron add --every 30m --cmd "..."` |
| `alias` | Create shortcuts | `alias mine="drone deploy --type mining"` |
| `watch` | Monitor values | `watch power --interval 5s` |
| `pipe` | Chain commands | `scan | filter --resource iron | drone deploy` |

---

## Flags & Options

Most commands support common flags:

| Flag | Description |
|------|-------------|
| `--help`, `-h` | Show command help |
| `--verbose`, `-v` | Detailed output |
| `--format json` | Output as JSON (for piping) |
| `--quiet`, `-q` | Suppress non-essential output |
| `--dry-run` | Preview without executing |

---

*This reference will be expanded as new commands are designed and implemented.*
