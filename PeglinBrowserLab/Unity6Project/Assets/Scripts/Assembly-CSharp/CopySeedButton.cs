using Peglin.OdinSerializer.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

public class CopySeedButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private bool _mouseHovering;

	public SeedDisplay seedDisplay;

	private AudioSource _audioSource;

	public bool mouseHovering => _mouseHovering;

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		base.gameObject.SetActive(value: false);
	}

	public void CopyCurrentSeedToClipboard()
	{
		if (seedDisplay != null && !seedDisplay.currentlyDisplayedSeed.IsNullOrWhitespace())
		{
			GUIUtility.systemCopyBuffer = seedDisplay.currentlyDisplayedSeed;
		}
		else
		{
			string text = "";
			string currentSeed = StaticGameData.currentSeed;
			for (int i = 0; i < currentSeed.Length; i++)
			{
				text += int.Parse(currentSeed[i].ToString()) + 1;
			}
			GUIUtility.systemCopyBuffer = text;
		}
		_audioSource.Play();
	}

	public void OnPointerEnter(PointerEventData ped)
	{
		_mouseHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_mouseHovering = false;
	}
}
