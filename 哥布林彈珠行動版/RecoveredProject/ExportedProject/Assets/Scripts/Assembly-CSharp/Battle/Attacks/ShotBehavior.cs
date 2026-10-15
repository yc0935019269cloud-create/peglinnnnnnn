using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using UnityEngine;

namespace Battle.Attacks
{
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class ShotBehavior : MonoBehaviour
	{
		public delegate void ShotHit(Enemy enemy, ShotBehavior behavior, bool attackFinished);

		public delegate void ShotDisabled(ShotBehavior shot);

		public enum ShotType
		{
			NORMAL = 0,
			PINPOINT = 1,
			PIERCE = 2,
			OVERFLOW = 3
		}

		public const float X_DISTANCE_AUTO_DESTROY = 100f;

		public const float X_DISTANCE_AUTO_DESTROY_PINCER = -15f;

		private const float DEFAULT_CIRCLE_SCALE = 0.4f;

		private const float DEFAULT_CIRCLE_RADIUS = 0.2f;

		public ShotHit OnShotHit;

		public ShotDisabled OnShotDisabled;

		private ProjectileAttack _attack;

		[SerializeField]
		private GameObject _explosionPrefab;

		[SerializeField]
		private GameObject _startupSpawn;

		[SerializeField]
		private bool _startupSpawnFollowRotation;

		[Header("Shot Properties")]
		[SerializeField]
		private float _minForce = 100f;

		[SerializeField]
		private float _maxForce = 900f;

		[SerializeField]
		private float _forcePerPeg = 50f;

		[SerializeField]
		protected Vector3 _minSize = new Vector3(0.2f, 0.2f, 1f);

		[SerializeField]
		protected Vector3 _maxSize = new Vector3(2f, 2f, 1f);

		[SerializeField]
		protected ShotType _shotType;

		private ShotType _previousShotType;

		[SerializeField]
		protected int _enemiesToPierce;

		[Tooltip("Can this attack target air?")]
		[SerializeField]
		protected bool _canAimUp = true;

		[Header("Animation Data")]
		[SerializeField]
		private bool _shouldRotate;

		public float startupDelay;

		public bool invisibleDuringStartupDelay;

		[SerializeField]
		private string _startupAnimationState = "ice_charge";

		[SerializeField]
		private string _endAnimationState = "ice_hit";

		[SerializeField]
		private float _delayColliderEnable;

		[SerializeField]
		private float _delayToKillMovement;

		[SerializeField]
		private float _delayAnimationEnd;

		[SerializeField]
		private Vector3 _startupOffset = Vector3.zero;

		[Header("Audio")]
		public AttackSfx AttackSfx;

		[SerializeField]
		private float _sfxChangeForceThreshold = 500f;

		[SerializeField]
		private float _audioStartupDelay;

		protected Animator _anim;

		protected Collider2D _collider;

		private SpriteRenderer _renderer;

		private Rigidbody2D _rigid;

		private GameObject _explosion;

		private RelicManager _relicManager;

		private AudioSource _sfxAudioSource;

		[HideInInspector]
		public Vector2 initialPos;

		private Enemy _target;

		private int _pierceCount;

		private bool _isReverseAttack;

		private List<Enemy> _enemiesToHit;

		private List<Enemy> _posibleOverflowEnemiesToHit;

		private List<Collider2D> _allEnemiesToHitColliders;

		private List<Enemy> _enemiesActuallyHitByShot = new List<Enemy>();

		[HideInInspector]
		public bool isAPincerManeuverBackwardsShot;

		public float shotDamage
		{
			get
			{
				if (!(_attack == null))
				{
					return _attack.shotDamage;
				}
				return 0f;
			}
		}

		public float shotDamageMod
		{
			get
			{
				if (!(_attack == null))
				{
					return _attack.shotDamageMod;
				}
				return 0f;
			}
		}

		public bool isNaturalOverflow => _shotType == ShotType.OVERFLOW;

		public bool isOverflowShot
		{
			get
			{
				if (_shotType != ShotType.OVERFLOW)
				{
					if (_shotType == ShotType.NORMAL)
					{
						return _relicManager.RelicEffectActive(RelicEffect.NORMAL_ATTACKS_OVERFLOW);
					}
					return false;
				}
				return true;
			}
		}

		public bool isPierceShot => _shotType == ShotType.PIERCE;

		public bool canAimUp => _canAimUp;

		public bool isReverseAttack => _isReverseAttack;

		public List<Enemy> enemiesActuallyHitByShot => _enemiesActuallyHitByShot;

		public void Initialize(ProjectileAttack attack, AudioSource sfxAudioSource, RelicManager relicManager)
		{
			_attack = attack;
			_sfxAudioSource = sfxAudioSource;
			_relicManager = relicManager;
			if (RelicManager.IsNumberPrime(BattleController.ShotCount) && _relicManager.RelicEffectActive(RelicEffect.ALL_ATTACKS_PIERCE))
			{
				if (_shotType == ShotType.NORMAL || _shotType == ShotType.OVERFLOW)
				{
					_previousShotType = _shotType;
					_shotType = ShotType.PIERCE;
					_enemiesToPierce = 1;
				}
				else if (_shotType == ShotType.PIERCE)
				{
					_enemiesToPierce++;
				}
			}
			if (_relicManager.RelicEffectActive(RelicEffect.MORE_PIERCE_LESS_BOMB_DAMAGE))
			{
				if (_shotType == ShotType.NORMAL || _shotType == ShotType.OVERFLOW)
				{
					_previousShotType = _shotType;
					_shotType = ShotType.PIERCE;
					_enemiesToPierce = 2;
				}
				else if (_shotType == ShotType.PIERCE)
				{
					_enemiesToPierce += 2;
				}
			}
		}

		private void OnEnable()
		{
			if (_startupSpawn != null)
			{
				initialPos = _startupSpawn.transform.position + _startupOffset;
			}
			else
			{
				initialPos = base.transform.position + _startupOffset;
			}
			if (_anim == null)
			{
				_anim = GetComponentInChildren<Animator>();
			}
			if (_collider == null)
			{
				_collider = GetComponent<Collider2D>();
			}
			if (_renderer == null)
			{
				_renderer = GetComponentInChildren<SpriteRenderer>();
			}
			if (_rigid == null)
			{
				_rigid = GetComponent<Rigidbody2D>();
			}
			if (_explosionPrefab != null)
			{
				_explosion = Object.Instantiate(_explosionPrefab, base.transform.position, Quaternion.identity, base.transform);
				_explosion.SetActive(value: false);
			}
			_renderer.enabled = false;
			_collider.enabled = false;
			_rigid.velocity = Vector3.zero;
			_rigid.isKinematic = false;
			if (_anim != null)
			{
				_anim.enabled = true;
			}
			_pierceCount = 0;
		}

		public virtual void Fire(int numPegsHit, Enemy target, int critCount = 0)
		{
			if (critCount > 0 && _relicManager != null && _relicManager.AttemptUseRelic(RelicEffect.CRITS_PROVIDE_PIERCE))
			{
				if (_shotType == ShotType.NORMAL || _shotType == ShotType.OVERFLOW)
				{
					_previousShotType = _shotType;
					_shotType = ShotType.PIERCE;
					_enemiesToPierce = critCount;
				}
				else if (_shotType == ShotType.PIERCE)
				{
					_enemiesToPierce += critCount;
				}
			}
			CritsGrantPierceAttackBehaviour component = GetComponent<CritsGrantPierceAttackBehaviour>();
			if (component != null && critCount > 0)
			{
				int num = Mathf.Min(critCount, component.maxPierceAmount);
				if (_shotType == ShotType.NORMAL || _shotType == ShotType.OVERFLOW)
				{
					_previousShotType = _shotType;
					_shotType = ShotType.PIERCE;
					_enemiesToPierce = num;
				}
				else if (_shotType == ShotType.PIERCE)
				{
					_enemiesToPierce += num;
				}
			}
			_target = target;
			_rigid.velocity = Vector3.zero;
			base.transform.position = initialPos;
			if (TryGetComponent<SnowballShotBehavior>(out var component2))
			{
				base.transform.position += component2.initialPositionOffset;
			}
			_renderer.enabled = true;
			if (_delayColliderEnable <= 0f)
			{
				_collider.enabled = true;
			}
			else
			{
				StartCoroutine(EnableColliderAfterDelay(_delayColliderEnable));
			}
			float num2 = Mathf.Clamp((float)numPegsHit * _forcePerPeg, _minForce, _maxForce);
			if (base.transform.position.x > 15f)
			{
				_isReverseAttack = true;
				num2 = 0f - num2;
				_renderer.flipX = !_renderer.flipX;
				Vector2 offset = _collider.offset;
				offset.x *= -1f;
				_collider.offset = offset;
				_delayToKillMovement = 0f;
				_delayAnimationEnd = 0f;
			}
			else
			{
				_isReverseAttack = false;
			}
			if (_maxForce != 0f)
			{
				Vector3 vector = Vector3.Lerp(_minSize, _maxSize, Mathf.Abs(num2) / _maxForce);
				SetColliderSize(base.transform.localScale, vector);
				base.transform.localScale = vector;
			}
			_rigid.gravityScale = 0f;
			AudioClip audioClip = ((Mathf.Abs(num2) >= _sfxChangeForceThreshold) ? AttackSfx.FastAttackClip : AttackSfx.SlowAttackClip);
			if (_startupSpawn != null)
			{
				GameObject gameObject = Object.Instantiate(_startupSpawn);
				if (_startupSpawnFollowRotation)
				{
					gameObject.transform.right = GetFireDirection(target);
				}
			}
			if (startupDelay > 0f)
			{
				if (invisibleDuringStartupDelay)
				{
					if (TryGetComponent<SpriteRenderer>(out var component3))
					{
						component3.enabled = false;
					}
					else
					{
						SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
						if (componentInChildren != null)
						{
							componentInChildren.enabled = false;
						}
					}
				}
				if (_startupAnimationState != "none" && _anim != null)
				{
					_anim.Play(_startupAnimationState);
				}
				StartCoroutine(DelayAddForce(target, num2));
				StartCoroutine(DelayPlaySfx(audioClip));
			}
			else
			{
				ApplyFiringForce(target, num2);
				_sfxAudioSource?.PlayOneShot(audioClip);
			}
			if (_delayToKillMovement > 0f)
			{
				StartCoroutine(DelayKillForce(_delayToKillMovement, _delayAnimationEnd));
			}
		}

		private IEnumerator EnableColliderAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			_collider.enabled = true;
		}

