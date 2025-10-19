using Data;
using GamePlay.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public sealed class AssaultRifle : RangeWeapon
	{
		public override ItemType Type => ItemType.AssaultRifle;

		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService, AudioPlayer audioPlayer, 
			IStaticDataService staticData)
		{
			InputService = inputService;
			CameraService = cameraService;
			AudioPlayer = audioPlayer;
		}
	}
}