using Data;
using GamePlay.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public class Rifle : RangeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService, AudioPlayer audioPlayer, IStaticDataService staticData)
		{
			InputService = inputService;
			CameraService = cameraService;
			AudioPlayer = audioPlayer;
		}

		public override ItemType Type => ItemType.Rifle;
	}
}