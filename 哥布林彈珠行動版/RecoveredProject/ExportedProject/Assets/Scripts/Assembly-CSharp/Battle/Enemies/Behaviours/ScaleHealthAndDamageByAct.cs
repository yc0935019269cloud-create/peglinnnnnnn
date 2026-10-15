using Map;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class ScaleHealthAndDamageByAct : MonoBehaviour
	{
		public int act1Health = 300;

		public int act1CruciballHealth = 400;

		public int act1Damage = 3;

		[Space(10f)]
		public int act2Health = 900;

		public int act2CruciballHealth = 1100;

		public int act2Damage = 4;

		[Space(10f)]
		public int act3Health = 1200;

		public int act3CruciballHealth = 1500;

		public int act3Damage = 5;

		[Space(10f)]
		public int act4Health = 1500;

		public int act4CruciballHealth = 1800;

		public int act4Damage = 6;

		private void Awake()
		{
			Enemy component = GetComponent<Enemy>();
			switch (MapController.instance.Act)
			{
			case 1:
				component.StartingHealth = act1Health;
				component.StartingHealthFirstCruciball = act1CruciballHealth;
				component.DamagePerMeleeAttack = act1Damage;
				component.DamagePerRangedAttack = act1Damage;
				break;
			case 2:
				component.StartingHealth = act2Health;
				component.StartingHealthFirstCruciball = act2CruciballHealth;
				component.DamagePerMeleeAttack = act2Damage;
				component.DamagePerRangedAttack = act2Damage;
				break;
			case 3:
				component.StartingHealth = act3Health;
				component.StartingHealthFirstCruciball = act3CruciballHealth;
				component.DamagePerMeleeAttack = act3Damage;
				component.DamagePerRangedAttack = act3Damage;
				break;
			case 4:
				component.StartingHealth = act4Health;
				component.StartingHealthFirstCruciball = act4CruciballHealth;
				component.DamagePerMeleeAttack = act4Damage;
				component.DamagePerRangedAttack = act4Damage;
				break;
			}
		}

		private void Update()
		{
		}
	}
}
