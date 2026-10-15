using Map;
using Newtonsoft.Json;
using ToolBox.Serialization;
using UnityEngine;

namespace Worldmap
{
	public class MapNodeSaveData : SaveObjectData
	{
		[JsonProperty]
		[SerializeField]
		private RoomType _roomType;

		[JsonProperty]
		[SerializeField]
		private RoomState _roomStatus;

		[JsonProperty]
		[SerializeField]
		private string _mapDataName;

		[JsonProperty]
		[SerializeField]
		private MapDataType _mapDataType;

		[JsonProperty]
		[SerializeField]
		private int _isSelectedBossIndex;

		[JsonProperty]
		[SerializeField]
		private string _name;

		[JsonProperty]
		[SerializeField]
		private SeededMapNodeSaveData _seededNodeSaveData;

		[JsonIgnore]
		public RoomType RoomType => _roomType;

		[JsonIgnore]
		public RoomState RoomStatus => _roomStatus;

		[JsonIgnore]
		public MapDataType MapDataType => _mapDataType;

		[JsonIgnore]
		public string MapDataName => _mapDataName;

		[JsonIgnore]
		public int SelectedBossIndex => _isSelectedBossIndex;

		[JsonIgnore]
		public override string Name => _name;

		[JsonIgnore]
		public SeededMapNodeSaveData seededNodeSaveData => _seededNodeSaveData;

		public MapNodeSaveData(string name, RoomType roomType, RoomState roomStatus, string mapData, MapDataType mapDataType, int isSelectedBossIndex, SeededMapNodeSaveData seededNodeSaveData)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			_roomStatus = roomStatus;
			_roomType = roomType;
			_name = name;
			_mapDataName = mapData;
			_mapDataType = mapDataType;
			_isSelectedBossIndex = isSelectedBossIndex;
			_seededNodeSaveData = seededNodeSaveData;
		}
	}
}
