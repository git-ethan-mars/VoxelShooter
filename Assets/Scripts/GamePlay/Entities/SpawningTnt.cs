using System;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.MapFeatures;
using UnityEngine;

namespace GamePlay.Entities
{
	public class SpawningTnt : Entity
	{
		[SerializeField]
		private ExplosionData explosion;
		
		private TntData _data;

		public void Construct(TntData data)
		{
			_data = data;
		}

		public async UniTask ExplodeAsync()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(_data.DelayInSeconds));
		}
	}
}