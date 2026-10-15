using System;
using Map.MinimapView;
using UnityEngine;

namespace UI
{
	public class MapButton : MonoBehaviour
	{
		private Animator _anim;

		private static readonly int MapOpen = Animator.StringToHash("MapOpen");

		private void Awake()
		{
			_anim = GetComponentInChildren<Animator>();
		}

		private void OnEnable()
		{
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Combine(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(MapPreviewToggled));
		}

		private void OnDisable()
		{
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Remove(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(MapPreviewToggled));
		}

		private void MapPreviewToggled(bool open)
		{
			_anim.SetBool(MapOpen, open);
		}
	}
}
