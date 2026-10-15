using UnityEngine;

namespace Battle.PegBehaviour
{
	public class RegularPegSlimeBehaviour : MonoBehaviour
	{
		public RuntimeAnimatorController flameAnimator;

		private RuntimeAnimatorController _defaultAnimator;

		public PegSlimeData pegSlimeData;

		private PhysicsMaterial2D _defaultMaterial;

		private SpriteRenderer _spriteRenderer;

		private Animator _animator;

		private Animator _shineAnimator;

		private static readonly int SlimeOnAnimHash = Animator.StringToHash("SlimeOn");

		private static readonly int SlimeOffAnimHash = Animator.StringToHash("SlimeOff");

		private static readonly int SlimeMinimalAnimHash = Animator.StringToHash("ShineMinimal");

		private static readonly int SlimeShineRegularHash = Animator.StringToHash("ShineRegular");

		private bool _isDisappearing;

		private void Awake()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
			_animator = componentsInChildren[0];
			if (componentsInChildren.Length > 1)
			{
				_shineAnimator = componentsInChildren[1];
			}
			_defaultAnimator = _animator.runtimeAnimatorController;
			_defaultMaterial = GetComponentInParent<Collider2D>().sharedMaterial;
		}

		private void OnDisable()
		{
			if (_isDisappearing)
			{
				SlimeOffFinished();
			}
		}

		public void ApplySlime(Peg.SlimeType slimeType, int pegBuffAmount)
		{
			_animator.runtimeAnimatorController = GetAnimatorForSlimeType(slimeType);
			_spriteRenderer.color = GetColorForSlimeType(slimeType);
			if (slimeType != Peg.SlimeType.FlameOverlay)
			{
				_animator.SetTrigger(SlimeOnAnimHash);
				SetSlimeShine(pegBuffAmount, puttingSlimeOn: true);
				_shineAnimator.gameObject.SetActive(value: true);
			}
			else
			{
				_shineAnimator.gameObject.SetActive(value: false);
			}
		}

		public void StartSlimeDestruction()
		{
			_animator.SetTrigger(SlimeOffAnimHash);
			_shineAnimator.SetTrigger(SlimeOffAnimHash);
			_isDisappearing = true;
		}

		public void SlimeOffFinished()
		{
			_isDisappearing = false;
			base.gameObject.SetActive(value: false);
		}

		public void SetSlimeShine(int pegBuffAmount, bool puttingSlimeOn = false)
		{
			if (pegBuffAmount == 0)
			{
				if (puttingSlimeOn)
				{
					_shineAnimator.SetTrigger(SlimeOnAnimHash);
				}
				else
				{
					_shineAnimator.SetTrigger(SlimeShineRegularHash);
				}
				return;
			}
			if (puttingSlimeOn)
			{
				_shineAnimator.SetTrigger(SlimeOnAnimHash);
			}
			_shineAnimator.ResetTrigger(SlimeShineRegularHash);
			_shineAnimator.SetTrigger(SlimeMinimalAnimHash);
		}

		public RuntimeAnimatorController GetAnimatorForSlimeType(Peg.SlimeType slimeType)
		{
			if (slimeType == Peg.SlimeType.FlameOverlay)
			{
				return flameAnimator;
			}
			return _defaultAnimator;
		}

		public Color GetColorForSlimeType(Peg.SlimeType slimeType)
		{
			return slimeType switch
			{
				Peg.SlimeType.Slow => pegSlimeData.enemyColor, 
				Peg.SlimeType.Web => pegSlimeData.webColor, 
				Peg.SlimeType.Yolk => pegSlimeData.yolkColor, 
				Peg.SlimeType.LightningSlime => pegSlimeData.lightningColor, 
				Peg.SlimeType.BouncySlime => pegSlimeData.bounceColor, 
				Peg.SlimeType.PoisonSlime => pegSlimeData.poisonColor, 
				Peg.SlimeType.HealSlime => pegSlimeData.healColor, 
				Peg.SlimeType.DoubleDamageSlime => pegSlimeData.doubleDamageColor, 
				Peg.SlimeType.DamageReductionSlime => pegSlimeData.damageReductionColor, 
				_ => Color.white, 
			};
		}

		public PhysicsMaterial2D GetMaterialForSlimeType(Peg.SlimeType slimeType)
		{
			return slimeType switch
			{
				Peg.SlimeType.Slow => pegSlimeData.slowMaterial, 
				Peg.SlimeType.Web => pegSlimeData.noBounceMaterial, 
				Peg.SlimeType.Yolk => pegSlimeData.yolkMaterial, 
				Peg.SlimeType.BouncySlime => pegSlimeData.bouncyMaterial, 
				_ => _defaultMaterial, 
			};
		}
	}
}
