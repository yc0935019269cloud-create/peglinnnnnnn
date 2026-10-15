using UnityEngine;

public class TrajectorySimulation : MonoBehaviour
{
	public LayerMask ObstaclesLayer;

	public LineRenderer sightLine;

	public GameObject playerFire;

	public int segmentCount = 20;

	public float segmentScale = 1f;

	[HideInInspector]
	public float mass = 1f;

	[HideInInspector]
	public float fireForce = 600f;

	[HideInInspector]
	public float gravityMod = 1.2f;

	[HideInInspector]
	public float radius = 0.25f;

	[HideInInspector]
	public float bounciness = 1f;

	private Collider2D _hitObject;

	private Vector3[] perp1;

	private Vector3[] perp2;

	private Vector3[] segVel;

	public Collider2D hitObject => _hitObject;

	private void OnEnable()
	{
		if (sightLine != null)
		{
			sightLine.enabled = true;
		}
		perp1 = new Vector3[segmentCount];
		perp2 = new Vector3[segmentCount];
		segVel = new Vector3[segmentCount];
		sightLine.startWidth = radius * 2f * 0.6f;
		sightLine.endWidth = radius * 2f * 0.6f;
	}

	private void FixedUpdate()
	{
	}

	private void simulatePath()
	{
		Vector3[] array = new Vector3[segmentCount];
		array[0] = playerFire.transform.position;
		Vector3 vector = playerFire.transform.right * (fireForce * Time.deltaTime) / mass;
		_hitObject = null;
		for (int i = 1; i < segmentCount; i++)
		{
			float num = ((vector.sqrMagnitude != 0f) ? (segmentScale / vector.magnitude) : 0f);
			vector += Physics.gravity * (gravityMod * num);
			Vector3 vector2 = array[i - 1];
			Vector3 lhs = vector2 + vector.normalized - vector2;
			Vector3 vector3 = Vector3.Cross(lhs, Vector3.forward).normalized * radius + vector2;
			Vector3 vector4 = Vector3.Cross(lhs, Vector3.forward * -1f).normalized * radius + vector2;
			perp1[i] = vector3;
			perp2[i] = vector4;
			segVel[i] = vector2;
			Vector3[] listOfOrigins = new Vector3[3] { vector4, vector3, vector2 };
			RaycastHit2D raycastHit2D = DoRaycast(listOfOrigins, vector);
			if (raycastHit2D.collider != null)
			{
				_hitObject = raycastHit2D.collider;
				array[i] = array[i - 1] + vector.normalized * raycastHit2D.distance;
				vector -= Physics.gravity * (segmentScale - raycastHit2D.distance) / vector.magnitude;
				Vector3 vector5 = array[i];
				for (; i < segmentCount; i++)
				{
					array[i] = vector5;
				}
				break;
			}
			array[i] = array[i - 1] + vector * num;
		}
		Color red = Color.red;
		Color endColor = red;
		red.a = 1f;
		endColor.a = 0f;
		sightLine.startColor = red;
		sightLine.endColor = endColor;
		sightLine.positionCount = segmentCount;
		for (int j = 0; j < segmentCount; j++)
		{
			sightLine.SetPosition(j, array[j]);
		}
	}

	private Vector3 GetBounce(Vector3 velocity, Vector3 hitNormal, float bounciness)
	{
		Vector3 vector = Vector3.Project(velocity, hitNormal);
		return velocity - vector - vector * bounciness;
	}

	private RaycastHit2D DoRaycast(Vector3[] listOfOrigins, Vector3 segVelocity)
	{
		RaycastHit2D result = default;
		for (int i = 0; i < listOfOrigins.Length; i++)
		{
			result = Physics2D.Raycast(listOfOrigins[i], segVelocity, segmentScale, ObstaclesLayer);
			if (result.collider != null)
			{
				return result;
			}
		}
		return result;
	}

	public void OnDrawGizmos()
	{
	}

	private void OnDisable()
	{
		sightLine.enabled = false;
	}
}
