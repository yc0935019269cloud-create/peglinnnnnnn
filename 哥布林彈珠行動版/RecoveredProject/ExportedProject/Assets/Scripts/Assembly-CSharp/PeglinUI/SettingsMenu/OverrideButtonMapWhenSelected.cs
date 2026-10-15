using PeglinUI.SettingsMenu.Options;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.SettingsMenu
{
	public class OverrideButtonMapWhenSelected : MonoBehaviour
	{
		public RewiredUIButtonTrigger mapperToEnable;

		public RewiredUIButtonTrigger mapperToDisable;

		public ButtonGraphicMapper buttonGraphicMapperToEnable;

		public ButtonGraphicMapper buttonGraphicMapperToDisable;

		private Selectable _selectable;

		private void OnEnable()
		{
			if (_selectable == null)
			{
				_selectable = GetComponent<Selectable>();
			}
		}

		private void Update()
		{
			if (_selectable.gameObject == EventSystem.current.currentSelectedGameObject && !mapperToEnable.enabled)
			{
				mapperToEnable.enabled = true;
				buttonGraphicMapperToEnable?.gameObject.SetActive(value: true);
				mapperToDisable.enabled = false;
				buttonGraphicMapperToDisable?.gameObject.SetActive(value: false);
			}
			else if (EventSystem.current.currentSelectedGameObject != _selectable.gameObject && mapperToEnable.enabled)
			{
				mapperToEnable.enabled = false;
				buttonGraphicMapperToEnable?.gameObject.SetActive(value: false);
				mapperToDisable.enabled = true;
				buttonGraphicMapperToDisable?.gameObject.SetActive(value: true);
			}
		}
	}
}
