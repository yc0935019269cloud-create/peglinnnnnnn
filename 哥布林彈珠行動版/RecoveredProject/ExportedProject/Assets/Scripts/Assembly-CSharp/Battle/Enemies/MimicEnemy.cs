using Battle.StatusEffects;
using Challenges;
using Map;
using UnityEngine;

namespace Battle.Enemies
{
	public class MimicEnemy : StationaryEnemy
	{
		public delegate void MimicOpened();

		public static MimicOpened OnMimicOpened = () =>
		{
		};

		[Space(20f)]
		public Vector3 startPosition;

		public GameObject chestHighlight;

		public GameObject chestUI;

		public Canvas enemyCanvas;

		public Canvas statusEffectPanelCanvas;

		public AnimatorOverrideController coreMimicAnimationController;

		[HideInInspector]
		public bool skipped;

		private bool _opened;

		protected override void Awake()
		{
			enemyCanvas.enabled = false;
			statusEffectPanelCanvas.enabled = false;
			base.Awake();
			if (MapController.instance.Act == 4)
			{
				_anim = GetComponent<Animator>();
				_anim.runtimeAnimatorController = coreMimicAnimationController;
			}
		}

		protected override void Start()
		{
			base.Start();
			base.transform.localPosition = startPosition;
		}

		private void OnMouseEnter()
		{
			if (!_opened && !skipped)
			{
				chestHighlight.SetActive(value: true);
			}
		}

		private void OnMouseExit()
		{
			chestHighlight.SetActive(value: false);
		}

		public void OnMouseDown()
		{
			if (!_opened)
			{
				OpenMimic();
			}
		}

		public void OpenMimic()
		{
			if (!_opened && !skipped)
			{
				_opened = true;
				_audioSource.PlayOneShot(_enemySfx.GetRandomEnemySpecialClip());
				_anim.SetTrigger("change");
				chestUI.SetActive(value: false);
				enemyCanvas.enabled = true;
				statusEffectPanelCanvas.enabled = true;
				OnMimicOpened?.Invoke();
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.MIMICS_ARE_MORE_COMMON) && StaticGameData.mimicsBattled > 0)
				{
					int num = ((MapController.instance.Act > 2) ? 150 : 75);
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, StaticGameData.mimicsBattled * num), StatusEffectSource.ENEMY);
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, StaticGameData.mimicsBattled), StatusEffectSource.ENEMY);
				}
			}
		}
	}
}