		private void SetColliderSize(Vector3 currentScale, Vector3 newScale)
		{
			Vector2 vector = new Vector2(newScale.x / currentScale.x, newScale.y / currentScale.y);
			CircleCollider2D circleCollider2D = _collider as CircleCollider2D;
			if (circleCollider2D != null)
			{
				float num = 0.4f / currentScale.x;
				float num2 = 0.2f / circleCollider2D.radius;
				circleCollider2D.radius *= num * num2;
				circleCollider2D.radius /= vector.x;
			}
		}

		public void Whiff()
		{
			base.transform.position = initialPos;
			base.transform.localScale = _minSize;
			_renderer.enabled = true;
			_collider.enabled = true;
			_rigid.gravityScale = 0.5f;
			_rigid.AddForce(Vector3.right * 5f);
		}

		private IEnumerator DelayAddForce(Enemy target, float force)
		{
			yield return new WaitForSeconds(startupDelay);
			if (invisibleDuringStartupDelay)
			{
				if (TryGetComponent<SpriteRenderer>(out var component))
				{
					component.enabled = true;
				}
				else
				{
					SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
					if (componentInChildren != null)
					{
						componentInChildren.enabled = true;
					}
				}
			}
			if (isAPincerManeuverBackwardsShot && (target == null || target.CurrentHealth <= 0f))
			{
				EnemyManager enemyManager = Object.FindObjectOfType<EnemyManager>();
				if (enemyManager != null)
				{
					target = enemyManager.GetFarthestEnemyFromPlayer();
				}
			}
			ApplyFiringForce(target, force);
		}

