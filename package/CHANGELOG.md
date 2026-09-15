# Changelog

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
