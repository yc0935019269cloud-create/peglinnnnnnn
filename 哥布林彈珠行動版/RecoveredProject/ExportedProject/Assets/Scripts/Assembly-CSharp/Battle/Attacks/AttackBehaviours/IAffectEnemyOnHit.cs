using Battle.Enemies;
using Relics;

namespace Battle.Attacks.AttackBehaviours
{
	public interface IAffectEnemyOnHit
	{
		void AffectEnemy(Enemy enemy, RelicManager rm, int critCount);
	}
}
