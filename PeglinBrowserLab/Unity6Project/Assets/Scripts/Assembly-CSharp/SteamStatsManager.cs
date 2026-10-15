using System.Collections.Generic;
using Peglin.Achievements;
using Steamworks;
using UnityEngine;

public class SteamStatsManager : MonoBehaviour, IAchievementUnlocker
{
	protected Callback<UserStatsReceived_t> _UserStatsReceived;

	protected Callback<UserStatsStored_t> _UserStatsStored;

	private CGameID _gameId;

	private SteamStat _statData;

	private int _numStats;

	private bool _initialized;

	private bool _requestedStats;

	private bool _statsValid;

	private bool _storeStats;

	private Dictionary<StateChange, SteamStat> _steamStats;

	public static SteamStatsManager Instance;

	private void OnEnable()
	{
		if (!_initialized)
		{
			FullInit();
		}
	}

	private void FullInit()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		if (Instance == null)
		{
			Instance = this;
		}
		if (SteamManager.Initialized && !_initialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		_initialized = true;
		_gameId = new CGameID(SteamUtils.GetAppID());
		_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		_requestedStats = false;
		_statsValid = false;
		_steamStats = new Dictionary<StateChange, SteamStat>
		{
			{
				StateChange.ENCOUNTER_BEATEN,
				CreateSteamStat("encounters_beaten", SteamStatTypes.INT)
			},
			{
				StateChange.LEVEL_1_BEATEN,
				CreateSteamStat("tb_level1", SteamStatTypes.INT)
			},
			{
				StateChange.LEVEL_2_BEATEN,
				CreateSteamStat("tb_level2", SteamStatTypes.INT)
			},
			{
				StateChange.ENEMY_BEATEN,
				CreateSteamStat("enemies_beaten", SteamStatTypes.INT)
			},
			{
				StateChange.DIED,
				CreateSteamStat("t_died", SteamStatTypes.INT)
			},
			{
				StateChange.BRAMBLE_ORB_ADDED,
				CreateSteamStat("t_bramble_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.CRIT_ORB_ADDED,
				CreateSteamStat("t_crit_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.FIRE_ORB_ADDED,
				CreateSteamStat("t_fire_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.GHOST_ORB_ADDED,
				CreateSteamStat("t_ghost_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.HEALING_ORB_ADDED,
				CreateSteamStat("t_healing_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.ICE_ORB_ADDED,
				CreateSteamStat("t_ice_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.LIGHTNING_ORB_ADDED,
				CreateSteamStat("t_lightning_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.SKIPPED_ORBS,
				CreateSteamStat("t_skipped_orbs", SteamStatTypes.INT)
			},
			{
				StateChange.MATRYOSHKA_ORB_ADDED,
				CreateSteamStat("t_matryoshka_orb_added", SteamStatTypes.INT)
			},
			{
				StateChange.RUBBER_ORB_ADDED,
				CreateSteamStat("t_rubber_orb_added", SteamStatTypes.INT)
			}
		};
	}

	private void Update()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!_requestedStats)
		{
			if (!SteamManager.Initialized)
			{
				_requestedStats = true;
				return;
			}
			_requestedStats = SteamUserStats.RequestCurrentStats();
		}
		if (_statsValid && _storeStats)
		{
			StoreStats();
		}
	}

	private bool StoreStats()
	{
		foreach (SteamStat value in _steamStats.Values)
		{
			bool flag;
			if (value.Type == SteamStatTypes.INT)
			{
				flag = SteamUserStats.SetStat(value.Name, value.IntValue);
			}
			else if (value.Type == SteamStatTypes.FLOAT)
			{
				flag = SteamUserStats.SetStat(value.Name, value.FloatValue);
			}
			else
			{
				flag = SteamUserStats.UpdateAvgRateStat(value.Name, value.AvgNumerator, value.AvgDenominator);
				if (flag)
				{
					SteamUserStats.GetStat(value.Name, out value.FloatValue);
				}
			}
			if (!flag)
			{
				Debug.LogWarning("SteamStatsManager::StoreStats(): Storing state for " + value.Name + " failed.");
			}
		}
		return SteamUserStats.StoreStats();
	}

	private void OnUserStatsReceived(UserStatsReceived_t callback)
	{
		if (!SteamManager.Initialized || (ulong)_gameId != callback.m_nGameID)
		{
			return;
		}
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError("RequestStats failed - " + callback.m_eResult);
			return;
		}
		_statsValid = true;
		foreach (SteamStat value in _steamStats.Values)
		{
			if (!((value.Type != SteamStatTypes.INT) ? SteamUserStats.GetStat(value.Name, out value.FloatValue) : SteamUserStats.GetStat(value.Name, out value.IntValue)))
			{
				Debug.LogWarning("SteamStatsManager::OnUserStatsReceived(): Failed to get value for " + value.Name);
			}
		}
	}

	private void OnUserStatsStored(UserStatsStored_t callback)
	{
		if (!SteamManager.Initialized || (ulong)_gameId != callback.m_nGameID)
		{
			return;
		}
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			if (callback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.LogWarning("StoreStats - some stats failed to validate");
				UserStatsReceived_t callback2 = new UserStatsReceived_t
				{
					m_eResult = EResult.k_EResultOK,
					m_nGameID = (ulong)_gameId
				};
				OnUserStatsReceived(callback2);
			}
			else
			{
				Debug.LogError("StoreStats failed - " + callback.m_eResult);
			}
		}
		_storeStats = false;
	}

	private static SteamStat CreateSteamStat(string name, SteamStatTypes type)
	{
		return new SteamStat
		{
			Name = name,
			Type = type
		};
	}

	public void StateChanged(StateChange state)
	{
		if (_statsValid)
		{
			SteamStat steamStat = _steamStats[state];
			switch (steamStat.Type)
			{
			case SteamStatTypes.INT:
				steamStat.IntValue++;
				break;
			case SteamStatTypes.FLOAT:
			case SteamStatTypes.AVGRATE:
				steamStat.FloatValue++;
				break;
			}
			_storeStats = true;
		}
	}

	public void UnlockAchievement(Achievement a)
	{
		if (!_initialized)
		{
			Debug.Log($"Requested Achievement {a.Id} unlock...");
			if (!a.Unlocked)
			{
				Debug.Log($"Achievement {a.Id} unlocked!");
				a.Unlocked = true;
			}
		}
		else
		{
			a.Unlocked = true;
			SteamUserStats.SetAchievement(a.Id.ToString());
			_storeStats = true;
		}
	}

	public void SetupAchievementData()
	{
		if (!_initialized)
		{
			FullInit();
		}
		if (_initialized)
		{
			foreach (KeyValuePair<AchievementData.AchievementId, Achievement> achievement in AchievementData.Achievements)
			{
				Debug.Log($"Getting achievement {achievement.Value.Id}, local value {achievement.Value.Unlocked}");
				SteamUserStats.GetAchievement(achievement.Value.Id.ToString(), out var pbAchieved);
				Debug.Log($"Achievement {achievement.Value.Id} unlocked: {pbAchieved}");
				if (pbAchieved && !achievement.Value.Unlocked)
				{
					achievement.Value.Unlocked = true;
				}
			}
			return;
		}
		Debug.Log("Failed to initialize steam, ignoring achievement data...");
	}
}
