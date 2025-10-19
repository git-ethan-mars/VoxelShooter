using Data;
using GamePlay.Audio;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public sealed class Spade : MeleeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService, 
			IStaticDataService staticData, CharacterProvider characterProvider, AudioPlayer audioPlayer)
		{
			InputService = inputService;
			CameraService = cameraService;
			AudioPlayer = audioPlayer;
		}

		public override ItemType Type => ItemType.Spade;
	}
}