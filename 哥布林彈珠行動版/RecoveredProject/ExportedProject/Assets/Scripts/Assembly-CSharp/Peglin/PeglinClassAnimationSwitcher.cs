using Cruciball;
using Peglin.ClassSystem;
using UnityEngine;

namespace Peglin
{
	public class PeglinClassAnimationSwitcher : MonoBehaviour
	{
		[Header("Parameters")]
		public RuntimeAnimatorController peglinAnimationController;

		public RuntimeAnimatorController balladinAnimationController;

		public RuntimeAnimatorController roundrelAnimationController;

		public RuntimeAnimatorController spinventorAnimationController;

		public Sprite peglinBaseSprite;

		public Sprite balladinBaseSprite;

		public Sprite roundrelBaseSprite;

		public Sprite spinventorBaseSprite;

		[Header("Scriptable Object Refs")]
		public CruciballManager cruciballManager;

		private void OnEnable()
		{
			Animator component = GetComponent<Animator>();
			SpriteRenderer component2 = GetComponent<SpriteRenderer>();
			switch (cruciballManager.currentClass)
			{
			default:
				component2.sprite = peglinBaseSprite;
				component.runtimeAnimatorController = peglinAnimationController;
				break;
			case Class.Balladin:
				component2.sprite = balladinBaseSprite;
				component.runtimeAnimatorController = balladinAnimationController;
				break;
			case Class.Roundrel:
				component2.sprite = roundrelBaseSprite;
				component.runtimeAnimatorController = roundrelAnimationController;
				break;
			case Class.Spinventor:
				component2.sprite = spinventorBaseSprite;
				component.runtimeAnimatorController = spinventorAnimationController;
				break;
			}
		}
	}
}
