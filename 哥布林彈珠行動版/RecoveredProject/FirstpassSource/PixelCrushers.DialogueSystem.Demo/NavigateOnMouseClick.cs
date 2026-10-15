using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace PixelCrushers.DialogueSystem.Demo
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavigateOnMouseClick : MonoBehaviour
	{
		public enum MouseButtonType
		{
			Left,
			Right,
			Middle
		}

		public string animatorSpeedParameter = "Speed";

		public float stoppingDistance = 0.5f;

		public MouseButtonType mouseButton;

		public bool ignoreClicksOnUI = true;

		private Transform m_myTransform;

		private Animator m_animator;

		private NavMeshAgent m_navMeshAgent;

		private void Awake()
		{
			m_myTransform = base.transform;
			m_animator = GetComponent<Animator>();
			m_navMeshAgent = GetComponent<NavMeshAgent>();
			if (m_navMeshAgent == null)
			{
				Debug.LogWarning("Dialogue System: NavigateOnMouseClick didn't find a NavMeshAgent on " + base.name + ". Disabling component.", this);
				base.enabled = false;
			}
		}

		private void Update()
		{
			if (!(m_animator == null) && !string.IsNullOrEmpty(animatorSpeedParameter))
			{
				m_animator.SetFloat(animatorSpeedParameter, m_navMeshAgent.velocity.magnitude);
			}
			if (ignoreClicksOnUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}
			if (Input.GetMouseButtonDown((int)mouseButton) && GUIUtility.hotControl == 0)
			{
				Plane plane = new Plane(Vector3.up, m_myTransform.position);
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				float enter = 0f;
				if (plane.Raycast(ray, out enter))
				{
					m_navMeshAgent.SetDestination(ray.GetPoint(enter));
				}
			}
			else if (Input.GetMouseButton((int)mouseButton) && GUIUtility.hotControl == 0)
			{
				Plane plane2 = new Plane(Vector3.up, m_myTransform.position);
				Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
				float enter2 = 0f;
				if (plane2.Raycast(ray2, out enter2))
				{
					m_navMeshAgent.SetDestination(ray2.GetPoint(enter2));
				}
			}
		}
	}
}
