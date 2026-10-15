using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIToggleTemplate : StandardUIContentTemplate
	{
		[Tooltip("Toggle UI element.")]
		public UnityEngine.UI.Toggle toggle;

		protected object m_data;

		public event ToggleChangedDelegate onToggleChanged = delegate
		{
		};

		public virtual void Awake()
		{
			if (toggle == null && DialogueDebug.logWarnings)
			{
				Debug.LogWarning("Dialogue System: UI Toggle is unassigned.", this);
			}
		}

		public virtual void Assign(bool isVisible, bool isOn, object data, ToggleChangedDelegate toggleDelegate)
		{
			m_data = data;
			if (toggle != null)
			{
				if (isVisible)
				{
					toggle.isOn = isOn;
					toggle.onValueChanged.AddListener(OnToggleChanged);
					onToggleChanged += toggleDelegate;
				}
				else
				{
					base.gameObject.SetActive(value: false);
				}
			}
		}

		protected virtual void OnToggleChanged(bool value)
		{
			try
			{
				this.onToggleChanged(value, m_data);
			}
			catch (Exception exception)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogException(exception);
				}
			}
		}
	}
}
