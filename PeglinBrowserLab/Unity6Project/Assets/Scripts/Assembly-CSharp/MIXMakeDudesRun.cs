using DG.Tweening;
using UnityEngine;

public class MIXMakeDudesRun : MonoBehaviour
{
	public Animator peglin;

	public Animator[] playerCharacters;

	public Animator[] enemies;

	public float moveOffset = 20f;

	public float peglinMoveTime = 10f;

	public float playerMoveTime = 8f;

	public float enemyMoveTime = 10f;

	private void Start()
	{
		peglin.SetBool("moving", value: true);
		peglin.transform.DOMoveX(peglin.transform.position.x + moveOffset, peglinMoveTime).SetEase(Ease.Linear);
		Animator[] array = playerCharacters;
		foreach (Animator animator in array)
		{
			animator.SetBool("moving", value: true);
			animator.transform.DOMoveX(animator.transform.position.x + moveOffset, playerMoveTime).SetEase(Ease.Linear);
		}
		array = enemies;
		foreach (Animator animator2 in array)
		{
			animator2.SetBool("Running", value: true);
			animator2.transform.DOMoveX(animator2.transform.position.x + moveOffset, enemyMoveTime);
		}
	}
}
