using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Challenges
{
	public class ChallengeUI : MonoBehaviour
	{
		public delegate void ExternalUpdateUIRequest(int act = -1);

		public static ExternalUpdateUIRequest OnExternalUpdateUIRequest = (int act) =>
		{
		};

		public Vector2 mapScenePosition = new Vector2(-2f, -7f);

		public Vector2 inNodeDefaultPosition = new Vector2(-2f, -7f);

		public Vector2 battlePosition = new Vector2(-2f, -7f);

		public Vector2 pegMinigamePosition = new Vector2(-2f, -7f);

		public Vector2 chestPosition = new Vector2(-2f, -7f);

		public Vector2 shopPosition = new Vector2(-2f, -7f);

		public Vector2 textScenarioPosition = new Vector2(-2f, -7f);

		public Vector2 winScenePosition = new Vector2(-22f, -5f);

		public List<string> sceneNameWhitelist;

		public List<string> sceneNamesToHideBackground;

		public List<string> mapSceneNames;

		public List<string> winSceneNames;

		public GameObject ChallengeIconPrefab;

		public Transform ChallengeIconContainer;

		public GameObject UIContainer;

		public RectTransform UIContainerRect;

		public GameObject background;

		public GameObject inspectionPrompt;

		private Dictionary<int, ChallengeIcon> _iconsForEachAct = new Dictionary<int, ChallengeIcon>
		{
			{ 1, null },
			{ 2, null },
			{ 3, null },
			{ 4, null }
		};

		private Dictionary<int, UIEffect> _iconGreyscaleEffectorsForEachAct = new Dictionary<int, UIEffect>
		{
			{ 1, null },
			{ 2, null },
			{ 3, null },
			{ 4, null }
		};

		public ChallengeIcon[] icons
		{
			get
			{
				List<ChallengeIcon> list = new List<ChallengeIcon>();
				if (_iconsForEachAct[1] != null)
				{
					list.Add(_iconsForEachAct[1]);
				}
				if (_iconsForEachAct[2] != null)
				{
					list.Add(_iconsForEachAct[2]);
				}
				if (_iconsForEachAct[3] != null)
				{
					list.Add(_iconsForEachAct[3]);
				}
				if (_iconsForEachAct[4] != null)
				{
					list.Add(_iconsForEachAct[4]);
				}
				return list.ToArray();
			}
		}

		private void OnEnable()
		{
			OnExternalUpdateUIRequest = (ExternalUpdateUIRequest)Delegate.Combine(OnExternalUpdateUIRequest, new ExternalUpdateUIRequest(UpdateChallengeUI));
			ChallengeManager.OnChallengeAdded = (ChallengeManager.ChallengeEvent)Delegate.Combine(ChallengeManager.OnChallengeAdded, new ChallengeManager.ChallengeEvent(AddChallenge));
			ChallengeManager.OnChallengeCompleted = (ChallengeManager.ChallengeEvent)Delegate.Combine(ChallengeManager.OnChallengeCompleted, new ChallengeManager.ChallengeEvent(OnChallengeCompleted));
			ChallengeIcon.ShowTooltip = (ChallengeIcon.ChallengeTooltip)Delegate.Combine(ChallengeIcon.ShowTooltip, new ChallengeIcon.ChallengeTooltip(ShowTooltip));
			ChallengeIcon.HideTooltip = (ChallengeIcon.HideChallengeTooltip)Delegate.Combine(ChallengeIcon.HideTooltip, new ChallengeIcon.HideChallengeTooltip(HideTooltip));
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			UpdateChallengeUI();
		}

		private void HideTooltip(bool ondisable)
		{
			TooltipManager.Instance.HideTooltip();
		}

		private void ShowTooltip(ChallengeEffect challenge, RectTransform icon, Vector2 anchoredpos)
		{
			TooltipManager.Instance.ShowTooltipChallenge(challenge, icon.transform.position, anchoredpos, followMousePos: true);
		}

		private void OnDisable()
		{
			OnExternalUpdateUIRequest = (ExternalUpdateUIRequest)Delegate.Remove(OnExternalUpdateUIRequest, new ExternalUpdateUIRequest(UpdateChallengeUI));
			ChallengeManager.OnChallengeAdded = (ChallengeManager.ChallengeEvent)Delegate.Remove(ChallengeManager.OnChallengeAdded, new ChallengeManager.ChallengeEvent(AddChallenge));
			ChallengeManager.OnChallengeCompleted = (ChallengeManager.ChallengeEvent)Delegate.Remove(ChallengeManager.OnChallengeCompleted, new ChallengeManager.ChallengeEvent(OnChallengeCompleted));
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnChallengeCompleted(int forAct, ChallengeData challengeData)
		{
			if (_iconsForEachAct[forAct].challenge == challengeData.effect)
			{
				_iconGreyscaleEffectorsForEachAct[forAct].effectFactor = 1f;
			}
		}

		private void AddChallenge(int forAct, ChallengeData challengeData)
		{
			if (!(_iconsForEachAct[forAct] != null))
			{
				ChallengeIcon component = UnityEngine.Object.Instantiate(ChallengeIconPrefab).GetComponent<ChallengeIcon>();
				component.SetChallenge(challengeData.effect);
				component.transform.SetParent(ChallengeIconContainer, worldPositionStays: false);
				component.GetComponent<Image>().DOFade(1f, 0.5f).From(0f);
				UIEffect component2 = component.GetComponent<UIEffect>();
				component2.effectFactor = (ChallengeManager.Instance.AttemptCheckChallenge(challengeData.effect) ? 0f : 1f);
				_iconsForEachAct[forAct] = component;
				_iconGreyscaleEffectorsForEachAct[forAct] = component2;
				List<ChallengeIcon> list = new List<ChallengeIcon>(icons);
				if (list.Count >= 2)
				{
					ChallengeIcon challengeIcon = list[list.Count - 2];
					ChallengeIcon challengeIcon2 = list[list.Count - 1];
					Navigation navigation = new Navigation
					{
						mode = Navigation.Mode.Explicit,
						selectOnLeft = challengeIcon.selectable.navigation.selectOnLeft,
						selectOnRight = challengeIcon2.selectable
					};
					challengeIcon.selectable.navigation = navigation;
					Navigation navigation2 = new Navigation
					{
						mode = Navigation.Mode.Explicit,
						selectOnLeft = challengeIcon.selectable
					};
					challengeIcon2.selectable.navigation = navigation2;
				}
			}
		}

		private void UpdateChallengeUI(int actOverride = -1)
		{
			int num;
			if (actOverride != -1)
			{
				num = actOverride;
			}
			else
			{
				if (!(MapController.instance != null))
				{
					return;
				}
				num = MapController.instance.Act;
			}
			string text = SceneManager.GetActiveScene().name;
			if (!sceneNameWhitelist.Contains(text))
			{
				UIContainer.SetActive(value: false);
				return;
			}
			bool flag = false;
			for (int num2 = num; num2 >= 1; num2--)
			{
				if (ChallengeManager.Instance.challengesOfEachAct[num2] != ChallengeEffect.NONE)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				UIContainer.SetActive(value: false);
				return;
			}
			for (int i = 1; i <= ChallengeManager.Instance.challengesOfEachAct.Count; i++)
			{
				if (_iconsForEachAct[i] == null && ChallengeManager.Instance.challengesOfEachAct[i] != ChallengeEffect.NONE)
				{
					AddChallenge(i, ChallengeManager.Instance.GetDataForChallenge(ChallengeManager.Instance.challengesOfEachAct[i]));
				}
				else if (_iconsForEachAct[i] != null && ChallengeManager.Instance.challengesOfEachAct[i] == ChallengeEffect.NONE)
				{
					UnityEngine.Object.Destroy(_iconsForEachAct[i]);
					_iconsForEachAct[i] = null;
					_iconGreyscaleEffectorsForEachAct[i] = null;
				}
				else if (_iconsForEachAct[i] != null && _iconsForEachAct[i].challenge != ChallengeManager.Instance.challengesOfEachAct[i])
				{
					UnityEngine.Object.Destroy(_iconsForEachAct[i]);
					_iconsForEachAct[i] = null;
					_iconGreyscaleEffectorsForEachAct[i] = null;
					AddChallenge(i, ChallengeManager.Instance.GetDataForChallenge(ChallengeManager.Instance.challengesOfEachAct[i]));
				}
			}
			for (int j = 1; j <= ChallengeManager.Instance.challengesOfEachAct.Count; j++)
			{
				if (_iconsForEachAct[j] != null)
				{
					_iconGreyscaleEffectorsForEachAct[j].effectFactor = (ChallengeManager.Instance.AttemptCheckChallenge(_iconsForEachAct[j].challenge, j) ? 0f : 1f);
				}
			}
			UIContainer.SetActive(value: true);
			background.SetActive(value: true);
			if (sceneNamesToHideBackground.Contains(text))
			{
				background.SetActive(value: false);
			}
			if (mapSceneNames.Contains(text))
			{
				UIContainerRect.anchoredPosition = mapScenePosition;
				return;
			}
			if (winSceneNames.Contains(text))
			{
				UIContainerRect.anchoredPosition = winScenePosition;
				return;
			}
			switch (text)
			{
			case "Battle":
				UIContainerRect.anchoredPosition = battlePosition;
				break;
			case "PegMinigame":
				UIContainerRect.anchoredPosition = pegMinigamePosition;
				break;
			case "Treasure":
				UIContainerRect.anchoredPosition = chestPosition;
				break;
			case "ShopScenario":
				UIContainerRect.anchoredPosition = shopPosition;
				break;
			case "TextScenario":
				UIContainerRect.anchoredPosition = textScenarioPosition;
				break;
			default:
				UIContainerRect.anchoredPosition = inNodeDefaultPosition;
				break;
			}
		}

		public void SetLeftmostIconSelected()
		{
			foreach (ChallengeIcon value in _iconsForEachAct.Values)
			{
				if (value != null)
				{
					EventSystem.current.SetSelectedGameObject(value.gameObject);
					break;
				}
			}
		}
	}
}
