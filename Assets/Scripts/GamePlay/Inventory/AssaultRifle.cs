using Data;
using Networking;
using Reflex.Attributes;
using Services;

namespace GamePlay
{
	public sealed class AssaultRifle : RangeWeapon
	{
		public override ItemType Type => ItemType.AssaultRifle;

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
