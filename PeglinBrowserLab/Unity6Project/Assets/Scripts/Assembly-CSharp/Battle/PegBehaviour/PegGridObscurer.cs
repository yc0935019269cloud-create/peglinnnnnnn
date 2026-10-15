using System;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegGridObscurer : MonoBehaviour, IDummyPeg
	{
		public Sprite defaultSprite;

		public Sprite backgroundSprite;

		public Peg assignedPeg;

		[HideInInspector]
		public string id;

		private Collider2D _collider;

		private SpriteRenderer _renderer;

		private ParticleSystem _particles;

		private RandomObscuredPegGrid _pegGrid;

		private int _x;

		private int _y;

		private bool _isRevealed;

		private RandomObscuredPegGrid.CellPegType _revealedPegType;

		public RandomObscuredPegGrid.CellPegType revealedPegType => _revealedPegType;

		public bool isRevealed => _isRevealed;

		private void Awake()
		{
			_collider = GetComponent<Collider2D>();
			_renderer = GetComponent<SpriteRenderer>();
			_particles = GetComponentInChildren<ParticleSystem>();
		}

		public void Init(RandomObscuredPegGrid grid, int x, int y)
		{
			_pegGrid = grid;
			_x = x;
			_y = y;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			PachinkoBall component = collision.collider.GetComponent<PachinkoBall>();
			if (!(component == null) && !component.IsDummy)
			{
				if ((bool)assignedPeg && !assignedPeg.IsPopped())
				{
					assignedPeg.DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
				}
				RevealCell();
			}
		}

		public void RevealCell()
		{
			_isRevealed = true;
			_collider.enabled = false;
			_renderer.sprite = backgroundSprite;
			_renderer.sortingLayerID = SortingLayer.NameToID("PegBoardBg");
			_renderer.sortingOrder = 5;
			_pegGrid.HandlePoppedGridCell(this, _x, _y);
		}

		public void SetRevealedCellColour(RandomObscuredPegGrid.CellPegType pegType, Color color)
		{
			if (_isRevealed)
			{
				_revealedPegType = pegType;
				_renderer.color = color;
			}
		}

		public void PegTypeChanged(Peg.PegType pegType)
		{
			_pegGrid.HandlePoppedGridCell(this, _x, _y);
		}

		public void ResetCell()
		{
			_particles.Play();
			if (_isRevealed)
			{
				_isRevealed = false;
				_revealedPegType = RandomObscuredPegGrid.CellPegType.None;
				_collider.enabled = true;
				_renderer.sprite = defaultSprite;
				_renderer.color = Color.white;
				_renderer.sortingLayerID = SortingLayer.NameToID("PegBoardOverlay");
				_renderer.sortingOrder = 500;
			}
		}

		public void GenerateId(bool force = false)
		{
			if (id.IsNullOrWhitespace() | force)
			{
				id = Guid.NewGuid().ToString();
			}
		}

		public string GetId()
		{
			return id;
		}

		public void SetDummyStatus(bool status)
		{
		}

		public void SetPegStatus(bool status)
		{
			_collider.enabled = status;
		}

		public bool GetPegStatus()
		{
			return _collider.enabled;
		}

		public bool IsSpecialPeg()
		{
			return false;
		}

		public void SetSpecialCollider(bool value)
		{
		}

		public Peg.PegType GetPegType()
		{
			return Peg.PegType.NONE;
		}

		public void SetPegType(Peg.PegType pegType)
		{
		}
	}
}
