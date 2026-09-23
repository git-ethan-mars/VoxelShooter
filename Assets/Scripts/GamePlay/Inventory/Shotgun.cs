using Data;
using Networking;
using Reflex.Attributes;
using Services;

namespace GamePlay
{
	public class Shotgun : RangeWeapon
	{
		public override ItemType Type => ItemType.Shotgun;

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
