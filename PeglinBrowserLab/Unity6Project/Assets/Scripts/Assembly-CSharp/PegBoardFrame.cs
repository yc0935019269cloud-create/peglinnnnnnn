using Relics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PegBoardFrame : MonoBehaviour
{
	protected const float DEFAULT_LOST_BALL_Y = 5.5f;

	protected const float CORE_LOST_BALL_Y = 6f;

	public static float lostBallYThresholdPos = 5.5f;

	[SerializeField]
	private RelicManager _relicManager;

	public TilemapCollider2D wallsCollider;

	public PhysicsMaterial2D bouncyMaterial;

	private void Awake()
	{
		if (_relicManager == null)
		{
			Debug.LogWarning("Peg board frame does not have _relicManager refrence set.");
		}
		if (wallsCollider == null)
		{
			Debug.LogWarning("Peg board frame does not have wallsCollider refrence set.");
		}
		if (bouncyMaterial == null)
		{
			Debug.LogWarning("Peg board frame does not have bouncyMaterial refrence set.");
		}
	}

	protected virtual void OnEnable()
	{
		lostBallYThresholdPos = 5.5f;
		if (!(_relicManager == null) && !(wallsCollider == null) && !(bouncyMaterial == null) && _relicManager.AttemptUseRelic(RelicEffect.BOUNCY_WALLS_AND_BOUNCERS))
		{
			wallsCollider.sharedMaterial = bouncyMaterial;
		}
	}
}
