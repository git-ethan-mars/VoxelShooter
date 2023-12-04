﻿using System.Collections.Generic;
using Data;

namespace Infrastructure.Services.StaticData
{
    public interface IStaticDataService : IService
    {
        void LoadItems();
        InventoryItem GetItem(int id);
        void LoadInventories();
        List<InventoryItem> GetInventory(GameClass gameClass);
        void LoadPlayerCharacteristics();
        PlayerCharacteristic GetPlayerCharacteristic(GameClass gameClass);
        void LoadMapConfigures();
        MapConfigure GetMapConfigure(string mapName);
        void LoadLobbyBalance();
        LobbyBalance GetLobbyBalance();
    }
}