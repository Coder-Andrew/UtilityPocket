using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace FoodPocket
{
    public class ModConfig
    {
        public SButton UsePocketedItemKey { get; set; } = SButton.Q;
        public SButton PocketActiveItemKey { get; set; } = SButton.R;
        public int EnergyCost { get; set; } = 6;
    }
    public class PocketData
    {
        public string PlayerUniqueID { get; set; } = "";
        public string QualifiedItemId { get; set; } = "";
        public int Quality { get; set; }
        public int Quantity { get; set; }
    }
    public class PlayerPockets
    {
        public List<PocketData> pocketDatas { get; set; } = new List<PocketData>();
    }

    internal sealed class ModEntry : Mod
    {
        private Texture2D? customHud;
        private Item? pocketItem;
        private bool isItemPocketed = false;
        private ModConfig Config;
        private PocketData pocketData = new PocketData();
        //private PlayerPockets playerPockets = new PlayerPockets();

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.Saved += this.OnSaving;
            helper.Events.GameLoop.SaveLoaded += this.OnLoading;
            //helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Display.RenderedHud += this.OnRenderingHud;
        }

        private void OnLoading(object? sender, SaveLoadedEventArgs e)
        {
            pocketItem = null;
            isItemPocketed = false;

            // Load the entire player pockets data
            PlayerPockets playersPockets = this.Helper.Data.ReadJsonFile<PlayerPockets>(Path.Join("assets", "data.json"))
                ?? new PlayerPockets { pocketDatas = new List<PocketData>() };

            string playerID = Game1.player.UniqueMultiplayerID.ToString();

            // Find the current player's pocket data
            pocketData = playersPockets.pocketDatas.Find(p => p.PlayerUniqueID == playerID)
                ?? new PocketData { PlayerUniqueID = playerID };

            // If no item is found in the player's pocket, return
            if (string.IsNullOrEmpty(pocketData.QualifiedItemId)) return;

            // Load the pocketed item
            pocketItem = ItemRegistry.Create(pocketData.QualifiedItemId, pocketData.Quantity, quality: pocketData.Quality);
            isItemPocketed = true;

            LogMessage($"Loading {pocketItem.Stack.ToString()}x {pocketItem.Name} from data.json from player with ID {pocketData.PlayerUniqueID}");
        }


        private void OnSaving(object? sender, SavedEventArgs e)
        {
            // Load the entire player pockets data, ensuring we keep all players' data intact
            PlayerPockets playersPockets = this.Helper.Data.ReadJsonFile<PlayerPockets>(Path.Join("assets", "data.json")) ?? new PlayerPockets();

            string playerID = Game1.player.UniqueMultiplayerID.ToString();

            // Find existing data for the current player
            PocketData? existingPocketData = playersPockets.pocketDatas.Find(p => p.PlayerUniqueID == playerID);

            if (existingPocketData != null)
            {
                // Update existing player's pocket data
                existingPocketData.QualifiedItemId = pocketItem?.QualifiedItemId ?? "";
                existingPocketData.Quantity = pocketItem?.Stack ?? 0;
                existingPocketData.Quality = pocketItem?.Quality ?? 0;
            }
            else
            {
                // Add new data if no existing data is found for this player
                playersPockets.pocketDatas.Add(new PocketData
                {
                    PlayerUniqueID = playerID,
                    QualifiedItemId = pocketItem?.QualifiedItemId ?? "",
                    Quantity = pocketItem?.Stack ?? 0,
                    Quality = pocketItem?.Quality ?? 0
                });
            }

            // Save the entire player pockets data, which now includes the updated data for the current player
            this.Helper.Data.WriteJsonFile(Path.Join("assets", "data.json"), playersPockets);
        }



        private void OnRenderingHud(object? sender, RenderedHudEventArgs e)
        {
            if (customHud != null)
            {
                int screenWidth = Game1.uiViewport.Width;
                int screenHeight = Game1.uiViewport.Height;
                int hudWidth = customHud.Width;
                int hudHeight = customHud.Height;

                Vector2 hudPosition = new Vector2(10, screenHeight - hudHeight - 10);

                e.SpriteBatch.Draw(
                    customHud,
                    hudPosition,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0f
                );

                if (isItemPocketed && pocketItem != null)
                {
                    ParsedItemData pid = ItemRegistry.GetData(pocketItem.QualifiedItemId);
                    Texture2D pocketItemTexture = pid.GetTexture();
                    Rectangle rec = pid.GetSourceRect();

                    Vector2 drawPosition = new Vector2(hudPosition.X + 7, hudPosition.Y + 8);

                    e.SpriteBatch.Draw(
                        pocketItemTexture,
                        drawPosition,
                        rec,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        3f,
                        SpriteEffects.None,
                        0f
                    );

                    if (pocketItem is StardewValley.Object)
                    {
                        e.SpriteBatch.DrawString(
                            Game1.smallFont,
                            pocketItem.Stack.ToString(),
                            new Vector2(hudPosition.X + hudWidth, hudPosition.Y + hudHeight - 30),
                            Color.White
                        );
                    }
                }
            }

        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(Path.Join("assets", "UtilityPocketHUD.png")))
            {
                e.LoadFromModFile<Texture2D>(Path.Join("assets", "UtilityPocketHUD.png"), AssetLoadPriority.Medium);
            }

            if (customHud == null && Context.IsWorldReady)
            {
                LogMessage("Loading HUD...");
                customHud = Helper.ModContent.Load<Texture2D>(Path.Join("assets", "UtilityPocketHUD.png"));
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            if (e.Button == Config.UsePocketedItemKey)
            {
                // Use item
                LogMessage("Begin using tool process");

                if (pocketItem == null || !isItemPocketed) return;

                if (pocketItem is StardewValley.Tools.MeleeWeapon weapon)
                {

                    LogMessage($"test");
                }
                else if (pocketItem is Tool tool)
                {
                    LogMessage($"Using tool {tool.Name}");


                    var player = Game1.player;

                    tool.DoFunction(player.currentLocation, (int)player.GetToolLocation().X, (int)player.GetToolLocation().Y, 0, player);
                    ToolAnimation(player);
                    player.Stamina -= Config.EnergyCost;

                }
                else if (pocketItem is StardewValley.Object obj && obj.Edibility > -300)
                {
                    if (pocketItem.Stack == 1)
                    {
                        LogMessage($"Eating last {pocketItem.Name}");
                        Game1.player.eatObject(obj);
                        pocketItem = null;
                    }
                    else if (pocketItem.Stack > 1)
                    {
                        LogMessage($"Eating {pocketItem.Name} --- {pocketItem.Stack.ToString()} left");
                        Game1.player.eatObject(obj);
                        pocketItem.Stack--;
                    }
                }
            }
            else if (e.Button == Config.PocketActiveItemKey)
            {
                // Store item in pocket
                Item currentItem = Game1.player.CurrentItem;

                if (currentItem != null && !isItemPocketed && currentItem is not StardewValley.Tool)
                {
                    LogMessage($"Storing {currentItem.Name} in pocket");
                    pocketItem = currentItem;
                    Game1.player.removeItemFromInventory(currentItem);
                    isItemPocketed = true;
                }
                else if (!Game1.player.isInventoryFull() && pocketItem != null)
                {
                    LogMessage($"Removing {pocketItem.Name} from pocket");
                    Game1.player.addItemToInventory(pocketItem);
                    pocketItem = null;
                    isItemPocketed = false;
                }
                else if (Game1.player.isInventoryFull())
                {
                    Game1.showRedMessage("Inventory is full", true);
                }
            }
        }

        private void ToolAnimation(Farmer player)
        {
            player.playNearbySoundLocal("swordswipe");
            int animationNumber = player.FacingDirection switch
            {
                0 => 276,
                1 => 274,
                2 => 277,
                3 => 278,
                _ => 0
            };
            player.animateOnce(animationNumber);

            //player.FarmerSprite.animateOnce(animationNumber, 6000, 20);
            //Game1.freezeControls
        }

        private void LogMessage(string message)
        {
            this.Monitor.Log(message, LogLevel.Trace);
        }
    }
}
