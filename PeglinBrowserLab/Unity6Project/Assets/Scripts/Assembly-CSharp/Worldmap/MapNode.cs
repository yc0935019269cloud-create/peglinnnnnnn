using System;
using Challenges;
using DG.Tweening;
using Data;
using Data.Scenarios;
using Map;
using Map.MinimapView;
using NaughtyAttributes;
using Relics;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Worldmap
{
	public class MapNode : MonoBehaviour
	{
		public static readonly float UNKNOWN_NODE_CHANCE = 0.34f;

		public static readonly float SCENARIO_NODE_CHANCE = 0.095f;

		public static readonly float MINIBOSS_NODE_CHANCE = 0.115f;

		public static readonly float BATTLE_NODE_CHANCE = 0.345f;

		public static readonly float TREASURE_NODE_CHANCE = 0.03f;

		public static readonly float STORE_NODE_CHANCE = 0.05f;

		public static readonly float PEG_MINIGAME_NODE_CHANCE = 0.025f;

		private MapController _mapController;

		private SpriteRenderer _rend;

		public RoomType RoomType;

		private RoomState _roomStatus = RoomState.UPCOMING;

		[Tooltip("Left, Right or Left, Middle, Right")]
		[SerializeField]
		private MapNode[] _childNodes;

		[Tooltip("If empty, will be randomly selected")]
		[SerializeField]
		private MapData[] _potentialMapData;

		[Tooltip("Just used to prevent the first few rows from spawning minibosses.")]
		[SerializeField]
		private bool _canBeMiniboss = true;

		[Header("Prefab/Generic Fields")]
		public float lineOffset = 0.9f;

		[Tooltip("Needs to correspond to the number of States in RoomState")]
		[SerializeField]
		private Color[] _stateColours;

		[SerializeField]
		private GameObject[] _icons;

		[SerializeField]
		private Color _unavailableColor = Color.grey;

		private float _endColorAlpha = 1f;

		[SerializeField]
		private SpriteRenderer _iconBackgroundRenderer;

		[Header("Forest")]
		[SerializeField]
		private Sprite _forestIconBackgroundPlain;

		[SerializeField]
		private Sprite _forestIconBackgroundMiniboss;

		[SerializeField]
		private Sprite _forestIconBackgroundBoss;

		[SerializeField]
		private Color _forestNodeIconColor;

		[SerializeField]
		private Color _forestNodeMinibossIconColor;

		[SerializeField]
		private Color _forestNodeBossIconColor = Color.white;

		[SerializeField]
		private Color _forestLineColor;

		[Header("Castle")]
		[SerializeField]
		private Sprite _castleIconBackgroundPlain;

		[SerializeField]
		private Sprite _castleIconBackgroundMiniboss;

		[SerializeField]
		private Sprite _castleIconBackgroundBoss;

		[SerializeField]
		private Color _castleNodeIconColor;

		[SerializeField]
		private Color _castleNodeMinibossIconColor;

		[SerializeField]
		private Color _castleNodeBossIconColor = Color.white;

		[SerializeField]
		private Color _castleLineColor;

		[Header("Mines")]
		[SerializeField]
		private Sprite _minesIconBackgroundPlain;

		[SerializeField]
		private Sprite _minesIconBackgroundMiniboss;

		[SerializeField]
		private Sprite _minesIconBackgroundBoss;

		[SerializeField]
		private Color _minesNodeIconColor;

		[SerializeField]
		private Color _minesNodeMinibossIconColor;

		[SerializeField]
		private Color _minesNodeBossIconColor = Color.white;

		[SerializeField]
		private Color _minesLineColor;

		[Header("Core")]
		[SerializeField]
		private Sprite _coreIconBackgroundPlain;

		[SerializeField]
		private Sprite _coreIconBackgroundMiniboss;

		[SerializeField]
		private Sprite _coreIconBackgroundBoss;

		[SerializeField]
		private Color _coreNodeIconColor;

		[SerializeField]
		private Color _coreNodeMinibossIconColor;

		[SerializeField]
		private Color _coreNodeBossIconColor = Color.white;

		[SerializeField]
		private Color _coreLineColor;

		[Space(10f)]
		[SerializeField]
		private LineRenderer _leftRenderer;

		[SerializeField]
		private LineRenderer _middleRenderer;

		[SerializeField]
		private LineRenderer _rightRenderer;

		private Sprite _activeIcon;

		private bool _displayIconFrame = true;

		public bool isFinalNode;

		public bool isFixedNode;

		public MapNode secretTunnelConnection;

		private MapData _mapData;

		private MapDataType _mapDataType;

		[NonSerialized]
		public RelicManager relicManager;

		[NonSerialized]
		public DeckManager deckManager;

		public SeededMapNodeData seededNodeData;

		private bool _isPreviewingMap;

		private int _selectedBossIndex = -1;

		public bool canBeMiniboss
		{
			get
			{
				return _canBeMiniboss;
			}
			set
			{
				_canBeMiniboss = value;
			}
		}

		public Sprite activeIcon
		{
			get
			{
				if (RoomType == RoomType.BOSS && potentialMapData.Length != 0)
				{
					MapDataBattle mapDataBattle = potentialMapData[_selectedBossIndex] as MapDataBattle;
					if (mapDataBattle != null && mapDataBattle.mapIconOverride != null)
					{
						return mapDataBattle.mapIconOverride;
					}
				}
				return _activeIcon;
			}
		}

		public MapNode[] ChildNodes => _childNodes;

		public MapNode RightChild => _childNodes[_childNodes.Length - 1];

		public MapData MapData
		{
			get
			{
				return _mapData;
			}
			set
			{
				_mapData = value;
			}
		}

		public MapDataType MapDataType
		{
			get
			{
				return _mapDataType;
			}
			set
			{
				_mapDataType = value;
			}
		}

		public MapData[] potentialMapData => _potentialMapData;

		public int selectedBossIndex => _selectedBossIndex;

		[Button(null, EButtonEnableMode.Always)]
		private void PrintSeededData()
		{
			if (seededNodeData != null)
			{
				seededNodeData.PrintSeededData();
			}
			else
			{
				Debug.Log("No save data.");
			}
		}

		private void Awake()
		{
			_rend = GetComponent<SpriteRenderer>();
			_mapController = base.transform.GetComponentInParent<MapController>();
			DebugHideSprite component = GetComponent<DebugHideSprite>();
			if (component != null && component.enabled)
			{
				_displayIconFrame = false;
			}
			MapData[] array = potentialMapData;
			if (array != null && array.Length != 0)
			{
				_mapDataType = MapDataType.SPECIFIED;
			}
		}

		private void OnEnable()
		{
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Combine(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(ToggleIsPreviewingMap));
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDisable()
		{
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Remove(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(ToggleIsPreviewingMap));
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (_isPreviewingMap)
			{
				ToggleIsPreviewingMap(isOpen: false);
			}
		}

		private void ToggleIsPreviewingMap(bool isOpen)
		{
			if (_isPreviewingMap != isOpen)
			{
				_isPreviewingMap = isOpen;
				GenerateIcon();
				DrawLinesToChildren();
			}
		}

		public void SetActiveState(RoomState activeData, bool recursive = true, bool setIcon = true)
		{
			_roomStatus = activeData;
			if (RoomType != RoomType.BOSS)
			{
				_rend.enabled = _displayIconFrame;
				_rend.color = _stateColours[(int)_roomStatus];
			}
			switch (_roomStatus)
			{
			case RoomState.HIDDEN:
				setIcon = false;
				_rend.enabled = false;
				break;
			case RoomState.TRAVERSED:
				if (recursive)
				{
					MapNode[] childNodes = ChildNodes;
					for (int i = 0; i < childNodes.Length; i++)
					{
						childNodes[i].SetActiveState(RoomState.UNAVAILABLE, recursive, setIcon);
					}
				}
				break;
			case RoomState.NEXT:
				if (recursive)
				{
					MapNode[] childNodes = ChildNodes;
					for (int i = 0; i < childNodes.Length; i++)
					{
						childNodes[i].SetActiveState(RoomState.UPCOMING, recursive, setIcon);
					}
				}
				break;
			case RoomState.UPCOMING:
			case RoomState.FUTURE_CHILDREN:
				if (recursive)
				{
					MapNode[] childNodes = ChildNodes;
					for (int i = 0; i < childNodes.Length; i++)
					{
						childNodes[i].SetActiveState(RoomState.FUTURE_CHILDREN, recursive, setIcon);
					}
				}
				break;
			}
			DrawLinesToChildren();
			if (setIcon)
			{
				if (RoomType == RoomType.NONE || (RoomType == RoomType.TREASURE && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.NEXT_ACT_HAS_NO_TREASURE_NODES)))
				{
					GenerateRoomType();
				}
				GenerateIcon();
			}
		}

		public void GenerateIcon(float bossSelectionSeededRoll = -1f)
		{
			Color nodeColorByAct = GetNodeColorByAct(RoomType);
			int num = (int)(RoomType - 1);
			for (int i = 0; i < _icons.Length; i++)
			{
				if (i == num)
				{
					_icons[i].SetActive(value: true);
					_activeIcon = _icons[num].GetComponent<SpriteRenderer>().sprite;
				}
				else
				{
					_icons[i].SetActive(value: false);
				}
			}
			_iconBackgroundRenderer.sprite = GetNodeBackgroundByAct(RoomType);
			_iconBackgroundRenderer.maskInteraction = ((MapController.instance.Act == 3) ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None);
			_icons[num].GetComponent<SpriteRenderer>().color = nodeColorByAct;
			if (RoomType != RoomType.BOSS)
			{
				return;
			}
			if (_selectedBossIndex == -1)
			{
				if (bossSelectionSeededRoll == -1f)
				{
					_selectedBossIndex = UnityEngine.Random.Range(0, potentialMapData.Length);
				}
				else
				{
					_selectedBossIndex = SeededMapNodeData.ConvertPercentageToListIndex(bossSelectionSeededRoll, potentialMapData.Length);
				}
			}
			MapDataBattle mapDataBattle = potentialMapData[_selectedBossIndex] as MapDataBattle;
			_icons[num].GetComponent<SpriteRenderer>().sprite = mapDataBattle.mapIconOverride;
		}

		private Color GetNodeColorByAct(RoomType roomType)
		{
			if (_isPreviewingMap)
			{
				if (roomType == RoomType.MINI_BOSS)
				{
					return new Color(0.6235f, 0.1294f, 0.1215f);
				}
				return Color.white;
			}
			return MapController.instance.Act switch
			{
				1 => roomType switch
				{
					RoomType.MINI_BOSS => _forestNodeMinibossIconColor, 
					RoomType.BOSS => _forestNodeBossIconColor, 
					_ => _forestNodeIconColor, 
				}, 
				2 => roomType switch
				{
					RoomType.MINI_BOSS => _castleNodeMinibossIconColor, 
					RoomType.BOSS => _castleNodeBossIconColor, 
					_ => _castleNodeIconColor, 
				}, 
				3 => roomType switch
				{
					RoomType.MINI_BOSS => _minesNodeMinibossIconColor, 
					RoomType.BOSS => _minesNodeBossIconColor, 
					_ => _minesNodeIconColor, 
				}, 
				4 => roomType switch
				{
					RoomType.MINI_BOSS => _coreNodeMinibossIconColor, 
					RoomType.BOSS => _coreNodeBossIconColor, 
					_ => _coreNodeIconColor, 
				}, 
				_ => Color.white, 
			};
		}

		private Sprite GetNodeBackgroundByAct(RoomType roomType)
		{
			if (_isPreviewingMap)
			{
				return null;
			}
			return MapController.instance.Act switch
			{
				1 => roomType switch
				{
					RoomType.MINI_BOSS => _forestIconBackgroundMiniboss, 
					RoomType.BOSS => _forestIconBackgroundBoss, 
					_ => _forestIconBackgroundPlain, 
				}, 
				2 => roomType switch
				{
					RoomType.MINI_BOSS => _castleIconBackgroundMiniboss, 
					RoomType.BOSS => _castleIconBackgroundBoss, 
					_ => _castleIconBackgroundPlain, 
				}, 
				3 => roomType switch
				{
					RoomType.MINI_BOSS => _minesIconBackgroundMiniboss, 
					RoomType.BOSS => _minesIconBackgroundBoss, 
					_ => _minesIconBackgroundPlain, 
				}, 
				4 => roomType switch
				{
					RoomType.MINI_BOSS => _coreIconBackgroundMiniboss, 
					RoomType.BOSS => _coreIconBackgroundBoss, 
					_ => _coreIconBackgroundPlain, 
				}, 
				_ => null, 
			};
		}

		public void GenerateRoomType()
		{
			float value = UnityEngine.Random.value;
			for (int i = 0; i < 100; i++)
			{
				if (IsRoomTypeRandomNumberValid(value))
				{
					break;
				}
				value = UnityEngine.Random.value;
			}
			float num = 0f;
			if (value < num + UNKNOWN_NODE_CHANCE)
			{
				RoomType = RoomType.UNKNOWN;
				return;
			}
			num += UNKNOWN_NODE_CHANCE;
			if (value < num + SCENARIO_NODE_CHANCE)
			{
				RoomType = RoomType.SCENARIO;
				return;
			}
			num += SCENARIO_NODE_CHANCE;
			if (value < num + MINIBOSS_NODE_CHANCE && _canBeMiniboss)
			{
				RoomType = RoomType.MINI_BOSS;
				return;
			}
			num += MINIBOSS_NODE_CHANCE;
			if (value <= num + BATTLE_NODE_CHANCE)
			{
				RoomType = RoomType.BATTLE;
				return;
			}
			num += BATTLE_NODE_CHANCE;
			if (value <= num + TREASURE_NODE_CHANCE && !ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.NEXT_ACT_HAS_NO_TREASURE_NODES))
			{
				RoomType = RoomType.TREASURE;
				return;
			}
			num += TREASURE_NODE_CHANCE;
			if (value <= num + STORE_NODE_CHANCE && _canBeMiniboss)
			{
				RoomType = RoomType.STORE;
				return;
			}
			num += STORE_NODE_CHANCE;
			RoomType = RoomType.PEG_MINIGAME;
		}

		private bool IsRoomTypeRandomNumberValid(float randomNumber)
		{
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.NEXT_ACT_HAS_NO_TREASURE_NODES))
			{
				float num = UNKNOWN_NODE_CHANCE + SCENARIO_NODE_CHANCE + MINIBOSS_NODE_CHANCE + BATTLE_NODE_CHANCE;
				if (randomNumber > num && randomNumber <= num + TREASURE_NODE_CHANCE)
				{
					return false;
				}
			}
			return true;
		}

		private void DrawLinesToChildren()
		{
			if (_childNodes == null || _childNodes.Length == 0)
			{
				return;
			}
			Vector3 position = base.transform.position + Vector3.back * 0.05f + Vector3.down * lineOffset;
			Vector3 position2 = _childNodes[0].transform.position + Vector3.up * lineOffset;
			Color colorForNodeType = GetColorForNodeType(_childNodes[0].RoomType);
			colorForNodeType.a = _endColorAlpha;
			_leftRenderer.gameObject.SetActive(value: true);
			_leftRenderer.SetPosition(0, position);
			_leftRenderer.SetPosition(1, position2);
			Color unavailableLineColorByAct = GetUnavailableLineColorByAct();
			_leftRenderer.startColor = unavailableLineColorByAct;
			_leftRenderer.endColor = unavailableLineColorByAct;
			if ((_roomStatus == RoomState.NEXT || _roomStatus == RoomState.PREVIOUS) && _childNodes.Length > 1 && !_isPreviewingMap)
			{
				_leftRenderer.startColor = GetColorForNodeType(RoomType);
				_leftRenderer.endColor = colorForNodeType;
			}
			int num = 1;
			if (ChildNodes.Length > 1)
			{
				position2 = _childNodes[num].transform.position + Vector3.up * lineOffset;
				_rightRenderer.gameObject.SetActive(value: true);
				_rightRenderer.SetPosition(0, position);
				_rightRenderer.SetPosition(1, position2);
				_rightRenderer.startColor = unavailableLineColorByAct;
				_rightRenderer.endColor = unavailableLineColorByAct;
				if ((_roomStatus == RoomState.NEXT || _roomStatus == RoomState.PREVIOUS) && !_isPreviewingMap)
				{
					_rightRenderer.startColor = GetColorForNodeType(RoomType);
					colorForNodeType = GetColorForNodeType(_childNodes[1].RoomType);
					colorForNodeType.a = _endColorAlpha;
					_rightRenderer.endColor = colorForNodeType;
				}
			}
		}

		public void AnimateNextLine(int index, float animTime)
		{
			Color colorForNodeType = GetColorForNodeType(_childNodes[index].RoomType);
			switch (index)
			{
			case 0:
				DOTween.To(() => _leftRenderer.endColor, (Color x) =>
				{
					_leftRenderer.endColor = x;
				}, colorForNodeType, animTime);
				break;
			case 1:
				if (_childNodes.Length == 2)
				{
					DOTween.To(() => _rightRenderer.endColor, (Color x) =>
					{
						_rightRenderer.endColor = x;
					}, colorForNodeType, animTime);
				}
				else
				{
					DOTween.To(() => _middleRenderer.endColor, (Color x) =>
					{
						_middleRenderer.endColor = x;
					}, colorForNodeType, animTime);
				}
				break;
			case 2:
				DOTween.To(() => _rightRenderer.endColor, (Color x) =>
				{
					_rightRenderer.endColor = x;
				}, colorForNodeType, animTime);
				break;
			}
		}

		private void HideLines()
		{
			_leftRenderer.gameObject.SetActive(value: false);
			_middleRenderer.gameObject.SetActive(value: false);
			_rightRenderer.gameObject.SetActive(value: false);
		}

		private Color GetUnavailableLineColorByAct()
		{
			if (_isPreviewingMap)
			{
				return _unavailableColor;
			}
			return MapController.instance.Act switch
			{
				1 => _forestLineColor, 
				2 => _castleLineColor, 
				3 => _minesLineColor, 
				4 => _coreLineColor, 
				_ => _unavailableColor, 
			};
		}

		public void GenerateMapData()
		{
			if (_mapData != null)
			{
				return;
			}
			MapData[] array = potentialMapData;
			if (array != null && array.Length != 0)
			{
				if (RoomType == RoomType.BOSS)
				{
					_mapData = potentialMapData[_selectedBossIndex];
				}
				else if (relicManager.RelicEffectActive(RelicEffect.ONLY_MINIBOSSES))
				{
					RoomType = RoomType.MINI_BOSS;
					_mapController.GenerateRandomMapData(this);
				}
				else
				{
					_mapData = potentialMapData[UnityEngine.Random.Range(0, potentialMapData.Length)];
					MapDataScenario mapDataScenario = _mapData as MapDataScenario;
					if (mapDataScenario != null)
					{
						_mapController.CheckDebugScenerioPreReqs(this, mapDataScenario);
					}
				}
				_mapDataType = MapDataType.SPECIFIED;
			}
			else
			{
				_mapController.GenerateRandomMapData(this);
			}
		}

		public void ActivateNode()
		{
			StaticGameData.loadNextScene = false;
			if (RoomType == RoomType.BOSS)
			{
				StaticGameData.loadNextScene = true;
			}
			else if (RoomType == RoomType.BATTLE || RoomType == RoomType.MINI_BOSS || (RoomType == RoomType.UNKNOWN && IsMapDataBattleType(MapDataType)))
			{
				StaticGameData.battleVictorySceneOverride = _mapController.thisScene;
			}
			if (_roomStatus == RoomState.NEXT)
			{
				_mapController.ResolveNode(this);
			}
			else
			{
				Debug.Log("Room Not Available: " + _roomStatus);
			}
		}

		public static Color GetColorForNodeType(RoomType roomType)
		{
			return roomType switch
			{
				RoomType.UNKNOWN => Color.white, 
				RoomType.TREASURE => Color.yellow, 
				RoomType.BATTLE => Color.red, 
				RoomType.MINI_BOSS => new Color(0.6235294f, 0.1294118f, 0.1215686f), 
				RoomType.BOSS => new Color(0.6235294f, 0.1294118f, 0.1215686f), 
				_ => Color.white, 
			};
		}

		public void SaveNode()
		{
			if (seededNodeData != null)
			{
				SeededMapNodeSaveData saveData = seededNodeData.GetSaveData();
				new MapNodeSaveData(base.gameObject.name, RoomType, _roomStatus, _mapData?.name, _mapDataType, _selectedBossIndex, saveData).Save();
			}
		}

		public void LoadNode()
		{
			if (!DataSerializer.HasKey(base.gameObject.name, DataSerializer.SaveType.RUN))
			{
				return;
			}
			MapNodeSaveData mapNodeSaveData = (MapNodeSaveData)DataSerializer.Load<SaveObjectData>(base.gameObject.name, DataSerializer.SaveType.RUN);
			if (mapNodeSaveData == null)
			{
				Debug.LogError("Null mapdata");
			}
			RoomType = mapNodeSaveData.RoomType;
			_roomStatus = mapNodeSaveData.RoomStatus;
			_mapDataType = mapNodeSaveData.MapDataType;
			InitializeSeededDataBySaveDataType(mapNodeSaveData.seededNodeSaveData);
			if (RoomType == RoomType.BOSS || relicManager.RelicEffectActive(RelicEffect.ONLY_MINIBOSSES))
			{
				if (potentialMapData.Length != 0)
				{
					for (int i = 0; i < potentialMapData.Length; i++)
					{
						if (potentialMapData[i].name == mapNodeSaveData.MapDataName)
						{
							_mapData = potentialMapData[i];
						}
					}
				}
				else if (mapNodeSaveData.MapDataName != null && _mapDataType != MapDataType.UNKNOWN)
				{
					_mapData = _mapController.GetMapDataByName(mapNodeSaveData.MapDataName, _mapDataType);
				}
			}
			_selectedBossIndex = mapNodeSaveData.SelectedBossIndex;
			SetActiveState(_roomStatus, recursive: false);
		}

		public static bool IsMapDataBattleType(MapDataType type)
		{
			if (type != MapDataType.EASY_BATTLE && type != MapDataType.RANDOM_BATTLE)
			{
				return type == MapDataType.ELITE_BATTLE;
			}
			return true;
		}

		private void InitializeSeededDataBySaveDataType(SeededMapNodeSaveData seededNodeSaveData)
		{
			if (seededNodeSaveData is SeededTreasureNodeSaveData)
			{
				seededNodeData = new SeededTreasureNodeData();
			}
			else if (seededNodeSaveData is SeededShopNodeSaveData)
			{
				seededNodeData = new SeededShopNodeData();
			}
			else if (seededNodeSaveData is SeededBattleNodeSaveData)
			{
				seededNodeData = new SeededBattleNodeData();
			}
			else if (seededNodeSaveData is SeededTextScenarioNodeSaveData)
			{
				seededNodeData = new SeededTextScenarioNodeData();
			}
			else if (seededNodeSaveData is SeededUnknownNodeSaveData)
			{
				seededNodeData = new SeededUnknownNodeData();
			}
			else if (seededNodeSaveData is SeededPegMinigameNodeSaveData)
			{
				seededNodeData = new SeededPegMinigameNodeData();
			}
			else
			{
				Debug.LogError("Seeded data type does not exist.");
			}
			seededNodeData.LoadSaveData(seededNodeSaveData, deckManager, relicManager, _mapController);
		}

		public void ForceSetPotentialMapData(MapData[] newMapData)
		{
			_potentialMapData = newMapData;
		}
	}
}
