using UnityEngine;
using UnityEngine.UI;

namespace Battle.StatusEffects
{
	public class FlashStatusEffectIcon : MonoBehaviour
	{
		public Image icon;

		private float flashTime = 0.1f;

		private float _elapsedFlashTime;

		private bool _flash;

		public Sprite statusEffectFlash;

		private Sprite _defaultSprite;

		private StatusEffectIcon _statusEffectIcon;

		private PlayerHealthController.PlayerHealthEvent _callback;

		private void OnEnable()
		{
			if (icon == null)
			{
				icon = GetComponent<Image>();
			}
		}

		private void OnDisable()
		{
			if (icon != null)
			{
				icon.sprite = _defaultSprite;
			}
			_flash = false;
		}

		private void Update()
		{
			if (_flash && _elapsedFlashTime < flashTime)
			{
				_elapsedFlashTime += Time.deltaTime;
				if (_elapsedFlashTime >= flashTime)
				{
					_flash = false;
					icon.sprite = _defaultSprite;
				}
			}
		}

		public void SetSprite(Sprite sprite)
		{
			_defaultSprite = sprite;
		}

		public void FlashWhite(float armourLost)
		{
			_flash = true;
			_elapsedFlashTime = 0f;
			if (icon != null)
			{
				icon.sprite = statusEffectFlash;
			}
		}
	}
}
