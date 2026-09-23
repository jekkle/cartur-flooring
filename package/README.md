# Cartur's Flooring

A floor overhead keeps the rain off, and a fire sits on the floor you built for it.

![Cartur's Flooring](https://raw.githubusercontent.com/jekkle/cartur-flooring/master/media/nexus-header.png)

![A fire on a wooden floor, under a floor](https://raw.githubusercontent.com/jekkle/cartur-flooring/master/media/nexus-gallery.png)

**Floors count as roof.** Build a second storey and the room underneath still
soaks — your fire goes out, your walls take rain damage, and you stand indoors
with the wet debuff on. Wooden, ashwood and iron floors ship tagged `leaky`;
stone and grausten don't, which is why those always sheltered you. This clears
the tag, so rain damage, fire wetness and shelter status all follow at once.

**Fires go on floors.** Campfires, hearths, bonfires and braziers refuse to leave
bare earth. Three prefab flags do that, and all three are cleared on anything the
game counts as a fire. Smelters, kilns and blast furnaces get the same treatment.

Everything else about placement is untouched — it still needs support, it still
can't overlap, and smoke still has to get out.

**Floors stop rotting.** A floor with open sky above it is wet forever, and wet is
what the game charges rent for - a deck, a jetty or a bridge loses health to rain it
can never get out of. Floor pieces are now exempt from that rain damage. They still
darken when it rains, they still collapse without support, and they still refuse to
catch fire while wet; only the rot is gone.

**Existing buildings get it too.** The change is made to the prefabs before
anything spawns from them, so floors already standing behave the same. No
rebuilding.

## Settings

`BepInEx/config/com.jekkle.valheim.carturflooring.cfg`.

| Setting | Default | What it does |
| --- | --- | --- |
| `FloorsAreRoofs` | true | Floor tiles shelter the room below. |
| `FiresOnFloors` | true | Fireplace pieces place on floors, not just bare ground. |
| `SmeltersOnFloors` | true | Smelters, kilns and blast furnaces place on floors. |
| `FloorsDontDecay` | true | Floor tiles take no rain damage. |

The first three are made once to the shared prefabs at world load, so **those toggles
take effect after a game restart** - turning one off mid-session cannot put the prefab
back. `FloorsDontDecay` is read as the game runs and takes effect immediately.

[ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/)
makes the file easier to edit from the F1 menu. Optional.

## Worth knowing

**Smoke still matters.** A fire indoors with no outlet will still choke you.

**Multiplayer: install it on every client.** Each client edits its own copy of the
prefabs. Nothing is synced and nothing conflicts — a player without the mod simply
still sees rain through their floors.

Mods that add their own floors or fireplaces are covered automatically, as long as
they use the game's own floor flag or `Fireplace` component.

## Install

Use a mod manager (r2modman / Thunderstore / Gale) and it pulls in BepInEx for you.
Manually: drop `CarturFlooring.dll` into `BepInEx/plugins`.

---

*Free, and always will be. If it improved your game you can [tip me on Patreon](https://www.patreon.com/c/cartur).*

**More from Cartur:**
[HD Blood](https://thunderstore.io/c/valheim/p/Cartur/Carturs_HD_Blood/) ·
[Map Pins](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Map_Pins/) ·
[Compass and Clock](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Compass_and_Clock/) ·
[Safe Stamina](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Safe_Stamina/) ·
[Follow Command](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Follow_Command/) ·
[UI HUD](https://thunderstore.io/c/valheim/p/Cartur/Carturs_UI_HUD/) ·
[Waste Management](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Waste_Management/)
