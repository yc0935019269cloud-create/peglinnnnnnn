using UnityEngine;
using UnityEngine.Serialization;

namespace Battle.Attacks
{
	public class TargetedSpellWithStartupBehavior : SpellBehavior
	{
		[FormerlySerializedAs("ballanxLobPrefab")]
		public GameObject startupAnimationPrefab;

		public bool zeroVertical;

		private Animator _animator;

		protected void Start()
		{
			_animator = GetComponent<Animator>();
			Object.Instantiate(startupAnimationPrefab).GetComponentInChildren<TargetedSpellStartup>().maxPointReached.AddListener(SpawnAnimation);
		}

		private void SpawnAnimation()
		{
			float y = (zeroVertical ? 0f : base.transform.position.y);
			base.transform.position = new Vector3(base.transform.position.x, y, base.transform.position.z);
			_animator.enabled = true;
		}
	}
}
