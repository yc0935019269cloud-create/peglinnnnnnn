using Battle.Attacks;
using UnityEngine;

namespace Peglin.PegMinigame
{
	public class OrbReward : Reward
	{
		private GameObject _orb;

		private PachinkoBall _pb;

		private DeckManager _deckManager;

		private Attack _attack;

		public GameObject Orb
		{
			get
			{
				return _orb;
			}
			set
			{
				_orb = value;
			}
		}

		public OrbReward(GameObject orb, DeckManager deckManager)
		{
			_orb = orb;
			_deckManager = deckManager;
			_pb = _orb.GetComponent<PachinkoBall>();
			Attack component = _pb.GetComponent<Attack>();
			component.UpdateBattleParameters();
			_attack = component;
			if (_pb == null)
			{
				Debug.LogError("Received GameObject with no PachinkoBall script!");
			}
		}

		public override Sprite GetSprite()
		{
			return _pb.sprite;
		}

		public override void Grant()
		{
			_deckManager.AddOrbToDeck(_orb);
		}

		public override Color GetSpriteColor()
		{
			return _pb.color;
		}

		public override Vector3 GetSpriteLocalScale()
		{
			return _orb.GetComponentInChildren<SpriteRenderer>().transform.localScale;
		}

		public override void DrawTooltip(Canvas canvas, Vector3 position)
		{
			TooltipManager.Instance.ShowTooltipOrb(_attack, position, new Vector3(0f, 1f));
		}

		public override void DisableTooltip()
		{
			TooltipManager.Instance.HideTooltip();
		}
	}
}
