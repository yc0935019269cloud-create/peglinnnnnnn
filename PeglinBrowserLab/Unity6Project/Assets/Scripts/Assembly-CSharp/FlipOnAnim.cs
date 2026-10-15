using UnityEngine;

public class FlipOnAnim : MonoBehaviour
{
	private Animator _anim;

	private SpriteRenderer _rend;

	private bool _didFlip;

	private void Start()
	{
		_anim = GetComponent<Animator>();
		_rend = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Flip"))
		{
			if (!_didFlip)
			{
				_rend.flipX = !_rend.flipX;
				_didFlip = true;
			}
		}
		else
		{
			_didFlip = false;
		}
	}
}
