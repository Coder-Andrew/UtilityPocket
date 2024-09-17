using FoodPocket;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityPocket
{
    public class PocketData
    {
        public string PlayerUniqueID { get; set; } = "";
        public string QualifiedItemId { get; set; } = "";
        public int Quality { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"{PlayerUniqueID} --- {QualifiedItemId} --- {Quality} --- {Quantity}";
        }
    }
    public class PlayerPockets
    {
        public List<PocketData> pocketDatas { get; set; } = new List<PocketData>();
    }
    public class SaveHandler
    {
        private IModHelper Helper;
        private IMonitor Logger;
        public SaveHandler(IModHelper helper, IMonitor logger)
        {
            Helper = helper;
            Logger = logger;
        }

        public void SavePocketData(PocketData pocketData)
        {
            try
            {
                // Load the entire player pockets data
                PlayerPockets playersPockets = Helper.Data.ReadJsonFile<PlayerPockets>(Path.Join("assets", "data.json"))
                    ?? new PlayerPockets { pocketDatas = new List<PocketData>() };

                // Find the current player's pocket data
                PocketData? playerPocketData = playersPockets.pocketDatas.Find(p => p.PlayerUniqueID == pocketData.PlayerUniqueID);

                if (playerPocketData != null)
                {
                    playerPocketData.PlayerUniqueID = pocketData.PlayerUniqueID;
                    playerPocketData.QualifiedItemId = pocketData.QualifiedItemId;
                    playerPocketData.Quality = pocketData.Quality;
                    playerPocketData.Quantity = pocketData.Quantity;
                }
                else
                {
                    playersPockets.pocketDatas.Add(pocketData);
                }

                Helper.Data.WriteJsonFile(Path.Join("assets", "data.json"), playersPockets);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error saving data: {ex.Message}", LogLevel.Error);
            }
        }

        public PocketData LoadPocketData(string playerUniqueID)
        {
            try
            {
                PlayerPockets playersPockets = Helper.Data.ReadJsonFile<PlayerPockets>(Path.Join("assets", "data.json"))
                    ?? new PlayerPockets { pocketDatas = new List<PocketData>() };

                return playersPockets.pocketDatas.Find(p => p.PlayerUniqueID == playerUniqueID) 
                    ?? new PocketData { PlayerUniqueID = playerUniqueID };
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading data: {ex.Message}", LogLevel.Error);
                return new PocketData();
            }
        }
    }
}
