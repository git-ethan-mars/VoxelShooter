using System.Collections.Generic;

namespace Data.Mappers
{
	public static class KillReasonsMapper
	{
		private static readonly Dictionary<ItemType, KillReason> ReasonByItemType = new Dictionary<ItemType, KillReason>
		{
			[ItemType.AssaultRifle] = KillReason.AssaultRifle,
			[ItemType.Rifle] = KillReason.Rifle,
			[ItemType.Shotgun] = KillReason.Shotgun,
			[ItemType.MachineGun] = KillReason.MachineGun,
			[ItemType.Revolver] = KillReason.Revolver,
			[ItemType.Spade] = KillReason.Spade,
			[ItemType.Knife] = KillReason.Knife,
		};

		private static readonly Dictionary<ExplosiveType, KillReason> ReasonByExplosiveType = new Dictionary<ExplosiveType, KillReason>
		{
			[ExplosiveType.Drill] = KillReason.Drill,
			[ExplosiveType.Grenade] = KillReason.Grenade,
			[ExplosiveType.Rocket] = KillReason.Rocket,
			[ExplosiveType.Tnt] = KillReason.TNT,
			[ExplosiveType.Tombstone] = KillReason.Tombstone,
		};

		public static KillReason KillReasonByItemType(ItemType itemType)
		{
			if (!ReasonByItemType.TryGetValue(itemType, out KillReason killReason))
			{
				throw new KeyNotFoundException($"Mapper doesn't have mapping for {itemType}");
			}

			return killReason;
		}

		public static KillReason KillReasonByExplosiveType(ExplosiveType explosiveType)
		{
			if (!ReasonByExplosiveType.TryGetValue(explosiveType, out KillReason killReason))
			{
				throw new KeyNotFoundException($"Mapped doesn't have mapping for {explosiveType}");
			}

			return killReason;
		}
	}
}
