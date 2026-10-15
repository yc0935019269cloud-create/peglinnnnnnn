using System.Collections.Generic;
using Battle.Attacks;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Pachinko.Obstacles
{
	public class PegBoardBramballVine : MonoBehaviour, IUpdateableBySimulation
	{
		public delegate void BramballVineDestroyed(PegBoardBramballVine vine);

		public delegate void VineMutliballCreated();

		private struct OrbSizeStats
		{
			public Vector3 localScale;

			public Vector3 spriteScale;

			public float PPU;

			public float colliderRadius;

			public Sprite sprite;

			public OrbSizeStats(Vector3 localScale, Vector3 spriteScale, float PPU, float colliderRadius, Sprite sprite)
			{
				this.localScale = localScale;
				this.spriteScale = spriteScale;
				this.PPU = PPU;
				this.colliderRadius = colliderRadius;
				this.sprite = sprite;
			}
		}

		public static BramballVineDestroyed OnBramballVineDestroyed = (PegBoardBramballVine vine) =>
		{
		};

		public static VineMutliballCreated OnVineMutliballCreated = () =>
		{
		};

		private const string GOLD_PEG_LOC_NAME = "gold_peg";

		private const float COLLIDER_WIDTH = 0.12f;

		private const float COLLIDER_LENGTH_OFFSET = 0.5f;

		[HideInInspector]
		public PlayerStatusEffectController playerStatusEffectController;

		[HideInInspector]
		public BattleController battleController;

		public List<GameObject> vineMultiballObjects;

		public GameObject bramball;

		public GameObject orbCaughtInVinePrefab;

		public List<Material> vineMaterials = new List<Material>();

		public float randomVineMultiballSpawnXVel = 50f;

		private BoxCollider2D _boxCollider2D;

		public BoxCollider2D _triggerBoxCollider2D;

		private LineRenderer _lineRenderer;

		private string _bramballLocNameString;

		private static List<OrbSizeStats> _vineMultiballOrbSizeStats = null;

		private bool _isDummy;

		private Peg _peg1;

		private Peg _peg2;

		public List<OrbCaughtInVine> allOrbsCaughtInVine = new List<OrbCaughtInVine>();

		private PredictionManager _predictionManager;

		private bool _disabledDuringShot;

		private void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_bramballLocNameString = bramball.GetComponent<Attack>().locNameString;
			if (_vineMultiballOrbSizeStats != null)
			{
				return;
			}
			_vineMultiballOrbSizeStats = new List<OrbSizeStats>();
			foreach (GameObject vineMultiballObject in vineMultiballObjects)
			{
				SpriteRenderer componentInChildren = vineMultiballObject.GetComponentInChildren<SpriteRenderer>();
				_vineMultiballOrbSizeStats.Add(new OrbSizeStats(vineMultiballObject.transform.localScale, componentInChildren.transform.localScale, componentInChildren.sprite.pixelsPerUnit, vineMultiballObject.GetComponent<CircleCollider2D>().radius, componentInChildren.sprite));
			}
		}

		public void Initialize(Peg peg1, Peg peg2, PredictionManager predictionManager)
		{
			_peg1 = peg1;
			_peg2 = peg2;
			Vector3 vector = _peg1.transform.position;
			if (_peg1 is LongPeg)
			{
				vector = _peg1.GetCenterOfPeg();
			}
			Vector3 vector2 = _peg2.transform.position;
			if (_peg2 is LongPeg)
			{
				vector2 = _peg2.GetCenterOfPeg();
			}
			_lineRenderer.SetPosition(0, vector);
			_lineRenderer.SetPosition(1, vector2);
			base.transform.position = (vector + vector2) / 2f;
			Vector3 to = vector2 - vector;
			base.transform.eulerAngles = new Vector3(0f, 0f, Vector3.SignedAngle(new Vector3(0f, 1f, 0f), to, new Vector3(0f, 0f, 1f)));
			float magnitude = (vector - vector2).magnitude;
			_lineRenderer.material = vineMaterials[Random.Range(0, vineMaterials.Count - 1)];
			_lineRenderer.material.mainTextureScale *= Mathf.Sign(base.transform.eulerAngles.z) * Mathf.Sign(vector.x - vector2.x);
			_boxCollider2D.size = new Vector2(0.12f, magnitude - 0.5f);
			_triggerBoxCollider2D.size = _boxCollider2D.size;
			_predictionManager = predictionManager;
			_predictionManager.AddObjectToSimulationScene(base.gameObject);
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			PachinkoBall component = collision.gameObject.GetComponent<PachinkoBall>();
			if (component != null && component.TryGetComponent<BreakVines>(out var _))
			{
				if (!component.IsDummy)
				{
					DestroyVine();
				}
			}
			else
			{
				HandlePachinkoCollision(collision);
			}
		}

		public void HandlePachinkoCollision(Collider2D collision)
		{
			PachinkoBall component = collision.gameObject.GetComponent<PachinkoBall>();
			if (!(component != null))
			{
				return;
			}
			if (!component.IsDummy && component.IsFiring())
			{
				if (_peg1.pegType != Peg.PegType.VINE || _peg2.pegType != Peg.PegType.VINE)
				{
					Debug.LogError("Failsave kicked in to destroy vine whos pegs are no longer vine pegs for whatever reason.");
					_peg1.bramballVine = null;
					_peg2.bramballVine = null;
					_peg1.Reset(BattleController.criticalActive);
					_peg2.Reset(BattleController.criticalActive);
					_predictionManager.DestroyObjectInSimulation(base.gameObject);
					Object.Destroy(base.gameObject);
				}
				else
				{
					OrbCaughtInVine item = CreateNewOrbCaughtInVineObject(component);
					allOrbsCaughtInVine.Add(item);
					if (component.GetComponent<NoDamageOnCollideWithBramballVine>() == null)
					{
						PlayerHealthController.OnExternalDamageRequested?.Invoke(1f, PlayerHealthController.DamageSource.PEG);
					}
					component.StartDestroy();
					OnVineMutliballCreated?.Invoke();
				}
			}
			else if (_isDummy && component.IsDummy)
			{
				component.OnPachinkoBallDummyDestroyed(component.transform.position);
				component.HasDummyCollided = true;
			}
		}

		private OrbCaughtInVine CreateNewOrbCaughtInVineObject(PachinkoBall pBall)
		{
			GameObject obj = Object.Instantiate(orbCaughtInVinePrefab);
			OrbCaughtInVine component = obj.GetComponent<OrbCaughtInVine>();
			obj.name = "orbCaughtInVine" + allOrbsCaughtInVine.Count;
			obj.transform.position = pBall.transform.position;
			obj.transform.parent = base.transform;
			Attack component2 = pBall.gameObject.GetComponent<Attack>();
			SpriteRenderer componentInChildren = pBall.GetComponentInChildren<SpriteRenderer>();
			Vector3 localScale = pBall.transform.localScale;
			Vector3 vector = componentInChildren.transform.localScale;
			float num = componentInChildren.sprite.pixelsPerUnit;
			float num2 = _vineMultiballOrbSizeStats[1].PPU;
			if (pBall.TryGetComponent<CircleCollider2D>(out var component3))
			{
				num2 = component3.radius;
			}
			OrbCaughtInVine.OrbSize orbSize = GetOrbSize(localScale, vector, num, num2);
			Sprite sprite = _vineMultiballOrbSizeStats[1].sprite;
			if (orbSize != OrbCaughtInVine.OrbSize.CUSTOM)
			{
				sprite = _vineMultiballOrbSizeStats[(int)orbSize].sprite;
			}
			if (component2 != null && component2.locNameString == _bramballLocNameString)
			{
				component.isBramball = true;
				sprite = componentInChildren.sprite;
			}
			if (component2 == null || component2.locNameString == "gold_peg")
			{
				localScale = _vineMultiballOrbSizeStats[1].localScale;
				vector = _vineMultiballOrbSizeStats[1].spriteScale;
				num = _vineMultiballOrbSizeStats[1].PPU;
				num2 = _vineMultiballOrbSizeStats[1].colliderRadius;
				sprite = _vineMultiballOrbSizeStats[1].sprite;
				orbSize = OrbCaughtInVine.OrbSize.REGULAR;
			}
			component.orbScale = localScale;
			component.spriteRendererScale = vector;
			component.ppu = num;
			component.colliderRadius = num2;
			component.spriteRenderer.sprite = sprite;
			component.orbSize = orbSize;
			component.gameObject.transform.localScale = new Vector3(localScale.x * vector.x, localScale.y * vector.y, 1f);
			return component;
		}

		private OrbCaughtInVine.OrbSize GetOrbSize(Vector3 localScale, Vector3 spriteScale, float PPU, float orbColliderRadius)
		{
			Vector2 vector = new Vector2(localScale.x, localScale.y);
			Vector2 vector2 = new Vector2(spriteScale.x, spriteScale.y);
			int num = -1;
			foreach (OrbSizeStats vineMultiballOrbSizeStat in _vineMultiballOrbSizeStats)
			{
				num++;
				Vector2 vector3 = new Vector2(vineMultiballOrbSizeStat.localScale.x, vineMultiballOrbSizeStat.localScale.y);
				Vector2 vector4 = new Vector2(vineMultiballOrbSizeStat.spriteScale.x, vineMultiballOrbSizeStat.spriteScale.y);
				if (!((vector - vector3).magnitude > 0.1f) && !((vector2 - vector4).magnitude > 0.1f) && !(PPU - vineMultiballOrbSizeStat.PPU > 0.1f) && !(orbColliderRadius - vineMultiballOrbSizeStat.colliderRadius > 0.1f))
				{
					return (OrbCaughtInVine.OrbSize)num;
				}
			}
			return OrbCaughtInVine.OrbSize.CUSTOM;
		}

		public void DestroyVine(bool doDeathEffects = true)
		{
			_peg1.bramballVine = null;
			_peg2.bramballVine = null;
			_peg1.Reset(BattleController.criticalActive);
			_peg2.Reset(BattleController.criticalActive);
			if (doDeathEffects)
			{
				GameObject activePachinkoBall = battleController.activePachinkoBall;
				if (activePachinkoBall == null)
				{
					playerStatusEffectController.RemoveStatusEffectIntensity(StatusEffectType.Thorned, 1);
					OnBramballVineDestroyed?.Invoke(this);
					Object.Destroy(base.gameObject);
					return;
				}
				PachinkoBall component = activePachinkoBall.GetComponent<PachinkoBall>();
				if (component == null)
				{
					playerStatusEffectController.RemoveStatusEffectIntensity(StatusEffectType.Thorned, 1);
					OnBramballVineDestroyed?.Invoke(this);
					Object.Destroy(base.gameObject);
					return;
				}
				foreach (OrbCaughtInVine item in allOrbsCaughtInVine)
				{
					Vector2 fireForce = new Vector2(Random.value * randomVineMultiballSpawnXVel * 2f - randomVineMultiballSpawnXVel, 0f);
					if (!item.isBramball && item.orbSize == OrbCaughtInVine.OrbSize.CUSTOM)
					{
						GameObject gameObject = component.SpawnMultiballFromLocation(vineMultiballObjects[1], item.gameObject.transform.position, fireForce);
						if (!(gameObject == null))
						{
							SpriteRenderer componentInChildren = gameObject.GetComponentInChildren<SpriteRenderer>();
							gameObject.transform.localScale = item.orbScale;
							componentInChildren.transform.localScale = item.spriteRendererScale * _vineMultiballOrbSizeStats[1].PPU / item.ppu;
							gameObject.GetComponent<CircleCollider2D>().radius = item.colliderRadius;
						}
					}
					else
					{
						GameObject multiballGameObject = vineMultiballObjects[(int)item.orbSize];
						if (item.isBramball)
						{
							multiballGameObject = bramball;
						}
						component.SpawnMultiballFromLocation(multiballGameObject, item.gameObject.transform.position, fireForce);
					}
				}
				playerStatusEffectController.RemoveStatusEffectIntensity(StatusEffectType.Thorned, 1);
			}
			OnBramballVineDestroyed?.Invoke(this);
			_predictionManager.DestroyObjectInSimulation(base.gameObject);
			Object.Destroy(base.gameObject);
		}

		public void DoUpdate(PhysicsScene2D physicsScene2D)
		{
		}

		public void Disable()
		{
			base.gameObject.SetActive(value: false);
			_disabledDuringShot = true;
		}

		public void EnableIfDisabledDuringShot()
		{
			if (_disabledDuringShot)
			{
				base.gameObject.SetActive(value: true);
				_disabledDuringShot = false;
			}
		}

		public void SetDummy(bool value)
		{
			_isDummy = value;
		}
	}
}
