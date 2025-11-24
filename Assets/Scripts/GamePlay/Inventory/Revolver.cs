using Data;
using Networking.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public class Revolver : RangeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, NetworkAudioPlayer audioPlayer, IStaticDataService staticData)
		{
			InputService = inputService;
			CameraProvider = cameraProvider;
			AudioPlayer = audioPlayer;
		}

		public override ItemType Type => ItemType.Revolver;
	}
}