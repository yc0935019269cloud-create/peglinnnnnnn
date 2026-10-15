using UnityEngine;

public class CameraFollowPoint : MonoBehaviour
{
	public enum MovementLock
	{
		NONE = 0,
		POSITIVE = 1,
		NEGATIVE = 2,
		ALL = 3
	}

	[Header("This script is enabled externally by MapController. Don't delete.")]
	[SerializeField]
	private Transform _targetTransform;

	[SerializeField]
	private Transform _bossIconTransform;

	[SerializeField]
	private float _offset = 3f;

	[SerializeField]
	private MovementLock _lockY;

	private Vector3 _initialPos = Vector3.zero;

	public Transform targetTransform => _targetTransform;

	public Transform bossIconTransform => _bossIconTransform;

	public float yOffset => _offset;

	private void OnEnable()
	{
		_initialPos = Camera.main.transform.position;
	}

	private void Update()
	{
		if ((bool)_targetTransform && _lockY != MovementLock.ALL)
		{
			Vector3 position = Camera.main.transform.position;
			position.x = _targetTransform.position.x;
			position.y = _targetTransform.position.y + _offset;
			if (position.y < _bossIconTransform.position.y)
			{
				position.y = _bossIconTransform.position.y;
			}
			if ((!(position.y > _initialPos.y) || _lockY != MovementLock.POSITIVE) && (!(position.y < _initialPos.y) || _lockY != MovementLock.NEGATIVE))
			{
				Camera.main.transform.position = position;
			}
		}
	}
}
