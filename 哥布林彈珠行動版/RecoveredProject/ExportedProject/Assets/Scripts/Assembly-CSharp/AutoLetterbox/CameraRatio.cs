using System;
using UnityEngine;

namespace AutoLetterbox
{
	[Serializable]
	public class CameraRatio
	{
		public enum CameraAnchor
		{
			Center = 0,
			Top = 1,
			Bottom = 2,
			Left = 3,
			Right = 4,
			TopLeft = 5,
			TopRight = 6,
			BottomLeft = 7,
			BottomRight = 8
		}

		[Tooltip("The Camera assigned to have an automatically calculated Viewport Ratio")]
		public Camera camera;

		[Tooltip("When a Camera Viewport is shrunk to fit a ratio, it will anchor the new Viewport Rectangle at the given point (relative to the original, unshrunk Viewport)")]
		public CameraAnchor anchor;

		[HideInInspector]
		public Vector2 vectorAnchor;

		private Rect originViewPort;

		public CameraRatio(Camera _camera, Vector2 _anchor)
		{
			camera = _camera;
			vectorAnchor = _anchor;
			originViewPort = camera.rect;
		}

		public void ResetOriginViewport()
		{
			originViewPort = camera.rect;
			SetAnchorBasedOnEnum(anchor);
		}

		public void SetAnchorBasedOnEnum(CameraAnchor _anchor)
		{
			switch (_anchor)
			{
			case CameraAnchor.Center:
				vectorAnchor = new Vector2(0.5f, 0.5f);
				break;
			case CameraAnchor.Top:
				vectorAnchor = new Vector2(0.5f, 1f);
				break;
			case CameraAnchor.Bottom:
				vectorAnchor = new Vector2(0.5f, 0f);
				break;
			case CameraAnchor.Left:
				vectorAnchor = new Vector2(0f, 0.5f);
				break;
			case CameraAnchor.Right:
				vectorAnchor = new Vector2(1f, 0.5f);
				break;
			case CameraAnchor.TopLeft:
				vectorAnchor = new Vector2(0f, 1f);
				break;
			case CameraAnchor.TopRight:
				vectorAnchor = new Vector2(1f, 1f);
				break;
			case CameraAnchor.BottomLeft:
				vectorAnchor = new Vector2(0f, 0f);
				break;
			case CameraAnchor.BottomRight:
				vectorAnchor = new Vector2(1f, 0f);
				break;
			}
		}

		public void CalculateAndSetCameraRatio(float _width, float _height, bool _horizontalLetterbox)
		{
			Rect rect = default;
			if (_horizontalLetterbox)
			{
				rect.height = _height;
				rect.width = 1f;
			}
			else
			{
				rect.height = 1f;
				rect.width = _width;
			}
			Rect rect2 = default;
			Rect rect3 = default;
			rect2.width = originViewPort.width;
			rect2.height = originViewPort.width * (rect.height / rect.width);
			rect2.x = originViewPort.x;
			rect2.y = Mathf.Lerp(originViewPort.y, originViewPort.y + (originViewPort.height - rect2.height), vectorAnchor.y);
			rect3.width = originViewPort.height * (rect.width / rect.height);
			rect3.height = originViewPort.height;
			rect3.x = Mathf.Lerp(originViewPort.x, originViewPort.x + (originViewPort.width - rect3.width), vectorAnchor.x);
			rect3.y = originViewPort.y;
			if (rect2.height >= rect3.height && rect2.width >= rect3.width)
			{
				if (rect2.height <= originViewPort.height && rect2.width <= originViewPort.width)
				{
					camera.rect = rect2;
				}
				else
				{
					camera.rect = rect3;
				}
			}
			else if (rect3.height <= originViewPort.height && rect3.width <= originViewPort.width)
			{
				camera.rect = rect3;
			}
			else
			{
				camera.rect = rect2;
			}
		}
	}
}
