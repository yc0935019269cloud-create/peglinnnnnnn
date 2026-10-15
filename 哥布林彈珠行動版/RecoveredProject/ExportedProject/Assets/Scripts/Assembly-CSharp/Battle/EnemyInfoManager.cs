using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Enemies;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data;
using Loading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
	public class EnemyInfoManager : MonoBehaviour
	{
		[Header("Scene Elements")]
		[SerializeField]
		private EnemyInfoElement _enemyInfoElementPrefab;

		[SerializeField]
		private TMP_Text _moreUpcomingEnemiesIndicator;

		[SerializeField]
		private Transform _enemyInfoContainer;

		[Header("ScrollPieces")]
		public Animator _scrollAnim;

		public Mask _mask;

		[Header("Scroll Mask Values")]
		public float _maskMinHeight = 1.3f;

		public float _maskFullHeight = 6.195f;

		public float _maskHideTime = 0.25f;

		public float _maskDisplayTime = 0.65f;

		private static readonly int RollUp = Animator.StringToHash("RollUp");

		private static readonly int RollDown = Animator.StringToHash("RollDown");

		[Header("Other")]
		[SerializeField]
		private int _maxPreviewCount = 10;

		private List<Spawn> _upcomingSpawns = new List<Spawn>();

		private Queue<EnemyInfoElement> _enemyInfoElements = new Queue<EnemyInfoElement>();

		private int _lastEnemyPreviewedIndex;

		private int _numToPop;

		public void Initialize(WaveGroup[] waveGroups)
		{
			List<Spawn> list = new List<Spawn>();
			for (int i = 0; i < waveGroups.Length; i++)
			{
				WaveData[] waveData = waveGroups[i].waveData;
				foreach (WaveData waveData2 in waveData)
				{
					object runtimeKey = waveData2.spawnData.enemyAssetReference.RuntimeKey;
					list.Add(new Spawn(waveData2.turnNumber, AssetLoading.Instance.EnemyPrefabs[runtimeKey.ToString()].GetComponent<Enemy>()));
				}
			}
			_upcomingSpawns = list.OrderBy((Spawn s) => s.turnNumber).ToList();
			if (_moreUpcomingEnemiesIndicator == null)
			{
				Debug.LogError("Missing assignment for MoreUpcomingEnemiesIndicator");
			}
			if (_upcomingSpawns.Count > 0)
			{
				RevealScroll();
				CreateEnemyImages();
			}
			UpdateMoreUpcomingEnemiesIndicator();
		}

		public void PopSpawn()
		{
			if (_upcomingSpawns.Count != 0)
			{
				_numToPop++;
				if (_numToPop <= 1)
				{
					_mask.rectTransform.DOKill();
					_scrollAnim.SetTrigger(RollUp);
					Vector2 sizeDelta = _mask.rectTransform.sizeDelta;
					sizeDelta.y = _maskMinHeight;
					TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = _mask.rectTransform.DOSizeDelta(sizeDelta, _maskHideTime);
					tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(FinishPop));
				}
			}
		}

		private void FinishPop()
		{
			for (int i = 0; i < _numToPop; i++)
			{
				UnityEngine.Object.Destroy(_enemyInfoElements.Dequeue().gameObject);
			}
			_numToPop = 0;
			UpdatePreviewAndReveal();
		}

		private void UpdatePreviewAndReveal()
		{
			if (_upcomingSpawns.Count - 1 > _lastEnemyPreviewedIndex)
			{
				_lastEnemyPreviewedIndex++;
				CreateEnemyInfoElement(_upcomingSpawns[_lastEnemyPreviewedIndex]);
				UpdateMoreUpcomingEnemiesIndicator();
				RevealScroll();
			}
			else if (_enemyInfoElements.Count > 0)
			{
				RevealScroll();
			}
		}

		private void RevealScroll()
		{
			_scrollAnim.SetTrigger(RollDown);
			Vector2 sizeDelta = _mask.rectTransform.sizeDelta;
			sizeDelta.y = _maskFullHeight;
			_mask.rectTransform.DOSizeDelta(sizeDelta, _maskDisplayTime);
		}

		private void CreateEnemyImages()
		{
			for (int i = 0; i < _maxPreviewCount && i < _upcomingSpawns.Count; i++)
			{
				Spawn spawn = _upcomingSpawns[i];
				CreateEnemyInfoElement(spawn);
				_lastEnemyPreviewedIndex = i;
			}
		}

		private void UpdateMoreUpcomingEnemiesIndicator()
		{
			int num = _upcomingSpawns.Count - _lastEnemyPreviewedIndex - 1;
			if (_moreUpcomingEnemiesIndicator.gameObject.activeInHierarchy && num <= 0)
			{
				_moreUpcomingEnemiesIndicator.gameObject.SetActive(value: false);
				return;
			}
			_moreUpcomingEnemiesIndicator.gameObject.SetActive(value: true);
			_moreUpcomingEnemiesIndicator.text = "+" + num;
		}

		private void CreateEnemyInfoElement(Spawn spawn)
		{
			EnemyInfoElement component = UnityEngine.Object.Instantiate(_enemyInfoElementPrefab.gameObject, _enemyInfoContainer).GetComponent<EnemyInfoElement>();
			component.SetEnemy(spawn.enemy);
			_enemyInfoElements.Enqueue(component);
		}
	}
}
