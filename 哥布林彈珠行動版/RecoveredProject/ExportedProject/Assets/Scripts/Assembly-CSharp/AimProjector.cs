using UnityEngine;

public class AimProjector : MonoBehaviour
{
	public float minLerp;

	public float maxLerp = 1f;

	[SerializeField]
	private Transform _projectionIndicator;

	private Renderer _renderer;

	private Material _scrollingMaterial;

	private PachinkoBall _ball;

	private void Start()
	{
		_renderer = _projectionIndicator.gameObject.GetComponent<Renderer>();
		_scrollingMaterial = _renderer.material;
		_ball = GetComponentInParent<PachinkoBall>();
		_renderer.enabled = false;
	}

	public void Initialize(LineRenderer lineRenderer)
	{
		_renderer = lineRenderer;
	}

	private void Update()
	{
		if (_ball.IsAiming())
		{
			_scrollingMaterial.mainTextureOffset = -(Vector2.one * Mathf.Repeat(Time.time, maxLerp));
			if (!_renderer.enabled)
			{
				_renderer.enabled = true;
			}
		}
		else
		{
			_renderer.enabled = false;
		}
	}
}
