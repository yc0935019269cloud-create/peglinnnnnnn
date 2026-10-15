using System.Collections;
using System.Globalization;
using Battle.Pachinko.Obstacles;
using Currency;
using DG.Tweening;
using Relics;
using TMPro;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegCoinOverlay : DisableFromPrediction
	{
		public delegate void CoinOverlayEvent(Vector3 position);

		public static CoinOverlayEvent OnCoinCollected = (Vector3 position) =>
		{
		};

		private const float _multiCoinCollectionDelay = 0.35f;

		public static int ongoingCoinCollections = 0;

		private static readonly WaitForSeconds coinCollectionWait = new WaitForSeconds(0.35f);

		private Animator _anim;

		private Peg _peg;

		private SpriteRenderer _rend;

		private TMP_Text _text;

		private int _numCoins;

		private static readonly int NumCoinsParam = Animator.StringToHash("num_coins");

		private bool _onSpecialPeg;

		public int NumCoins => _numCoins;

		private void OnEnable()
		{
			CheckComponents();
			_anim.SetInteger(NumCoinsParam, _numCoins);
			_rend.enabled = _numCoins > 0 && !_onSpecialPeg;
			UpdateText();
		}

		private void CheckComponents()
		{
			_anim = GetComponent<Animator>();
			_peg = GetComponentsInParent<Peg>(includeInactive: true)[0];
			_rend = GetComponent<SpriteRenderer>();
			_text = GetComponentInChildren<TMP_Text>(includeInactive: true);
		}

		public void AddCoins(int num = 1)
		{
			CheckComponents();
			_numCoins += num;
			_rend.enabled = _numCoins > 0 && !_onSpecialPeg;
			_anim.SetInteger(NumCoinsParam, _numCoins);
			UpdateText();
			_peg.AddBuff(0);
		}

		public void RemoveCoinsWithoutCollecting(int num = -1)
		{
			if (_numCoins > 0)
			{
				if (num < 0)
				{
					num = _numCoins;
				}
				num = Mathf.Min(num, _numCoins);
				_numCoins -= num;
				_anim.SetInteger(NumCoinsParam, _numCoins);
				UpdateText();
				FinishCollectingCoins(_numCoins);
			}
		}

		private void UpdateText()
		{
			_text.text = _numCoins.ToString(CultureInfo.InvariantCulture);
			_text.enabled = _numCoins > 1 && !_onSpecialPeg;
		}

		public void UpdateAlpha(float a)
		{
			if (_rend != null)
			{
				Color color = _rend.color;
				color.a = a;
				_rend.color = color;
				Color color2 = _text.color;
				color2.a = a;
				_text.color = color2;
			}
		}

		public void CollectCoin()
		{
			CollectCoins(1);
		}

		public void CollectCoins(int num = -1)
		{
			if (_numCoins <= 0)
			{
				return;
			}
			if (num < 0)
			{
				num = _numCoins;
			}
			num = Mathf.Min(num, _numCoins);
			_numCoins -= num;
			_anim.SetInteger(NumCoinsParam, _numCoins);
			UpdateText();
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(CoinCollectionAnimation(num, _numCoins));
				return;
			}
			Debug.LogWarning("Attempting to collect coins from disabled peg");
			for (int i = 0; i < num; i++)
			{
				TriggerCoinCollected();
			}
			FinishCollectingCoins(_numCoins);
		}

		private IEnumerator CoinCollectionAnimation(int numCoins, int remainingCoins)
		{
			ongoingCoinCollections++;
			for (int i = 0; i < numCoins; i++)
			{
				TriggerCoinCollected();
				yield return coinCollectionWait;
			}
			ongoingCoinCollections--;
			FinishCollectingCoins(remainingCoins);
		}

		private void TriggerCoinCollected()
		{
			int num = (((BattleController.BattleActive && BattleController.CurrentBattleState != BattleController.BattleState.NAVIGATION) || !_peg.relicManager.AttemptUseRelic(RelicEffect.INCREASED_NAV_GOLD)) ? 1 : 4);
			if (!_peg.relicManager.RelicEffectActive(RelicEffect.CONVERT_COIN_TO_DAMAGE))
			{
				CurrencyManager.Instance.AddGold(num);
			}
			else
			{
				_peg.relicManager.DoCoinsProvideHealingRelicsEffect(num);
			}
			if (BattleController.CurrentBattleState == BattleController.BattleState.AWAITING_SHOT_COMPLETION && _peg.relicManager.AttemptUseRelic(RelicEffect.GOLD_ADDS_TO_DAMAGE))
			{
				BattleController.OnAdditionalBasicPegRequested?.Invoke(_peg.GetCenterOfPeg() + Vector3.up * 0.5f);
				Peg.OnPegAudioRequest?.Invoke(Peg.PegType.REGULAR);
			}
			OnCoinCollected?.Invoke(_peg.GetCenterOfPeg());
		}

		private void FinishCollectingCoins(int remainingCoins)
		{
			if (remainingCoins == 0)
			{
				base.gameObject.SetActive(value: false);
				_peg.AddBuff(0);
			}
		}

		public void ToggleGuardingSpecialPeg(bool isSpecial)
		{
			_onSpecialPeg = isSpecial;
			_rend.enabled = _numCoins > 0 && !_onSpecialPeg;
			_text.alpha = (_onSpecialPeg ? 0f : 1f);
			UpdateText();
		}

		public void EndBattleFadeOut(float fadeTime = 0.65f)
		{
			_anim.SetInteger(NumCoinsParam, 0);
			_rend.DOFade(0f, fadeTime);
			_text.DOFade(0f, fadeTime);
		}
	}
}
