using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Pachinko.BallBehaviours;
using Battle.PegBehaviour;
using Peglin.ClassSystem;
using UnityEngine;

namespace PeglinUI.MainMenu
{
	public class MainMenuRandomOrbDrop : MonoBehaviour
	{
		public delegate void PachinkoBallDropEvent(Vector2 pos);

		public static PachinkoBallDropEvent OnNextDropPosChosen = delegate
		{
		};

		public static PachinkoBallDropEvent OnPachinkoBallDropRequest = delegate
		{
		};

		public static Action RequestBoardReset = () =>
		{
		};

		public OrbPool availableOrbs;

		private float yDropOffset = 10f;

		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private RandomPegField[] _pegFields;

		private int numPachinkoBalls;

		private int _numDropRequests;

		private int _numFailures;

		private Vector2 _nextOrbPos;

		private float minDelay = 3f;

		private float maxDelay = 6f;

		private float[] delayProgressValues;

		private float delay;

		private Dictionary<PachinkoBall, int> pBallClassIndexMap = new Dictionary<PachinkoBall, int>();

		private void Awake()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			if (_spriteRenderer != null)
			{
				_spriteRenderer.enabled = false;
			}
			delay = UnityEngine.Random.Range(minDelay, maxDelay);
		}

		private void Update()
		{
			if (delay > 0f && delay - Time.unscaledDeltaTime <= 0f)
			{
				RequestPachinkoDrop();
			}
			delay -= Time.unscaledDeltaTime;
		}

		private IEnumerator Start()
		{
			yield return null;
			RandomPegField[] pegFields = _pegFields;
			foreach (RandomPegField obj in pegFields)
			{
				obj.ConvertRandomPegToReset();
				obj.ConvertRandomPegToReset();
			}
			_nextOrbPos = GetSpawnPos();
			WaitForSeconds waitForSeconds = new WaitForSeconds(0.6f);
			for (int j = 0; j < CharacterSelectController.NumClassesUnlocked; j++)
			{
				yield return waitForSeconds;
				RequestPachinkoDrop();
			}
		}

		private void OnEnable()
		{
			RequestBoardReset = (Action)Delegate.Combine(RequestBoardReset, new Action(ResetBoard));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegHit));
			SelectableCharacter.OnDropOrbAnimationTrigger = (SelectableCharacter.OrbDropAction)Delegate.Combine(SelectableCharacter.OnDropOrbAnimationTrigger, new SelectableCharacter.OrbDropAction(DropPachinkoBall));
			CharacterSelectController.OnOrbDropRequestFailed = (Action)Delegate.Combine(CharacterSelectController.OnOrbDropRequestFailed, new Action(OrbDropRequestFailed));
			PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Combine(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(HandlePachinkoBallCreated));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Combine(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(HandlePachinkoDestroyed));
		}

		private void OnDisable()
		{
			RequestBoardReset = (Action)Delegate.Remove(RequestBoardReset, new Action(ResetBoard));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegHit));
			SelectableCharacter.OnDropOrbAnimationTrigger = (SelectableCharacter.OrbDropAction)Delegate.Remove(SelectableCharacter.OnDropOrbAnimationTrigger, new SelectableCharacter.OrbDropAction(DropPachinkoBall));
			CharacterSelectController.OnOrbDropRequestFailed = (Action)Delegate.Remove(CharacterSelectController.OnOrbDropRequestFailed, new Action(OrbDropRequestFailed));
			PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Remove(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(HandlePachinkoBallCreated));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Remove(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(HandlePachinkoDestroyed));
		}

		private void CreateDelayValues()
		{
			delayProgressValues = new float[CharacterSelectController.NumClassesUnlocked];
			for (int i = 0; i < delayProgressValues.Length; i++)
			{
				delayProgressValues[i] = UnityEngine.Random.Range(minDelay, maxDelay);
			}
			delay = UnityEngine.Random.Range(minDelay, maxDelay);
		}

		private void HandlePegHit(Peg.PegType pegType, Peg peg)
		{
			if (pegType == Peg.PegType.RESET)
			{
				ResetBoard();
			}
		}

		private void ResetBoard()
		{
			RandomPegField[] pegFields = _pegFields;
			foreach (RandomPegField obj in pegFields)
			{
				obj.ResetAllPegs();
				obj.ConvertRandomPegToReset();
			}
		}

		private void HandlePachinkoBallCreated()
		{
			numPachinkoBalls++;
		}

		private void HandlePachinkoDestroyed(PachinkoBall pBall)
		{
			UnityEngine.Object.Destroy(pBall.gameObject);
			int num = numPachinkoBalls - 1;
			numPachinkoBalls = num;
			_ = CharacterSelectController.NumClassesUnlocked;
		}

		private void RequestPachinkoDrop()
		{
			_numDropRequests++;
			OnPachinkoBallDropRequest?.Invoke(_nextOrbPos);
			_nextOrbPos = GetSpawnPos();
			RandomPegField[] pegFields = _pegFields;
			foreach (RandomPegField randomPegField in pegFields)
			{
				if (randomPegField.AreAllPegsClear())
				{
					randomPegField.ResetAllPegs();
					randomPegField.ConvertRandomPegToReset();
				}
			}
		}

		private void OrbDropRequestFailed()
		{
			if (_numFailures >= 10)
			{
				Debug.LogWarning($"Random Orb Drop Failure count too high: {_numFailures}. Cancelling request.");
				return;
			}
			if (numPachinkoBalls + _numDropRequests < CharacterSelectController.NumClassesUnlocked)
			{
				StartCoroutine(WaitAndRequest());
				return;
			}
			_numFailures++;
			_numDropRequests--;
		}

		private IEnumerator WaitAndRequest()
		{
			yield return new WaitForSeconds(2f);
			RequestPachinkoDrop();
		}

		private void DropPachinkoBall(Vector2 pos, Class cl)
		{
			_numDropRequests--;
			_numFailures = 0;
			PachinkoBall component = UnityEngine.Object.Instantiate(availableOrbs.GetRandomOrbs(1)[0], pos, Quaternion.identity).GetComponent<PachinkoBall>();
			int num = 0;
			Multiball component2 = component.GetComponent<Multiball>();
			if (component2 != null)
			{
				num += component2.multiballLevel;
			}
			component.multiballLevel = num;
			component.FireForce = 0f;
			component.isBattleOrb = false;
			component._launchSound = null;
			if (++numPachinkoBalls < CharacterSelectController.NumClassesUnlocked)
			{
				RequestPachinkoDrop();
			}
			component.WaitAndFire();
			pBallClassIndexMap.Add(component, (int)cl);
			delay = UnityEngine.Random.Range(minDelay, maxDelay);
		}

		private Vector3 GetSpawnPos()
		{
			RandomPegField randomPegField = _pegFields[UnityEngine.Random.Range(0, _pegFields.Length)];
			return new Vector2(randomPegField.GetRandomPegPosition().x + (UnityEngine.Random.value - 0.5f) * 0.06f, base.transform.position.y + yDropOffset);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawSphere(base.transform.position + Vector3.up * yDropOffset, 1f);
		}
	}
}
