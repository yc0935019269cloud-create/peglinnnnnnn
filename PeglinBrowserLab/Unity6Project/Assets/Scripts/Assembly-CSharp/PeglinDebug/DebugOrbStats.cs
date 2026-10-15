using System.Text;
using Stats;
using TMPro;
using UnityEngine;

namespace PeglinDebug
{
	public class DebugOrbStats : MonoBehaviour
	{
		public TextMeshProUGUI orbId;

		public TextMeshProUGUI fired;

		public TextMeshProUGUI discarded;

		public TextMeshProUGUI removed;

		public TextMeshProUGUI inDeck;

		public TextMeshProUGUI levels;

		public void Init(RunStats.OrbPlayData orb)
		{
			orbId.text = orb.name.ToString();
			fired.text = orb.timesFired.ToString();
			discarded.text = orb.timesDiscarded.ToString();
			removed.text = orb.timesRemoved.ToString();
			inDeck.text = orb.amountInDeck.ToString();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 3; i++)
			{
				stringBuilder.Append((i < 2) ? (orb.levelInstances[i] + "/") : $"{orb.levelInstances[i]}");
			}
			levels.text = stringBuilder.ToString();
		}
	}
}
