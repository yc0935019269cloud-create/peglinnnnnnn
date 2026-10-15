using UnityEngine;

public class MultiballsGetSmaller : MonoBehaviour
{
	public enum OrbSizes
	{
		BIG = 0,
		MEDIUM_BIG = 1,
		REGULAR = 2,
		MEDIUM_SMALL = 3,
		SMALL = 4,
		SMALLER = 5,
		SMALLEST = 6
	}

	public static readonly float[] SPRITE_SCALE_FOR_SIZES = new float[7] { 1f, 1f, 0.6f, 0.6f, 0.5f, 0.5f, 0.5f };

	public Sprite bigOrbSprite;

	public Sprite mediumBigSprite;

	public Sprite regularSizeSprite;

	public Sprite mediumSmallSprite;

	public Sprite smallOrbSprite;

	public Sprite smallerOrbSprite;

	public Sprite smallestOrbSprite;

	public Sprite[] spritesList => new Sprite[7] { bigOrbSprite, mediumBigSprite, regularSizeSprite, mediumSmallSprite, smallOrbSprite, smallerOrbSprite, smallestOrbSprite };
}
