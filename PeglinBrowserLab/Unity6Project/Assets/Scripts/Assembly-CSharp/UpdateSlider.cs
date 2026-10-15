using System.Collections;
using PeglinUI;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSlider : MonoBehaviour
{
	public float SliderLerpTime = 0.5f;

	public float SliderFlashTime = 0.2f;

	[Tooltip("This is only used for flashing the colour of the bar, not for the actual fill (the slider is used for that)")]
	[SerializeField]
	private Image _fillImage;

	[SerializeField]
	private Transform _selfDamagePreview;

	[SerializeField]
	private Transform _backgroundRect;

	[SerializeField]
	private Transform _skull;

	[SerializeField]
	private Sprite _c19Background;

	[SerializeField]
	private Image _background;

	public RectTransform maxHPReductionRect;

	private float _initialValue = 1f;

	private float _targetValue = 1f;

	private float _currentValue = 1f;

	private float _currentMaxHPReductionValue;

	private float _targetMaxHPReductionValue;

	private float _valueMoveVel;

	private float _valueMoveVel2;

	private float _maxHealthWidth;

	private RectTransform _selfDamageRect;

	private Color _initialColour = Color.red;

	private Slider _slider;

	private bool _colorSet;

	private void OnEnable()
	{
		if (_slider == null)
		{
			_slider = GetComponent<Slider>();
		}
		_initialValue = 1f;
		if (!_colorSet)
		{
			_initialColour = _fillImage.color;
		}
		else
		{
			_fillImage.color = _initialColour;
		}
		_colorSet = true;
		if (_backgroundRect != null)
		{
			_maxHealthWidth = _backgroundRect.GetComponent<RectTransform>().rect.width;
		}
		if (_selfDamagePreview != null)
		{
			_selfDamageRect = _selfDamagePreview.GetComponent<RectTransform>();
		}
	}

	private void Update()
	{
		if (_currentValue != _targetValue)
		{
			_currentValue = Mathf.SmoothDamp(_currentValue, _targetValue, ref _valueMoveVel, SliderLerpTime);
			_slider.value = _currentValue;
		}
		if (_currentMaxHPReductionValue != _targetMaxHPReductionValue)
		{
			_currentMaxHPReductionValue = Mathf.SmoothDamp(_currentMaxHPReductionValue, _targetMaxHPReductionValue, ref _valueMoveVel2, SliderLerpTime);
			float x = _currentMaxHPReductionValue * _maxHealthWidth;
			maxHPReductionRect.sizeDelta = new Vector2(x, maxHPReductionRect.sizeDelta.y);
		}
	}

	public void UpdateSize(float currentHealth, float maxHealth, float reductionToMaxHP = 0f, bool maxZero = false)
	{
		if (base.gameObject != null && base.gameObject.activeInHierarchy)
		{
			float num = ((!(maxHealth + reductionToMaxHP <= 0f)) ? (currentHealth / (maxHealth + reductionToMaxHP)) : 0f);
			if (maxZero)
			{
				num = Mathf.Max(num, 0f);
			}
			_targetValue = _initialValue * num;
			if (maxHealth + reductionToMaxHP > 0f)
			{
				_targetMaxHPReductionValue = reductionToMaxHP / (maxHealth + reductionToMaxHP);
			}
			if (_colorSet)
			{
				StartCoroutine(FlashWhite());
			}
		}
	}

	public void ShowSelfDamagePreview(float selfDamage, float maxHealth, bool willKill)
	{
		float x = selfDamage / (maxHealth - _targetMaxHPReductionValue) * _maxHealthWidth;
		_selfDamageRect.sizeDelta = new Vector2(x, _selfDamageRect.sizeDelta.y);
		_selfDamagePreview.gameObject.GetComponent<ContinuousFlash>().StartFlash();
		if (willKill)
		{
			_skull.gameObject.SetActive(value: true);
			_skull.gameObject.GetComponent<ContinuousFlash>().StartFlash();
		}
	}

	public void HideSelfDamagePreview()
	{
		_selfDamageRect.sizeDelta = new Vector2(0f, _selfDamageRect.sizeDelta.y);
		if (_selfDamagePreview != null)
		{
			_selfDamagePreview.gameObject.GetComponent<ContinuousFlash>().StopFlash();
		}
		if (_skull != null)
		{
			_skull.gameObject.GetComponent<ContinuousFlash>().StopFlash();
			_skull.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator FlashWhite()
	{
		_fillImage.color = Color.white;
		yield return new WaitForSeconds(SliderFlashTime);
		_fillImage.color = _initialColour;
	}

	public void SetIsC19ExtraEnemy()
	{
		_background.sprite = _c19Background;
	}
}
