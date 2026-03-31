# PROJECT KERNEX — The Shell (vsh)

---

## The Shell (vsh) — The Innovation Layer

### Philosophy

The shell is NOT required to play. It is an in-game power tool that gives an optimization edge. Everything the shell can do, the UI can also do — but the shell can chain, script, and automate in ways the UI cannot.

### Commands

Commands mirror the UI but with piping, filtering, and scripting capabilities:

```
vsh> scan --sector 7 | filter --resource crystallis | sort --by quantity
vsh> drone deploy --target AST-7A --type mining --auto-return
vsh> refinery status | watch --interval 5s
vsh> cron add --every 30m --command "scan --nearby | alert --if threat"
```

Full command reference: [COMMANDS.md](COMMANDS.md)

### Scripting (.vsh)

Players can write scripts to automate complex workflows:

```bash
#!/vsh
# auto_mine.vsh — Automated mining script
sectors=$(scan --all --format json | filter --has-resource metallum)
for sector in $sectors; do
    available=$(drone list --status idle --count)
    if [ $available -gt 0 ]; then
        drone deploy --target $sector --type mining --priority metallum
        log --write "Auto-deployed drone to $sector"
    fi
done
```

### Automation Progression (from Factorio)

Each automation tier solves a real pain point the player has already experienced:

| Tier | Analog | Unlocked When | Solves |
|------|--------|--------------|--------|
| Manual commands | Factorio's hand-crafting | Start | "I have to type this every time" |
| Aliases & pipes | Factorio's belts | Station Tier 2 | "I keep chaining the same commands" |
| Scripts (.vsh) | Factorio's trains | Station Tier 3 | "I need complex multi-step processes" |
| Cron jobs | Factorio's logistics bots | Station Tier 4 | "I need things to run while I'm away" |
| KIRA autonomy | Factorio's megabase | Station Tier 5 | "KIRA handles everything, I make strategy decisions" |

---

*Last updated: March 2026*
