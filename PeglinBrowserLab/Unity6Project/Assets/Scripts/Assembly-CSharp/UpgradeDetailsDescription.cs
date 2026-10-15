using I2.Loc;

public class UpgradeDetailsDescription : UpgradeDetails
{
	private LocalizationParamsManager[] locParamsManagers;

	public new void Initialize(string title, string before, string after, float beforeBaseToModRatio = 1f, float afterBaseToModRatio = 1f)
	{
		if (string.IsNullOrEmpty(before))
		{
			_beforeText.gameObject.SetActive(value: false);
		}
		if (string.IsNullOrEmpty(after) || before.Equals(after))
		{
			_afterText.gameObject.SetActive(value: false);
		}
		base.Initialize(title, before, after, 1f, 1f);
	}

	public void UpdateLocalizationParameters(string key, string value)
	{
		if (locParamsManagers == null)
		{
			locParamsManagers = GetComponentsInChildren<LocalizationParamsManager>();
		}
		LocalizationParamsManager[] array = locParamsManagers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetParameterValue(key, value);
		}
	}
}
