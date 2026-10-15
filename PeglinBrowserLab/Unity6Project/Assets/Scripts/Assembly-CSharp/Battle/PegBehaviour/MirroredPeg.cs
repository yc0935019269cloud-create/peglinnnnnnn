using System;
using NaughtyAttributes;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class MirroredPeg : MonoBehaviour
	{
		public Vector2 mirrorVector = Vector2.left;

		[ReadOnly]
		public MirroredPeg mirrorPair;

		private Peg _peg;

		private bool _hit;

		private void OnEnable()
		{
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Combine(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(HandleRefreshPegHit));
			Peg.OnPegHit = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegHit, new Peg.PegHitEvent(CheckForPegHit));
		}

		private void OnDisable()
		{
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Remove(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(HandleRefreshPegHit));
			Peg.OnPegHit = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegHit, new Peg.PegHitEvent(CheckForPegHit));
		}

		private void Awake()
		{
			_peg = GetComponent<Peg>();
		}

		private void Start()
		{
			if (mirrorPair == null)
			{
				Vector2 vector = base.transform.position;
				if (mirrorVector.x < 0f)
				{
					vector.x *= -1f;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject, vector, Quaternion.identity);
				gameObject.transform.parent = base.transform.parent;
				mirrorPair = gameObject.GetComponent<MirroredPeg>();
				mirrorPair.mirrorPair = this;
				IDummiableMovingPeg component = gameObject.GetComponent<IDummiableMovingPeg>();
				if (component != null)
				{
					component.GenerateId(force: true);
				}
				else
				{
					gameObject.GetComponent<IDummyPeg>()?.GenerateId(force: true);
				}
				Peg componentInChildren = gameObject.GetComponentInChildren<Peg>();
				Peg.OnPegAdded?.Invoke(componentInChildren.gameObject);
			}
		}

		private void CheckForPegHit(Peg.PegType pegType, Peg peg)
		{
			if (peg == _peg && pegType != Peg.PegType.RESET)
			{
				_hit = true;
				mirrorPair.ActivateMirroredPeg();
			}
		}

		public void ActivateMirroredPeg()
		{
			if (!_hit && !_peg.IsDisabled())
			{
				_peg.PegActivated();
			}
		}

		private void HandleRefreshPegHit()
		{
			_hit = false;
		}

		private void LateUpdate()
		{
			_hit = false;
		}

		private void OnDrawGizmos()
		{
			Vector2 vector = base.transform.position;
			if (mirrorVector.x < 0f)
			{
				vector.x *= -1f;
			}
			Gizmos.DrawSphere(vector, 0.2f);
		}
	}
}
