using UnityEngine;

public abstract class MapData : ScriptableObject
{
	public GameObject backgroundOverride;

	public GameObject pegboardFrameOverride;

	private GameObject assignedBackground;

	private GameObject assignedPegboardFrame;

	[SerializeField]
	protected AudioClip _musicOverride;

	[SerializeField]
	protected string _sceneOverride;

	[Tooltip("If true, this mapdata will be repeatable.")]
	public bool ignoreRemove;

	public GameObject background
	{
		get
		{
			if (backgroundOverride != null)
			{
				return backgroundOverride;
			}
			return assignedBackground;
		}
		set
		{
			assignedBackground = value;
		}
	}

	public GameObject pegboardFrame
	{
		get
		{
			if (pegboardFrameOverride != null)
			{
				return pegboardFrameOverride;
			}
			return assignedPegboardFrame;
		}
		set
		{
			assignedPegboardFrame = value;
		}
	}

	public AudioClip GetMusicOverride()
	{
		return _musicOverride;
	}

	public string GetSceneOverride()
	{
		return _sceneOverride;
	}

	public abstract SceneType GetSceneType();
}
