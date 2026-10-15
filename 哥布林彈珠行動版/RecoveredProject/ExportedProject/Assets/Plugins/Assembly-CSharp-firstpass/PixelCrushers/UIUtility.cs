using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers
{
	public static class UIUtility
	{
		public static void RequireEventSystem(string message = null)
		{
			if (Object.FindObjectOfType<EventSystem>() == null)
			{
				if (message != null)
				{
					Debug.LogWarning(message);
				}
				new GameObject("EventSystem").AddComponent<EventSystem>().gameObject.AddComponent<StandaloneInputModule>();
			}
		}

		public static int GetAnimatorNameHash(AnimatorStateInfo animatorStateInfo)
		{
			return animatorStateInfo.fullPathHash;
		}

		public static void Select(Selectable selectable, bool allowStealFocus = true)
		{
			EventSystem current = EventSystem.current;
			if (!(current == null) && !(selectable == null) && !current.alreadySelecting && ((current.currentSelectedGameObject == null) | allowStealFocus))
			{
				current.SetSelectedGameObject(selectable.gameObject);
				selectable.Select();
				selectable.OnSelect(null);
			}
		}
	}
}
