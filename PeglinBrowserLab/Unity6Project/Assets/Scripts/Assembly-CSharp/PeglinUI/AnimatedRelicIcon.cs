using UnityEngine;
using UnityEngine.Serialization;

namespace PeglinUI
{
	public class AnimatedRelicIcon : RelicIcon
	{
		[FormerlySerializedAs("ClockAnimations")]
		public Sprite[] RelicAnimations;

		private int _stateCounter;

		protected override void OnEnable()
		{
			base.OnEnable();
			_image.sprite = RelicAnimations[0];
		}

		public override void Flash()
		{
			_stateCounter = ((_stateCounter + 1 < RelicAnimations.Length) ? (_stateCounter + 1) : 0);
			_image.sprite = RelicAnimations[_stateCounter];
			base.Flash();
		}
	}
}
