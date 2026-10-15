using System;
using DG.Tweening;
using UnityEngine;

namespace Scenarios.Shop
{
	public class PreviewShopItem : MonoBehaviour
	{
		public Transform spriteChild;

		private SpriteRenderer _spriteRenderer;

		private static Vector3 SCALE_ZERO_STATE = new Vector3(0f, 0f, 1f);

		private Tween _scaleTween;

		private float _startY;

		private void OnEnable()
		{
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
			base.transform.localScale = SCALE_ZERO_STATE;
			_startY = spriteChild.position.y;
			ShopItem.OnShopItemHover = (ShopItem.ShopItemHoverEvent)Delegate.Combine(ShopItem.OnShopItemHover, new ShopItem.ShopItemHoverEvent(SetSpriteAndScaleUp));
		}

		private void OnDisable()
		{
			ShopItem.OnShopItemHover = (ShopItem.ShopItemHoverEvent)Delegate.Remove(ShopItem.OnShopItemHover, new ShopItem.ShopItemHoverEvent(SetSpriteAndScaleUp));
		}

		private void SetSpriteAndScaleUp(IPurchasableItem _item)
		{
			_scaleTween.Kill();
			_scaleTween = null;
			_spriteRenderer.sprite = _item.GetPurchasableImage();
			_spriteRenderer.color = _item.GetSpriteColor();
			Vector3 spriteLocalScale = _item.GetSpriteLocalScale();
			if (_item is PurchasableOrb)
			{
				if (spriteLocalScale.y >= 1.25f)
				{
					spriteChild.localPosition = new Vector3(spriteChild.localPosition.x, 0.5f);
				}
				else
				{
					spriteChild.localPosition = new Vector3(spriteChild.localPosition.x, 1f);
				}
				spriteLocalScale *= 0.5f;
			}
			else
			{
				spriteChild.localPosition = new Vector3(spriteChild.localPosition.x, 0.5f);
			}
			_scaleTween = base.transform.DOScale(spriteLocalScale, 0.2f).SetEase(Ease.OutQuint).From(SCALE_ZERO_STATE);
		}

		private void OnDrawGizmos()
		{
			if (_spriteRenderer != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(_spriteRenderer.bounds.center, _spriteRenderer.bounds.size);
			}
		}
	}
}
