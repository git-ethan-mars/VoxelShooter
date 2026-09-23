using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[Serializable]
	public class BlueprintLayout
	{
		[field: SerializeField] public string Name { get; private set; }
		[field: SerializeField] public List<Vector3Int> Positions { get; private set; }
	}
}
