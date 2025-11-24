using Data;
using Networking.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public sealed class AssaultRifle : RangeWeapon
	{
		public override ItemType Type => ItemType.AssaultRifle;

		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, NetworkAudioPlayer audioPlayer, 
			IStaticDataService staticData)
		{
			InputService = inputService;
			CameraProvider = cameraProvider;
			AudioPlayer = audioPlayer;
		}
	}
}