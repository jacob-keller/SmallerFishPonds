# The Return of Smaller Fish Ponds - 1.0.0

This is an update/re-write of the functionality from Smaller Fish Ponds
originally created by Peasly Wellbott. This implementation borrows somewhat
heavily from the original but lacks some of the features I felt were
unnecessary and struggled to get working.

## Requirements

* Stardew Valley 1.6
* SMAPI 4.0.0
* Generic Mod Menu (Optional)

## Features

* Converts Fish Ponds to 3x3 buildings
* Custom logic to implement shadows and fish silhouettes
* All Fish Ponds are converted automatically
* Fish Ponds are restored to their usual 5x5 size on saving
* Configurable with Generic Mod Menu
* Supports Stardew Valley 1.6

## Technical Details

This mod is similar to the
[original](https://www.nexusmods.com/stardewvalley/mods/7651) but has a few
distinctions.

Primarily, this mod does not allow having both normal and small sized ponds
at the same time. All ponds are automatically converted to the small size on
creation and at the start of the day.

Additionally, ponds are restored to their 5x5 size on saving. If you disable
this mod, all the ponds should revert to normal. The only damage to your
save file should be the need to have Robin move buildings due to their size
change.

## Known Issues

* The Fish Pond background is white in the construction menu and during the
  animation for building.

* Changing Instant Construction configuration requires a restart.

## Credits

* Peasly Wellbott for the original mod, and for making it open source
* platinummyr for updating to Stardew Valley 1.6
