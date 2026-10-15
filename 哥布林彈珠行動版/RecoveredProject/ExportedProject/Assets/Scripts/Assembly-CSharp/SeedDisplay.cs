using Peglin.OdinSerializer.Utilities;
using Seeding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public SeedSymbols seedSymbolsImageData;

	public Image[] seedSymbolImages;

	public GameObject copySeedButton;

	public float copySeedButtonDisapearAfterTime = 2f;

	private bool seedSet;

	private bool _canShowCopySeedButton;

	private float _copySeedButtonDisapearTimer = -1f;

	private CopySeedButton _copySeedButtonComponent;

	private bool _hovering;

	private string _currentlyDisplayedSeed;

	public string currentlyDisplayedSeed => _currentlyDisplayedSeed;

	private void Start()
	{
		if (copySeedButton == null)
		{
			_canShowCopySeedButton = false;
			return;
		}
		_canShowCopySeedButton = true;
		_copySeedButtonComponent = copySeedButton.GetComponent<CopySeedButton>();
	}

	private void OnEnable()
	{
		if (!seedSet)
		{
			UpdateSeedDisplay();
		}
	}

	private void Update()
	{
		if (!_canShowCopySeedButton)
		{
			return;
		}
		if (_copySeedButtonDisapearTimer > 0f)
		{
			_copySeedButtonDisapearTimer -= Time.unscaledDeltaTime;
		}
		if (_copySeedButtonDisapearTimer <= 0f && copySeedButton.activeSelf)
		{
			copySeedButton.SetActive(value: false);
		}
		if (_hovering || _copySeedButtonComponent.mouseHovering)
		{
			if (!copySeedButton.activeSelf)
			{
				copySeedButton.SetActive(value: true);
			}
			_copySeedButtonDisapearTimer = copySeedButtonDisapearAfterTime;
		}
	}

	public void UpdateSeedDisplay()
	{
		ViewSeedDisplay(StaticGameData.currentSeed);
	}

	public void ViewSeedDisplay(string seed)
	{
		if (!seed.IsNullOrWhitespace())
		{
			seedSet = true;
			_currentlyDisplayedSeed = "";
			for (int i = 0; i < seedSymbolImages.Length; i++)
			{
				int num = int.Parse(seed[i].ToString());
				seedSymbolImages[i].sprite = seedSymbolsImageData.seedSymbols[num];
				_currentlyDisplayedSeed += num + 1;
			}
		}
	}

	public void OnPointerEnter(PointerEventData ped)
	{
		_hovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hovering = false;
	}
}
