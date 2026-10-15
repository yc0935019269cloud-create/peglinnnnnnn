using System.Collections;
using PeglinUtils;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map.MinimapView
{
	public class MapPreviewOpenClose : MonoBehaviour
	{
		public delegate void MapPreviewToggle(bool open);

		public static MapPreviewToggle onMapPreviewToggled = (bool open) =>
		{
		};

		public GameObject mapContents;

		public GameObject mapPreviewBackground;

		public GameObject closeButton;

		public static bool isOpen = false;

		public static bool wasOpenThisFrame = false;

		private RewiredEventSystem _rewiredEventSystem;

		private GameObject _previouslySelected;

		private void OnEnable()
		{
			SceneManager.sceneLoaded += CloseOnSceneLoaded;
		}

		private void OnDisable()
		{
			SceneManager.sceneLoaded -= CloseOnSceneLoaded;
			isOpen = false;
			wasOpenThisFrame = false;
		}

		public void CloseButtonPressed()
		{
			MapPreviewCameraMovement component = Camera.main.GetComponent<MapPreviewCameraMovement>();
			if (component != null)
			{
				component.MapButtonPressed();
			}
		}

		public void Open()
		{
			if (!(mapContents == null))
			{
				isOpen = true;
				wasOpenThisFrame = true;
				_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
				_previouslySelected = _rewiredEventSystem.currentSelectedGameObject;
				_rewiredEventSystem.SetSelectedGameObject(closeButton);
				mapContents.SetActive(value: true);
				mapPreviewBackground.SetActive(value: true);
				onMapPreviewToggled?.Invoke(isOpen);
			}
		}

		public void Close(bool instant = false)
		{
			isOpen = false;
			onMapPreviewToggled?.Invoke(isOpen);
			StartCoroutine(MarkClosedAtEndOfFrame());
			mapContents.SetActive(value: false);
			mapPreviewBackground.SetActive(value: false);
		}

		private void CloseOnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
		{
			CloseMapUI();
		}

		private IEnumerator MarkClosedAtEndOfFrame()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			wasOpenThisFrame = false;
			if (_previouslySelected != null)
			{
				_rewiredEventSystem.SetSelectedGameObject(_previouslySelected);
			}
		}

		public void CloseMapUI()
		{
			isOpen = false;
			mapPreviewBackground.SetActive(value: false);
		}
	}
}
