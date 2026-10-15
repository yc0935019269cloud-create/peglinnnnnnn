using System.Collections.Generic;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using Cruciball;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks
{
	public class AssemballAttack : ProjectileAttack
	{
		public GameObject aoeShotPrefab;

		private AssemballType _components;

		public override string Description
		{
			get
			{
				CalcLoc();
				string text = "";
				for (int i = 0; i < locDescStrings.Length; i++)
				{
					string text2 = locDescStrings[i];
					if (text2 != null)
					{
						text = text + "<sprite name=\"BULLET\"><indent=8%>" + LocalizationManager.GetTranslation("Orbs/" + text2, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true, base.gameObject) + "</indent>\n";
					}
				}
				return text;
			}
		}

		public override void Initialize(Vector2 playerPosition, AttackManager attackManager, RelicManager relicManager, DeckManager deckManager, CruciballManager cruciballManager, PlayerHealthController healthController, PlayerStatusEffectController playerStatusEffectController)
		{
			base.Initialize(playerPosition, attackManager, relicManager, deckManager, cruciballManager, healthController, playerStatusEffectController);
			AssemballParent component = GetComponent<AssemballParent>();
			ApplyComponent(component.assemballComponents);
			_shot.GetComponent<SpriteRenderer>().sprite = component.sprite;
			CalcLoc();
		}

		public override void SoftInit(DeckManager deckManager, RelicManager relicManager, CruciballManager cruciballManager, bool forUI = true)
		{
			base.SoftInit(deckManager, relicManager, cruciballManager);
			if (_components.HasFlag(AssemballType.ALL))
			{
				DamagePerPeg = AssemballComponent.ASSEMBALL_NORMAL_DMG + AssemballComponent.FULLY_FORMED_DMG;
				CritDamagePerPeg = AssemballComponent.ASSEMBALL_CRIT_DMG + AssemballComponent.FULLY_FORMED_CRIT;
			}
			else
			{
				if (_components.HasFlag(AssemballType.RED))
				{
					DamagePerPeg = AssemballComponent.ASSEMBALL_NORMAL_DMG;
				}
				if (_components.HasFlag(AssemballType.YELLOW))
				{
					CritDamagePerPeg = AssemballComponent.ASSEMBALL_CRIT_DMG;
				}
			}
			_hideOrbLevel = true;
		}

		public static int GetAssemballDisplayLevel(int numberOfComponents)
		{
			if (numberOfComponents == 5)
			{
				return 3;
			}
			if (numberOfComponents >= 3)
			{
				return 2;
			}
			return 1;
		}

		public int GetNumberOfComponents()
		{
			int num = 0;
			if (_components.HasFlag(AssemballType.RED))
			{
				num++;
			}
			if (_components.HasFlag(AssemballType.PINK))
			{
				num++;
			}
			if (_components.HasFlag(AssemballType.YELLOW))
			{
				num++;
			}
			if (_components.HasFlag(AssemballType.GREEN))
			{
				num++;
			}
			if (_components.HasFlag(AssemballType.WHITE))
			{
				num++;
			}
			return num;
		}

		protected override void CalcLoc()
		{
			ClearBattleParameters();
			locName = base.Name;
			List<string> list = new List<string>();
			AssemballParent component = GetComponent<AssemballParent>();
			list.Add("assemball_fully_formed");
			if (component.assemballComponents.HasFlag(AssemballType.GREEN))
			{
				list.Add("buff_orb_desc3");
			}
			if (component.assemballComponents.HasFlag(AssemballType.PINK))
			{
				list.Add("spreads_bouncy_slime");
			}
			if (component.assemballComponents.HasFlag(AssemballType.WHITE))
			{
				list.Add("attacks_all_enemies");
			}
			locDescStrings = list.ToArray();
		}

		public void SetComponents(AssemballType components)
		{
			_components = components;
		}

		private void ApplyComponent(AssemballType componentType)
		{
			_components = componentType;
			if (componentType.HasFlag(AssemballType.RED))
			{
				DamagePerPeg = AssemballComponent.ASSEMBALL_NORMAL_DMG;
			}
			if (componentType.HasFlag(AssemballType.YELLOW))
			{
				CritDamagePerPeg = AssemballComponent.ASSEMBALL_CRIT_DMG;
			}
			if (componentType.HasFlag(AssemballType.GREEN))
			{
				PachinkoBuff pachinkoBuff = base.gameObject.AddComponent<PachinkoBuff>();
				pachinkoBuff.buffAmount = AssemballComponent.BUFF_AMOUNT;
				pachinkoBuff.OnValidate();
			}
			if (componentType.HasFlag(AssemballType.PINK) && base.gameObject.GetComponent<PachinkoPegSlimer>() == null)
			{
				PachinkoPegSlimer pachinkoPegSlimer = base.gameObject.AddComponent<PachinkoPegSlimer>();
				pachinkoPegSlimer.slimeType = Peg.SlimeType.BouncySlime;
				pachinkoPegSlimer.slimeEveryXPegs = AssemballComponent.SLIME_X_PEGS_HIT;
				pachinkoPegSlimer.OnValidate();
			}
			if (componentType.HasFlag(AssemballType.WHITE))
			{
				SetShotPrefab(aoeShotPrefab, aoeShotPrefab);
			}
			if (componentType.HasFlag(AssemballType.ALL))
			{
				DamagePerPeg = AssemballComponent.ASSEMBALL_NORMAL_DMG + AssemballComponent.FULLY_FORMED_DMG;
				CritDamagePerPeg = AssemballComponent.ASSEMBALL_CRIT_DMG + AssemballComponent.FULLY_FORMED_CRIT;
			}
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			int sLIME_X_PEGS_HIT = AssemballComponent.SLIME_X_PEGS_HIT;
			component.SetParameterValue("SLIME_X_PEGS_HIT", sLIME_X_PEGS_HIT.ToString());
			sLIME_X_PEGS_HIT = AssemballComponent.BUFF_AMOUNT;
			component.SetParameterValue("BUFF_AMOUNT", sLIME_X_PEGS_HIT.ToString());
			sLIME_X_PEGS_HIT = AssemballComponent.FULLY_FORMED_DMG;
			component.SetParameterValue("FULLY_FORMED_DMG", sLIME_X_PEGS_HIT.ToString());
			sLIME_X_PEGS_HIT = AssemballComponent.FULLY_FORMED_CRIT;
			component.SetParameterValue("FULLY_FORMED_CRIT", sLIME_X_PEGS_HIT.ToString());
		}
	}
}
