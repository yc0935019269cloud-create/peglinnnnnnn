using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoLetterbox
{
	[Serializable]
	public class ForceCameraRatio : MonoBehaviour
	{
		public Vector2 ratio = new Vector2(16f, 9f);

		public bool forceRatioOnAwake = true;

		public bool listenForWindowChanges = true;

		public bool createCameraForLetterBoxRendering = true;

		public bool findCamerasAutomatically = true;

		public Color letterBoxCameraColor = new Color(0f, 0f, 0f, 1f);

		public Rect defaultViewportSize = new Rect(0f, 0f, 1f, 1f);

		public List<CameraRatio> cameras;

		public Camera letterBoxCamera;

		private void Start()
		{
			if (findCamerasAutomatically)
			{
				FindAllCamerasInScene();
			}
			else if (cameras == null || cameras.Count == 0)
			{
				cameras = new List<CameraRatio>();
			}
			ValidateCameraArray();
			for (int i = 0; i < cameras.Count; i++)
			{
				cameras[i].camera.rect = defaultViewportSize;
				cameras[i].ResetOriginViewport();
			}
			if (createCameraForLetterBoxRendering)
			{
				letterBoxCamera = new GameObject().AddComponent<Camera>();
				letterBoxCamera.backgroundColor = letterBoxCameraColor;
				letterBoxCamera.cullingMask = 0;
				letterBoxCamera.depth = -100f;
				letterBoxCamera.farClipPlane = 1f;
				letterBoxCamera.useOcclusionCulling = false;
				letterBoxCamera.allowHDR = false;
				letterBoxCamera.clearFlags = CameraClearFlags.Color;
				letterBoxCamera.name = "Letter Box Camera";
				for (int j = 0; j < cameras.Count; j++)
				{
					if (cameras[j].camera.depth == -100f)
					{
						Debug.LogError(cameras[j].camera.name + " has a depth of -100 and may conflict with the Letter Box Camera in Forced Camera Ratio!");
					}
				}
			}
			if (forceRatioOnAwake)
			{
				CalculateAndSetAllCameraRatios();
			}
		}

		private void Update()
		{
			if (listenForWindowChanges)
			{
				CalculateAndSetAllCameraRatios();
				if (letterBoxCamera != null)
				{
					letterBoxCamera.backgroundColor = letterBoxCameraColor;
				}
			}
		}

		private CameraRatio GetCameraRatioByCamera(Camera _camera)
		{
			if (cameras == null)
			{
				return null;
			}
			for (int i = 0; i < cameras.Count; i++)
			{
				if (cameras[i] != null && cameras[i].camera == _camera)
				{
					return cameras[i];
				}
			}
			return null;
		}

		private void ValidateCameraArray()
		{
			for (int num = cameras.Count - 1; num >= 0; num--)
			{
				if (cameras[num].camera == null)
				{
					cameras.RemoveAt(num);
				}
			}
		}

		public void FindAllCamerasInScene()
		{
			Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>();
			cameras = new List<CameraRatio>();
			for (int i = 0; i < array.Length; i++)
			{
				if (createCameraForLetterBoxRendering || array[i] != letterBoxCamera)
				{
					cameras.Add(new CameraRatio(array[i], new Vector2(0.5f, 0.5f)));
				}
			}
		}

		public void CalculateAndSetAllCameraRatios()
		{
			float num = ratio.x / ratio.y;
			float num2 = (float)Screen.width / (float)Screen.height;
			bool horizontalLetterbox = false;
			float width = num / num2;
			float height = num2 / num;
			if (num2 > num)
			{
				horizontalLetterbox = false;
			}
			for (int i = 0; i < cameras.Count; i++)
			{
				cameras[i].SetAnchorBasedOnEnum(cameras[i].anchor);
				cameras[i].CalculateAndSetCameraRatio(width, height, horizontalLetterbox);
			}
		}

		public void SetCameraAnchor(Camera _camera, Vector2 _anchor)
		{
			CameraRatio cameraRatioByCamera = GetCameraRatioByCamera(_camera);
			if (cameraRatioByCamera != null)
			{
				cameraRatioByCamera.vectorAnchor = _anchor;
			}
		}

		public CameraRatio[] GetCameras()
		{
			if (cameras == null)
			{
				cameras = new List<CameraRatio>();
			}
			return cameras.ToArray();
		}
	}
}
