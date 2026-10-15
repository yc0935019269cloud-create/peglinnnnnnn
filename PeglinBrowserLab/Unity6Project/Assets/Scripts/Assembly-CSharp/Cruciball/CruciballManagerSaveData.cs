using Newtonsoft.Json;
using Peglin.ClassSystem;
using ToolBox.Serialization;
using UnityEngine;

namespace Cruciball
{
	internal class CruciballManagerSaveData : SaveObjectData
	{
		public static readonly string KEY = "CruciballManager";

		[JsonProperty]
		[SerializeField]
		private int _cruciballLevel;

		[JsonProperty]
		[SerializeField]
		private int _class;

		public override string Name => KEY;

		[JsonIgnore]
		public int CruciballLevel => _cruciballLevel;

		[JsonIgnore]
		public Class Class => (Class)_class;

		public CruciballManagerSaveData(int level, Class cls)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			_cruciballLevel = level;
			_class = (int)cls;
		}
	}
}
