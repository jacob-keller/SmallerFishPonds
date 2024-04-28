using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace SmallerFishPondsSpace
{
    public class SmallerFishPond : FishPond
    {
        public float border_width = 0.8f;
        public List<SmallPondFishSilhouette> _smallPondFishSilhouettes;

        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="b"></param>
        /// <param name="tileLocation"></param>
        public SmallerFishPond(Vector2 tileLocation)
            : base(tileLocation)
        {
            fadeWhenPlayerIsBehind.Value = false;
            Reseed();
            _fishSilhouettes = new List<PondFishSilhouette>();
            _smallPondFishSilhouettes = new List<SmallPondFishSilhouette>();
            _jumpingFish = new List<JumpingFish>();
        }
        public SmallerFishPond()
        {
            _fishSilhouettes = new List<PondFishSilhouette>();
            _smallPondFishSilhouettes = new List<SmallPondFishSilhouette>();
            _jumpingFish = new List<JumpingFish>();
        }

        /// <summary>
        /// The base functions are Private so these get used spottily. 
        /// I don't know enough to really understand when or why
        /// </summary>
        /// <returns></returns>
        public virtual new Vector2 GetItemBucketTile()
        {
            return new Vector2(tileX.Value + 2, tileY.Value + 2);
        }

        public virtual new Vector2 GetRequestTile()
        {
            return new Vector2(tileX.Value + 1, tileY.Value + 1);
        }

        public virtual new Vector2 GetCenterTile()
        {
            return new Vector2(tileX.Value + 1, tileY.Value + 1);
        }

        public override Rectangle? getSourceRectForMenu()
        {
            return new Rectangle(0, 0, 48, 48);
        }

        /// <summary>
        /// Custom fish shadows
        /// </summary>
        /// <returns></returns>
        public List<SmallPondFishSilhouette> GetWellSilhouettes()
        {
            return _smallPondFishSilhouettes;
        }

        /// <summary>
        /// Various bits to implement the custom fish shadows
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime time)
        {
            //also need to preempt the JumpFish call in the base context
            if (_numberOfFishToJump > 0 && _timeUntilFishHop > 0f && _timeUntilFishHop <= (float)time.ElapsedGameTime.TotalSeconds && JumpFish())
            {
                _numberOfFishToJump--;
                _timeUntilFishHop = Utility.RandomFloat(0.15f, 0.25f) + (float)time.ElapsedGameTime.TotalSeconds;
            }
            base.Update(time);
            SyncSilhouettes();
            for (int i = 0; i < _smallPondFishSilhouettes.Count; i++)
            {
                _smallPondFishSilhouettes[i].Update((float)time.ElapsedGameTime.TotalSeconds);
            }
        }

        public override void resetLocalState()
        {
            base.resetLocalState();
            SyncSilhouettes();
        }

        public virtual void SyncSilhouettes()
        {
            if (_fishSilhouettes.Count > _smallPondFishSilhouettes.Count)
            {
                _smallPondFishSilhouettes.Add(new SmallPondFishSilhouette(this, border_width));
            }
            else if (_fishSilhouettes.Count < _smallPondFishSilhouettes.Count)
            {
                _smallPondFishSilhouettes.RemoveAt(0);
            }
        }

        public virtual void ReplacePondFish()
        {
            List<PondFishSilhouette> newFishSilhouettes = new List<PondFishSilhouette>();
            newFishSilhouettes = _fishSilhouettes.ConvertAll(new Converter<PondFishSilhouette, PondFishSilhouette>(PondFishToWellFish));
            _fishSilhouettes = newFishSilhouettes;
        }

        public virtual SmallPondFishSilhouette PondFishToWellFish(PondFishSilhouette pFish)
        {
            if (pFish.GetType().Equals(typeof(SmallPondFishSilhouette)))
            {
                return (SmallPondFishSilhouette)pFish;
            }
            else
            {
                return new SmallPondFishSilhouette(this, border_width);
            }
        }

        /// <summary>
        /// Adjust fish jumping
        /// </summary>
        /// <returns></returns>
        public new bool JumpFish()
        {
            if (_fishSilhouettes.Count == 0)
            {
                return false;
            }
            SmallPondFishSilhouette fish_silhouette = Game1.random.ChooseFrom(_smallPondFishSilhouettes);
            _smallPondFishSilhouettes.Remove(fish_silhouette);
            _fishSilhouettes.RemoveAt(0);
            _jumpingFish.Add(new JumpingFish(this,
                fish_silhouette.position,
                new Vector2(
                    tileX.Value + Utility.Lerp(border_width + 0.2f, (float)tilesWide.Value - border_width - 0.2f, (float)Game1.random.NextDouble()),
                    tileY.Value + Utility.Lerp(border_width + 0.2f, (float)tilesHigh.Value - border_width - 0.2f, (float)Game1.random.NextDouble())) * 64f
                ));
            return true;
        }

        /// <summary>
        /// Have to replace some values, move some pieces for the new textures to work
        /// Also makes some of the private overrides work
        /// </summary>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            y += 32;
            drawShadow(b, x, y);
            b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 48, 48, 48), new Color(60, 126, 150) * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 1f);
            for (int yWater = tileY.Value; yWater < tileY.Value + 3; yWater++)
            {
                for (int xWater = tileX.Value; xWater < tileX.Value + 2; xWater++)
                {
                    bool num = yWater == tileY.Value + 2;
                    bool topY = yWater == tileY.Value;
                    if (num)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(x + xWater * 64 + 32, y + (yWater + 1) * 64 - (int)Game1.currentLocation.waterPosition - 32), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((xWater + yWater) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 64, 32 + (int)Game1.currentLocation.waterPosition - 5), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.8f);
                    }
                    else
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(x + xWater * 64 + 32, y + yWater * 64 + 32 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f)), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((xWater + yWater) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(0f - Game1.currentLocation.waterPosition)) : 0)), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.8f);
                    }
                }
            }
            b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 0, 48, 48), color * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.9f);
            b.Draw(texture.Value, new Vector2(x + 64, y + 44 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2500.0 < 1250.0) ? 4 : 0)), new Rectangle(16, 160, 48, 7), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.95f);
            //no netting for now
            //b.Draw(texture.Value, new Vector2(x, y - 128), new Rectangle(48, 0, 48, 48), color.Value * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 1f);
        }

        public override void draw(SpriteBatch b)
        {
            if (base.isMoving)
            {
                return;
            }
            if (daysOfConstructionLeft.Value > 0)
            {
                drawInConstruction(b);
                return;
            }
            for (int l = animations.Count - 1; l >= 0; l--)
            {
                animations[l].draw(b);
            }
            drawShadow(b);
            //pond bed
            b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64, tileY.Value * 64 + tilesHigh.Value * 64)), new Rectangle(0, 48, 48, 48), (overrideWaterColor.Value.Equals(Color.White) ? new Color(60, 126, 150) : overrideWaterColor.Value * alpha), 0f, new Vector2(0f, 48f), 4f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f - 3f) / 10000f);
            for (int y = tileY.Value; y < tileY.Value + 3; y++)
            {
                for (int x = tileX.Value; x < tileX.Value + 2; x++)
                {
                    //water textures
                    bool num = y == tileY.Value + 2;
                    bool topY = y == tileY.Value;
                    if (num)
                    {
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (y + 1) * 64 - (int)Game1.currentLocation.waterPosition - 32)), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 64, 32 + (int)Game1.currentLocation.waterPosition - 5), overrideWaterColor.Equals(Color.White) ? (Game1.currentLocation.waterColor.Value) : (overrideWaterColor.Value * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f - 2f) / 10000f);
                    }
                    else
                    {
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f))), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(0f - Game1.currentLocation.waterPosition)) : 0)), (overrideWaterColor.Value.Equals(Color.White) ? Game1.currentLocation.waterColor.Value : (overrideWaterColor.Value * 0.5f)), 0f, Vector2.Zero, 1f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f - 2f) / 10000f);
                    }
                }
            }
            if (overrideWaterColor.Value.Equals(Color.White))
            {
                //water recolor?
                b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 64, tileY.Value * 64 + 44 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2500.0 < 1250.0) ? 4 : 0))), new Rectangle(16, 160, 48, 7), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f + 1f) / 10000f);
            }
            //main structure
            b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64, tileY.Value * 64 + tilesHigh.Value * 64)), new Rectangle(0, 0, 48, 48), color * alpha, 0f, new Vector2(0f, 48f), 4f, SpriteEffects.None, ((float)tileY.Value + 0.5f) * 64f / 10000f);
            if (nettingStyle.Value < 3)
            {
                //netting styles, disabled for now
                //b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64, tileY.Value * 64 + tilesHigh.Value * 64 - 128)), new Rectangle(80, nettingStyle.Value * 48, 80, 48), color * alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f + 2f) / 10000f);
            }
            if (sign.Value != null)
            {
                //sign sprite
                //layers moved up to cover grass n' trees n' stuff
                ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(sign.Value.QualifiedItemId);
                b.Draw(dataOrErrorItem.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 8 - 20, tileY.Value * 64 + tilesHigh.Value * 64 - 128 - 32 + 16)), dataOrErrorItem.GetSourceRect(), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)tileY.Value + 2.5f) * 64f + 2f) / 10000f); // previous tileY offset was 0.5f
                if (fishType.Value != null)
                {
                    //fish sprites
                    ParsedItemData data = ItemRegistry.GetData(fishType.Value);
                    if (data != null)
                    {
                        Texture2D texture2D = data.GetTexture();
                        Rectangle sourceRect = data.GetSourceRect();

                        b.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 8 + 8 - 4 - 20, tileY.Value * 64 + tilesHigh.Value * 64 - 128 - 8 + 4 + 16)), sourceRect, Color.Black * 0.4f * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)tileY.Value + 2.5f) * 64f + 3f) / 10000f);
                        b.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 8 + 8 - 1 - 20, tileY.Value * 64 + tilesHigh.Value * 64 - 128 - 8 + 1 + 16)), sourceRect, color * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)tileY.Value + 2.5f) * 64f + 4f) / 10000f);
                        //number
                        Utility.drawTinyDigits(currentOccupants.Value, b, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 32 + 8 - 20 + ((currentOccupants.Value < 10) ? 8 : 0), tileY.Value * 64 + tilesHigh.Value * 64 - 96 + 16)), 3f, (((float)tileY.Value + 2.5f) * 64f + 5f) / 10000f, Color.LightYellow * alpha);
                    }
                }
            }
            if (_fishObject != null && (_fishObject.QualifiedItemId == "(O)393" || _fishObject.QualifiedItemId == "(O)397"))
            {
                //resize and redistribute the coral, sea urchins
                for (int k = 0; k < currentOccupants.Value; k++)
                {
                    Vector2 drawOffset = Vector2.Zero;
                    float coralResize = 0.7f;
                    int drawI = (k + seedOffset.Value) % 10;
                    switch (drawI)
                    {
                        case 8: //0
                            drawOffset = new Vector2(0f, 0f);
                            break;
                        case 0: //1
                            drawOffset = new Vector2(48f, 32f);
                            break;
                        case 4: //2
                            drawOffset = new Vector2(80f, 72f);
                            break;
                        case 5: //3
                            drawOffset = new Vector2(140f, 28f);
                            break;
                        case 7: //4
                            drawOffset = new Vector2(96f, 0f);
                            break;
                        case 3: //5
                            drawOffset = new Vector2(0f, 96f);
                            break;
                        case 2: //6
                            drawOffset = new Vector2(140f, 80f);
                            break;
                        case 1: //7
                            drawOffset = new Vector2(64f, 120f);
                            break;
                        case 6: //8
                            drawOffset = new Vector2(140f, 140f);
                            break;
                        case 9: //9
                            drawOffset = new Vector2(0f, 150f);
                            break;
                    }
                    drawOffset = drawOffset * 0.6f + new Vector2(-28f, -28f);
                    b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 64 + (int)(7 * coralResize), tileY.Value * 64 + 64 + (int)(32 * coralResize)) + drawOffset), Game1.shadowTexture.Bounds, color * alpha, 0f, Vector2.Zero, 3f * coralResize, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f - 2f) / 10000f - 1.1E-05f);
                    ParsedItemData fishDataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + fishType.Value);
                    Texture2D texture2D2 = fishDataOrErrorItem.GetTexture();
                    Rectangle fishSourceRect = fishDataOrErrorItem.GetSourceRect();
                    b.Draw(texture2D2, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64 + 64, tileY.Value * 64 + 64) + drawOffset), fishSourceRect, color * alpha * 0.75f, 0f, Vector2.Zero, 3f * coralResize, (drawI % 3 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f - 2f) / 10000f - 1E-05f);
                }
            }
            else
            {
                //draw custom shadows instead
                for (int j = 0; j < _smallPondFishSilhouettes.Count; j++)
                {
                    _smallPondFishSilhouettes[j].Draw(b);
                }
            }
            for (int i = 0; i < _jumpingFish.Count; i++)
            {
                _jumpingFish[i].Draw(b);
            }
            if (HasUnresolvedNeeds())
            {
                //quest marker
                Vector2 drawn_position = GetRequestTile() * 64f;
                drawn_position += 64f * new Vector2(0.5f, 0.5f);
                float y_offset2 = 3f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                float bubble_layer_depth2 = (drawn_position.Y + 160f) / 10000f + 1E-06f;
                drawn_position.Y += y_offset2 - 32f;
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, drawn_position), new Rectangle(403, 496, 5, 14), Color.White * 0.75f, 0f, new Vector2(2f, 14f), 4f, SpriteEffects.None, bubble_layer_depth2);
            }

            if (goldenAnimalCracker.Value)
            {
                b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64, tileY.Value * 64) + new Vector2(124f, 128f) * 4f), new Rectangle(130, 96, 17, 16), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f + 2f) / 10000f);
            }

            if (output.Value != null)
            {
                //full bucket and output bubble
                b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64, tileY.Value * 64) + new Vector2(124f, 128f)), new Rectangle(0, 96, 17, 16), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f + 1f) / 10000f);
                if (goldenAnimalCracker.Value)
                {
                    b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileX.Value * 64, tileY.Value * 64) + new Vector2(124f, 128f) * 4f), new Rectangle(145, 96, 17, 16), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)tileY.Value + 0.5f) * 64f + 3f) / 10000f);
                }
                Vector2 value = GetItemBucketTile() * 64f;
                float y_offset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                Vector2 bubble_draw_position = value + new Vector2(0.15f, -1.4f) * 64f + new Vector2(0f, y_offset);
                Vector2 item_relative_to_bubble = new Vector2(40f, 36f);
                float bubble_layer_depth = (value.Y + 64f) / 10000f + 1E-06f;
                float item_layer_depth = (value.Y + 64f) / 10000f + 1E-05f;
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, bubble_layer_depth);
                ParsedItemData dataOrErrorItem3 = ItemRegistry.GetDataOrErrorItem(output.Value.QualifiedItemId);
                Texture2D texture2D3 = dataOrErrorItem3.GetTexture();
                b.Draw(texture2D3, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble), dataOrErrorItem3.GetSourceRect(), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, item_layer_depth);
                if (output.Value is ColoredObject)
                {
                    Rectangle sourceRect3 = ItemRegistry.GetDataOrErrorItem(output.Value.QualifiedItemId).GetSourceRect(1);
                    b.Draw(texture2D3, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble), sourceRect3, (output.Value as ColoredObject).color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, item_layer_depth + 1E-05f);
                }
            }
        }
    }
}
