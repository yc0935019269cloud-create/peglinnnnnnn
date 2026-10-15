using System.Collections;
using Battle.Enemies.C20Upgrades;
using Battle.PegBehaviour;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ResetObscurerGrid : AlternateEnemyAction
	{
		private Animator _anim;

		private RandomObscuredPegGrid _randomObscuredPegGrid;

		private BossUpgrade _bossUpgrade;

		private int turnCounter;

		public int turnsPerBoardReset = 5;

		public string resetGridAnimation = "attack";

		public string resetGridAnimationLoopEnd = "loop_end";

		protected override void Awake()
		{
			base.Awake();
			_anim = GetComponent<Animator>();
			_bossUpgrade = GetComponent<BossUpgrade>();
		}

		private void Start()
		{
			_randomObscuredPegGrid = Object.FindObjectOfType<RandomObscuredPegGrid>();
		}

		public override bool ConditionsMet()
		{
			bool flag = ++turnCounter >= turnsPerBoardReset;
			turnCounter %= turnsPerBoardReset;
			return _bossUpgrade.isUpgraded & flag;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_enemy.CurrentlyActing = true;
			_anim.SetTrigger(resetGridAnimation);
			yield return WaitObjectAllocator.waitForEndOfFrame;
			float timer = 0f;
			AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
			while (timer <= stateInfo.length)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			yield return StartCoroutine(_randomObscuredPegGrid.ResetObscurerGrid());
			_anim.SetTrigger(resetGridAnimationLoopEnd);
			_enemy.CurrentlyActing = false;
		}
	}
}
