using PeglinUI.ControllerSupport;
using Rewired;
using Rewired.Integration.UnityUI;
using Seeding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedSymbolPicker : MonoBehaviour
{
	public Image symbol;

	public GameObject selectedIndicator;

	public SeedSymbols seedSymbolsImageData;

	public int currentSeedSymbolIndex;

	private bool _inputLock;

	private EventSystem _eventSystem;

	private Player _player;

	private void OnEnable()
	{
		if (_eventSystem == null)
		{
			_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		}
		if (_player == null)
		{
			_player = ReInput.players.GetPlayer(0);
		}
	}

	private void Update()
	{
		if (!ControllerInputManager.IsPointerBased)
		{
			selectedIndicator.SetActive(_eventSystem.currentSelectedGameObject == base.gameObject);
		}
		else
		{
			selectedIndicator.SetActive(value: false);
		}
		float axis = _player.GetAxis("UIVertical");
		if (Mathf.Abs(axis) > 0.05f && _eventSystem.currentSelectedGameObject == base.gameObject)
		{
			if (!_inputLock)
			{
				if (axis > 0.05f)
				{
					IncrementSymbol();
				}
				else
				{
					DecrementSymbol();
				}
				_inputLock = true;
			}
		}
		else
		{
			_inputLock = false;
		}
	}

	public void SetSymbolIndex(int newIndex)
	{
		currentSeedSymbolIndex = newIndex;
		symbol.sprite = seedSymbolsImageData.seedSymbols[currentSeedSymbolIndex];
	}

	public void IncrementSymbol()
	{
		currentSeedSymbolIndex++;
		currentSeedSymbolIndex %= 9;
		symbol.sprite = seedSymbolsImageData.seedSymbols[currentSeedSymbolIndex];
	}

	public void DecrementSymbol()
	{
		currentSeedSymbolIndex += 8;
		currentSeedSymbolIndex %= 9;
		symbol.sprite = seedSymbolsImageData.seedSymbols[currentSeedSymbolIndex];
	}
}
