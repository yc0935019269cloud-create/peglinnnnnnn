using System.Collections;
using TMPro;
using UnityEngine;

public class Emoter : MonoBehaviour
{
	public enum Emote
	{
		NONE = 0,
		QUESTION = 1,
		EXCLAMATION = 2
	}

	public TMP_Text _emoteText;

	public float _exclaimHeight = 0.2f;

	private float _emoteStartHeight;

	private void Awake()
	{
		_emoteStartHeight = _emoteText.transform.position.y;
	}

	public void ShowEmote(Emote emoteType, float duration = -1f)
	{
		if (duration > 0f)
		{
			StartCoroutine(HideEmoteAfterTime(duration));
		}
		switch (emoteType)
		{
		case Emote.QUESTION:
			_emoteText.gameObject.SetActive(value: true);
			_emoteText.text = "?";
			break;
		case Emote.EXCLAMATION:
			_emoteText.gameObject.SetActive(value: true);
			_emoteText.text = "!";
			break;
		default:
			Debug.LogWarning("Unsupported Emote requested: " + emoteType);
			break;
		}
	}

	private IEnumerator HideEmoteAfterTime(float duration)
	{
		yield return new WaitForSeconds(duration);
		HideEmote();
	}

	public void HideEmote()
	{
		_emoteText.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (_emoteText.isActiveAndEnabled)
		{
			Vector3 position = _emoteText.transform.position;
			position.y = _emoteStartHeight + _exclaimHeight * Mathf.Sin(Time.time * 2f);
			_emoteText.transform.position = position;
		}
	}
}
