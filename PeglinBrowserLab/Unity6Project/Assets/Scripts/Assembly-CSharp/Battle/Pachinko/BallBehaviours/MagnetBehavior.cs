using System;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	[RequireComponent(typeof(PachinkoBall))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class MagnetBehavior : MonoBehaviour, IUpdateableBySimulation
	{
		[SerializeField]
		protected string[] _pegLayers = new string[2] { "PegCollider", "Bombs" };

		public const float MAGNET_MIN_FORCE_THRESHOLD = 0.05f;

		public const float MAGNET_DISTANCE = 2.5f;

		public const float MAGNET_REGULAR_FORCE = 1.1f;

		public const float MAGNET_LONG_FORCE = 0.9f;

		public const float MAGNET_SPECIAL_PEG_FORCE = 6f;

		public const float MAGNET_FULL_STR_TIME = 4f;

		public const float MAGNET_WEAKENING_TIME = 8f;

		public float defaultFullStrengthDuration;

		public float defaultFadingDuration;

		private Collider2D[] _nearbyPegs = new Collider2D[30];

		private PachinkoBall _pachinko;

		private Rigidbody2D _rigid;

		private List<Peg.PegType> _magnetIngorePegTypes;

		private float _fullStrTime;

		private float _weakeningTime;

		private bool _isDummy;

		private LayerMask _circleCastLayer;

		public void Initialize(float addedFullStrengthTime, float addedWeakeningTime)
		{
			_fullStrTime = defaultFullStrengthDuration + addedFullStrengthTime;
			_weakeningTime = defaultFadingDuration + addedWeakeningTime;
		}

		private void OnEnable()
		{
			_pachinko = GetComponent<PachinkoBall>();
			_rigid = GetComponent<Rigidbody2D>();
			_circleCastLayer = LayerMask.GetMask(_pegLayers);
			DontPopSpecialPegsOnHit component = GetComponent<DontPopSpecialPegsOnHit>();
			_magnetIngorePegTypes = new List<Peg.PegType>();
			if (!(component != null))
			{
				return;
			}
			foreach (Peg.PegType value in Enum.GetValues(typeof(Peg.PegType)))
			{
				if (value != Peg.PegType.NONE && component.dontPopTheseTypes.HasFlag(value))
				{
					_magnetIngorePegTypes.Add(value);
				}
			}
		}

		private void FixedUpdate()
		{
			if (_pachinko.CurrentState == PachinkoBall.FireballState.FIRING && !_pachinko.IsDummy && !_isDummy)
			{
				DoMagnetAttraction(Physics2D.defaultPhysicsScene);
			}
		}

		private void DoMagnetAttraction(PhysicsScene2D physicsScene2D)
		{
			if (!_pachinko.IsDummy && _pachinko.fixedShotTime >= _fullStrTime + _weakeningTime)
			{
				return;
			}
			float num = ((_weakeningTime > 0f) ? ((_pachinko.fixedShotTime - _fullStrTime) / _weakeningTime) : 0f);
			float forceMod = ((_pachinko.fixedShotTime <= _fullStrTime) ? 0f : num);
			physicsScene2D.OverlapCircle(base.transform.position, 2.5f, _nearbyPegs, _circleCastLayer);
			for (int i = 0; i < _nearbyPegs.Length; i++)
			{
				Collider2D collider2D = _nearbyPegs[i];
				if (!(collider2D == null))
				{
					Peg component = collider2D.GetComponent<Peg>();
					LongPeg component2 = collider2D.GetComponent<LongPeg>();
					if (component != null && !component.IsDisabled() && !Peg.PegTypeIndestructible(component.pegType) && !_magnetIngorePegTypes.Contains(component.pegType) && (component2 == null || !component2.hit))
					{
						AddMagnetForce(component.GetCenterOfPeg(), component.GetMagnetForce(), forceMod);
					}
					_nearbyPegs[i] = null;
				}
			}
		}

		private void AddMagnetForce(Vector3 centerOfPeg, float force, float forceMod)
		{
			Vector2 vector = centerOfPeg - base.transform.position;
			float num = vector.magnitude / 2.5f;
			if (num > 0.05f)
			{
				float num2 = Mathf.Lerp(force, 0f, forceMod);
				_rigid.AddForce(vector.normalized * (num2 / (num * num)));
			}
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("MAGNET_ORB_FULL", defaultFullStrengthDuration.ToString(CultureInfo.InvariantCulture));
				component.SetParameterValue("MAGNET_ORB_WEAK", defaultFadingDuration.ToString(CultureInfo.InvariantCulture));
			}
		}

		public void DoUpdate(PhysicsScene2D physicsScene2D)
		{
			DoMagnetAttraction(physicsScene2D);
		}

		public void EnableIfDisabledDuringShot()
		{
		}

		public void Disable()
		{
		}

		public void SetDummy(bool value)
		{
			_isDummy = value;
		}
	}
}
