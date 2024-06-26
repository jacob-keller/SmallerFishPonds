using System;
using System.Collections.Generic;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using xTile.Tiles;

namespace SmallerFishPondsSpace
{
    public class SmallerFishPondsEntry : Mod
    {
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.Saving += this.OnSaving;
        }

        private void SaveConfig()
        {
            this.Helper.WriteConfig(this.Config);
            RecreateAllPonds(smallSize: this.Config.ModEnabled);
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: SaveConfig
            );

            // add boolean options
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => this.Config.ModEnabled,
                setValue: value => this.Config.ModEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("GMCM_Option_InstantConstruction_Name"),
                tooltip: () => this.Helper.Translation.Get("GMCM_Option_InstantConstruction_Description"),
                getValue: () => this.Config.InstantConstruction,
                setValue: value => this.Config.InstantConstruction = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("GMCM_Option_KeepSmallerSizeOnSave_Name"),
                tooltip: () => this.Helper.Translation.Get("GMCM_Option_KeepSmallerSizeOnSave_Description"),
                getValue: () => this.Config.KeepSmallSizeOnSave,
                setValue: value => this.Config.KeepSmallSizeOnSave = value
            );
        }

        private void ReplacePondData(Building fromBuilding, FishPond toPond)
        {
            this.Monitor.Log($"Copying basic Building data.", LogLevel.Trace);
            toPond.daysOfConstructionLeft.Value = fromBuilding.daysOfConstructionLeft.Value;
            toPond.daysUntilUpgrade.Value = fromBuilding.daysUntilUpgrade.Value;
            toPond.owner.Value = fromBuilding.owner.Value;
            toPond.currentOccupants.Value = fromBuilding.currentOccupants.Value;
            toPond.maxOccupants.Value = fromBuilding.maxOccupants.Value;

            if (fromBuilding is FishPond fromPond)
            {
                this.Monitor.Log($"Copying detailed Fish Pond data.", LogLevel.Trace);
                toPond.fishType.Value = fromPond.fishType.Value;
                toPond.lastUnlockedPopulationGate.Value = fromPond.lastUnlockedPopulationGate.Value;
                toPond.hasCompletedRequest.Value = fromPond.hasCompletedRequest.Value;
                toPond.goldenAnimalCracker.Value = fromPond.goldenAnimalCracker.Value;
                toPond.sign.Value = fromPond.sign.Value;
                toPond.overrideWaterColor.Value = fromPond.overrideWaterColor.Value;
                toPond.output.Value = fromPond.output.Value;
                toPond.neededItemCount.Value = fromPond.neededItemCount.Value;
                toPond.neededItem.Value = fromPond.neededItem.Value;
                toPond.daysSinceSpawn.Value = fromPond.daysSinceSpawn.Value;
                toPond.nettingStyle.Value = fromPond.nettingStyle.Value;
                toPond.seedOffset.Value = fromPond.seedOffset.Value;
                toPond.hasSpawnedFish.Value = fromPond.hasSpawnedFish.Value;
            }
        }

        private void RecreateAsSmallerPond(GameLocation location, Vector2 pondTile)
        {
            this.Monitor.Log($"Converting Pond at {pondTile} in {location} to smaller 3x3 size.", LogLevel.Trace);
            Building oldBuilding = location.getBuildingAt(pondTile);
            SmallerFishPond newPond = new(Vector2.Zero);
            ReplacePondData(oldBuilding, newPond);
            newPond.tilesWide.Value = 3;
            newPond.tilesHigh.Value = 3;
            location.destroyStructure(oldBuilding);
            location.buildStructure(newPond, pondTile, Game1.player, true);
            newPond.performActionOnBuildingPlacement();
            newPond.resetTexture();
        }

        private void RecreateAsNormalPond(GameLocation location, Vector2 pondTile)
        {
            this.Monitor.Log($"Converting Pond at {pondTile} in {location} to normal 5x5 size.", LogLevel.Trace);
            Building oldBuilding = location.getBuildingAt(pondTile);
            FishPond newPond = new(Vector2.Zero);
            ReplacePondData(oldBuilding, newPond);
            if (this.Config.KeepSmallSizeOnSave) {
                newPond.tilesWide.Value = 3;
                newPond.tilesHigh.Value = 3;
            } else {
                newPond.tilesWide.Value = 5;
                newPond.tilesHigh.Value = 5;
            }
            location.destroyStructure(oldBuilding);
            location.buildStructure(newPond, pondTile, Game1.player, true);
            newPond.performActionOnBuildingPlacement();
            newPond.UpdateMaximumOccupancy();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!this.Config.ModEnabled)
                return;

            if (e.Name.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var buildings = asset.AsDictionary<string, StardewValley.GameData.Buildings.BuildingData>().Data;
                    var fishPond = buildings["Fish Pond"];
                    if (this.Config.InstantConstruction)
                    {
                        fishPond.BuildDays = 0;
                    }
                    fishPond.BuildingType = null; // Use default building type during construction
                    fishPond.Size.X = 3;
                    fishPond.Size.Y = 3;
                    fishPond.SourceRect = new Rectangle(0, 0, 48, 48);
                    fishPond.DrawLayers = new List<BuildingDrawLayer>();
                    var shadowLayer = new BuildingDrawLayer
                    {
                        Id = "DefaultShadow",
                        SourceRect = new Rectangle(0, 48, 48, 48),
                        DrawInBackground = true
                    };
                    fishPond.DrawLayers.Add(shadowLayer);
                });
            }
            else if (e.Name.IsEquivalentTo("Buildings/Fish Pond"))
            {
                e.LoadFromModFile<Texture2D>("assets/Smaller Fish Pond.png", AssetLoadPriority.High);
            }
        }

        private void RecreatePondsAt(GameLocation location, IEnumerable<Building> buildings, bool smallSize)
        {
            List<Vector2> tilesWithPonds = new();
            foreach (Building building in buildings) {
                if (building.buildingType.Value == "Fish Pond") {
                    // Skip if we're already the right size
                    if (smallSize && building is SmallerFishPond)
                        continue;
                    if (!smallSize && building is FishPond)
                        continue;
                    tilesWithPonds.Add(new Vector2(building.tileX.Value, building.tileY.Value));
                }
            }

            foreach (Vector2 tile in tilesWithPonds) {
                if (smallSize) {
                    RecreateAsSmallerPond(location, tile);
                } else {
                    RecreateAsNormalPond(location, tile);
                }
            }
        }

        private void RecreateAllPonds(bool smallSize)
        {
            foreach (GameLocation location in Game1.locations) {
                RecreatePondsAt(location, location.buildings, smallSize);
            }
        }

        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            if (!this.Config.ModEnabled)
                return;

            RecreatePondsAt(e.Location, e.Added, smallSize: true);
        }

        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            if (!this.Config.ModEnabled)
                return;

            foreach (GameLocation location in e.Added) {
                RecreatePondsAt(location, location.buildings, smallSize: true);
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Only convert to smaller size if the mod is enabled
            if (this.Config.ModEnabled && Context.IsMainPlayer) {
                RecreateAllPonds(smallSize: true);
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Always convert back to normal size when saving even if we're disabled
            if (Context.IsMainPlayer) {
                RecreateAllPonds(smallSize: false);
            }
        }
    }
}
