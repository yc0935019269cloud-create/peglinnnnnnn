using System;
using System.Collections.Generic;
using Battle.Pachinko.Obstacles;
using Cruciball;
using Map;
using PeglinUtils;
using Relics;
using UnityEngine;

namespace Battle
{
	public class SpecialSlotController : MonoBehaviour
	{
		public delegate void AddStaticDamageSlotsRequest(int amount);

		public delegate void AddStaticDamageToSlotIndex(int slotIndex);

		public delegate void ClearStaticDamageSlotsRequest();

		public static Action PlaceInhaleZoneRequest = () =>
		{
		};

		public static Action RemoveInhaleZoneRequest = () =>
		{
		};

		public static Action OrbInhaled = () =>
		{
		};

		public static AddStaticDamageSlotsRequest OnAddStaticDamageSlotsRequest = (int amount) =>
		{
		};

		public static AddStaticDamageSlotsRequest OnAddStaticDamageToSlotIndex = (int slotIndex) =>
		{
		};

		public static ClearStaticDamageSlotsRequest OnClearStaticDamageSlotsRequest = () =>
		{
		};

		public CruciballManager cruciballManager;

		public RelicManager relicManager;

		public SlotTrigger[] slotTriggers;

		private readonly int[] _slotMultipliersRelicAmounts = new int[5] { -2, -1, 1, 2, 2 };

		private PegboardSlotInhale _pegboardSlotInhale;

		private int _inhaleSlot = -1;

		public Color topPortalColor = new Color(0f, 0.396f, 1f);

		public Color bottomPortalColor = new Color(0.8f, 0.35f, 0f);

		private BattleController _battleController;

		private PlayerHealthController _playerHealthController;

		private int _staticDamageSlotCount;

		private List<int> _staticDamageSlotIndexes = new List<int>();

		[HideInInspector]
		public List<int> purpleFireSlotsThatHaveTurnedOrange = new List<int>();

		public int staticDamageSlotCount => _staticDamageSlotCount;

		private void OnEnable()
		{
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(TurnComplete));
			SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Combine(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(SlotActivated));
			PlaceInhaleZoneRequest = (Action)Delegate.Combine(PlaceInhaleZoneRequest, new Action(MoveInhaleZone));
			RemoveInhaleZoneRequest = (Action)Delegate.Combine(RemoveInhaleZoneRequest, new Action(RemoveInhaleZone));
			OnAddStaticDamageSlotsRequest = (AddStaticDamageSlotsRequest)Delegate.Combine(OnAddStaticDamageSlotsRequest, new AddStaticDamageSlotsRequest(AddStaticDamageSlots));
			OnAddStaticDamageToSlotIndex = (AddStaticDamageSlotsRequest)Delegate.Combine(OnAddStaticDamageToSlotIndex, new AddStaticDamageSlotsRequest(AddStaticDamageToSlotByIndex));
			OnClearStaticDamageSlotsRequest = (ClearStaticDamageSlotsRequest)Delegate.Combine(OnClearStaticDamageSlotsRequest, new ClearStaticDamageSlotsRequest(ClearStaticDamageSlots));
		}

