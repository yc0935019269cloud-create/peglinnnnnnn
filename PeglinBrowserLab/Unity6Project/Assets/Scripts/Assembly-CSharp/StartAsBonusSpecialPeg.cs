using UnityEngine;

public class StartAsBonusSpecialPeg : MonoBehaviour
{
	private void Awake()
	{
		Peg component = GetComponent<Peg>();
		if (component != null && (component.pegType == Peg.PegType.CRIT || component.pegType == Peg.PegType.RESET))
		{
			component.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
		}
	}
}
