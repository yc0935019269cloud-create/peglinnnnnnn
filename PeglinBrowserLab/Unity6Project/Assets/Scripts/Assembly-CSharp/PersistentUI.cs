using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentUI : MonoBehaviour
{
	public List<string> blacklistedScenes = new List<string>();

	private Canvas _canvas;

	private void Awake()
	{
		if (GameObject.FindGameObjectsWithTag(base.gameObject.tag).Length > 1)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_canvas = GetComponent<Canvas>();
		Object.DontDestroyOnLoad(this);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (_canvas.worldCamera == null)
		{
			_canvas.worldCamera = Camera.main;
		}
		base.gameObject.SetActive(value: true);
		if (blacklistedScenes.Contains(scene.name))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
