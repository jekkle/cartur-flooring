# Cartur's Flooring

Floors count as a roof, fires and smelters can be placed on them, and floors stop rotting in the rain.

![Cartur's Flooring](https://raw.githubusercontent.com/jekkle/cartur-flooring/master/media/nexus-header.png)

## What it does

Build a second storey out of wood and the room underneath still counts as open sky. Rain gets in, the fire goes out and you stand indoors with the wet debuff. The cause is that wooden, ashwood and iron floor pieces ship with their colliders tagged `leaky`, which the game's roof checks skip over. Stone and grausten floors are not tagged that way, which is why those always sheltered you. This mod clears the tag on floor pieces, so shelter, rain damage and fire wetness all follow the floor as a roof.

Campfires, hearths, bonfires and braziers normally refuse to leave bare ground. Three flags on the prefab cause that (`m_notOnWood`, `m_groundOnly` and `m_groundPiece`), and the mod clears all three on any piece the game counts as a fire. Smelters, charcoal kilns and blast furnaces get the same treatment. The rule is applied to every piece with a `Smelter` component, which also includes the windmill, the spinning wheel and the bathtub, and to modded pieces built on it. Stone stations also lose their no-support-wear flag, because otherwise the game would demolish a stone smelter in one tick for standing on a wooden plank.

A floor with open sky above it is wet forever, and the game charges rent for that: a deck, a jetty or a bridge loses health to rain it can never get out of. Floor pieces are exempt from that rain damage. They still darken when it rains, they still collapse without support, and snow, ash and fire wetness behave as before. Only the rot is gone.

The changes are made to the prefabs before anything spawns from them, so floors already standing behave the same. You do not need to rebuild. Other placement rules are not changed.

## Settings

Config file: `BepInEx/config/com.jekkle.valheim.carturflooring.cfg`.

| Setting | Default | What it does |
| --- | --- | --- |
| `FloorsAreRoofs` | true | Floor tiles shelter the room below. |
| `FiresOnFloors` | true | Fireplace pieces place on floors as well as bare ground. |
| `SmeltersOnFloors` | true | Smelters, kilns and blast furnaces place on floors. |
| `FloorsDontDecay` | true | Floor tiles take no rain damage. |

The first three are applied once to the shared prefabs at world load, so those toggles take effect after a game restart. Turning one off mid-session cannot put the prefab back. `FloorsDontDecay` is read as the game runs and takes effect immediately.

[ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/) makes the file easier to edit from the F1 menu. It is optional.

## Compatibility and known issues

Every player on a server needs the mod. Each client edits its own copy of the prefabs, and wear is calculated by whoever owns the piece. A smelter, kiln or windmill on a floor owned by a player without the mod still takes wear and breaks, and that player still sees rain through their floors.

Smoke still matters. A fire indoors with no outlet will still choke you.

Mods that add their own floors or fireplaces are covered automatically, as long as they use the game's own floor flag or a `Fireplace` component. Iron grates do not carry the game's floor flag, so the mod does not treat them as roof. The mod logs the pieces it changed at world load, and warns about any piece named like a floor that is still leaky because it lacks the floor flag.

## Install

Use a mod manager (r2modman, Thunderstore or Gale) and it pulls in BepInEx for you. To install by hand, drop `CarturFlooring.dll` into `BepInEx/plugins`.

---

*Free, and always will be. If it improved your game you can [tip me on Patreon](https://www.patreon.com/c/cartur).*

**[Discord](https://discord.gg/nd5RqpwNkz)**: bug reports, install help, and mod requests.
Bug reports get their own thread so nothing is lost in a chat scroll, and requests are voted on.

**More from Cartur:**
[HD Blood](https://thunderstore.io/c/valheim/p/Cartur/Carturs_HD_Blood/) ·
[Map Pins](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Map_Pins/) ·
[Compass and Clock](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Compass_and_Clock/) ·
[Safe Stamina](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Safe_Stamina/) ·
[Follow Command](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Follow_Command/) ·
[UI HUD](https://thunderstore.io/c/valheim/p/Cartur/Carturs_UI_HUD/) ·
[Waste Management](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Waste_Management/) ·
[Feeding Trough](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Feeding_Trough/) ·
[Combat Text](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Combat_Text/)
