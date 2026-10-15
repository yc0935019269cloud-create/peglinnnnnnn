namespace Battle.Pachinko.BallBehaviours
{
	public class GrantStatusOnPegCoinHits : GrantStatusOnPegHits
	{
		public override void PegHit(Peg peg, PachinkoBall pBall)
		{
			for (int i = 0; i < peg.NumCoins(); i++)
			{
				base.PegHit(peg, pBall);
			}
		}
	}
}
