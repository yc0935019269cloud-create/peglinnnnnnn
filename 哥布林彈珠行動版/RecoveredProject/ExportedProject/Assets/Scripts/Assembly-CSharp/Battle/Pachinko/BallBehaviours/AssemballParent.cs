using Battle.Attacks;
using Battle.StatusEffects;
using I2.Loc;
using Peglin.Achievements;
using Relics;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class AssemballParent : PachinkoBall
	{
		public Texture2D baseFrame;

		public Texture2D redSprite;

		public Texture2D greenSprite;

		public Texture2D yellowSprite;

		public Texture2D whiteSprite;

		public Texture2D pinkSprite;

		public AssemballType assemballComponents;

		private AssemballAttack _attack;

		private Sprite _sprite;

		public override Sprite sprite => BuildSprite(assemballComponents);

		public override void Init(RelicManager relicManager, DeckManager deckManager, Vector2 previousAimDir, PredictionManager predictionManager, PlayerStatusEffectController playerStatusEffectController)
		{
			base.Init(relicManager, deckManager, previousAimDir, predictionManager, playerStatusEffectController);
			GetComponentInChildren<SpriteRenderer>().sprite = BuildSprite(assemballComponents);
		}

		public void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			int aSSEMBALL_NORMAL_DMG = AssemballComponent.ASSEMBALL_NORMAL_DMG;
			component.SetParameterValue("ASSEMBALL_NORMAL_DMG", aSSEMBALL_NORMAL_DMG.ToString());
			aSSEMBALL_NORMAL_DMG = AssemballComponent.ASSEMBALL_CRIT_DMG;
			component.SetParameterValue("ASSEMBALL_CRIT_DMG", aSSEMBALL_NORMAL_DMG.ToString());
			aSSEMBALL_NORMAL_DMG = AssemballComponent.SLIME_X_PEGS_HIT;
			component.SetParameterValue("SLIME_X_PEGS_HIT", aSSEMBALL_NORMAL_DMG.ToString());
			aSSEMBALL_NORMAL_DMG = AssemballComponent.BUFF_AMOUNT;
			component.SetParameterValue("BUFF_AMOUNT", aSSEMBALL_NORMAL_DMG.ToString());
		}

		public Sprite BuildSprite(AssemballType assemballType)
		{
			Texture2D texture2D = new Texture2D(16, 16, TextureFormat.ARGB32, mipChain: false);
			_sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 8f);
			texture2D.filterMode = FilterMode.Point;
			for (int i = 0; i < texture2D.width; i++)
			{
				for (int j = 0; j < texture2D.height; j++)
				{
					texture2D.SetPixel(i, j, Color.clear);
					SetPixelFromTexture(i, j, texture2D, baseFrame);
					if (assemballType.HasFlag(AssemballType.RED))
					{
						SetPixelFromTexture(i, j, texture2D, redSprite);
					}
					if (assemballType.HasFlag(AssemballType.PINK))
					{
						SetPixelFromTexture(i, j, texture2D, pinkSprite);
					}
					if (assemballType.HasFlag(AssemballType.YELLOW))
					{
						SetPixelFromTexture(i, j, texture2D, yellowSprite);
					}
					if (assemballType.HasFlag(AssemballType.GREEN))
					{
						SetPixelFromTexture(i, j, texture2D, greenSprite);
					}
					if (assemballType.HasFlag(AssemballType.WHITE))
					{
						SetPixelFromTexture(i, j, texture2D, whiteSprite);
					}
				}
			}
			texture2D.Apply();
			return _sprite;
		}

		private void SetPixelFromTexture(int x, int y, Texture2D final, Texture2D overlay)
		{
			Color pixel = overlay.GetPixel(x, y);
			if (pixel.a > 0f)
			{
				final.SetPixel(x, y, pixel);
			}
		}

		public bool TryApplyComponent(AssemballType type)
		{
			if (assemballComponents.HasFlag(type))
			{
				return false;
			}
			assemballComponents |= type;
			if (_attack == null)
			{
				_attack = GetComponent<AssemballAttack>();
			}
			_attack.SetComponents(assemballComponents);
			if (assemballComponents.HasFlag(AssemballType.ALL))
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementData.AchievementId.ASSEMBLE_ASSEMBALL]);
			}
			return true;
		}
	}
}
