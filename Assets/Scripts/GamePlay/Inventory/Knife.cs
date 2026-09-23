using Data;
using Networking;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public sealed class Knife : MeleeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider,
			IStaticDataService staticData, CharacterProvider characterProvider, NetworkAudioSender audioSender)
		{
			InputService = inputService;
			CameraProvider = cameraProvider;
			AudioSender = audioSender;
		}

		public override ItemType Type => ItemType.Knife;
	}
}