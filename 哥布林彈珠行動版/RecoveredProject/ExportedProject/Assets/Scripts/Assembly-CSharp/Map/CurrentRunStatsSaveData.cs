using Newtonsoft.Json;
using Stats;
using ToolBox.Serialization;
using UnityEngine;

namespace Map
{
	public class CurrentRunStatsSaveData : SaveObjectData
	{
		public static readonly string KEY = "CurrentRunStats";

		[JsonProperty]
		[SerializeField]
		private RunStats.RunStatsSaveData _runStatsSaveData;

		public override string Name => KEY;

		[JsonIgnore]
		public RunStats.RunStatsSaveData runStatsSaveData
		{
			get
			{
				return _runStatsSaveData;
			}
			set
			{
				_runStatsSaveData = value;
			}
		}

		public CurrentRunStatsSaveData()
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
		}

		public CurrentRunStatsSaveData(RunStats runStats)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			runStatsSaveData = new RunStats.RunStatsSaveData(runStats);
		}
	}
}
