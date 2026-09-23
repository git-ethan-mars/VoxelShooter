using Data;
using Networking;
using Reflex.Attributes;
using Services;
namespace GamePlay
{
	public class Rifle : RangeWeapon
	{
		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, NetworkAudioSender audioSender, 
			IStaticDataService staticData)
		{
			InputService = inputService;
			CameraProvider = cameraProvider;
			AudioSender = audioSender;
		}

		public override ItemType Type => ItemType.Rifle;
	}
}