		private Vector2 GetFireDirection(Enemy target)
		{
			Vector2 result = Vector3.right;
			if (target != null && target.IsFlying && _canAimUp && !_relicManager.RelicEffectActive(RelicEffect.FLYING_HORIZONTAL_PIERCE))
			{
				Vector2 vector = target.GetComponentInChildren<TargetingUI>().transform.position - base.transform.position;
				if (base.transform.position.x > target.transform.position.x)
				{
					result = -vector;
					if (base.transform.localScale.x > 0f)
					{
						Vector3 localScale = base.transform.localScale;
						localScale.x *= -1f;
						base.transform.localScale = localScale;
					}
				}
				else
				{
					result = vector;
				}
			}
			return result;
		}

		private void ApplyFiringForce(Enemy target, float force)
		{
			base.transform.right = GetFireDirection(target);
			Vector3 vector = base.transform.right * force;
			RaycastShotFlight(target, new Vector2(base.transform.position.x, base.transform.position.y), new Vector2(vector.x, vector.y));
			_rigid.AddForce(base.transform.right * force);
			if (_shouldRotate)
			{
				_rigid.AddTorque(0f - force);
			}
		}

		private void RaycastShotFlight(Enemy target, Vector2 origin, Vector2 aimDirection)
		{
			CircleCollider2D circleCollider2D = _collider as CircleCollider2D;
			if (_collider is BoxCollider2D || circleCollider2D == null)
			{
				if (_enemiesToPierce < 99)
				{
					Debug.LogError("Only attacks all enemies shots should have box colliders.");
				}
				return;
			}
			aimDirection = aimDirection.normalized;
			Vector2 vector = Vector3.Cross(aimDirection, Vector3.back).normalized;
			float num = 0.080000006f;
			origin += circleCollider2D.offset;
			Vector2 origin2 = origin + vector * num;
			Vector2 origin3 = origin + -vector * num;
			List<RaycastHit2D> list = new List<RaycastHit2D>();
			list.AddRange(Physics2D.RaycastAll(origin, aimDirection, 50f));
			list.AddRange(Physics2D.RaycastAll(origin2, aimDirection, 50f));
			list.AddRange(Physics2D.RaycastAll(origin3, aimDirection, 50f));
			Dictionary<Enemy, float> raycastHitEnemiesAndDistance = new Dictionary<Enemy, float>();
			foreach (RaycastHit2D item in list)
			{
				if (item.collider.TryGetComponent<Enemy>(out var component) && !raycastHitEnemiesAndDistance.ContainsKey(component) && (target == null || (_canAimUp && target.IsFlying == component.IsFlying) || (!_canAimUp && !component.IsFlying)))
				{
					raycastHitEnemiesAndDistance.Add(component, item.distance);
				}
			}
			List<Enemy> list2 = new List<Enemy>(raycastHitEnemiesAndDistance.Keys);
			list2.Sort((Enemy x, Enemy y) => (int)Mathf.Sign(raycastHitEnemiesAndDistance[x] - raycastHitEnemiesAndDistance[y]));
			_allEnemiesToHitColliders = new List<Collider2D>();
			foreach (Enemy item2 in list2)
			{
				_allEnemiesToHitColliders.Add(item2.GetComponent<Collider2D>());
			}
			int num2 = _enemiesToPierce + 1;
			_enemiesToHit = new List<Enemy>();
			foreach (Enemy item3 in list2)
			{
				StatusEffect effect = item3.GetEffect(StatusEffectType.Transpherency);
				if (effect == null || effect.Intensity < 10)
				{
					num2--;
				}
				_enemiesToHit.Add(item3);
				if (num2 <= 0 && effect == null)
				{
					break;
				}
			}
			_posibleOverflowEnemiesToHit = new List<Enemy>(list2.Skip(_enemiesToHit.Count));
		}

