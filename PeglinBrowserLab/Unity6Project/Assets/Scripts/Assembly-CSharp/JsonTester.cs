using ToolBox.Serialization;
using UnityEngine;

public class JsonTester : MonoBehaviour
{
	private void OnEnable()
	{
		DataSerializer.DeserializeJson(DataSerializer.SerializeToJson(DataSerializer.SaveType.BASE));
		Debug.Log("Successfully deserialized base file");
		DataSerializer.DeserializeJson(DataSerializer.SerializeToJson(DataSerializer.SaveType.RUN));
		Debug.Log("Successfully deserialized run file");
	}
}
