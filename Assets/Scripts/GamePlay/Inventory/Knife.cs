using Data;
using Networking.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public sealed class Knife : MeleeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService,
			IStaticDataService staticData, CharacterProvider characterProvider, NetworkAudioPlayer audioPlayer)
		{
			InputService = inputService;
			CameraService = cameraService;
			AudioPlayer = audioPlayer;
		}

		public override ItemType Type => ItemType.Knife;
	}
}