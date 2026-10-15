using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	[RequireComponent(typeof(Image))]
	public class ImageReactToButtonPressDepress : MonoBehaviour
	{
		private Image _image;

		private void Awake()
		{
			_image = GetComponent<Image>();
		}

		public void Select()
		{
			Vector3 position = _image.rectTransform.position;
			_image.rectTransform.position = new Vector3(position.x, position.y + 0.05f, position.z);
		}

		public void Deselect()
		{
			Vector3 position = _image.rectTransform.position;
			_image.rectTransform.position = new Vector3(position.x, position.y - 0.05f, position.z);
		}

		public void Press()
		{
			Vector3 position = _image.rectTransform.position;
			_image.rectTransform.position = new Vector3(position.x, position.y - 0.05f, position.z);
		}

		public void Depress()
		{
			Vector3 position = _image.rectTransform.position;
			_image.rectTransform.position = new Vector3(position.x, position.y + 0.05f, position.z);
		}
	}
}
