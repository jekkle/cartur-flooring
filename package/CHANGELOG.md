# Changelog

## 1.1.0

- **Smelters, kilns and blast furnaces place on floors.** Same three placement flags
  as the fire pieces, on a different set of prefabs; the `Smelter` component is the
  selector. Freeing the flags alone was not enough - `WearNTear.UpdateWear` deals
  `damage = 100f` when `m_noSupportWear` is set and support is short, which is a full
  health bar in one tick, and a stone piece needs 100 support where wood tops out at
  100 and loses a share per link. `m_noSupportWear` is cleared on those prefabs too,
  so the game stops demolishing a smelter the moment it stands on a plank. New
  `SmeltersOnFloors` setting.
- **Floors take no rain damage.** A deck, a jetty or a bridge has nothing overhead,
  fails the roof check forever and rots forever. The rain branch of
  `WearNTear.UpdateWear` only bites when sixty seconds have passed on `m_rainTimer`,
  so that timer is held at now for floor pieces and the branch never reaches its
  damage line. Clearing the prefab's `m_noRoofWear` would have been simpler and
  wrong: the same flag drives the wet sheen, and a floor that cannot rot should still
  darken when it rains. Support collapse, snow, ash and fire wetness all behave as
  before. New `FloorsDontDecay` setting, and this one is read live - no restart.

## 1.0.1

- Hearths and braziers now place on floors. 1.0.0 picked fire pieces by the
  `Fireplace` component and cleared `notOnWood` and `groundOnly`; those two carry
  `groundPiece` instead, which fails the same heightmap test and returns before the
  other checks are even reached. All three flags are cleared now, and a piece counts
  as a fire if it has a `Fireplace` **or** its comfort group is Fire.
- `iron_grate` was listed as sheltering the room below. It never did — grates do not
  carry the game's Floor usage flag, so the mod does not touch them. Documentation fix
  only, no behaviour change.

## 1.0.0

First release.

Floor pieces no longer count as `leaky`, so a floor overhead shelters the room
below — rain damage, fire wetness and the shelter status all follow. Fireplace
pieces have `notOnWood` and `groundOnly` cleared, so fires place on floor tiles
instead of only bare terrain. Each feature has its own config switch.