		private IEnumerator DelayPlaySfx(AudioClip audioClip)
		{
			yield return new WaitForSeconds(_audioStartupDelay);
			_sfxAudioSource?.PlayOneShot(audioClip);
		}

		private IEnumerator DelayKillForce(float delay, float endAnimationDelay)
		{
			yield return new WaitForSeconds(delay);
			_rigid.velocity = Vector3.zero;
			yield return new WaitForSeconds(endAnimationDelay);
			DisableShot();
			OnShotHit?.Invoke(null, this, attackFinished: true);
		}

		private void Update()
		{
			if (base.transform.position.x > initialPos.x + 100f || (isAPincerManeuverBackwardsShot && base.transform.position.x < -15f))
			{
				Debug.LogWarning("Shot being destroyed after missing a shot and flying forever.");
				_rigid.gravityScale = 0f;
				_rigid.velocity = Vector2.zero;
				_renderer.enabled = false;
				_collider.enabled = false;
				if (_explosion != null)
				{
					_explosion.SetActive(value: true);
				}
				OnShotHit?.Invoke(null, this, attackFinished: true);
			}
			CheckForMissedEnemiesByXPos();
		}

		public bool ShouldHitEnemy(Enemy enemy)
		{
			if (!_enemiesActuallyHitByShot.Contains(enemy))
			{
				if (_enemiesToHit != null)
				{
					if (!_enemiesToHit.Contains(enemy))
					{
						if (isOverflowShot)
						{
							return _posibleOverflowEnemiesToHit.Contains(enemy);
						}
						return false;
					}
					return true;
				}
				return true;
			}
			return false;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Enemy component = collision.GetComponent<Enemy>();
			if (collision.gameObject.CompareTag("Wall"))
			{
				_rigid.gravityScale = 0f;
				DisableShot();
				if (shotDamage * shotDamageMod <= 0f)
				{
					_sfxAudioSource?.PlayOneShot(AttackSfx.OnWhiffClip);
				}
				OnShotHit?.Invoke(null, this, attackFinished: true);
			}
			else
			{
				if (component == null || component.CurrentHealth <= 0f || !ShouldHitEnemy(component))
				{
					return;
				}
				_enemiesActuallyHitByShot.Add(component);
				StatusEffect statusEffect = null;
				statusEffect = component.GetEffect(StatusEffectType.Transpherency);
				if (isOverflowShot)
				{
					_sfxAudioSource?.PlayOneShot(AttackSfx.OnHitClip);
					OnShotHit?.Invoke(component, this, attackFinished: false);
				}
				else if (_shotType == ShotType.NORMAL)
				{
					_sfxAudioSource?.PlayOneShot(AttackSfx.OnHitClip);
					if (statusEffect == null)
					{
						if (_delayAnimationEnd > 0f)
						{
							_anim.Play(_endAnimationState);
							OnShotHit?.Invoke(component, this, attackFinished: false);
							StartCoroutine(DelayKillForce(_delayToKillMovement, _delayAnimationEnd));
						}
						else
						{
							DisableShot();
							OnShotHit?.Invoke(component, this, attackFinished: true);
						}
					}
					else
					{
						OnShotHit?.Invoke(component, this, attackFinished: false);
					}
				}
				else if (_shotType == ShotType.PINPOINT && component == _target)
				{
					DisableShot();
					_sfxAudioSource?.PlayOneShot(AttackSfx.OnHitClip);
					OnShotHit?.Invoke(component, this, attackFinished: true);
				}
				else if (_shotType == ShotType.PIERCE)
				{
					if (statusEffect == null || statusEffect.Intensity < 10)
					{
						_pierceCount++;
					}
					if (_pierceCount == 1)
					{
						_relicManager.AttemptUseRelic(RelicEffect.ALL_ATTACKS_PIERCE);
					}
					_sfxAudioSource?.PlayOneShot(AttackSfx.OnHitClip);
					OnShotHit?.Invoke(component, this, attackFinished: false);
					if (_pierceCount >= _enemiesToPierce)
					{
						_pierceCount = 0;
						_shotType = _previousShotType;
					}
				}
			}
		}

