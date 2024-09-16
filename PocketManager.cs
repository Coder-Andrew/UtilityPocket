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
                if (item != null && !isItemPocketed && !(item is Tool))
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
                if (pocketedItem != null)
                {
                    who.addItemByMenuIfNecessary(pocketedItem);
                    pocketedItem = null;
                    isItemPocketed = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in retrieving item from pocket: {ex.Message}", LogLevel.Error);
            }
        }

        public bool IsItemPocketed()
        {
            return isItemPocketed;
        }

        public Item? GetPocketedItem()
        {
            return pocketedItem;
        }
    }
}
