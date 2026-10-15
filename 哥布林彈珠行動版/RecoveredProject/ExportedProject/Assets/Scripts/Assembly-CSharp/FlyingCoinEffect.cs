using System.Collections;
using Battle;
using Currency;
using DG.Tweening;
using Relics;
using UnityEngine;

public class FlyingCoinEffect : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public ParticleSystem particles;

	public AudioSource audioSource;

	[SerializeField]
	private RelicManager _relicManager;

	private const float EXPLODE_OUT_TO_DISTANCE = 1f;

	private const float EXPLODE_OUT_TO_DISTANCE_RANDOM = 0.1f;

	private const float EXPLODE_OUT_TIME = 0.3f;

	private const float SETTLE_DISTANCE = 0.1f;

	private const float SETTLE_HANG_TIME = 0.5f;

	private const float FLY_TIME = 1.2f;

	private const float START_PARTICLES_EARLY = 0.25f;

	private const float PARTICLES_WAIT_TIME = 0.5f;

	private const float SFX_WAIT_TIME = 1f;

	private const float PRE_REMOVAL_WAIT_TIME = 0.25f;

	private const float REMOVAL_FLY_TIME = 3.2f;

	public void DoCoinAdditionAnimation(Vector3 startPosition, Peg pegToAddCoinToWhenFinished)
	{
		StartCoroutine(AnimateCoinAddition(startPosition, pegToAddCoinToWhenFinished));
	}

	private IEnumerator AnimateCoinAddition(Vector3 startPosition, Peg pegToAddCoinToWhenFinished)
	{
		base.transform.position = startPosition;
		Vector3 explodeOutToPosition = startPosition + Vector3.Slerp(new Vector3(-1f, 0.5f, 0f), new Vector3(1f, 0.5f, 0f), Random.value).normalized * (1f + Random.Range(-0.1f, 0.1f));
		Tween t = base.transform.DOMove(explodeOutToPosition, 0.3f).SetEase(Ease.OutQuad);
		yield return t.WaitForCompletion(returnCustomYieldInstruction: true);
		Vector3 endValue = explodeOutToPosition + new Vector3(Random.value, Random.value) * 0.1f;
		Tween t2 = base.transform.DOMove(endValue, 0.5f).SetEase(Ease.Linear);
		yield return t2.WaitForCompletion(returnCustomYieldInstruction: true);
		Vector3 endValue2 = pegToAddCoinToWhenFinished.transform.position;
		LongPeg longPeg = pegToAddCoinToWhenFinished as LongPeg;
		if (longPeg != null)
		{
			endValue2 = longPeg.GetCenterOfPeg();
		}
		float num = 1.2f + Random.Range(-0.3f, 0.3f);
		base.transform.DOMove(endValue2, num).SetEase(Ease.InOutCubic);
		yield return new WaitForSeconds(num - 0.25f);
		particles.Play();
		yield return new WaitForSeconds(0.25f);
		audioSource.pitch = 1f + Random.Range(-0.4f, 0.4f);
		audioSource.Play();
		if (pegToAddCoinToWhenFinished.IsDisabled() || !pegToAddCoinToWhenFinished.CanAddCoin() || (longPeg != null && longPeg.hit))
		{
			int amount = (((BattleController.BattleActive && BattleController.CurrentBattleState != BattleController.BattleState.NAVIGATION) || !_relicManager.AttemptUseRelic(RelicEffect.INCREASED_NAV_GOLD)) ? 1 : 4);
			if (!_relicManager.RelicEffectActive(RelicEffect.CONVERT_COIN_TO_DAMAGE))
			{
				CurrencyManager.Instance.AddGold(amount);
			}
		}
		else
		{
			pegToAddCoinToWhenFinished.AddCoin(doParticleEffects: false);
		}
		spriteRenderer.enabled = false;
		yield return new WaitForSeconds(1.5f);
		Object.Destroy(base.gameObject);
	}

	public void DoCoinRemovalAnimation(Vector3 endPosition, Peg pegToRemoveCoinFrom)
	{
		StartCoroutine(AnimateCoinRemoval(endPosition, pegToRemoveCoinFrom));
	}

	private IEnumerator AnimateCoinRemoval(Vector3 endPosition, Peg pegToRemoveCoinFrom)
	{
		base.transform.position = pegToRemoveCoinFrom.GetCenterOfPeg();
		audioSource.pitch = 1f + Random.Range(-0.4f, 0.4f);
		audioSource.Play();
		if (pegToRemoveCoinFrom.RemoveCoinsWithoutCollecting())
		{
			float seconds = 0.25f + Random.Range(-0.15f, 0.15f);
			yield return new WaitForSeconds(seconds);
			float num = 3.2f + Random.Range(-0.3f, 0.3f);
			base.transform.DOMove(endPosition, num).SetEase(Ease.InOutCubic);
			yield return new WaitForSeconds(num);
			spriteRenderer.enabled = false;
			yield return new WaitForSeconds(1f);
			Object.Destroy(base.gameObject);
		}
	}
}
