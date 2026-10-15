using System.Collections.Generic;
using System.Globalization;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using ClassSystem;
using I2.Loc;
using Loading;
using Peglin.ClassSystem;
using Peglin.OdinSerializer.Utilities;
using PeglinUI.Encirclepedia;
using PeglinUI.LoadoutManager;
using PeglinUI.MainMenu.Cruciball;
using Relics;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Worldmap;

namespace PeglinUI.RunSummary
{
	public class RunStatisticsDetails : MonoBehaviour
	{
		[Header("External References")]
		public GameObject statLinesPrefab;

		public GameObject routeItemPrefab;

		public GameObject loadoutOrbPrefab;

		public GameObject loadoutRelicsPrefab;

		public Sprite level1bg;

		public Sprite level2bg;

		public Sprite level3bg;

		public Sprite winBg;

		public Sprite lossBg;

		public RelicManager relicManager;

		public StatusEffectData effectData;

		[Header("Scene References")]
		public Image classSprite;

		public Image backgroundImage;

		public TextMeshProUGUI headerText;

		public Image act4WinIcon;

		public Sprite lostInAct4Icon;

		public ChallengesDisplay challengesDisplay;

		public TextMeshProUGUI endTimeText;

		public TextMeshProUGUI customGameText;

		public Transform headerParent;

		public Transform statLinesParent;

		public Transform routeParent;

		public Transform orbsParent;

		public Transform relicsParent;

		public ImageCarousel orbsCarousel;

		public ImageCarousel relicsCarousel;

		public CruciballDisplay cruciballDisplay;

		public SeedDisplay seedDisplay;

		private Dictionary<Class, string> classLocKeys = new Dictionary<Class, string>
		{
			{
				Class.Peglin,
				"Classes/peglin_class_title"
			},
			{
				Class.Balladin,
				"Classes/warrior_class_title"
			},
			{
				Class.Roundrel,
				"Classes/rogue_class_title"
			},
			{
				Class.Spinventor,
				"Classes/spinventor_class_title"
			}
		};

		public ClassInfo[] classInfos;

		public void Initialize(RunStats stats)
		{
			Clear();
			string text = (stats.hasWon ? LocalizationManager.GetTranslation("Menu/RunSummary/win_label") : LocalizationManager.GetTranslation("Menu/RunSummary/loss_label"));
			ClassInfo classInfo = null;
			ClassInfo[] array = classInfos;
			foreach (ClassInfo classInfo2 in array)
			{
				if (classInfo2.characterClass == stats.selectedClass)
				{
					classInfo = classInfo2;
				}
			}
			if (classInfo == null)
			{
				Debug.LogError("Selected class not found in Class Info list");
				return;
			}
			headerText.text = LocalizationManager.GetTranslation(classInfo.classNameLocKey) + " - " + text;
			act4WinIcon.enabled = false;
			if (stats.hasWonCore)
			{
				act4WinIcon.enabled = true;
			}
			endTimeText.text = stats.endDate.ToString(CultureInfo.CurrentCulture);
			customGameText.gameObject.SetActive(stats.isCustomRun);
			backgroundImage.sprite = (stats.hasWon ? winBg : lossBg);
			classSprite.sprite = classInfo.classSprite;
			Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(LocalizationManager.GetTranslation("Menu/RunSummary/run_time"), stats.GetRunTime());
			Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(LocalizationManager.GetTranslation("Menu/RunSummary/final_hp"), $"{stats.finalHp} / {stats.maxHp}");
			Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(LocalizationManager.GetTranslation("Menu/RunSummary/gold_obtained"), $"{stats.coinsEarned}");
			Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(LocalizationManager.GetTranslation("Menu/RunSummary/pegs_hit"), $"{stats.pegsHit}");
			string value = $"{stats.totalDamageDealt}";
			if (stats.totalDamageDealt == int.MaxValue)
			{
				value = "∞";
			}
			Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(LocalizationManager.GetTranslation("Menu/RunSummary/damage_dealt"), value);
			(string, string) statsForClass = GetStatsForClass(stats, classInfo.characterClass);
			if (!statsForClass.Item1.IsNullOrWhitespace() && !statsForClass.Item2.IsNullOrWhitespace())
			{
				Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(statsForClass.Item1, statsForClass.Item2);
			}
			(int, string) randomInterestingStat = stats.GetRandomInterestingStat();
			Object.Instantiate(statLinesPrefab, statLinesParent).GetComponent<StatsLine>().Initialize(LocalizationManager.GetTranslation(randomInterestingStat.Item2), $"{randomInterestingStat.Item1}");
			int num = 1;
			foreach (Transform item in statLinesParent.transform)
			{
				if (num == 0)
				{
					item.GetComponentInChildren<Image>().enabled = false;
				}
				num = 1 - num;
			}
			int num2 = 0;
			Queue<RunStats.BossType> queue = new Queue<RunStats.BossType>(stats.visitedBosses);
			foreach (RoomType visitedRoom in stats.visitedRooms)
			{
				if (visitedRoom == RoomType.BOSS && queue.Count > 0)
				{
					Object.Instantiate(routeItemPrefab, routeParent).GetComponent<RouteItem>().InitializeBoss(queue.Dequeue(), num2 + 1 == stats.visitedRooms.Count);
				}
				else
				{
					Object.Instantiate(routeItemPrefab, routeParent).GetComponent<RouteItem>().Initialize(visitedRoom, num2 + 1 == stats.visitedRooms.Count);
				}
				num2++;
			}
			CreateOrbs(stats);
			CreateRelics(stats);
			challengesDisplay.SetChallenges(stats);
			if (stats.cruciballLevel > 0)
			{
				cruciballDisplay.gameObject.SetActive(value: true);
				cruciballDisplay.Initialize(stats.cruciballLevel);
			}
			else
			{
				cruciballDisplay.gameObject.SetActive(value: false);
			}
			headerParent.gameObject.SetActive(value: true);
			if (stats.seed.IsNullOrWhitespace())
			{
				seedDisplay.gameObject.SetActive(value: false);
				return;
			}
			seedDisplay.gameObject.SetActive(value: true);
			seedDisplay.ViewSeedDisplay(stats.seed);
		}