		public void DisableShot()
		{
			_rigid.velocity = Vector2.zero;
			_collider.enabled = false;
			_renderer.enabled = false;
			if (_anim != null)
			{
				_anim.enabled = false;
			}
			if (_explosion != null)
			{
				_explosion.SetActive(value: true);
			}
			OnShotDisabled?.Invoke(this);
		}

		private void OnDisable()
		{
			base.transform.position = initialPos;
			if (_explosion != null)
			{
				Object.Destroy(_explosion);
			}
		}

		private void CheckForMissedEnemiesByXPos()
		{
			if (_enemiesToHit == null || _allEnemiesToHitColliders == null)
			{
				return;
			}
			for (int i = 0; i < _enemiesToHit.Count; i++)
			{
				if (_enemiesToHit[i] != null && _enemiesToHit[i].CurrentHealth > 0f && !_enemiesActuallyHitByShot.Contains(_enemiesToHit[i]) && _allEnemiesToHitColliders[i] != null && ((!isReverseAttack && _allEnemiesToHitColliders[i].bounds.max.x < _collider.bounds.min.x) || (isReverseAttack && _allEnemiesToHitColliders[i].bounds.min.x > _collider.bounds.max.x)))
				{
					_enemiesActuallyHitByShot.Add(_enemiesToHit[i]);
					bool flag = false;
					if (!isOverflowShot && _shotType == ShotType.NORMAL && _enemiesToHit[i].GetEffect(StatusEffectType.Transpherency) == null && _delayAnimationEnd <= 0f)
					{
						flag = true;
					}
					OnShotHit?.Invoke(_enemiesToHit[i], this, flag);
					if (flag)
					{
						DisableShot();
						return;
					}
				}
			}
			if (!isOverflowShot)
			{
				return;
			}
			for (int j = 0; j < _posibleOverflowEnemiesToHit.Count; j++)
			{
				if (_posibleOverflowEnemiesToHit[j] != null && _posibleOverflowEnemiesToHit[j].CurrentHealth > 0f && !_enemiesActuallyHitByShot.Contains(_posibleOverflowEnemiesToHit[j]) && _allEnemiesToHitColliders[_enemiesToHit.Count + j] != null && ((!isReverseAttack && _allEnemiesToHitColliders[_enemiesToHit.Count + j].bounds.max.x < _collider.bounds.min.x) || (isReverseAttack && _allEnemiesToHitColliders[_enemiesToHit.Count + j].bounds.min.x > _collider.bounds.max.x)))
				{
					_enemiesActuallyHitByShot.Add(_posibleOverflowEnemiesToHit[j]);
					OnShotHit?.Invoke(_posibleOverflowEnemiesToHit[j], this, attackFinished: false);
				}
			}
		}

		public void SetShotTypePierce(int enemiesToPierce)
		{
			_shotType = ShotType.PIERCE;
			_enemiesToPierce = enemiesToPierce;
		}
	}
}
