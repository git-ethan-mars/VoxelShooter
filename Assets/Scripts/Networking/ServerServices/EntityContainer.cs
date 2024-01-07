using System.Collections.Generic;
using Entities;

namespace Networking.ServerServices
{
    public class EntityContainer
    {
        public HashSet<LootBox> LootBoxes => _lootBoxes;
        public List<IExplosive> Explosives => _explosives;
        public IEnumerable<IPushable> PushableObjects => _pushableObjects;

        private readonly HashSet<LootBox> _lootBoxes = new();
        private readonly List<IExplosive> _explosives = new();
        private readonly HashSet<IPushable> _pushableObjects = new();

        public void AddLootBox(LootBox lootBox)
        {
            _lootBoxes.Add(lootBox);
        }

        public void RemoveLootBox(LootBox lootBox)
        {
            _lootBoxes.Remove(lootBox);
        }

        public void AddExplosive(IExplosive explosive)
        {
            _explosives.Add(explosive);
        }

        public void RemoveExplosive(IExplosive explosive)
        {
            _explosives.Remove(explosive);
        }

        public void AddPushable(IPushable pushable)
        {
            _pushableObjects.Add(pushable);
        }

        public void RemovePushable(IPushable pushable)
        {
            _pushableObjects.Remove(pushable);
        }
    }
}