		private void OnDisable()
		{
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(TurnComplete));
			SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Remove(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(SlotActivated));
			PlaceInhaleZoneRequest = (Action)Delegate.Remove(PlaceInhaleZoneRequest, new Action(MoveInhaleZone));
			RemoveInhaleZoneRequest = (Action)Delegate.Remove(RemoveInhaleZoneRequest, new Action(RemoveInhaleZone));
			OnAddStaticDamageSlotsRequest = (AddStaticDamageSlotsRequest)Delegate.Remove(OnAddStaticDamageSlotsRequest, new AddStaticDamageSlotsRequest(AddStaticDamageSlots));
			OnAddStaticDamageToSlotIndex = (AddStaticDamageSlotsRequest)Delegate.Remove(OnAddStaticDamageToSlotIndex, new AddStaticDamageSlotsRequest(AddStaticDamageToSlotByIndex));
			OnClearStaticDamageSlotsRequest = (ClearStaticDamageSlotsRequest)Delegate.Remove(OnClearStaticDamageSlotsRequest, new ClearStaticDamageSlotsRequest(ClearStaticDamageSlots));
		}

		private void Awake()
		{
			_pegboardSlotInhale = GetComponentInChildren<PegboardSlotInhale>(includeInactive: true);
			_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			_playerHealthController = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
		}

		private void Start()
		{
			if (relicManager.RelicEffectActive(RelicEffect.SLOT_MULTIPLIERS))
			{
				SlotManager[] componentsInChildren = GetComponentsInChildren<SlotManager>();
				if (componentsInChildren != null && componentsInChildren.Length != 0)
				{
					SlotManager[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].ConfigureForBattle();
					}
				}
			}
			TurnComplete();
		}

		private void TurnComplete()
		{
			purpleFireSlotsThatHaveTurnedOrange.Clear();
			if (MapController.instance != null && MapController.instance.Act == 4)
			{
				SlotTrigger[] array = slotTriggers;
				foreach (SlotTrigger slotTrigger in array)
				{
					if (slotTrigger != null)
					{
						slotTrigger.SetSlotColorPurple();
					}
				}
			}
			_slotMultipliersRelicAmounts.Shuffle();
			if (_battleController != null && !_battleController.IsNavigating())
			{
				if (relicManager.RelicEffectActive(RelicEffect.SLOT_MULTIPLIERS))
				{
					for (int j = 0; j < slotTriggers.Length; j++)
					{
						slotTriggers[j].multiplier = _slotMultipliersRelicAmounts[j];
					}
				}
				if (relicManager.RelicEffectActive(RelicEffect.SLOT_PORTAL))
				{
					for (int k = 0; k < _slotMultipliersRelicAmounts.Length; k++)
					{
						slotTriggers[k].TogglePortal((float)Math.Abs(_slotMultipliersRelicAmounts[k] - -1) < 0.001f, bottomPortalColor);
					}
				}
			}
			UpdateDamageSlots();
		}

		private void UpdateDamageSlots()
		{
			if (_staticDamageSlotCount >= slotTriggers.Length)
			{
				for (int i = 0; i < slotTriggers.Length; i++)
				{
					slotTriggers[i].SetSlotFlame(on: true);
				}
				return;
			}
			List<int> list = new List<int>();
			for (int j = 0; j < slotTriggers.Length; j++)
			{
				list.Add(j);
			}
			List<int> list2 = new List<int>();
			if (relicManager.RelicEffectActive(RelicEffect.SLOT_MULTIPLIERS))
			{
				for (int k = 0; k < slotTriggers.Length; k++)
				{
					if (_slotMultipliersRelicAmounts[k] == -2)
					{
						list2.Add(k);
						break;
					}
				}
			}
			if (relicManager.RelicEffectActive(RelicEffect.SLOT_PORTAL))
			{
				for (int l = 0; l < slotTriggers.Length; l++)
				{
					if (_slotMultipliersRelicAmounts[l] == -1)
					{
						list2.Add(l);
						break;
					}
				}
			}
			int num = 0;
			int num2 = Mathf.Max(_staticDamageSlotCount, relicManager.RelicEffectActive(RelicEffect.SLOT_MULTIPLIERS) ? 1 : 0);
			foreach (int item2 in list2)
			{
				if (num >= num2)
				{
					break;
				}
				slotTriggers[item2].SetSlotFlame(on: true);
				list.Remove(item2);
				num++;
			}
			if (_staticDamageSlotCount > _staticDamageSlotIndexes.Count)
			{
				List<int> list3 = new List<int>();
				for (int m = 0; m < slotTriggers.Length; m++)
				{
					if (!_staticDamageSlotIndexes.Contains(m))
					{
						list3.Add(m);
					}
				}
				for (int n = 0; n < _staticDamageSlotCount - _staticDamageSlotIndexes.Count; n++)
				{
					int item = list3[UnityEngine.Random.Range(0, list3.Count)];
					_staticDamageSlotIndexes.Add(item);
					list3.Remove(item);
				}
			}
			foreach (int staticDamageSlotIndex in _staticDamageSlotIndexes)
			{
				if (num >= num2)
				{
					break;
				}
				if (!list2.Contains(staticDamageSlotIndex))
				{
					slotTriggers[staticDamageSlotIndex].SetSlotFlame(on: true);
					list.Remove(staticDamageSlotIndex);
					num++;
				}
			}
			foreach (int item3 in list)
			{
				slotTriggers[item3].SetSlotFlame(on: false);
			}
		}

		private void AddStaticDamageSlots(int amount)
		{
			_staticDamageSlotCount += amount;
			UpdateDamageSlots();
		}

		private void AddStaticDamageToSlotByIndex(int slotIndex)
		{
			if (!_staticDamageSlotIndexes.Contains(slotIndex))
			{
				_staticDamageSlotIndexes.Add(slotIndex);
				_staticDamageSlotCount++;
				UpdateDamageSlots();
			}
		}

		private void ClearStaticDamageSlots()
		{
			_staticDamageSlotCount = 0;
			UpdateDamageSlots();
		}

		private void SlotActivated(int index, PachinkoBall pBall)
		{
			if (!(_battleController != null) || _battleController.IsNavigating())
			{
				return;
			}
			bool damageOnEnter = slotTriggers[index].damageOnEnter;
			float multiplier = slotTriggers[index].multiplier;
			_battleController.AddDamageMultiplier(multiplier);
			if (damageOnEnter && _playerHealthController != null && !relicManager.AttemptUseRelic(RelicEffect.SUPER_BOOTS))
			{
				if (MapController.instance != null && MapController.instance.Act == 4 && !purpleFireSlotsThatHaveTurnedOrange.Contains(index))
				{
					_playerHealthController.Damage(Mathf.RoundToInt(cruciballManager.GetModifiedMisnavigationDamage() / 2f), melee: true, null, PlayerHealthController.DamageSource.UNSPECIFIED, forceMaxHPDamage: true);
					purpleFireSlotsThatHaveTurnedOrange.Add(index);
					SetSlotIndexOrange(index);
				}
				else
				{
					_playerHealthController.Damage(cruciballManager.GetModifiedMisnavigationDamage());
				}
			}
			if (index == _inhaleSlot)
			{
				OrbInhaled?.Invoke();
			}
		}

		public void SetSlotIndexOrange(int index)
		{
			slotTriggers[index].SetSlotColorOrange();
		}

		public void SetSlotIndexPurple(int index)
		{
			slotTriggers[index].SetSlotColorPurple();
		}

		private void MoveInhaleZone()
		{
			_pegboardSlotInhale.gameObject.SetActive(value: true);
			Vector3 position = _pegboardSlotInhale.transform.position;
			if (StaticGameData.seededNodeData is SeededBattleNodeData seededBattleNodeData)
			{
				_inhaleSlot = ((!(seededBattleNodeData.bossMechanicsRandomStatesPerAct[2].GetRandomValue() >= 0.5f)) ? (slotTriggers.Length - 1) : 0);
			}
			else
			{
				_inhaleSlot = ((!(UnityEngine.Random.value >= 0.5f)) ? (slotTriggers.Length - 1) : 0);
			}
			position.x = slotTriggers[_inhaleSlot].transform.position.x;
			_pegboardSlotInhale.transform.position = position;
		}

		private void RemoveInhaleZone()
		{
			_inhaleSlot = -1;
			_pegboardSlotInhale.gameObject.SetActive(value: false);
		}
	}
}
