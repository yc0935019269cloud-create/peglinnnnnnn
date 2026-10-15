using UnityEngine;

namespace Map.Doodads
{
	public class AnimatedSpriteMask : MonoBehaviour
	{
		private SpriteRenderer _spriteRenderer;

		private SpriteMask _spriteMask;

		private void Awake()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_spriteMask = GetComponent<SpriteMask>();
		}

		private void Update()
		{
			_spriteMask.sprite = _spriteRenderer.sprite;
		}
	}
}
