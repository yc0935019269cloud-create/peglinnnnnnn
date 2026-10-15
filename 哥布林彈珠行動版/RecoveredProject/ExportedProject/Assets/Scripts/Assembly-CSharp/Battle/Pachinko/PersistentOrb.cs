using I2.Loc;
using NaughtyAttributes;
using Relics;
using UnityEngine;

namespace Battle.Pachinko
{
	public class PersistentOrb : MonoBehaviour
	{
		[SerializeField]
		private RelicManager _relicManager;

		public int basePersistLevel = 1;

		[ReadOnly]
		public int remainingPersistence;

		private LocalizationParamsManager _paramsMan;

		public int modifiedPersistLevel => basePersistLevel + (_relicManager.RelicEffectActive(RelicEffect.ALL_ORBS_PERSIST) ? 1 : 0);

		public bool shouldPersist => remainingPersistence > 0;

		private void Awake()
		{
			_paramsMan = GetComponent<LocalizationParamsManager>();
		}

		public void ResetPersistenceLevel()
		{
			remainingPersistence = modifiedPersistLevel;
			SetPersistParam(remainingPersistence);
		}

		public void DecreasePersistenceLevel()
		{
			remainingPersistence--;
		}

		private void SetPersistParam(int paramLevel)
		{
			if (_paramsMan != null)
			{
				_paramsMan.SetParameterValue("PERSIST_LEVEL", (basePersistLevel + 1).ToString());
			}
		}
	}
}
