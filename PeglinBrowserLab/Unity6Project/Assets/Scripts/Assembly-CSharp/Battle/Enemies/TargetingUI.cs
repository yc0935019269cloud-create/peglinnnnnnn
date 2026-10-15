using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.Enemies
{
	public class TargetingUI : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
	{
		public bool ignoreAutoCorrectTargetingBox;

		private Enemy _enemy;

		public Enemy Enemy
		{
			get
			{
				if (_enemy != null)
				{
					return _enemy;
				}
				_enemy = FindEnemyInParent(base.transform);
				return _enemy;
			}
		}

		private void OnEnable()
		{
			if (!ignoreAutoCorrectTargetingBox)
			{
				BoxCollider2D component = GetComponent<BoxCollider2D>();
				RectTransform component2 = GetComponent<RectTransform>();
				if (component != null)
				{
					component.size = new Vector2(component2.rect.size.x * 0.6f, component2.rect.size.y * 0.75f);
					component.offset = new Vector2(0f, -0.3f);
				}
			}
		}

		private Enemy FindEnemyInParent(Transform t)
		{
			Enemy componentInParent = t.gameObject.GetComponentInParent<Enemy>();
			if (componentInParent == null && t.parent == null)
			{
				Debug.LogError("FindEnemyInParent(): No Enemy script found");
			}
			if (componentInParent != null || t.parent == null)
			{
				return componentInParent;
			}
			return FindEnemyInParent(t.parent);
		}

		public void SetTargeting(bool status)
		{
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(status);
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				Enemy.OnEnemyClicked(Enemy);
			}
		}

		public void OnMouseEnter()
		{
			Enemy.OnMouseHoverEnterEnemy?.Invoke();
		}

		public void OnMouseExit()
		{
			Enemy.OnMouseHoverExitEnemy?.Invoke();
		}
	}
}
