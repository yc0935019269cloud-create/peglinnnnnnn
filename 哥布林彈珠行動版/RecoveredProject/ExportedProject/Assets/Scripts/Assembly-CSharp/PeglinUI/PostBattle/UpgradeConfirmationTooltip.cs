using System.Globalization;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;

namespace PeglinUI.PostBattle
{
	public class UpgradeConfirmationTooltip : MonoBehaviour
	{
		public TextMeshProUGUI title;

		public TextMeshProUGUI desc;

		public TextMeshProUGUI damage;

		public TextMeshProUGUI crit;

		private UIHsvModifier _hsvModifier;

		private void OnEnable()
		{
			if (_hsvModifier == null)
			{
				_hsvModifier = GetComponentInChildren<UIHsvModifier>();
			}
		}

		public void Init(Attack attack, Attack comparison = null)
		{
			if (_hsvModifier == null)
			{
				_hsvModifier = GetComponentInChildren<UIHsvModifier>();
			}
			title.text = attack.GetNameWithLevel();
			desc.text = attack.Description;
			Tooltip.UpdateDamage(attack, damage, crit);
			if (comparison != null)
			{
				if (comparison.GetModifiedDamagePerPeg() < attack.GetModifiedDamagePerPeg())
				{
					damage.color = Tooltip.BETTER_DAMAGE_COLOR;
				}
				else if (comparison.GetModifiedDamagePerPeg() > attack.GetModifiedDamagePerPeg())
				{
					damage.color = Tooltip.WORSE_DAMAGE_COLOR;
				}
				else
				{
					damage.color = Tooltip.DEFAULT_DAMAGE_COLOR;
				}
				if (comparison.GetModifiedDamagePerPeg(1) < attack.GetModifiedDamagePerPeg(1))
				{
					crit.color = Tooltip.BETTER_DAMAGE_COLOR;
				}
				else if (comparison.GetModifiedDamagePerPeg(1) > attack.GetModifiedDamagePerPeg(1))
				{
					crit.color = Tooltip.WORSE_DAMAGE_COLOR;
				}
				else
				{
					crit.color = Tooltip.DEFAULT_DAMAGE_COLOR;
				}
			}
		}

		public void InitUnlimitedUpgrades(Attack attack, UnlimitedUpgrades comparison = null)
		{
			if (_hsvModifier == null)
			{
				_hsvModifier = GetComponentInChildren<UIHsvModifier>();
			}
			title.text = ((comparison == null) ? attack.GetNameWithLevel() : attack.GetNameWithCustomLevel(attack.Level + comparison.timesUpgraded + 1));
			desc.text = attack.Description;
			int num = attack.GetModifiedDamagePerPeg();
			int num2 = attack.GetModifiedDamagePerPeg(1);
			if (comparison != null)
			{
				num += comparison.damageIncrease;
				num2 += comparison.critIncrease;
				SetGrayedOut(val: true);
			}
			damage.text = num.ToString(CultureInfo.InvariantCulture);
			crit.text = num2.ToString(CultureInfo.InvariantCulture);
			if (comparison != null)
			{
				if (comparison.damageIncrease > 0)
				{
					damage.color = Tooltip.BETTER_DAMAGE_COLOR;
				}
				else if (comparison.damageIncrease < 0)
				{
					damage.color = Tooltip.WORSE_DAMAGE_COLOR;
				}
				else
				{
					damage.color = Tooltip.DEFAULT_DAMAGE_COLOR;
				}
				if (comparison.critIncrease > 0)
				{
					crit.color = Tooltip.BETTER_DAMAGE_COLOR;
				}
				else if (comparison.critIncrease < 0)
				{
					crit.color = Tooltip.WORSE_DAMAGE_COLOR;
				}
				else
				{
					crit.color = Tooltip.DEFAULT_DAMAGE_COLOR;
				}
			}
			else
			{
				damage.color = Tooltip.DEFAULT_DAMAGE_COLOR;
				crit.color = Tooltip.DEFAULT_DAMAGE_COLOR;
			}
		}

		public void SetGrayedOut(bool val)
		{
			_hsvModifier.enabled = val;
		}
	}
}