		public void Clear()
		{
			foreach (Transform item in statLinesParent)
			{
				Object.Destroy(item.gameObject);
			}
			foreach (Transform item2 in routeParent)
			{
				Object.Destroy(item2.gameObject);
			}
			orbsCarousel.ClearElements();
			relicsCarousel.ClearElements();
		}

		private void CreateRelics(RunStats stats)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (RelicEffect relic in stats.relics)
			{
				LoadoutIcon component = Object.Instantiate(loadoutRelicsPrefab, relicsParent).GetComponent<LoadoutIcon>();
				component.InitializeRelic(relicManager.GetRelicForEffectFromAllData(relic));
				list.Add(component.gameObject);
			}
			relicsCarousel.Initialize(list);
		}

		private void CreateOrbs(RunStats stats)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (RunStats.OrbPlayData value in stats.orbStats.Values)
			{
				if (value is RunStats.AssemballPlayData assemballPlayData)
				{
					Dictionary<AssemballType, int> dictionary = new Dictionary<AssemballType, int>();
					int[] array = (int[])assemballPlayData.types.Clone();
					for (int i = 0; i < assemballPlayData.amountInDeck - assemballPlayData.timesRemoved; i++)
					{
						AssemballType assemballType = AssemballType.NONE;
						for (int j = 0; j < assemballPlayData.types.Length; j++)
						{
							if (array[j] > 0)
							{
								assemballType |= AssemballComponent.GetTypeForIndex(j);
								array[j]--;
							}
						}
						if (dictionary.ContainsKey(assemballType))
						{
							dictionary[assemballType]++;
						}
						else
						{
							dictionary[assemballType] = 1;
						}
					}
					foreach (KeyValuePair<AssemballType, int> item in dictionary)
					{
						if (item.Value >= 5)
						{
							LoadoutIcon component = Object.Instantiate(loadoutOrbPrefab, orbsParent).GetComponent<LoadoutIcon>();
							component.InitializeAssemball(AssetLoading.Instance.GetOrbPrefab(assemballPlayData.name), item.Key, item.Value);
							list.Add(component.gameObject);
							int num = 0;
							AssemballType[] array2 = new AssemballType[5]
							{
								AssemballType.GREEN,
								AssemballType.RED,
								AssemballType.WHITE,
								AssemballType.YELLOW,
								AssemballType.PINK
							};
							foreach (AssemballType assemballType2 in array2)
							{
								if (item.Key.HasFlag(assemballType2))
								{
									num++;
								}
							}
							SetBackgroundImage(component, AssemballAttack.GetAssemballDisplayLevel(num) - 1);
							continue;
						}
						for (int l = 0; l < item.Value; l++)
						{
							LoadoutIcon component2 = Object.Instantiate(loadoutOrbPrefab, orbsParent).GetComponent<LoadoutIcon>();
							component2.InitializeAssemball(AssetLoading.Instance.GetOrbPrefab(assemballPlayData.name), item.Key, 0);
							list.Add(component2.gameObject);
							int num2 = 0;
							AssemballType[] array2 = new AssemballType[5]
							{
								AssemballType.GREEN,
								AssemballType.RED,
								AssemballType.WHITE,
								AssemballType.YELLOW,
								AssemballType.PINK
							};
							foreach (AssemballType assemballType3 in array2)
							{
								if (item.Key.HasFlag(assemballType3))
								{
									num2++;
								}
							}
							SetBackgroundImage(component2, AssemballAttack.GetAssemballDisplayLevel(num2) - 1);
						}
					}
					continue;
				}
				if (value is RunStats.UnlimitedUpgradesPlayData { levelsPerOrb: var levelsPerOrb })
				{
					foreach (int num3 in levelsPerOrb)
					{
						LoadoutIcon component3 = Object.Instantiate(loadoutOrbPrefab, orbsParent).GetComponent<LoadoutIcon>();
						component3.InitializeUnlimitedUpgrades(AssetLoading.Instance.GetOrbPrefab(value.name), num3);
						list.Add(component3.gameObject);
						SetBackgroundImage(component3, num3 - 1);
					}
					continue;
				}
				for (int m = 0; m < value.levelInstances.Length; m++)
				{
					int num4 = value.levelInstances[m];
					if (num4 >= 5)
					{
						LoadoutIcon component4 = Object.Instantiate(loadoutOrbPrefab, orbsParent).GetComponent<LoadoutIcon>();
						component4.InitializeOrb(AssetLoading.Instance.GetOrbPrefab(value.name), num4);
						list.Add(component4.gameObject);
						SetBackgroundImage(component4, m);
						continue;
					}
					for (int n = 0; n < num4; n++)
					{
						LoadoutIcon component5 = Object.Instantiate(loadoutOrbPrefab, orbsParent).GetComponent<LoadoutIcon>();
						component5.InitializeOrb(AssetLoading.Instance.GetOrbPrefab(value.name), 0);
						list.Add(component5.gameObject);
						SetBackgroundImage(component5, m);
					}
				}
			}
			orbsCarousel.Initialize(list);
		}

		public (string, string) GetStatsForClass(RunStats stats, Class cl)
		{
			string item = "";
			string item2 = "";
			switch (cl)
			{
			case Class.Balladin:
			case Class.Roundrel:
			{
				(StatusEffectType, int) buffWithMostStacks2 = stats.GetBuffWithMostStacks();
				(StatusEffectType, int) debuffsWithMostStacks2 = stats.GetDebuffsWithMostStacks();
				if (debuffsWithMostStacks2.Item2 > buffWithMostStacks2.Item2 || debuffsWithMostStacks2.Item2 > 0)
				{
					item = GetLabelForStatusEffect(debuffsWithMostStacks2.Item1);
					item2 = $"{debuffsWithMostStacks2.Item2}";
				}
				else if (buffWithMostStacks2.Item2 > 0)
				{
					item = GetLabelForStatusEffect(buffWithMostStacks2.Item1);
					item2 = $"{buffWithMostStacks2.Item2}";
				}
				break;
			}
			case Class.Spinventor:
			{
				(Peg.SlimeType, int) slimeTypeWithHighestValue2 = stats.GetSlimeTypeWithHighestValue();
				if (slimeTypeWithHighestValue2.Item2 > 0)
				{
					item = GetLabelForSlime(slimeTypeWithHighestValue2.Item1);
					item2 = $"{slimeTypeWithHighestValue2.Item2}";
				}
				break;
			}
			case Class.Peglin:
			{
				(StatusEffectType, int) buffWithMostStacks = stats.GetBuffWithMostStacks();
				(StatusEffectType, int) debuffsWithMostStacks = stats.GetDebuffsWithMostStacks();
				(Peg.SlimeType, int) slimeTypeWithHighestValue = stats.GetSlimeTypeWithHighestValue();
				if (buffWithMostStacks.Item2 > debuffsWithMostStacks.Item2 && buffWithMostStacks.Item2 > slimeTypeWithHighestValue.Item2)
				{
					item = GetLabelForStatusEffect(buffWithMostStacks.Item1);
					item2 = $"{buffWithMostStacks.Item2}";
				}
				else if (debuffsWithMostStacks.Item2 > buffWithMostStacks.Item2 && debuffsWithMostStacks.Item2 > slimeTypeWithHighestValue.Item2)
				{
					item = GetLabelForStatusEffect(debuffsWithMostStacks.Item1);
					item2 = $"{debuffsWithMostStacks.Item2}";
				}
				else if (slimeTypeWithHighestValue.Item2 > 0)
				{
					item = GetLabelForSlime(slimeTypeWithHighestValue.Item1);
					item2 = $"{slimeTypeWithHighestValue.Item2}";
				}
				break;
			}
			}
			return (item, item2);
		}

		public string GetLabelForStatusEffect(StatusEffectType type)
		{
			string translation = LocalizationManager.GetTranslation("Statuses/" + effectData.GetStatusEffectName(type));
			return LocalizationManager.GetTranslation("Menu/RunSummary/stacks_applied") + " - " + translation;
		}

		public string GetLabelForSlime(Peg.SlimeType type)
		{
			string translation = LocalizationManager.GetTranslation(Peg.GetSlimeLocKey(type));
			return LocalizationManager.GetTranslation("Menu/RunSummary/slime_applied") + " - " + translation;
		}

		private void SetBackgroundImage(LoadoutIcon icon, int i)
		{
			Image image = icon.backgroundImage;
			image.sprite = i switch
			{
				0 => level1bg, 
				1 => level2bg, 
				_ => level3bg, 
			};
		}
	}
}
