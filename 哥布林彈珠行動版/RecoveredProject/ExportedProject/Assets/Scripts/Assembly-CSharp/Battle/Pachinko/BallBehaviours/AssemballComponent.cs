using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class AssemballComponent : MonoBehaviour
	{
		public AssemballType type;

		public static readonly int ASSEMBALL_NORMAL_DMG = 4;

		public static readonly int ASSEMBALL_CRIT_DMG = 8;

		public static readonly int SLIME_X_PEGS_HIT = 4;

		public static readonly int BUFF_AMOUNT = 3;

		public static readonly int FULLY_FORMED_DMG = 2;

		public static readonly int FULLY_FORMED_CRIT = 4;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			int aSSEMBALL_NORMAL_DMG = ASSEMBALL_NORMAL_DMG;
			component.SetParameterValue("ASSEMBALL_NORMAL_DMG", aSSEMBALL_NORMAL_DMG.ToString());
			aSSEMBALL_NORMAL_DMG = ASSEMBALL_CRIT_DMG;
			component.SetParameterValue("ASSEMBALL_CRIT_DMG", aSSEMBALL_NORMAL_DMG.ToString());
			aSSEMBALL_NORMAL_DMG = SLIME_X_PEGS_HIT;
			component.SetParameterValue("SLIME_X_PEGS_HIT", aSSEMBALL_NORMAL_DMG.ToString());
			aSSEMBALL_NORMAL_DMG = BUFF_AMOUNT;
			component.SetParameterValue("BUFF_AMOUNT", aSSEMBALL_NORMAL_DMG.ToString());
		}

		public static AssemballType GetTypeForIndex(int i)
		{
			return i switch
			{
				0 => AssemballType.RED, 
				1 => AssemballType.YELLOW, 
				2 => AssemballType.WHITE, 
				3 => AssemballType.GREEN, 
				4 => AssemballType.PINK, 
				_ => AssemballType.NONE, 
			};
		}

		public static int GetIndexForType(AssemballType type)
		{
			return type switch
			{
				AssemballType.RED => 0, 
				AssemballType.YELLOW => 1, 
				AssemballType.WHITE => 2, 
				AssemballType.GREEN => 3, 
				AssemballType.PINK => 4, 
				_ => -1, 
			};
		}
	}
}
