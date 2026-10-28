# Custom Cheats for Mount & Blade II: Bannerlord

A cheat mod for **Bannerlord v1.4.8**, rebuilt from scratch to replace *Bannerlord Cheats v3.0.3* (made for v1.1.5), which crashes on current game versions. It covers the original mod's cheats plus a few extras, and every setting can be changed live in **Esc → Mod Options → Custom Cheats**.

## Requirements

- Bannerlord v1.4.8
- [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006), [ButterLib](https://www.nexusmods.com/mountandblade2bannerlord/mods/2018), [UIExtenderEx](https://www.nexusmods.com/mountandblade2bannerlord/mods/2102), [Mod Configuration Menu v5](https://www.nexusmods.com/mountandblade2bannerlord/mods/612)

## Install

1. Copy `release/CustomCheats` into `<Bannerlord>/Modules/` so you end up with `Modules/CustomCheats/SubModule.xml`.
2. Enable **Custom Cheats** in the launcher, below Harmony, ButterLib, UIExtenderEx and MCM. Put it after gameplay mods such as RBM and Diplomacy.

## Works with other mods

Cheats never replace game models. Each one is a Harmony patch on whatever model the game actually uses, including overrides from RBM, Diplomacy and similar mods, so their mechanics stay intact and the cheat multipliers apply on top. Patches are re-applied each time a campaign loads.

If a cheat can't find its hook (for example after a game update), only that cheat is turned off. The game shows a warning and the details go to `Modules/CustomCheats/bin/Win64_Shipping_Client/CustomCheats.log`. Errors inside a cheat are caught and logged once, so they never crash the campaign.

## Cheats

| Group | Cheats |
|---|---|
| General | Enable hotkeys, hotkey tips |
| Instant actions (buttons) | +1,000,000 gold, +1,000 influence, +1,000 renown, +10 attribute and +10 focus points, max all skills (300), max all attributes |
| Combat – player | God mode, mount god mode, one-hit kills, damage multiplier, damage taken %, health regeneration, infinite ammo, unbreakable shield, always crush through shields, slice through everyone, always knock down, never knocked back, no stuck arrows, instant crossbow reload |
| Combat – party & allies | Party invincible, party heroes invincible, allies invincible, party one-hit kills, party damage multiplier, party damage taken %, party health regeneration, party infinite ammo, party never flees, no friendly fire, knockout-or-killed for troops, companions and friendly lords |
| Combat – enemies | Knockout-or-killed for enemy lords and troops, enemies never flee, enemy damage % |
| Combat – rewards & misc | Battle renown and influence multipliers, always win simulated battles, no troop sacrifice, bandit hideout troop limit, camera zoom-out multiplier |
| Map | Map speed multiplier, map visibility multiplier, NPC map speed %, party invisible, caravans invisible |
| Party | Extra party size, extra prisoner size, extra morale, food consumption %, troop wages %, free troop upgrades, upgrade without required items (horses), free companion hiring, instant prisoner recruitment, no prisoner escape, instant escape from captivity, party healing multiplier |
| Inventory & trade | Extra carrying capacity, item spawning in inventory, item buying price %, selling price multiplier |
| Clan | Extra companion limit, extra clan party limit, extra clan party size |
| Characters | Relation gain after battle, perfect relationships, never die of old age, barter always accepted, no barter cooldown, persuasion always succeeds, perfect attraction, same-sex marriage, pregnancy chance multiplier, pregnancy duration |
| Kingdom | Decision weight multiplier, no relation loss on decisions, no crime rating, decision override influence cost % |
| Experience | Player, companion and clan skill XP multipliers, learning rate and learning limit multipliers, troop XP multiplier, free focus point assignment |
| Settlements | Daily food, loyalty, security, prosperity, hearth, militia and garrison bonuses, construction speed multiplier, garrison wages %, free troop recruitment, villages never raided, settlements never rebel, disguise always works, no bribe to enter keep, never require civilian equipment, tournament max bet multiplier |
| Sieges & armies | Siege construction speed (player side), enemy siege construction %, player and faction army cohesion loss %, player army food consumption % |
| Smithing | Stamina cost % (0% = infinite stamina), unlock all parts, part difficulty %, material cost % |
| Workshops | Workshop buying cost % |

## Hotkeys

All hotkeys need **Enable Hotkeys** turned on.

| Where | Keys |
|---|---|
| Battle | `Ctrl+H` fully heal you and your horse · `Ctrl+K` kill all enemies |
| Inventory | `Ctrl+X` +1,000 gold (`Ctrl+Shift+X` +100,000) · `Ctrl+H` +1 of the selected item (`Ctrl+Shift+H` +10) |
| Party screen | `Ctrl+H` +1 of the selected troop (`Ctrl+Shift+H` +10) · `Ctrl+X` give the selected troop enough XP to upgrade |
| Character screen | `Ctrl+A` max attributes · `Ctrl+1`…`Ctrl+6` +1 Vigor/Control/Endurance/Cunning/Social/Intelligence · `Ctrl+F` +1 focus point · `Ctrl+G` +1 attribute point |
| Clan screen | `Ctrl+X` +1,000 influence |
| Encyclopedia | Hero page: `Ctrl+H` play as this hero, `Ctrl+X` kill this hero · Town/castle page: `Ctrl+H` take ownership · Troop page: `Ctrl+H` add troops (`Ctrl+Shift+H` +10) |

The encyclopedia actions ask for confirmation first.

## Not carried over from v3.0.3

These features depended on game code that no longer exists in v1.4.8:

- **Crafted weapon stat bonuses** (handling, swing/thrust damage and speed). Crafting now gives quality modifiers instead of stat overrides.
- **Workshop daily expense, upgrade cost and selling price**, and **everyone buys workshops**. Workshop upgrades and that selling system were removed from the game.
- **Per-campaign settings**. Settings are global.

## Building

You need the .NET SDK 8 or newer.

```bash
dotnet build -c Release
```

The build references the game DLLs in place: nothing is copied into the output except `CustomCheats.dll`. It also copies the module into the game's `Modules` folder and refreshes `release/CustomCheats`. If your game or Steam Workshop folder is somewhere else, override the paths:

```bash
dotnet build -c Release -p:GameFolder="X:\Games\Mount & Blade II Bannerlord" -p:WorkshopFolder="X:\Steam\steamapps\workshop\content\261550"
```

### Patch check

`tests/PatchCheck` applies every Harmony patch to vanilla and RBM model classes without starting the game. It reports any hook that is missing or rejected, so run it after game updates.

```bash
dotnet run --project tests/PatchCheck
```
