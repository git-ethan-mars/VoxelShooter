using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[Serializable]
	public class Characteristics
	{
		[field: SerializeField] public int MaxHealth { get; private set; }
		[field: SerializeField] public float Speed { get; private set; }
		[field: SerializeField] public float JumpHeight { get; private set; }
		[field: SerializeField] public float PlaceDistance { get; private set; }
		[field: SerializeField] public int VoxelsCount { get; private set; }
		[field: SerializeField] public List<BlueprintLayout> BlueprintLayouts { get; private set; }
	}
}
