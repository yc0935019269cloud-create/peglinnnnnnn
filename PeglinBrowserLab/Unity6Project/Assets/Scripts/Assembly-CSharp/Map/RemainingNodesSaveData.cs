using System.Collections.Generic;
using Newtonsoft.Json;
using ToolBox.Serialization;
using UnityEngine;

namespace Map
{
	public class RemainingNodesSaveData : SaveObjectData
	{
		public static readonly string REMAINING_NODES_SAVE_KEY = "RemainingNodes_";

		[JsonProperty]
		[SerializeField]
		private string _nodeListName;

		[JsonProperty]
		[SerializeField]
		private List<string> _mapNameList;

		[JsonProperty]
		[SerializeField]
		private MapDataType _mapType;

		public override string Name => REMAINING_NODES_SAVE_KEY + _nodeListName;

		[JsonIgnore]
		public string NodeListName => _nodeListName;

		[JsonIgnore]
		public List<string> MapNameList => _mapNameList;

		[JsonIgnore]
		public MapDataType MapType => _mapType;

		public RemainingNodesSaveData(string nodeListName, List<string> mapNameList, MapDataType mapType)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			_nodeListName = nodeListName;
			_mapNameList = mapNameList;
			_mapType = mapType;
		}
	}
}
