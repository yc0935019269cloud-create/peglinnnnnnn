using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
	public Button ApplyButton;

	public Button CancelButton;

	public Button AcceptButton;

	private void OnEnable()
	{
		ApplyButton.onClick.AddListener(() =>
		{
			Apply();
		});
		AcceptButton.onClick.AddListener(() =>
		{
			Apply();
		});
		CancelButton.onClick.AddListener(() =>
		{
			Revert();
		});
	}

	private void Apply()
	{
		SettingsManager.Instance.ApplySettings();
	}

	private void Revert()
	{
		SettingsManager.Instance.RevertSettings();
	}
}
