using StardewModdingAPI;
using StardewValley;
using StardewValley.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityPocket
{
    public class PocketManager
    {
        private Item? pocketedItem;
        private bool isItemPocketed = false;
        private IMonitor Logger;

        public PocketManager(IMonitor logger)
        {
            Logger = logger;
        }

        public void StoreItemInPocket(Item item)
        {
            try
            {
                if (item != null && !IsItemPocketed() && !(item is Tool))
                {
                    pocketedItem = item;
                    isItemPocketed = true;
                }

            }
            catch (Exception ex)
            {
                Logger.Log($"Error pocketing item: {ex.Message}", LogLevel.Error);
            }
        }

        public void RemoveItemFromPocket(Farmer who)
        {
            try
            {
                if (who.isInventoryFull())
                {
                    Game1.showRedMessage("Inventory full");
                    who.playNearbySoundLocal("cancel");
                }
                else if (isItemPocketed)
                {
                    who.addItemToInventory(pocketedItem);
                    pocketedItem = null;
                    isItemPocketed = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in retrieving item from pocket: {ex.Message}", LogLevel.Error);
            }
        }

        public void UsePocketedItem(Farmer who)
        {
            try
            {
                if (pocketedItem is StardewValley.Object obj && obj.Edibility > -300)
                {
                    who.eatObject(obj);
                    HandleUsedObject(obj);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error using item in pocket: {ex.Message}", LogLevel.Error);
            }
        }

        public bool IsItemPocketed()
        {
            return isItemPocketed;
            //return pocketedItem is null;
        }

        public Item? GetPocketedItem()
        {
            return pocketedItem;
        }

        private void HandleUsedObject(StardewValley.Object item)
        {
            if (item.Stack > 1)
            {
                item.Stack--;
            }
            else
            {
                pocketedItem = null;
                isItemPocketed = false;
            }
        }
    }
}
