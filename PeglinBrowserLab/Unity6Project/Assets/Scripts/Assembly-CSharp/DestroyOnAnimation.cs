using UnityEngine;

public class DestroyOnAnimation : MonoBehaviour
{
	private Animator _anim;

	private void Start()
	{
		_anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (_anim.GetCurrentAnimatorStateInfo(0).IsName("destroy"))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
