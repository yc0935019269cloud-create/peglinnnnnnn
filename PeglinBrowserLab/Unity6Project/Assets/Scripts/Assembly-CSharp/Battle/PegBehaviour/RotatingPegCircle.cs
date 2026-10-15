using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class RotatingPegCircle : NonDummyPegboardBehaviour
	{
		public GameObject pegPrefab;

		public int numPegs = 15;

		public float radius = 1f;

		public float rotateSpeed = 1f;

		private List<Peg> _pegs;

		private List<Bomb> _bombs;

		private void Start()
		{
			if (!isDummy)
			{
				CreatePegs();
			}
		}

		private void FixedUpdate()
		{
			base.transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
		}

		private void CreatePegs()
		{
			bool flag = false;
			_pegs = new List<Peg>();
			_bombs = new List<Bomb>();
			for (int i = 0; i < numPegs; i++)
			{
				float f = (float)i * 2f * MathF.PI / (float)numPegs;
				float x = Mathf.Sin(f) * radius;
				float y = Mathf.Cos(f) * radius;
				GameObject gameObject = UnityEngine.Object.Instantiate(pegPrefab, base.transform);
				gameObject.transform.localPosition = new Vector3(x, y, 0f);
				if (gameObject.GetComponent<Bomb>() == null)
				{
					_pegs.Add(gameObject.GetComponent<Peg>());
					continue;
				}
				_bombs.Add(gameObject.GetComponent<Bomb>());
				flag = true;
			}
			if (!Application.IsPlaying(base.gameObject))
			{
				return;
			}
			BattleController battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
			if (battleController != null)
			{
				if (flag)
				{
					battleController.AddBombs(_bombs);
				}
				else
				{
					battleController.AddPegs(_pegs);
				}
			}
		}
	}
}
