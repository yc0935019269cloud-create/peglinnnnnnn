using System;
using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class ExtraDatabases : MonoBehaviour
	{
		public DialogueTriggerEvent addTrigger = DialogueTriggerEvent.OnStart;

		public DialogueTriggerEvent removeTrigger = DialogueTriggerEvent.None;

		public DialogueDatabase[] databases = new DialogueDatabase[0];

		public Condition condition = new Condition();

		[Tooltip("As soon as one event (add or remove) has occurred, destroy this component.")]
		public bool once;

		[Tooltip("Add/remove one database per frame instead of adding them all at the same time. Useful to avoid stutter when adding several databases.")]
		public bool onePerFrame;

		private bool m_trying;

		private Coroutine m_destroyCoroutine;

		private int m_numActiveCoroutines;

		public static event Action addedDatabases;

		public static event Action removedDatabases;

		private void TryAddDatabases(Transform interactor, bool onePerFrame)
		{
			if (m_trying)
			{
				return;
			}
			m_trying = true;
			try
			{
				if (condition == null || condition.IsTrue(interactor))
				{
					AddDatabases(onePerFrame);
				}
			}
			finally
			{
				m_trying = false;
			}
		}

		public void AddDatabases(bool onePerFrame)
		{
			if (onePerFrame)
			{
				StartCoroutine(AddDatabasesCoroutine());
			}
			else if (base.gameObject.activeInHierarchy && base.enabled)
			{
				AddDatabasesImmediate();
			}
		}

		private void AddDatabasesImmediate()
		{
			DialogueDatabase[] array = databases;
			foreach (DialogueDatabase database in array)
			{
				AddDatabase(database);
			}
			ExtraDatabases.addedDatabases();
			if (once)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		private IEnumerator AddDatabasesCoroutine()
		{
			m_numActiveCoroutines++;
			if (once && m_destroyCoroutine == null)
			{
				m_destroyCoroutine = StartCoroutine(DestroyCoroutine());
			}
			DialogueDatabase[] array = databases;
			foreach (DialogueDatabase database in array)
			{
				AddDatabase(database);
				yield return null;
			}
			ExtraDatabases.addedDatabases();
			m_numActiveCoroutines--;
		}

		private void AddDatabase(DialogueDatabase database)
		{
			if (database != null)
			{
				if (DialogueDebug.logInfo)
				{
					Debug.Log("Dialogue System: Adding database " + database.name, this);
				}
				DialogueManager.AddDatabase(database);
			}
		}

		private void TryRemoveDatabases(Transform interactor, bool onePerFrame)
		{
			if (m_trying)
			{
				return;
			}
			m_trying = true;
			try
			{
				if (condition == null || condition.IsTrue(interactor))
				{
					RemoveDatabases(onePerFrame);
				}
			}
			finally
			{
				m_trying = false;
			}
		}

		public void RemoveDatabases(bool onePerFrame)
		{
			if (onePerFrame)
			{
				StartCoroutine(RemoveDatabasesCoroutine());
			}
			else if (base.gameObject.activeInHierarchy && base.enabled)
			{
				RemoveDatabasesImmediate();
			}
		}

		private void RemoveDatabasesImmediate()
		{
			DialogueDatabase[] array = databases;
			foreach (DialogueDatabase database in array)
			{
				RemoveDatabase(database);
			}
			ExtraDatabases.removedDatabases();
			if (once)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		private IEnumerator RemoveDatabasesCoroutine()
		{
			m_numActiveCoroutines++;
			if (once && m_destroyCoroutine == null)
			{
				m_destroyCoroutine = StartCoroutine(DestroyCoroutine());
			}
			DialogueDatabase[] array = databases;
			foreach (DialogueDatabase database in array)
			{
				RemoveDatabase(database);
				yield return null;
			}
			ExtraDatabases.removedDatabases();
			m_numActiveCoroutines--;
		}

		private void RemoveDatabase(DialogueDatabase database)
		{
			if (database != null)
			{
				if (DialogueDebug.logInfo)
				{
					Debug.Log("Dialogue System: Removing database " + database.name, this);
				}
				DialogueManager.RemoveDatabase(database);
			}
		}

		private IEnumerator DestroyCoroutine()
		{
			while (m_numActiveCoroutines > 0)
			{
				yield return null;
			}
			m_destroyCoroutine = null;
			UnityEngine.Object.Destroy(this);
		}

		public void Start()
		{
			if (addTrigger == DialogueTriggerEvent.OnStart || removeTrigger == DialogueTriggerEvent.OnStart)
			{
				StartCoroutine(StartEndOfFrame());
			}
		}

		private IEnumerator StartEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			if (addTrigger == DialogueTriggerEvent.OnStart)
			{
				TryAddDatabases(null, onePerFrame);
			}
			if (removeTrigger == DialogueTriggerEvent.OnStart)
			{
				TryRemoveDatabases(null, onePerFrame);
			}
		}

		public void OnEnable()
		{
			if (addTrigger == DialogueTriggerEvent.OnEnable)
			{
				TryAddDatabases(null, onePerFrame);
			}
			if (removeTrigger == DialogueTriggerEvent.OnEnable)
			{
				TryRemoveDatabases(null, onePerFrame);
			}
		}

		public void OnDisable()
		{
			if (addTrigger == DialogueTriggerEvent.OnDisable)
			{
				TryAddDatabases(null, onePerFrame: false);
			}
			if (removeTrigger == DialogueTriggerEvent.OnDisable)
			{
				TryRemoveDatabases(null, onePerFrame: false);
			}
		}

		public void OnDestroy()
		{
			if (addTrigger == DialogueTriggerEvent.OnDestroy)
			{
				TryAddDatabases(null, onePerFrame: false);
			}
			if (removeTrigger == DialogueTriggerEvent.OnDestroy)
			{
				TryRemoveDatabases(null, onePerFrame: false);
			}
		}

		public void OnUse(Transform actor)
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnUse)
				{
					TryAddDatabases(actor, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnUse)
				{
					TryRemoveDatabases(actor, onePerFrame);
				}
			}
		}

		public void OnUse(string message)
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnUse)
				{
					TryAddDatabases(null, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnUse)
				{
					TryRemoveDatabases(null, onePerFrame);
				}
			}
		}

		public void OnUse()
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnUse)
				{
					TryAddDatabases(null, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnUse)
				{
					TryRemoveDatabases(null, onePerFrame);
				}
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnTriggerEnter)
				{
					TryAddDatabases(other.transform, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnTriggerEnter)
				{
					TryRemoveDatabases(other.transform, onePerFrame);
				}
			}
		}

		public void OnTriggerExit(Collider other)
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnTriggerExit)
				{
					TryAddDatabases(other.transform, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnTriggerExit)
				{
					TryRemoveDatabases(other.transform, onePerFrame);
				}
			}
		}

		public void OnTriggerEnter2D(Collider2D other)
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnTriggerEnter)
				{
					TryAddDatabases(other.transform, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnTriggerEnter)
				{
					TryRemoveDatabases(other.transform, onePerFrame);
				}
			}
		}

		public void OnTriggerExit2D(Collider2D other)
		{
			if (base.enabled)
			{
				if (addTrigger == DialogueTriggerEvent.OnTriggerExit)
				{
					TryAddDatabases(other.transform, onePerFrame);
				}
				if (removeTrigger == DialogueTriggerEvent.OnTriggerExit)
				{
					TryRemoveDatabases(other.transform, onePerFrame);
				}
			}
		}

		static ExtraDatabases()
		{
			ExtraDatabases.addedDatabases = delegate
			{
			};
			ExtraDatabases.removedDatabases = delegate
			{
			};
		}
	}
}
