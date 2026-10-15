using Battle.Enemies;

namespace Battle
{
	internal struct Spawn
	{
		public int turnNumber;

		public Enemy enemy;

		public Spawn(int turnNumber, Enemy enemy)
		{
			this.turnNumber = turnNumber;
			this.enemy = enemy;
		}
	}
}
