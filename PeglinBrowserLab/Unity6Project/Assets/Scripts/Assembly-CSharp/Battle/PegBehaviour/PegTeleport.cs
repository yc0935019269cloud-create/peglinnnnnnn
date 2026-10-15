using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NaughtyAttributes;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegTeleport : MonoBehaviour, IDummiableMovingPeg, IDummyPeg
	{
		[Tooltip("The percent chance this will teleport")]
		[Range(0f, 1f)]
		public float percentMovement = 0.5f;

		public float disappearTime = 0.35f;

		[Tooltip("What will this scale down to when disappearing? Can be both for a shrinking effect or just vert/horizfor more of a mirror teleport effect.")]
		[Dropdown("GetDisappearVectors")]
		public Vector2 disappearVector;

		[HideInInspector]
		public string id = string.Empty;

		private bool _isDummy;

		[Tooltip("Will move this to position + offset")]
		public Vector2 teleportOffset;

		private Vector2 _initialPos;

		private Vector3 _initialScale;

		private bool _atInitial = true;

		private DropdownList<Vector2> GetDisappearVectors()
		{
			return new DropdownList<Vector2>
			{
				{
					"Horizontal",
					Vector2.up
				},
				{
					"Vertical",
					Vector2.right
				},
				{
					"Shrink",
					Vector2.zero
				}
			};
		}

		private void Start()
		{
			_initialPos = base.transform.position;
			_initialScale = base.transform.localScale;
			if (!_isDummy && id.IsNullOrWhitespace())
			{
				id = Guid.NewGuid().ToString();
			}
		}

		private void OnEnable()
		{
			if (!_isDummy)
			{
				BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(StartTeleport));
			}
		}

		private void OnDisable()
		{
			if (!_isDummy)
			{
				BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(StartTeleport));
			}
		}

		private void StartTeleport()
		{
			if (!_isDummy && UnityEngine.Random.value < percentMovement)
			{
				Vector2 teleportTarget;
				if (_atInitial)
				{
					teleportTarget = _initialPos + teleportOffset;
					_atInitial = false;
				}
				else
				{
					teleportTarget = _initialPos;
					_atInitial = true;
				}
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOScale(disappearVector, disappearTime);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
				{
					DisappearComplete(teleportTarget);
				}));
			}
		}

		private void DisappearComplete(Vector2 target)
		{
			base.transform.position = target;
			base.transform.DOScale(_initialScale, disappearTime);
		}

		private void OnDrawGizmos()
		{
			Peg[] componentsInChildren = GetComponentsInChildren<Peg>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Gizmos.DrawSphere(componentsInChildren[i].transform.position + (Vector3)teleportOffset, 0.2f);
			}
		}

		public void GenerateId(bool force = false)
		{
			if (id.IsNullOrWhitespace() | force)
			{
				id = Guid.NewGuid().ToString();
			}
		}

		public string GetId()
		{
			return id;
		}

		public void SetDummyStatus(bool status)
		{
			_isDummy = status;
		}

		public void SetPegStatus(bool status)
		{
		}

		public bool GetPegStatus()
		{
			return true;
		}

		public bool IsSpecialPeg()
		{
			return false;
		}

		public void SetSpecialCollider(bool value)
		{
		}

		public Peg.PegType GetPegType()
		{
			return Peg.PegType.NONE;
		}

		public void SetPegType(Peg.PegType pegType)
		{
		}

		public Vector3 GetPosition()
		{
			return base.transform.position;
		}

		public void SetPosition(Vector3 position)
		{
			if (_isDummy)
			{
				base.transform.position = position;
			}
		}

		public Vector3 GetScale()
		{
			return base.transform.localScale;
		}

		public void SetScale(Vector3 scale)
		{
			if (_isDummy)
			{
				base.transform.localScale = scale;
			}
		}

		public string GetObjectName()
		{
			return base.gameObject.name;
		}
	}
}
