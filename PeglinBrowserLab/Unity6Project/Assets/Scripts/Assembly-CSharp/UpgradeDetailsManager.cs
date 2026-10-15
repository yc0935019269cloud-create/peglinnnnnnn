using System.Collections.Generic;
using System.Globalization;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Cruciball;
using I2.Loc;
using PeglinUI.PostBattle;
using Relics;
using TMPro;
using UnityEngine;

public class UpgradeDetailsManager : MonoBehaviour
{
	[Header("SO References")]
	[SerializeField]
	private CruciballManager _cruciballManager;

	[SerializeField]
	private DeckManager _deckManager;

	[SerializeField]
	private RelicManager _relicManager;

	[Header("Existing Elements")]
	[SerializeField]
	private TMP_Text _orbTitle;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject _upgradeDetailsPrefab;

	[SerializeField]
	private GameObject _upgradeDetailsDmgIconPrefab;

	[SerializeField]
	private GameObject _upgradeDetailsDmgIconNoUpgrade;

	[SerializeField]
	private GameObject _upgradeDetailsNoUpgradePossible;

	[SerializeField]
	private GameObject _upgradeDetailsDescriptionPrefab;

	private LocalizationParamsManager _localizationParamsManager;

	private List<Transform> _elementsToClear = new List<Transform>();

	private const string attackString = "Menu/attack_label";

	private const string critString = "Menu/critical_label";

	public void PopulateUpgradeDetails(Attack before, Attack after)
	{
		before.ClearBattleParameters();
		before.SoftInit(_deckManager, _relicManager, _cruciballManager);
		foreach (Transform item in _elementsToClear)
		{
			Object.Destroy(item.gameObject);
		}
		_elementsToClear.Clear();
		_orbTitle.text = before.Name;
		if (after == null)
		{
			CreateSimpleIconDetails(isCrit: false, before.GetModifiedDamagePerPeg().ToString(), before.GetDamageModMultiplier());
			CreateSimpleIconDetails(isCrit: true, before.GetModifiedDamagePerPeg(1).ToString(), before.GetDamageModMultiplier(1));
			CreateDescription(before.Description);
			return;
		}
		after.ClearBattleParameters();
		after.SoftInit(_deckManager, _relicManager, _cruciballManager);
		float num = after.GetModifiedDamagePerPeg();
		float num2 = after.GetModifiedDamagePerPeg(1);
		UnlimitedUpgrades component = before.GetComponent<UnlimitedUpgrades>();
		if (component != null)
		{
			num += (float)component.damageIncrease;
			num2 += (float)component.critIncrease;
		}
		if ((float)before.GetModifiedDamagePerPeg() < num)
		{
			CreateUpgradeIconDetails(isCrit: false, before.GetModifiedDamagePerPeg().ToString(CultureInfo.InvariantCulture), num.ToString(CultureInfo.InvariantCulture), before.GetDamageModMultiplier(), after.GetDamageModMultiplier());
		}
		else
		{
			CreateSimpleIconDetails(isCrit: false, before.GetModifiedDamagePerPeg().ToString(CultureInfo.InvariantCulture), before.GetDamageModMultiplier());
		}
		if ((float)before.GetModifiedDamagePerPeg(1) < num2)
		{
			CreateUpgradeIconDetails(isCrit: true, before.GetModifiedDamagePerPeg(1).ToString(CultureInfo.InvariantCulture), num2.ToString(CultureInfo.InvariantCulture), before.GetDamageModMultiplier(), after.GetDamageModMultiplier());
		}
		else
		{
			CreateSimpleIconDetails(isCrit: true, before.GetModifiedDamagePerPeg(1).ToString(CultureInfo.InvariantCulture), before.GetDamageModMultiplier(1));
		}
		CreateDescription(before.Description, after.Description);
	}

