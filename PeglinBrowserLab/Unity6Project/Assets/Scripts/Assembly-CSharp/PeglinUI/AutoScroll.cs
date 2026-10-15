using System.Collections;
using NaughtyAttributes;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI
{
	public class AutoScroll : MonoBehaviour, IBeginDragHandler, IEventSystemHandler
	{
		public delegate void ScrollFinished();

		public static ScrollFinished OnScrollFinished = () =>
		{
		};

		public float startDelay = 2f;

		public float endDelay = 2f;

		public float scrollLength = 30f;

		public float controllerScrollSpeed = 1f;

		public bool allowPlayerScrolling = true;

		private ScrollRect _scrollRect;

		private bool _autoScrolling;

		private Player _player;

		private bool _forceStopAutoScrolling;

		[Button(null, EButtonEnableMode.Always)]
		public void EditorScrollToTop()
		{
			_scrollRect = GetComponent<ScrollRect>();
			_scrollRect.normalizedPosition = Vector2.up;
		}

		[Button(null, EButtonEnableMode.Always)]
		public void EditorScrollToBottom()
		{
			_scrollRect = GetComponent<ScrollRect>();
			_scrollRect.normalizedPosition = Vector2.zero;
		}

		private void OnEnable()
		{
			_player = ReInput.players.GetPlayer(0);
			_scrollRect = GetComponent<ScrollRect>();
			_scrollRect.normalizedPosition = Vector2.up;
			if (!allowPlayerScrolling)
			{
				_scrollRect.enabled = false;
			}
			_forceStopAutoScrolling = false;
			StartCoroutine(WaitThenStartAutoScrolling());
		}

		private IEnumerator WaitThenStartAutoScrolling()
		{
			yield return new WaitForSecondsRealtime(startDelay);
			if (!_forceStopAutoScrolling)
			{
				_scrollRect.normalizedPosition = Vector2.up;
				_autoScrolling = true;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (allowPlayerScrolling)
			{
				StopAutoScrolling();
			}
		}

		private void OnDisable()
		{
			StopAutoScrolling();
		}

		private void StopAutoScrolling()
		{
			_autoScrolling = false;
			_forceStopAutoScrolling = true;
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(WaitAndEmitScrollFinishedDelegate());
			}
		}

		private IEnumerator WaitAndEmitScrollFinishedDelegate()
		{
			yield return new WaitForSecondsRealtime(endDelay);
			OnScrollFinished();
		}

		private void Update()
		{
			if (_autoScrolling)
			{
				_scrollRect.verticalNormalizedPosition -= 1f / scrollLength * Time.unscaledDeltaTime;
				if (_scrollRect.verticalNormalizedPosition <= 0f)
				{
					StopAutoScrolling();
				}
			}
			if (!allowPlayerScrolling)
			{
				return;
			}
			float axis = _player.GetAxis("Vertical");
			if (Mathf.Abs(axis) > 0.05f)
			{
				if (_autoScrolling)
				{
					StopAutoScrolling();
				}
				float num = axis * controllerScrollSpeed * Time.deltaTime;
				Vector2 normalizedPosition = _scrollRect.normalizedPosition;
				normalizedPosition.y = Mathf.Clamp(normalizedPosition.y + num, 0f, 1f);
				_scrollRect.normalizedPosition = normalizedPosition;
			}
			if (Input.mouseScrollDelta.magnitude > 0.1f)
			{
				StopAutoScrolling();
			}
		}
	}
}
