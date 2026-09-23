using Data;
using Networking;
using Reflex.Attributes;
using Services;

namespace GamePlay
{
	public class Revolver : RangeWeapon
	{
		public override ItemType Type => ItemType.Revolver;

		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, NetworkAudioSender audioSender,
			IStaticDataService staticData)
		{
			InputService = inputService;
			CameraProvider = cameraProvider;
			AudioSender = audioSender;
		}
	}
}
