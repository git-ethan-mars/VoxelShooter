﻿using System.Collections.Generic;
using Data;

namespace Infrastructure.Services
{
    public interface IStaticDataService : IService
    {
        void LoadItems();
        InventoryItem GetItem(int id);
        void LoadInventories();
        List<InventoryItem> GetInventory(GameClass gameClass);
    }
}