using System.Collections;
using Battle.Enemies;
using Coffee.UIEffects;
using PeglinUtils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class BestiaryIcon : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		[Header("Scene Refs")]
		public Image image;

		public GameObject nameRibbon;

		public TextMeshProUGUI nameText;

		public Image newIcon;

		public Image frame;

		public Image background;

		public UIEffect frameGrayscale;

		[FormerlySerializedAs("iconGrayscale")]
		public UIEffect unknownEnemyEffect;

		public GameObject shade;

		[Header("Params")]
		public Color unseenColor;

		public Sprite cardObtainedFrame;

		public Sprite forestBackground;

		public Sprite castleBackground;

		public Sprite minesBackground;

		public Sprite coreBackground;

		public Sprite questionMark;

		private EnemyData _enemy;

		private Button _button;

		public int Index { get; protected set; }

		private void OnEnable()
		{
			nameRibbon.SetActive(value: false);
		}

		public void Initialize(EnemyData enemy, BestiaryManager manager, int index)
		{
			_enemy = enemy;
			this.image.sprite = enemy.encirclepediaSprite;
			nameText.text = (enemy.Seen ? enemy.NameLocalized : "???");
			frameGrayscale.enabled = !enemy.Seen;
			unknownEnemyEffect.enabled = !enemy.Seen;
			_button = GetComponentInChildren<Button>();
			Index = index;
			if (enemy.Seen)
			{
				newIcon.enabled = !enemy.SeenInBestiary;
				_button = GetComponentInChildren<Button>();
				_button.onClick.AddListener(() =>
				{
					manager.ViewEnemy(_enemy, _button.gameObject, Index);
					nameRibbon.SetActive(value: false);
				});
				if (enemy.CardDrop > 0)
				{
					frame.sprite = cardObtainedFrame;
				}
				if (enemy.ResearchProgress >= enemy.ResearchPointTotal)
				{
					Image image = background;
					image.sprite = enemy.location switch
					{
						EnemyData.LocationTypes.FOREST => forestBackground, 
						EnemyData.LocationTypes.CASTLE => castleBackground, 
						EnemyData.LocationTypes.MINES => minesBackground, 
						EnemyData.LocationTypes.CORE => coreBackground, 
						_ => background.sprite, 
					};
				}
			}
			else
			{
				unknownEnemyEffect.enabled = true;
				this.image.color = unseenColor;
				newIcon.enabled = false;
				shade.SetActive(value: true);
				if (enemy.LocKey == "mimic_miniboss")
				{
					this.image.sprite = questionMark;
				}
			}
		}

		public void UpdateNewIcon()
		{
			newIcon.enabled = !_enemy.SeenInBestiary && _enemy.Seen;
		}

		private void OnDestroy()
		{
			GetComponentInChildren<Button>().onClick.RemoveAllListeners();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			nameRibbon.SetActive(value: true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			nameRibbon.SetActive(value: false);
		}

		public void OnSelect(BaseEventData eventData)
		{
			StartCoroutine(SelectButtonNextFrame());
		}

		private IEnumerator SelectButtonNextFrame()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			EventSystem.current.SetSelectedGameObject(_button.gameObject);
			nameRibbon.SetActive(value: true);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			nameRibbon.SetActive(value: false);
		}
	}
}
