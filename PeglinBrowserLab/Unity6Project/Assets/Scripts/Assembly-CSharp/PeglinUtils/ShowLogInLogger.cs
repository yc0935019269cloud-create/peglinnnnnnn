using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeglinUtils
{
	public class ShowLogInLogger : MonoBehaviour
	{
		public bool closed = true;

		public bool disabled = true;

		private Queue<string> _allLogs = new Queue<string>();

		private string _lastLog;

		private void OnEnable()
		{
			if (disabled)
			{
				Object.Destroy(this);
				return;
			}
			Object.DontDestroyOnLoad(this);
			Application.logMessageReceived += ApplicationOnlogMessageReceived;
			Application.logMessageReceivedThreaded += ApplicationOnlogMessageReceivedThreaded;
			SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		}

		private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (!closed)
			{
				LoggerButtonHookup loggerButtonHookup = Object.FindObjectOfType<LoggerButtonHookup>(includeInactive: true);
				if (loggerButtonHookup != null)
				{
					loggerButtonHookup.gameObject.SetActive(value: true);
					loggerButtonHookup.GetComponentInChildren<TextMeshProUGUI>().text = _lastLog;
				}
			}
		}

		private void ApplicationOnlogMessageReceived(string condition, string stacktrace, LogType type)
		{
			string text = condition + "\n" + stacktrace;
			if (_lastLog == null || !_lastLog.Equals(text))
			{
				_allLogs.Enqueue(text);
				LoggerButtonHookup loggerButtonHookup = Object.FindObjectOfType<LoggerButtonHookup>(includeInactive: true);
				loggerButtonHookup.gameObject.SetActive(value: true);
				loggerButtonHookup.GetComponentInChildren<TextMeshProUGUI>().text = text;
				_lastLog = text;
				closed = false;
			}
		}

		private void ApplicationOnlogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
		{
			if (type == LogType.Error || type == LogType.Exception)
			{
				string text = condition + "\n" + stacktrace;
				if (_lastLog == null || !_lastLog.Equals(text))
				{
					_allLogs.Enqueue(text);
					LoggerButtonHookup loggerButtonHookup = Object.FindObjectOfType<LoggerButtonHookup>(includeInactive: true);
					loggerButtonHookup.gameObject.SetActive(value: true);
					loggerButtonHookup.GetComponentInChildren<TextMeshProUGUI>().text = text;
					_lastLog = text;
					closed = false;
				}
			}
		}

		public void CloseLog()
		{
			closed = true;
		}

		public void CopyLogToClipboard()
		{
			Queue<string> queue = new Queue<string>(_allLogs);
			StringBuilder stringBuilder = new StringBuilder();
			string result;
			while (queue.TryDequeue(out result))
			{
				stringBuilder.Append(result);
			}
			GUIUtility.systemCopyBuffer = stringBuilder.ToString();
		}
	}
}
