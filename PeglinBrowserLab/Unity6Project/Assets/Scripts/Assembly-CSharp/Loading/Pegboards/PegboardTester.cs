using System.Collections.Generic;
using System.IO;
using Peglin.OdinSerializer;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Loading.Pegboards
{
	public class PegboardTester : MonoBehaviour
	{
		public TextMeshProUGUI infoText;

		public PegLayoutLoader loader;

		public Transform parent;

		private List<PegboardParser.PegboardData> pegboards = new List<PegboardParser.PegboardData>();

		private bool _isLoading = true;

		private float _waitTime = 10f;

		private float _timer;

		private int _currentIndex;

		private GameObject _currentPegboard;

		private void Update()
		{
			if (!_isLoading && _currentIndex != pegboards.Count)
			{
				if (Input.GetKeyDown(KeyCode.Space))
				{
					_timer = 0f;
				}
				if (_timer > 0f)
				{
					_timer -= Time.deltaTime;
					return;
				}
				Object.Destroy(_currentPegboard);
				infoText.text = pegboards[_currentIndex].name;
				_currentPegboard = new GameObject();
				loader.LoadPegboard(pegboards[_currentIndex++], parent.transform.position, _currentPegboard);
				_timer = _waitTime;
			}
		}

		private void OnEnable()
		{
			AsyncOperationHandle<IList<TextAsset>> asyncOperationHandle = Addressables.LoadAssetsAsync<TextAsset>("Pegboards", ProcessPegboardAsset);
			asyncOperationHandle.Completed += (AsyncOperationHandle<IList<TextAsset>> handle) =>
			{
				_isLoading = false;
			};
		}

		private void ProcessPegboardAsset(TextAsset obj)
		{
			PegboardParser.PegboardData item = SerializationUtility.DeserializeValue<PegboardParser.PegboardData>(new MemoryStream(obj.bytes), DataFormat.Binary);
			pegboards.Add(item);
		}
	}
}
