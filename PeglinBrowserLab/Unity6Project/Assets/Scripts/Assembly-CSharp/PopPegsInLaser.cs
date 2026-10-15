using System.Collections;
using UnityEngine;

public class PopPegsInLaser : PopPegsInArea
{
	public LineRenderer lineRenderer;

	private const float LINE_VISIBLE_DURATION = 0.3f;

	public float laser_width = 0.4f;

	private const float MAX_LASER_DISTANCE = 50f;

	protected override void PopPegs()
	{
		GameObject gameObject = pachinkoBall.gameObject;
		Vector2 vector = gameObject.transform.position;
		RaycastHit2D[] array = Physics2D.RaycastAll(vector, -gameObject.transform.up, 50f, LayerMask.GetMask("Obstacles"));
		Vector3 vector2 = -gameObject.transform.up * 50f;
		Vector2 vector3 = vector + new Vector2(vector2.x, vector2.y);
		RaycastHit2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit2D raycastHit2D = array2[i];
			if (raycastHit2D.collider.tag == "Wall")
			{
				vector3 = raycastHit2D.point;
				break;
			}
		}
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, vector);
		lineRenderer.SetPosition(1, vector3);
		StartCoroutine(LineAnimation());
		Vector2 size = new Vector2(laser_width, (vector3 - vector).magnitude);
		Collider2D[] collisions = Physics2D.OverlapBoxAll((vector3 + vector) / 2f, angle: Vector3.SignedAngle(new Vector3(0f, 1f, 0f), vector3 - vector, new Vector3(0f, 0f, 1f)), size: size);
		ActivatePegsFromCollision(collisions);
	}

	private IEnumerator LineAnimation()
	{
		yield return new WaitForSeconds(0.3f);
		lineRenderer.enabled = false;
	}
}
