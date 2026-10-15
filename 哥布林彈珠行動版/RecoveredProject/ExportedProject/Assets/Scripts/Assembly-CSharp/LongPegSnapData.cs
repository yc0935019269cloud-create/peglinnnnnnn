using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Long Peg Snap Data", menuName = "Long Peg Snap Data")]
public class LongPegSnapData : ScriptableObject
{
	[Flags]
	public enum LongPegSlideSide
	{
		NONE = 0,
		INSIDE_OF_CURVE = 1,
		OUTSIDE_OF_CURVE = 2
	}

	public LongPegSlideSide slideSide;

	[Range(0f, 180f)]
	public float maxSnapAngle = 30f;
}
