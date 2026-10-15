using UnityEngine;

namespace Battle.PegBehaviour
{
	public class LongPegSlimeBehaviour : MonoBehaviour
	{
		public PegSlimeData pegSlimeData;

		private PhysicsMaterial2D _defaultMaterial;

		public float scaleMod = 1.02f;

		private MeshRenderer _meshRenderer;

		private void Awake()
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			_defaultMaterial = GetComponentInParent<Collider2D>().sharedMaterial;
		}

		public void ApplySlime(Peg.SlimeType slimeType)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: true);
			}
			_meshRenderer.material.color = GetColorForSlimeType(slimeType);
		}

		public void StartSlimeDestruction()
		{
			base.gameObject.SetActive(value: false);
		}

		public Color GetColorForSlimeType(Peg.SlimeType slimeType)
		{
			return slimeType switch
			{
				Peg.SlimeType.Slow => pegSlimeData.enemyColor, 
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
			switch (slimeType)
			{
			case Peg.SlimeType.Slow:
				return pegSlimeData.slowMaterial;
			case Peg.SlimeType.Yolk:
				return pegSlimeData.yolkMaterial;
			case Peg.SlimeType.BouncySlime:
				return pegSlimeData.bouncyMaterial;
			case Peg.SlimeType.LightningSlime:
			case Peg.SlimeType.DoubleDamageSlime:
			case Peg.SlimeType.PoisonSlime:
			case Peg.SlimeType.DamageReductionSlime:
			case Peg.SlimeType.HealSlime:
				return _defaultMaterial;
			default:
				Debug.LogWarning("Material requested for unknown slime type: " + slimeType);
				return null;
			}
		}
	}
}
