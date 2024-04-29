# The Return of Smaller Fish Ponds - 2.0.0

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

On save, the ponds are restored to standard fish ponds. By default, the size
is kept at 3x3 to avoid any issues with crops or other structures nearby the
pond. However, this results in graphical glitches if the save is loaded
without this mod.

You can disable this behavior to revert the ponds to 5x5 size on save by
disabling the "Keep smaller size on save". This is only recommended to do if
you're about to remove the mod.

## Known Issues

* The Fish Pond background is white in the construction menu and during the
  animation for building.

* The mod (by default) will save fish ponds using a smaller 3x3 size in the
  save file. This may create graphical issues if you load the save without
  the mod enabled. To avoid this, disable the "Keep smaller size on save"
  option and save the game before removing this mod.

## Credits

* Peasly Wellbott for the original mod, and for making it open source
* platinummyr for updating to Stardew Valley 1.6
