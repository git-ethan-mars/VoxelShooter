using Data;
using Networking.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public class Shotgun : RangeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService, NetworkAudioPlayer audioPlayer, IStaticDataService staticData)
		{
			InputService = inputService;
			CameraService = cameraService;
			AudioPlayer = audioPlayer;
		}

		public override ItemType Type => ItemType.Shotgun;
	}
}