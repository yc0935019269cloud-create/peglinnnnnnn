using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class BarkOnIdle : BarkStarter
	{
		[Tooltip("Minimum seconds between barks.")]
		public float minSeconds = 5f;

		[Tooltip("Maximum seconds between barks.")]
		public float maxSeconds = 10f;

		[Tooltip("Target to whom bark is addressed. Leave unassigned to just bark into the air.")]
		public Transform target;

		private bool started;

		protected override bool useOnce => false;

		protected override void Start()
		{
			base.Start();
			started = true;
			StartBarkLoop();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartBarkLoop();
		}

		public void StartBarkLoop()
		{
			if (started)
			{
				StopAllCoroutines();
				StartCoroutine(BarkLoop());
			}
		}

		private IEnumerator BarkLoop()
		{
			while (true)
			{
				yield return new WaitForSeconds(Random.Range(minSeconds, maxSeconds));
				if (base.enabled && (!DialogueManager.isConversationActive || allowDuringConversations) && !DialogueTime.isPaused)
				{
					TryBark(target);
				}
			}
		}
	}
}
