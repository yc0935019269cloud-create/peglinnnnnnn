using System;
using Battle.Pachinko.Obstacles;

namespace Battle.Enemies
{
	public class LeshyBoss : StationaryEnemy
	{
		public int damageFromDestroyedVine = 10;

		protected override void OnEnable()
		{
			base.OnEnable();
			PegBoardBramballVine.OnBramballVineDestroyed = (PegBoardBramballVine.BramballVineDestroyed)Delegate.Combine(PegBoardBramballVine.OnBramballVineDestroyed, new PegBoardBramballVine.BramballVineDestroyed(HandleVineDestroyed));
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			PegBoardBramballVine.OnBramballVineDestroyed = (PegBoardBramballVine.BramballVineDestroyed)Delegate.Remove(PegBoardBramballVine.OnBramballVineDestroyed, new PegBoardBramballVine.BramballVineDestroyed(HandleVineDestroyed));
		}

		private void HandleVineDestroyed(PegBoardBramballVine vine)
		{
			SimpleDamage(damageFromDestroyedVine);
		}
	}
}
