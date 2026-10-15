using UnityEngine;

public class HideAfterAnim : MonoBehaviour
{
	private Animator _anim;

	private void Start()
	{
		_anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (_anim.GetCurrentAnimatorStateInfo(0).IsName("SetInvis"))
		{
			base.transform.gameObject.SetActive(value: false);
		}
	}
}
