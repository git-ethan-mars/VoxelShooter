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

		// Other players' hands turn around a point in the chest, so looking up or down moves the item in front of the
		// body instead of swinging it around the head into the torso. The hand is held clear of the chest, which sticks
		// out 0.25 in front of the spine, and steep looks only tilt the arms down to the belly or up to the chin.
		private static readonly Vector3 ChestPivot = new Vector3(0.12f, 0.3f, 0.05f);
		private const float HoldDistance = 0.38f;
		private const float HandPitchUpLimit = 30.0f;
		private const float HandPitchDownLimit = 60.0f;

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
		private float _leftArmReach;

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

			// Slightly short of the full length, so the elbow keeps a bend instead of snapping straight
			TwoBoneIKConstraintData leftArm = leftHandIK.data;
			_leftArmReach = 0.97f * (Vector3.Distance(leftArm.root.position, leftArm.mid.position) +
			                         Vector3.Distance(leftArm.mid.position, leftArm.tip.position));
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
				Vector3 horizontalDirection = Vector3.ProjectOnPlane(lookDirection, Vector3.up);

				if (horizontalDirection.sqrMagnitude > 0.0001f)
				{
					Quaternion yawRotation = Quaternion.LookRotation(horizontalDirection);
					float pitch = Vector3.SignedAngle(horizontalDirection, lookDirection, yawRotation * Vector3.right);
					Quaternion aimRotation = yawRotation * Quaternion.Euler(Mathf.Clamp(pitch, -HandPitchUpLimit, HandPitchDownLimit), 0.0f, 0.0f);
					Vector3 pivot = _head.parent.TransformPoint(ChestPivot);
					anchor.SetPositionAndRotation(pivot + aimRotation * Vector3.forward * HoldDistance, aimRotation);
					PlaceHandTargets(yawRotation);
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
		// The elbows are hinted in the horizontal aim frame, so they stay out and down whatever the pitch
		private void PlaceHandTargets(Quaternion yawRotation)
		{
			rightHandIK.data.target.SetPositionAndRotation(anchor.TransformPoint(RightHandOffset), anchor.rotation * GripToHand);
			rightHandIK.data.hint.position = anchor.position + yawRotation * RightElbowHintOffset;

			if (_isTwoHanded)
			{
				Vector3 supportPoint = anchor.TransformPoint(_leftHandOffset);
				leftHandIK.data.target.position = ClampToLeftArmReach(anchor.position, supportPoint);
				leftHandIK.data.hint.position = anchor.position + yawRotation * LeftElbowHintOffset;
			}
			else
			{
				Transform spine = _head.parent;
				leftHandIK.data.target.position = spine.TransformPoint(LeftHandRestPosition);
				leftHandIK.data.hint.position = spine.TransformPoint(LeftElbowRestHint);
			}
		}

		// The arms are short, so a support point far along the barrel is out of reach; the left hand then slides back
		// along the item towards the grip to the farthest point it can still hold
		private Vector3 ClampToLeftArmReach(Vector3 gripPoint, Vector3 supportPoint)
		{
			Vector3 shoulder = leftHandIK.data.root.position;

			if ((supportPoint - shoulder).sqrMagnitude <= _leftArmReach * _leftArmReach)
			{
				return supportPoint;
			}

			Vector3 along = supportPoint - gripPoint;
			Vector3 fromShoulder = gripPoint - shoulder;
			float a = along.sqrMagnitude;
			float b = 2.0f * Vector3.Dot(fromShoulder, along);
			float c = fromShoulder.sqrMagnitude - _leftArmReach * _leftArmReach;
			float discriminant = b * b - 4.0f * a * c;

			if (a < 0.0001f || discriminant < 0.0f)
			{
				return gripPoint;
			}

			float t = Mathf.Clamp01((-b + Mathf.Sqrt(discriminant)) / (2.0f * a));
			return gripPoint + along * t;
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
