using Newtonsoft.Json;
using ToolBox.Serialization;
using UnityEngine;

namespace Map
{
	public class MapControllerSaveData : SaveObjectData
	{
		public static readonly string MAP_CONTROLLER_SAVE_KEY = "MapController";

		public static readonly string STARTING_NODE = "START";

		[JsonProperty]
		[SerializeField]
		private int _scene;

		[JsonProperty]
		[SerializeField]
		private string _previousNodeName;

		[JsonProperty]
		[SerializeField]
		private int _chosenNextNodeIndex;

		[JsonProperty]
		[SerializeField]
		private string _secretTunnelOverrideNodeName;

		[JsonProperty]
		[SerializeField]
		private int _backgroundIndex;

		[JsonProperty]
		[SerializeField]
		private int _currentRoomCount;

		[JsonProperty]
		[SerializeField]
		private int _bombChestsOpened;

		[JsonProperty]
		[SerializeField]
		private int _mimicsBattled;

		[JsonProperty]
		[SerializeField]
		private int _numEasyBattles;

		[JsonProperty]
		[SerializeField]
		private bool _achievementsOn;

		[JsonProperty]
		[SerializeField]
		private int _floorCount;

		[JsonProperty]
		[SerializeField]
		private int _goldAmount;

		[JsonProperty]
		[SerializeField]
		private int _totalFloorCount;

		[JsonProperty]
		[SerializeField]
		private int _shopOrbRemovedCount;

		[JsonProperty]
		[SerializeField]
		private string[] _easyBattleNamesQueue;

		[JsonProperty]
		[SerializeField]
		private string[] _regularBattleNamesQueue;

		[JsonProperty]
		[SerializeField]
		private string[] _scenarioNamesQueue;

		[JsonProperty]
		[SerializeField]
		private string _seed;

		[JsonProperty]
		[SerializeField]
		private Random.State _nextMapSeedState;

		[JsonProperty]
		[SerializeField]
		private SeededMapNodeSaveData[] _nodeChangesFromCruciballSeededData;

		[JsonIgnore]
		public override string Name => MAP_CONTROLLER_SAVE_KEY;

		[JsonIgnore]
		public int Scene => _scene;

		[JsonIgnore]
		public string PreviousNodeName => _previousNodeName;

		[JsonIgnore]
		public int ChosenNextNodeIndex => _chosenNextNodeIndex;

		[JsonIgnore]
		public string SecretTunnelOverrideNodeName => _secretTunnelOverrideNodeName;

		[JsonIgnore]
		public int BackgroundIndex => _backgroundIndex;

		[JsonIgnore]
		public int CurrentRoomCount => _currentRoomCount;

		[JsonIgnore]
		public int BombChestsOpened => _bombChestsOpened;

		[JsonIgnore]
		public int MimicsBattled => _mimicsBattled;

		[JsonIgnore]
		public int NumEasyBattles => _numEasyBattles;

		[JsonIgnore]
		public bool AchievementsOn => _achievementsOn;

		[JsonIgnore]
		public int FloorCount => _floorCount;

		[JsonIgnore]
		public int GoldAmount => _goldAmount;

		[JsonIgnore]
		public int TotalFloorCount => _totalFloorCount;

		[JsonIgnore]
		public int ShopOrbRemovedCount => _shopOrbRemovedCount;

		[JsonIgnore]
		public string[] EasyBattleNamesQueue => _easyBattleNamesQueue;

		[JsonIgnore]
		public string[] RegularBattleNamesQueue => _regularBattleNamesQueue;

		[JsonIgnore]
		public string[] ScenarioNamesQueue => _scenarioNamesQueue;

		[JsonIgnore]
		public string Seed => _seed;

		[JsonIgnore]
		public Random.State NextMapSeedState => _nextMapSeedState;

		[JsonIgnore]
		public SeededMapNodeSaveData[] NodeChangesFromCruciballSeededData => _nodeChangesFromCruciballSeededData;

		public MapControllerSaveData()
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
		}

		public MapControllerSaveData(int scene, string previousNodeName, int chosenNextNodeIndex, string secretTunnelOverrideNodeName, int backgroundIndex, int currentRoomCount, int bombChestsOpened, int mimicsBattled, int numEasyBattles, bool achievementsOn, int floorCount, int goldAmount, int totalFloorCount, int shopOrbRemovedCount, string[] easyBattleNamesQueue, string[] regularBattleNamesQueue, string[] scenarioNamesQueue, string seed, Random.State nextMapSeedState, SeededMapNodeSaveData[] nodeChangesFromCruciballSeededData)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			_scene = scene;
			_previousNodeName = previousNodeName;
			_chosenNextNodeIndex = chosenNextNodeIndex;
			_secretTunnelOverrideNodeName = secretTunnelOverrideNodeName;
			_backgroundIndex = backgroundIndex;
			_currentRoomCount = currentRoomCount;
			_bombChestsOpened = bombChestsOpened;
			_mimicsBattled = mimicsBattled;
			_numEasyBattles = numEasyBattles;
			_achievementsOn = achievementsOn;
			_floorCount = floorCount;
			_goldAmount = goldAmount;
			_totalFloorCount = totalFloorCount;
			_shopOrbRemovedCount = shopOrbRemovedCount;
			_easyBattleNamesQueue = easyBattleNamesQueue;
			_regularBattleNamesQueue = regularBattleNamesQueue;
			_scenarioNamesQueue = scenarioNamesQueue;
			_seed = seed;
			_nextMapSeedState = nextMapSeedState;
			_nodeChangesFromCruciballSeededData = nodeChangesFromCruciballSeededData;
		}
	}
}