	public void PopulateSingleOrbDetails(Attack attack)
	{
		attack.ClearBattleParameters();
		attack.SoftInit(_deckManager, _relicManager, _cruciballManager);
		foreach (Transform item in _elementsToClear)
		{
			Object.Destroy(item.gameObject);
		}
		_elementsToClear.Clear();
		attack.ClearBattleParameters();
		_orbTitle.text = attack.Name;
		CreateSimpleIconDetails(isCrit: false, attack.GetModifiedDamagePerPeg().ToString());
		CreateSimpleIconDetails(isCrit: true, attack.GetModifiedDamagePerPeg(1).ToString());
		CreateDescription("", attack.Description);
	}

	public void PopulateRelicDetails(Relic relic)
	{
		UpdateLocManager();
		_orbTitle.text = LocalizationManager.GetTranslation(relic.nameKey);
		foreach (Transform item in _elementsToClear)
		{
			Object.Destroy(item.gameObject);
		}
		_elementsToClear.Clear();
		CreateDescriptionWithLocKey("", relic.descKey);
	}

	private void CreateUpgradeDetails(string title, string before, string after)
	{
		UpgradeDetails component = Object.Instantiate(_upgradeDetailsPrefab, base.transform).GetComponent<UpgradeDetails>();
		if (component != null)
		{
			component.Initialize(title, before, after);
			_elementsToClear.Add(component.transform);
		}
	}

	private void CreateSimpleDetails(string title, string stat, float modToBaseRatio = 1f)
	{
		UpgradeDetails component = Object.Instantiate(_upgradeDetailsNoUpgradePossible, base.transform).GetComponent<UpgradeDetails>();
		if (component != null)
		{
			component.Initialize(title, stat, "", modToBaseRatio);
			_elementsToClear.Add(component.transform);
		}
	}

	private void CreateSimpleIconDetails(bool isCrit, string stat, float modToBaseRatio = 1f)
	{
		UpgradeDetailsDamageIcon component = Object.Instantiate(_upgradeDetailsDmgIconNoUpgrade, base.transform).GetComponent<UpgradeDetailsDamageIcon>();
		if (component != null)
		{
			component.InitWithIcon(isCrit, stat, "", modToBaseRatio);
			_elementsToClear.Add(component.transform);
		}
	}

	private void CreateUpgradeIconDetails(bool isCrit, string before, string after, float beforeModToBaseRatio = 1f, float afterModToBaseRatio = 1f)
	{
		UpgradeDetailsDamageIcon component = Object.Instantiate(_upgradeDetailsDmgIconPrefab, base.transform).GetComponent<UpgradeDetailsDamageIcon>();
		if (component != null)
		{
			component.InitWithIcon(isCrit, before, after, beforeModToBaseRatio, afterModToBaseRatio);
			_elementsToClear.Add(component.transform);
		}
	}

	private void UpdateLocManager()
	{
		if (_localizationParamsManager == null)
		{
			_localizationParamsManager = GetComponent<LocalizationParamsManager>();
		}
		_localizationParamsManager.SetParameterValue("DECK_SIZE_HEAL_TOTAL", RelicManager.GetDeckSizeHealValue().ToString());
	}

	private void CreateDescription(string beforeDesc, string afterDesc = "")
	{
		UpgradeDetailsDescription component = Object.Instantiate(_upgradeDetailsDescriptionPrefab, base.transform).GetComponent<UpgradeDetailsDescription>();
		if (component != null)
		{
			component.Initialize("", beforeDesc, afterDesc);
			_elementsToClear.Add(component.transform);
		}
	}

	private void CreateDescriptionWithLocKey(string beforeLocKey, string afterLocKey = "")
	{
		UpgradeDetailsDescription component = Object.Instantiate(_upgradeDetailsDescriptionPrefab, base.transform).GetComponent<UpgradeDetailsDescription>();
		if (component != null)
		{
			component.UpdateLocalizationParameters("DECK_SIZE_HEAL_TOTAL", RelicManager.GetDeckSizeHealValue().ToString());
			component.InitializeWithLocKey("", beforeLocKey, afterLocKey);
			_elementsToClear.Add(component.transform);
		}
	}
}
