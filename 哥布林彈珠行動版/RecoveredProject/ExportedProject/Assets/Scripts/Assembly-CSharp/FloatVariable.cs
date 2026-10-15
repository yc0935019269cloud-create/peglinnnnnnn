using System;
using UnityEngine;

[CreateAssetMenu]
public class FloatVariable : ScriptableObject
{
	public delegate void ValueChanged(float newVal, float oldVal);

	public ValueChanged onValueChanged;

	[SerializeField]
	private float _initialValue;

	[Tooltip("Used if you want a flexible max value instead of a static limit, like the ability to increase max HP.")]
	[SerializeField]
	private FloatVariable _maxVariable;

	public float max = float.MaxValue;

	public float min;

	[NonSerialized]
	private float _value;

	public float Value => _value;

	public float Add(float val)
	{
		float value = _value;
		float num = ((_maxVariable == null) ? max : _maxVariable.Value);
		_value = Mathf.Clamp(_value + val, min, num);
		onValueChanged?.Invoke(_value, value);
		return _value;
	}

	public void AddVoid(float val)
	{
		Add(val);
	}

	public float Subtract(float val)
	{
		return Add(0f - val);
	}

	public void Reset()
	{
		_value = _initialValue;
	}

	public void Set(float value)
	{
		float num = ((_maxVariable == null) ? float.MaxValue : _maxVariable.Value);
		_value = Mathf.Clamp(value, min, num);
	}

	public void SetInitialValue(float val)
	{
		float num = ((_maxVariable == null) ? float.MaxValue : _maxVariable.Value);
		_initialValue = Mathf.Clamp(val, min, num);
	}
}
