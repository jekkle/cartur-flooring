# Cartur's Flooring

*Free, and always will be — if it improved your game you can [tip me on Patreon](https://www.patreon.com/c/cartur).*

**More from Cartur:** [HD Blood](https://thunderstore.io/c/valheim/p/Cartur/Carturs_HD_Blood/) ·
[Map Pins](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Map_Pins/) ·
[Follow Command](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Follow_Command/) ·
[Compass and Clock](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Compass_and_Clock/) ·
[Safe Stamina](https://thunderstore.io/c/valheim/p/Cartur/Carturs_Safe_Stamina/)

A floor overhead keeps the rain off. A fire sits on the floor you built for it.

Two things the base game says no to, for reasons that turn out to be one tag and
two flags on the prefabs rather than anything the game actually simulates.

## Floors count as roof

Build a second storey and the room underneath still soaks. Your fire goes out,
your walls take rain damage, and you are standing indoors with the wet debuff on.

That is not the roof check being clever about gaps. Valheim decides "am I under
cover" by casting straight up and taking the first collider that is **not tagged
`leaky`** — and the wood, ashwood and iron floor pieces ship with that tag on
their colliders. Stone and grausten floors do not, which is why those have always
sheltered you and the wooden ones never have.

This clears the tag on floor pieces. A floor above you is a roof, the same way a
stone floor already was. Rain damage, fire wetness and the shelter status all read
that one check, so all three follow at once.

Floors affected: `wood_floor`, `wood_floor_1x1`, `ashwood_floor_1x1`,
`ashwood_floor_2x2`, `ashwood_deco_floor`, `iron_floor_1x1`, `iron_floor_1x1_v2`,
`iron_floor_2x2`, `iron_grate`. Anything the game itself tags as a floor piece is
covered, mods included — there is no list of prefab names baked in.

## Fires on floors

Campfires, hearths, bonfires and braziers refuse to leave bare earth. Two prefab
flags do that: `notOnWood` rejects any wooden piece under the cursor, and
`groundOnly` rejects anything that is not raw terrain.

Both are cleared on every fireplace piece, so a fire places on the floor you built
for it. Everything else about placement is untouched — it still needs support, it
still cannot overlap, and smoke still has to get out.

## Config

`BepInEx/config/com.jekkle.valheim.carturflooring.cfg`, written on first run.

| Setting | Default | What it does |
| --- | --- | --- |
| `FloorsAreRoofs` | true | Floor tiles shelter the room below. |
| `FiresOnFloors` | true | Fireplace pieces place on floors, not just bare ground. |

Both changes are made once to the shared prefabs at world load, so **a toggle takes
effect after a game restart**. Turning one off mid-session cannot put the prefab
back, and the config description says so rather than pretending otherwise.

[ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/)
makes the file easier to edit from the F1 menu. It is optional; this mod does not
depend on it.

## Worth knowing

**Existing buildings get it too.** The change is made to the prefabs before
anything is spawned from them, so the floors already standing in your world behave
the same as ones you place afterwards. No rebuilding.

**Grates shelter as well.** `iron_grate` is a floor piece by the game's own
classification, so it stops leaking with the rest. If you wanted the grate to stay
open to the sky, turn `FloorsAreRoofs` off — there is no per-piece switch.

**Smoke still matters.** A fire indoors with no smoke outlet will still choke you.
Nothing here touches the smoke system.

**Multiplayer: install it on every client.** Both changes are made to each client's
own copy of the prefabs, so a player without the mod still sees rain come through
their floors and still cannot put a fire on one. Nothing is synced and nothing
conflicts — the two clients simply disagree about that one tag.

## Compatibility

No transpilers and no method is replaced. One postfix on `ZNetScene.Awake` edits
prefab data once and then does nothing for the rest of the session — there is no
per-frame cost and no behaviour for another mod to collide with. Mods that add
their own floor pieces are covered automatically if they set the game's Floor usage
flag, and modded fireplaces are covered if they use the `Fireplace` component.
