using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem
{
	public static class Tools
	{
		private static CursorLockMode previousLockMode = CursorLockMode.Locked;

		public static string loadedLevelName => SceneManager.GetActiveScene().name;

		public static void DeprecationWarning(MonoBehaviour mb, string extraInfo = null)
		{
			if (!(mb == null))
			{
				Debug.LogWarning("Dialogue System: " + mb.GetType().Name + " is deprecated and will be removed in the next version. " + extraInfo + "\nTo supress this message, add the scripting define symbol SUPPRESS_DEPRECATION_WARNINGS", mb);
			}
		}

		public static bool IsPrefab(GameObject go)
		{
			if (go == null)
			{
				return false;
			}
			if (go.activeInHierarchy)
			{
				return false;
			}
			if (go.transform.parent != null && go.transform.parent.gameObject.activeSelf)
			{
				return false;
			}
			GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == go)
				{
					return false;
				}
			}
			return true;
		}

		public static byte HexToByte(string hex)
		{
			return byte.Parse(hex, NumberStyles.HexNumber);
		}

		public static bool IsNumber(object o)
		{
			if (!(o is int) && !(o is float))
			{
				return o is double;
			}
			return true;
		}

		public static int StringToInt(string s)
		{
			return SafeConvert.ToInt(s);
		}

		public static float StringToFloat(string s)
		{
			return SafeConvert.ToFloat(s);
		}

		public static bool StringToBool(string s)
		{
			return string.Compare(s, "True", StringComparison.OrdinalIgnoreCase) == 0;
		}

		public static bool IsStringNullOrEmptyOrWhitespace(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				return string.IsNullOrEmpty(s.Trim());
			}
			return true;
		}

		public static string GetAllAfterSlashes(string s)
		{
			if (string.IsNullOrEmpty(s) || !s.Contains("/"))
			{
				return s;
			}
			int num = s.LastIndexOf("/") + 1;
			if (0 >= num || num >= s.Length)
			{
				return s;
			}
			return s.Substring(num);
		}

		public static string GetObjectName(UnityEngine.Object o)
		{
			if (!(o != null))
			{
				return "null";
			}
			return o.name;
		}

		public static string GetGameObjectName(Component c)
		{
			if (!(c == null))
			{
				return c.name;
			}
			return string.Empty;
		}

		public static string GetFullName(GameObject go)
		{
			string text = string.Empty;
			if (go != null)
			{
				text = go.name;
				Transform parent = go.transform.parent;
				while (parent != null)
				{
					text = parent.name + "." + text;
					parent = parent.parent;
				}
			}
			return text;
		}

		public static Transform Select(params Transform[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] != null)
				{
					return args[i];
				}
			}
			return null;
		}

		public static MonoBehaviour Select(params MonoBehaviour[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] != null)
				{
					return args[i];
				}
			}
			return null;
		}

		public static void SendMessageToEveryone(string message)
		{
			GameObject[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
		}

		public static void SendMessageToEveryone(string message, string arg)
		{
			GameObject[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SendMessage(message, arg, SendMessageOptions.DontRequireReceiver);
			}
		}

		public static IEnumerator SendMessageToEveryoneAsync(string message, int gameObjectsPerFrame)
		{
			GameObject[] gameObjects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			int count = 0;
			for (int i = 0; i < gameObjects.Length; i++)
			{
				gameObjects[i].SendMessage(message, SendMessageOptions.DontRequireReceiver);
				count++;
				if (count >= gameObjectsPerFrame)
				{
					count = 0;
					yield return null;
				}
			}
		}

		public static void SetGameObjectActive(Component component, bool value)
		{
			if (component != null)
			{
				component.gameObject.SetActive(value);
			}
		}

		public static void SetGameObjectActive(GameObject gameObject, bool value)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value);
			}
		}

		public static bool ApproximatelyZero(float x)
		{
			return x < 0.0001f;
		}

		public static Color WebColor(string colorCode)
		{
			int r = ((colorCode.Length > 2) ? HexToByte(colorCode.Substring(1, 2)) : 0);
			byte g = (byte)((colorCode.Length > 4) ? HexToByte(colorCode.Substring(3, 2)) : 0);
			byte b = (byte)((colorCode.Length > 6) ? HexToByte(colorCode.Substring(5, 2)) : 0);
			return new Color32((byte)r, g, b, byte.MaxValue);
		}

		public static string ToWebColor(Color color)
		{
			return $"#{(int)(255f * color.r):x2}{(int)(255f * color.g):x2}{(int)(255f * color.b):x2}{(int)(255f * color.a):x2}";
		}

		public static string StripRichTextCodes(string s)
		{
			if (!s.Contains("<"))
			{
				return s;
			}
			return Regex.Replace(s, "<b>|</b>|<i>|</i>|<color=[#]?\\w+>|</color>", string.Empty);
		}

		public static string StripTextMeshProTags(string s)
		{
			if (!s.Contains("<"))
			{
				return s;
			}
			return Regex.Replace(s, "<[Bb]>|</[Bb]>|<[Ii]>|</[Ii]>|<color=[#]?\\w+>|<color=\"\\w+\">|</color>|<align=\\w+>|</align>|<font=[^>]+>|</font>|<indent=\\w+\\%>|<indent=\\w+>|</indent>|<line-height=\\w+%>|<line-height=\\w+>|</line-height>|<line-indent=\\w+\\%>|<line-ident=\\w+>|</line-ident>|<link=\"[^\"]+\">|</link>|<lowercase>|</lowercase>|<uppercase>|</uppercase>|<smallcaps>|</smallcaps>|<margin=.+>|</margin>|<mark=#\\w+>|</mark>|<nobr>|</nobr>|<size=\\w+\\%>|<size=\\w+>|</size>|<sprite=.+>|<[Ss]>|</[Ss]>|<[Uu]>|</[Uu]>|<sup>|</sup>|<sub>|</sub>", string.Empty);
		}

		public static bool IsClipInAnimations(Animation animation, string clipName)
		{
			if (animation != null)
			{
				foreach (AnimationState item in animation)
				{
					if (string.Equals(item.name, clipName))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static GameObject GameObjectHardFind(string goName)
		{
			return GameObjectUtility.GameObjectHardFind(goName);
		}

		public static GameObject GameObjectHardFind(string goName, string tag)
		{
			return GameObjectUtility.GameObjectHardFind(goName, tag);
		}

		public static GameObject[] FindGameObjectsWithTagHard(string tag)
		{
			List<GameObject> list = new List<GameObject>();
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				GameObjectSearchForTags(rootGameObjects[i].transform, tag, list);
			}
			return list.ToArray();
		}

		private static void GameObjectSearchForTags(Transform t, string tag, List<GameObject> list)
		{
			if (t == null)
			{
				return;
			}
			if (string.Equals(t.tag, tag))
			{
				list.Add(t.gameObject);
			}
			foreach (Transform item in t)
			{
				GameObjectSearchForTags(item, tag, list);
			}
		}

		public static T GetComponentAnywhere<T>(GameObject gameObject) where T : Component
		{
			if (!gameObject)
			{
				return null;
			}
			T componentInChildren = gameObject.GetComponentInChildren<T>();
			if ((bool)componentInChildren)
			{
				return componentInChildren;
			}
			Transform parent = gameObject.transform.parent;
			while (!componentInChildren && (bool)parent)
			{
				componentInChildren = parent.GetComponentInChildren<T>();
				parent = parent.parent;
			}
			return componentInChildren;
		}

		public static float GetGameObjectHeight(GameObject gameObject)
		{
			CharacterController component = gameObject.GetComponent<CharacterController>();
			if (component != null)
			{
				return component.height;
			}
			CapsuleCollider component2 = gameObject.GetComponent<CapsuleCollider>();
			if (component2 != null)
			{
				return component2.height;
			}
			BoxCollider component3 = gameObject.GetComponent<BoxCollider>();
			if (component3 != null)
			{
				return component3.center.y + component3.size.y;
			}
			SphereCollider component4 = gameObject.GetComponent<SphereCollider>();
			if (component4 != null)
			{
				return component4.center.y + component4.radius;
			}
			return 0f;
		}

		public static void SetComponentEnabled(Component component, Toggle state)
		{
			if (component == null)
			{
				return;
			}
			bool flag;
			if (component is Renderer)
			{
				Renderer obj = component as Renderer;
				flag = (obj.enabled = ToggleUtility.GetNewValue(obj.enabled, state));
			}
			else if (component is Collider)
			{
				Collider obj2 = component as Collider;
				flag = (obj2.enabled = ToggleUtility.GetNewValue(obj2.enabled, state));
			}
			else if (component is Animation)
			{
				Animation obj3 = component as Animation;
				flag = (obj3.enabled = ToggleUtility.GetNewValue(obj3.enabled, state));
			}
			else if (component is Animator)
			{
				Animator obj4 = component as Animator;
				flag = (obj4.enabled = ToggleUtility.GetNewValue(obj4.enabled, state));
			}
			else if (component is AudioSource)
			{
				AudioSource obj5 = component as AudioSource;
				flag = (obj5.enabled = ToggleUtility.GetNewValue(obj5.enabled, state));
			}
			else
			{
				if (!(component is Behaviour))
				{
					if (DialogueDebug.logWarnings)
					{
						Debug.LogWarning(string.Format("{0}: Don't know how to enable/disable {1}.{2}", new object[3]
						{
							"Dialogue System",
							component.name,
							component.GetType().Name
						}));
					}
					return;
				}
				Behaviour obj6 = component as Behaviour;
				flag = (obj6.enabled = ToggleUtility.GetNewValue(obj6.enabled, state));
			}
			if (DialogueDebug.logInfo)
			{
				Debug.Log(string.Format("{0}: {1}.{2}.enabled = {3}", "Dialogue System", component.name, component.GetType().Name, flag));
			}
		}

		public static bool IsCursorActive()
		{
			if (IsCursorVisible())
			{
				return !IsCursorLocked();
			}
			return false;
		}

		public static void SetCursorActive(bool value)
		{
			ShowCursor(value);
			LockCursor(!value);
		}

		public static bool IsCursorVisible()
		{
			return Cursor.visible;
		}

		public static bool IsCursorLocked()
		{
			return Cursor.lockState != CursorLockMode.None;
		}

		public static void ShowCursor(bool value)
		{
			Cursor.visible = value;
		}

		public static void LockCursor(bool value)
		{
			if (!value && IsCursorLocked())
			{
				previousLockMode = Cursor.lockState;
			}
			Cursor.lockState = (value ? previousLockMode : CursorLockMode.None);
		}

		public static void LoadLevel(int index)
		{
			if (DialogueDebug.logInfo)
			{
				Debug.Log("Dialogue System: Loading level #" + index);
			}
			SceneManager.LoadScene(index);
		}

		public static void LoadLevel(string name)
		{
			if (DialogueDebug.logInfo)
			{
				Debug.Log("Dialogue System: Loading level " + name);
			}
			SceneManager.LoadScene(name);
		}

		public static AsyncOperation LoadLevelAsync(string name)
		{
			if (DialogueDebug.logInfo)
			{
				Debug.Log("Dialogue System: Asynchronously loading level " + name);
			}
			return SceneManager.LoadSceneAsync(name);
		}

		public static AsyncOperation LoadLevelAsync(int index)
		{
			if (DialogueDebug.logInfo)
			{
				Debug.Log("Dialogue System: Asynchronously loading level " + index);
			}
			return SceneManager.LoadSceneAsync(index);
		}
	}
}
