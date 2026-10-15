using I2.Loc;
using TMPro;
using UnityEngine;

public class UpgradeDetails : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text _titleText;

	[SerializeField]
	protected TMP_Text _beforeText;

	[SerializeField]
	protected TMP_Text _afterText;

	[Header("Colours")]
	private Color regularColor = Color.white;

	[SerializeField]
	protected Color upgradedColor = Color.green;

	[SerializeField]
	protected Color downgradedColor = Color.red;

	private void Awake()
	{
		if (_beforeText != null)
		{
			regularColor = _beforeText.color;
		}
	}

	public void Initialize(string title, string before, string after, float beforeBaseToModRatio = 1f, float afterBaseToModRatio = 1f)
	{
		if (_titleText != null)
		{
			_titleText.text = title;
		}
		_beforeText.text = before;
		if (beforeBaseToModRatio > 1f)
		{
			_beforeText.color = upgradedColor;
		}
		else if (beforeBaseToModRatio < 1f)
		{
			_beforeText.color = downgradedColor;
		}
		else
		{
			_beforeText.color = regularColor;
		}
		_afterText.text = after;
	}

	public void InitializeWithLocKey(string titleLocKey, string beforeLocKey, string afterLocKey, float beforeBaseToModRatio = 1f, float afterBaseToModRatio = 1f)
	{
		if (_titleText != null && !string.IsNullOrEmpty(titleLocKey))
		{
			_titleText.GetComponent<Localize>().Term = titleLocKey;
		}
		else
		{
			_titleText.text = "";
		}
		if (!string.IsNullOrEmpty(beforeLocKey))
		{
			_beforeText.GetComponent<Localize>().Term = beforeLocKey;
		}
		else
		{
			_beforeText.text = "";
		}
		if (beforeBaseToModRatio > 1f)
		{
			_beforeText.color = upgradedColor;
		}
		else if (beforeBaseToModRatio < 1f)
		{
			_beforeText.color = downgradedColor;
		}
		else
		{
			_beforeText.color = regularColor;
		}
		if (!string.IsNullOrEmpty(afterLocKey))
		{
			_afterText.GetComponent<Localize>().GetComponent<Localize>().Term = afterLocKey;
		}
		else
		{
			_afterText.text = "";
		}
	}
}
