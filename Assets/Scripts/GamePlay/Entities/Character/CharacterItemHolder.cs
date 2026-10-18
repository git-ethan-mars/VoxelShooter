using Mirror;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

namespace GamePlay
{
	// Keeps inventory items in the character's hands: in front of the camera for the local player (whose body is hidden)
	// and in the right hand for everyone else, with the arms brought up to the chest through the hand IK.
	public class CharacterItemHolder : MonoBehaviour
	{
		private const string HeadPath = "Skeleton/spine/head";

		private static readonly Vector3 FirstPersonOffset = new Vector3(0.25f, -0.28f, 0.45f);
		private static readonly Vector3 ThirdPersonOffset = new Vector3(0.04f, -0.3f, 0.18f);
		private static readonly Vector3 RightElbowHintOffset = new Vector3(0.6f, -0.5f, -0.4f);
		private static readonly Vector3 LeftElbowHintOffset = new Vector3(-0.6f, -0.5f, -0.2f);

		// The animations leave the arms in the T-pose, so a free left hand is lowered along the body by the IK
		private static readonly Vector3 LeftHandRestPosition = new Vector3(-0.33f, -0.16f, 0.02f);
		private static readonly Vector3 LeftElbowRestHint = new Vector3(-0.4f, 0.2f, -0.3f);

		// The hand bone points along the fingers; the grip looks the same way with the palm turned inwards
		private static readonly Vector3 PalmOffset = new Vector3(0.0f, 0.08f, 0.0f);
		private static readonly Quaternion GripToHand = Quaternion.LookRotation(Vector3.right, Vector3.forward);

		// The right hand is placed so that its grip matches the anchor, which makes its pose the inverse of the grip
		private static readonly Vector3 RightHandOffset = -(GripToHand * PalmOffset);

		[SerializeField] private NetworkIdentity networkIdentity;
		[SerializeField] private Transform headTrackingObject;
		[SerializeField] private Transform anchor;
		[SerializeField] private Rig handRig;
		[SerializeField] private TwoBoneIKConstraint rightHandIK;
		[SerializeField] private TwoBoneIKConstraint leftHandIK;

		private CameraProvider _cameraProvider;
		private Transform _head;
		private Transform _grip;
		private Vector3 _leftHandOffset;
		private bool _isTwoHanded;

		[Inject]
		private void Construct(CameraProvider cameraProvider)
		{
			_cameraProvider = cameraProvider;
		}

		private void Awake()
		{
			_head = transform.Find(HeadPath);
			_grip = new GameObject("Grip").transform;
			_grip.SetParent(rightHandIK.data.tip, false);
			_grip.SetLocalPositionAndRotation(PalmOffset, Quaternion.Inverse(GripToHand));
		}

		private void Start()
		{
			if (networkIdentity.isLocalPlayer)
			{
				handRig.weight = 0.0f;
				DisableShadows(anchor);
			}
		}

		private void Update()
		{
			// Remote characters: the hand IK reads its targets during the animation pass, so they are placed before it
			if (!networkIdentity.isLocalPlayer)
			{
				Vector3 lookDirection = headTrackingObject.position - _head.position;

				if (lookDirection.sqrMagnitude > 0.0001f)
				{
					Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
					anchor.SetPositionAndRotation(_head.position + lookRotation * ThirdPersonOffset, lookRotation);
					PlaceHandTargets();
				}
			}
		}

		// The camera follows the head bone in LateUpdate, so the item is placed from the same final head pose
		private void LateUpdate()
		{
			if (networkIdentity.isLocalPlayer && _cameraProvider.MainCamera != null)
			{
				Quaternion cameraRotation = _cameraProvider.MainCamera.transform.rotation;
				anchor.SetPositionAndRotation(_head.position + cameraRotation * FirstPersonOffset, cameraRotation);
			}
		}

		public void Attach(InventoryItem item)
		{
			if (networkIdentity.isLocalPlayer)
			{
				item.transform.SetParent(anchor, false);
				item.transform.SetLocalPositionAndRotation(item.HoldPosition, Quaternion.Euler(item.HoldRotation));
				DisableShadows(item.transform);
			}
			else
			{
				item.transform.SetParent(_grip, false);
				item.transform.SetLocalPositionAndRotation(item.GripPosition, Quaternion.Euler(item.GripRotation));
			}
		}

		// The left hand holds the item at its support point, as it is placed when the right hand reaches the anchor
		public void Hold(InventoryItem item)
		{
			_isTwoHanded = item.SupportHandPosition != Vector3.zero;
			_leftHandOffset = item.GripPosition + Quaternion.Euler(item.GripRotation) * item.SupportHandPosition;
		}

		// The rig syncs only the local pose of its targets, so they sit under a still parent and get world poses here
		private void PlaceHandTargets()
		{
			rightHandIK.data.target.SetPositionAndRotation(anchor.TransformPoint(RightHandOffset), anchor.rotation * GripToHand);
			rightHandIK.data.hint.position = anchor.TransformPoint(RightElbowHintOffset);

			if (_isTwoHanded)
			{
				leftHandIK.data.target.position = anchor.TransformPoint(_leftHandOffset);
				leftHandIK.data.hint.position = anchor.TransformPoint(LeftElbowHintOffset);
			}
			else
			{
				Transform spine = _head.parent;
				leftHandIK.data.target.position = spine.TransformPoint(LeftHandRestPosition);
				leftHandIK.data.hint.position = spine.TransformPoint(LeftElbowRestHint);
			}
		}

		// The local player has no visible body, so a floating item must not cast a shadow either
		private static void DisableShadows(Transform root)
		{
			foreach (Renderer itemRenderer in root.GetComponentsInChildren<Renderer>(true))
			{
				itemRenderer.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}
}
