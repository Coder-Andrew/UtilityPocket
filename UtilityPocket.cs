using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;
using UtilityPocket;

namespace FoodPocket
{
    public class ModConfig
    {
        public SButton UsePocketedItemKey { get; set; } = SButton.Q;
        public SButton PocketActiveItemKey { get; set; } = SButton.R;
        public SButton? UsePocketedItemModiferKey { get; set; } = null;
        public SButton? PocketActiveItemModiferKey { get; set; } = null;
        public int EnergyCost { get; set; } = 6;
    }

    internal sealed class ModEntry : Mod
    {
        private Texture2D? customHud;
        private ModConfig? Config;
        private bool usePocketedItemModifierKeyHeld = false;
        private PocketManager pocketManager;
        private SaveHandler saveHandler;
        private IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            pocketManager = new PocketManager(this.Monitor);
            saveHandler = new SaveHandler(helper, this.Monitor);
            this.helper = helper;

            SetupEvents();
        }

        private void SetupEvents()
        {
            helper.Events.GameLoop.Saved += this.OnSaving;
            helper.Events.GameLoop.SaveLoaded += this.OnLoading;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Display.RenderedHud += this.OnRenderingHud;
        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (Config!.UsePocketedItemModiferKey is not null && e.Button == Config.UsePocketedItemModiferKey)
            {
                usePocketedItemModifierKeyHeld = false;
            }
        }

        private void OnLoading(object? sender, SaveLoadedEventArgs e)
        {
            PocketData userPocketData = saveHandler!.LoadPocketData(Game1.player.uniqueMultiplayerID.ToString());
            
            if (string.IsNullOrEmpty(userPocketData.QualifiedItemId)) return;

            Item loadedItem = ItemRegistry.Create(
                itemId: userPocketData.QualifiedItemId,
                quality: userPocketData.Quality,
                amount: userPocketData.Quantity
            );

            pocketManager!.StoreItemInPocket(loadedItem);
        }


        private void OnSaving(object? sender, SavedEventArgs e)
        {
            Item? item = pocketManager!.GetPocketedItem();
            PocketData pocketData = new PocketData
            {
                PlayerUniqueID = Game1.player.uniqueMultiplayerID.ToString(),
                QualifiedItemId = item?.ItemId ?? "",
                Quality = item?.Quality ?? 0,
                Quantity = item?.Stack ?? 0
            };

            saveHandler!.SavePocketData(pocketData);
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

                if (pocketManager!.IsItemPocketed())
                {
                    ParsedItemData pid = ItemRegistry.GetData(pocketManager.GetPocketedItem()!.ItemId);
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

                    e.SpriteBatch.DrawString(
                        Game1.smallFont,
                        pocketManager!.GetPocketedItem()!.Stack.ToString(),
                        new Vector2(hudPosition.X + hudWidth, hudPosition.Y + hudHeight - 30),
                        Color.White
                    );                    
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

            Item? activeItem = Game1.player.ActiveItem;

            if (e.Button == Config!.UsePocketedItemKey && pocketManager!.IsItemPocketed())
            {
                
            }
            if (e.Button == Config.PocketActiveItemKey)
            {
                if (pocketManager!.IsItemPocketed())
                {
                    pocketManager.RemoveItemFromPocket(Game1.player);
                }
                else if (activeItem != null && !(activeItem is Tool))
                {
                    pocketManager.StoreItemInPocket(activeItem);
                    Game1.player.removeItemFromInventory(activeItem);
                }
            }
        }


        private void LogMessage(string message)
        {
            this.Monitor.Log(message, LogLevel.Debug);
        }
    }
}
