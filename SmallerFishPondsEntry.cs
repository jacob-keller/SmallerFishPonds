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
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.Saving += this.OnSaving;
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
                save: () => this.Helper.WriteConfig(this.Config),
                titleScreenOnly: true
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
                getValue: () => this.Config.ModEnabled,
                setValue: value => this.Config.ModEnabled = value
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

        private void RecreateAsSmallerPond(Vector2 pondTile)
        {
            this.Monitor.Log($"Converting Pond at {pondTile} to smaller 3x3 size.", LogLevel.Trace);
            Farm farm = Game1.getLocationFromName("Farm") as Farm;
            Building oldBuilding = farm.getBuildingAt(pondTile);
            SmallerFishPond newPond = new(Vector2.Zero);
            ReplacePondData(oldBuilding, newPond);
            farm.destroyStructure(oldBuilding);
            farm.buildStructure(newPond, pondTile, Game1.player, true);
            newPond.performActionOnBuildingPlacement();
            newPond.resetTexture();
        }

        private void RecreateAsNormalPond(Vector2 pondTile)
        {
            this.Monitor.Log($"Converting Pond at {pondTile} to normal 5x5 size.", LogLevel.Trace);
            Farm farm = Game1.getLocationFromName("Farm") as Farm;
            Building oldBuilding = farm.getBuildingAt(pondTile);
            FishPond newPond = new(Vector2.Zero);
            ReplacePondData(oldBuilding, newPond);
            newPond.tilesWide.Value = 5;
            newPond.tilesHigh.Value = 5;
            farm.destroyStructure(oldBuilding);
            farm.buildStructure(newPond, pondTile, Game1.player, true);
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

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            //on menu exit, scan for any new fish ponds and convert them to SmallerFishPond class
            if (e.OldMenu is CarpenterMenu)
            {
                List<Vector2> tilesWithPonds = new();
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.buildingType.Value == "Fish Pond" && building is not SmallerFishPond)
                    {
                        tilesWithPonds.Add(new Vector2(building.tileX.Value, building.tileY.Value));
                    }
                }
                if (this.Config.ModEnabled)
                {
                    tilesWithPonds.ForEach(RecreateAsSmallerPond);
                }
                else
                {
                    tilesWithPonds.ForEach(RecreateAsNormalPond);
                }
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Only convert to smaller size if the mod is enabled
            if (this.Config.ModEnabled && Context.IsMainPlayer)
            {
                List<Vector2> tilesWithPonds = new();
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.buildingType.Value == "Fish Pond" && building is not SmallerFishPond)
                    {
                        tilesWithPonds.Add(new Vector2(building.tileX.Value, building.tileY.Value));
                    }
                }
                tilesWithPonds.ForEach(RecreateAsSmallerPond);
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Always convert back to normal size when saving even if we're disabled
            if (Context.IsMainPlayer)
            {
                List<Vector2> tilesWithPonds = new();
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.buildingType.Value == "Fish Pond" && building is SmallerFishPond)
                    {
                        tilesWithPonds.Add(new Vector2(building.tileX.Value, building.tileY.Value));
                    }
                }
                tilesWithPonds.ForEach(RecreateAsNormalPond);
            }
        }

    }
}
