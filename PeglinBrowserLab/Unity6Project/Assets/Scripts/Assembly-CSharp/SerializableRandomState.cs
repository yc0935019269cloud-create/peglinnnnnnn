using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableRandomState
{
	public int initializationSeed;

	public int timesUsed;

	private UnityEngine.Random.State _randomState;

	public UnityEngine.Random.State randomState => _randomState;

	public SerializableRandomState(int InitializationSeed, int TimesUsed)
	{
		initializationSeed = InitializationSeed;
		timesUsed = TimesUsed;
		InitState();
	}

	private void InitState()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(initializationSeed);
		for (int i = 0; i < timesUsed; i++)
		{
			_ = UnityEngine.Random.value;
		}
		_randomState = UnityEngine.Random.state;
		UnityEngine.Random.state = state;
	}

	private void InitializeIfNeeded()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.State state2 = _randomState;
		UnityEngine.Random.state = _randomState;
		bool flag = true;
		for (int i = 0; i < 10; i++)
		{
			if ((double)UnityEngine.Random.value > 1E-05)
			{
				flag = false;
				break;
			}
		}
		_randomState = state2;
		if (flag)
		{
			Debug.LogWarning("SerializableRandomState not properly initialized. This class should always be made new and never directly set from a deserialized value. Fixing...");
			InitState();
		}
		UnityEngine.Random.state = state;
	}

	public float GetRandomValue()
	{
		InitializeIfNeeded();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.state = _randomState;
		float value = UnityEngine.Random.value;
		_randomState = UnityEngine.Random.state;
		timesUsed++;
		UnityEngine.Random.state = state;
		return value;
	}

	public int GetRandomRangeValue(int min, int max)
	{
		InitializeIfNeeded();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.state = _randomState;
		int result = UnityEngine.Random.Range(min, max);
		_randomState = UnityEngine.Random.state;
		timesUsed++;
		UnityEngine.Random.state = state;
		return result;
	}

	public void SeededShuffle<T>(List<T> list)
	{
		InitializeIfNeeded();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.state = _randomState;
		for (int i = 0; i < list.Count; i++)
		{
			int num = UnityEngine.Random.Range(i, list.Count);
			timesUsed++;
			int index = num;
			int index2 = i;
			T val = list[i];
			T val2 = list[num];
			T val3 = (list[index] = val);
			val3 = (list[index2] = val2);
		}
		_randomState = UnityEngine.Random.state;
		UnityEngine.Random.state = state;
	}
}
