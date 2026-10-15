using DG.Tweening;
using I2.Loc;
using PeglinUtils;
using Rewired;
using TMPro;
using UnityEngine;

public class SeedInputPopup : MonoBehaviour
{
	public GameObject loadoutBasePanel;

	public SeedDisplay seedDisplay;

	public TMP_Text copyPasteText;

	public string copyKey;

	public string pasteKey;

	private SeedSymbolPicker[] seedSymbolPickers;

	private Player _player;

	private Tween copyPasteTextTween;

	public bool hasCustomSeed;

	private void Awake()
	{
		seedSymbolPickers = GetComponentsInChildren<SeedSymbolPicker>();
		_player = ReInput.players.GetPlayer(0);
	}

	private void OnEnable()
	{
		if (StaticGameData.currentSeed != string.Empty)
		{
			for (int i = 0; i < seedSymbolPickers.Length; i++)
			{
				seedSymbolPickers[i].SetSymbolIndex(int.Parse(StaticGameData.currentSeed.Substring(i, 1)));
			}
		}
		else
		{
			for (int j = 0; j < seedSymbolPickers.Length; j++)
			{
				seedSymbolPickers[j].SetSymbolIndex(0);
			}
		}
	}

	private void Update()
	{
		if ((_player.GetButtonDown("PasteSeed") && Application.platform != RuntimePlatform.OSXPlayer) || (_player.GetButtonDown("PasteSeedOSX") && Application.platform == RuntimePlatform.OSXPlayer))
		{
			string systemCopyBuffer = GUIUtility.systemCopyBuffer;
			if (SeedUtils.IsStringAValidSeed(systemCopyBuffer))
			{
				for (int i = 0; i < seedSymbolPickers.Length; i++)
				{
					seedSymbolPickers[i].SetSymbolIndex(int.Parse(systemCopyBuffer.Substring(i, 1)) - 1);
				}
				copyPasteText.text = LocalizationManager.GetTranslation(pasteKey);
				ShowCopyPasteSeedText();
			}
		}
		if ((_player.GetButtonDown("CopySeed") && Application.platform != RuntimePlatform.OSXPlayer) || (_player.GetButtonDown("CopySeedOSX") && Application.platform == RuntimePlatform.OSXPlayer))
		{
			string text = "";
			SeedSymbolPicker[] array = seedSymbolPickers;
			foreach (SeedSymbolPicker seedSymbolPicker in array)
			{
				text += seedSymbolPicker.currentSeedSymbolIndex + 1;
			}
			GUIUtility.systemCopyBuffer = text;
			copyPasteText.text = LocalizationManager.GetTranslation(copyKey);
			ShowCopyPasteSeedText();
		}
	}

	private void ShowCopyPasteSeedText()
	{
		copyPasteTextTween.Kill();
		copyPasteText.color = Color.white;
		copyPasteTextTween = copyPasteText.DOColor(new Color(1f, 1f, 1f, 0f), 4f).SetEase(Ease.InSine).SetDelay(1f);
	}

	public void SetSeed()
	{
		string text = "";
		for (int i = 0; i < seedSymbolPickers.Length; i++)
		{
			text += seedSymbolPickers[i].currentSeedSymbolIndex;
		}
		StaticGameData.currentSeed = text;
		hasCustomSeed = true;
		ReturnToCustomStartMenu();
	}

	public void ResetSeed()
	{
		StaticGameData.currentSeed = string.Empty;
		hasCustomSeed = false;
		ReturnToCustomStartMenu();
	}

	private void ReturnToCustomStartMenu()
	{
		base.gameObject.SetActive(value: false);
		loadoutBasePanel.SetActive(value: true);
		seedDisplay.gameObject.SetActive(StaticGameData.currentSeed != string.Empty);
		seedDisplay.UpdateSeedDisplay();
	}
}
