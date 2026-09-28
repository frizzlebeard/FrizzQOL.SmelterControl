# Smelter Control

| GitHub |
| --- |
| [github.com/frizzlebeard/FrizzQOL.SmelterControl](https://github.com/frizzlebeard/FrizzQOL.SmelterControl) |

> Set how fast the smelter, blast furnace, and charcoal kiln work, and how much they hold. 🔥

## What it does

Each station keeps its own time and load size. Out of the box every value stays the normal Valheim value. Coal burned per bar stays at 2. The kiln has no fuel slot.

Lowering a count leaves what is already inside. New items stop at the new count.

## Config

`BepInEx/config/com.frizzqol.smeltercontrol.cfg`

| Section | Setting | Default | What it does |
| --- | --- | --- | --- |
| Smelter | Seconds | 0 | Seconds per bar. 0 keeps 30. |
| Smelter | Ore | 0 | Ore it will hold. 0 keeps 10. |
| Smelter | Fuel | 0 | Coal it will hold. 0 keeps 20. |
| BlastFurnace | Seconds | 0 | Seconds per bar. 0 keeps 30. |
| BlastFurnace | Ore | 0 | Ore it will hold. 0 keeps 10. |
| BlastFurnace | Fuel | 0 | Coal it will hold. 0 keeps 20. |
| CharcoalKiln | Seconds | 0 | Seconds per coal. 0 keeps 15. |
| CharcoalKiln | Wood | 0 | Wood it will hold. 0 keeps 25. |

A number below 0 is treated the same as 0. Saving the file updates stations that are already loaded.

## Multiplayer

Install this on the dedicated server and on every client. Use the same config on each of them. The player who clicks checks the cap. The station owner runs the timer. If those configs differ, an add past the owner's cap is refused, and the clicked item can already be gone.

## Install

Install with r2modman or the Thunderstore Mod Manager.

To install by hand, copy `FrizzQOL.SmelterControl.dll` into `BepInEx/plugins`.

## Requirements

- Valheim
- [BepInExPack for Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

## Support

☕ If you enjoy my work, please buy me a coffee.

Cash App: `$FrizzleFry4`

## License

[MIT License](https://opensource.org/licenses/MIT). You can use, copy, change, and share this mod. The LICENSE file shipped with the package has the full text.
