using System;
using Map;
using UnityEngine;

public class CastleTimeOfDayManager : MonoBehaviour
{
	public enum CastleTimeOfDay
	{
		DAY = 0,
		EVENING = 1,
		NIGHT = 2
	}

	public Sprite flatWindowDaySprite;

	public Sprite flatWindowEveningSprite;

	public Sprite flatWindowNightSprite;

	public Sprite angledWindowDaySprite;

	public Sprite angledWindowEveningSprite;

	public Sprite angledWindowNightSprite;

	public SpriteRenderer[] flatWindowRenderers;

	public SpriteRenderer[] angledWindowRenderers;

	public GameObject naturalLight;

	public Color shadowDayTint;

	public Color shadowEveningTint;

	public Color shadowNightTint;

	public SpriteRenderer shadowRenderer;

	public Color lightDayTint;

	public Color lightEveningTint;

	public Color lightNightTint;

	public SpriteRenderer lightRenderer;

	private int internalIndex;

	private void OnEnable()
	{
		MapController.OnMapReturnEvent = (MapController.MapReturnEvent)Delegate.Combine(MapController.OnMapReturnEvent, new MapController.MapReturnEvent(SetDaytime));
	}

	private void OnDisable()
	{
		MapController.OnMapReturnEvent = (MapController.MapReturnEvent)Delegate.Remove(MapController.OnMapReturnEvent, new MapController.MapReturnEvent(SetDaytime));
	}

	public void SetDaytime(int index)
	{
		Sprite sprite;
		Sprite sprite2;
		Color color;
		Color color2;
		switch ((CastleTimeOfDay)index)
		{
		case CastleTimeOfDay.EVENING:
			sprite = flatWindowEveningSprite;
			sprite2 = angledWindowEveningSprite;
			color = shadowEveningTint;
			color2 = lightEveningTint;
			naturalLight.SetActive(value: true);
			break;
		case CastleTimeOfDay.NIGHT:
			sprite = flatWindowNightSprite;
			sprite2 = angledWindowNightSprite;
			color = shadowNightTint;
			color2 = lightNightTint;
			naturalLight.SetActive(value: false);
			break;
		default:
			sprite = flatWindowDaySprite;
			sprite2 = angledWindowDaySprite;
			color = shadowDayTint;
			color2 = lightDayTint;
			naturalLight.SetActive(value: true);
			break;
		}
		SpriteRenderer[] array = flatWindowRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sprite = sprite;
		}
		array = angledWindowRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sprite = sprite2;
		}
		shadowRenderer.color = color;
		lightRenderer.color = color2;
	}
}
