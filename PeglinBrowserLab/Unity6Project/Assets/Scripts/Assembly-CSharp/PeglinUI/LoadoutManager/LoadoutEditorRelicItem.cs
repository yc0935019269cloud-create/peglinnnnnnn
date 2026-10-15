using System.Collections;
using I2.Loc;
using PeglinUI.ControllerSupport;
using PeglinUtils;
using Relics;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutEditorRelicItem : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public delegate void DeleteButtonClicked(LoadoutEditorRelicItem relicItem, Relic relic);

		public delegate void RelicItemIsSelected(bool selection);

		public DeleteButtonClicked OnDeleteButtonClicked;

		public static RelicItemIsSelected OnRelicItemIsSelected;

		public Image iconImage;

		public TextMeshProUGUI nameText;

		public Button deleteButton;

		public GameObject deleteContextButton;

		public LoadoutEditorShowTooltip tooltipDetector;

		private Relic _relic;

		private Player _player;

		private bool _listeningForControllerInput;

		private bool _tooltipShown;

		public void Initialize(Relic relic)
		{
			iconImage.sprite = relic.sprite;
			nameText.text = LocalizationManager.GetTranslation(relic.nameKey);
			_relic = relic;
			_player = ReInput.players.GetPlayer(0);
			tooltipDetector.Initialize(_relic);
		}

		private void Update()
		{
			if (!_listeningForControllerInput)
			{
				return;
			}
			if (_player.GetButtonUp("CatalogDeleteOrb"))
			{
				StartCoroutine(DeleteRelicNextFrame());
			}
			else if (_player.GetButtonDown("MapButton"))
			{
				if (_tooltipShown)
				{
					TooltipManager.Instance.HideTooltip();
					_tooltipShown = false;
				}
				else
				{
					tooltipDetector.ShowTooltip();
					_tooltipShown = true;
				}
			}
		}

		private IEnumerator DeleteRelicNextFrame()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			DeleteRelic();
		}

		public void DeleteRelic()
		{
			OnDeleteButtonClicked?.Invoke(this, _relic);
			Object.Destroy(base.gameObject);
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!ControllerInputManager.IsPointerBased && deleteContextButton != null)
			{
				_listeningForControllerInput = true;
			}
			OnRelicItemIsSelected?.Invoke(selection: true);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			_listeningForControllerInput = false;
			if (!ControllerInputManager.IsPointerBased)
			{
				_ = deleteContextButton != null;
			}
			OnRelicItemIsSelected?.Invoke(selection: false);
		}
	}
}
