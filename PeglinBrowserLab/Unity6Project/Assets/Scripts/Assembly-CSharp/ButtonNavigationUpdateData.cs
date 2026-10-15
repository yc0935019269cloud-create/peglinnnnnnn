using System;
using UnityEngine.UI;

[Serializable]
public class ButtonNavigationUpdateData
{
	public Button button;

	public Selectable navOnUp;

	public Selectable navOnDown;

	public Selectable navOnLeft;

	public Selectable navOnRight;

	public ButtonNavigationUpdateData(Button _button, Selectable _navOnUp = null, Selectable _navOnDown = null, Selectable _navOnLeft = null, Selectable _navOnRight = null)
	{
		button = _button;
		navOnUp = _navOnUp;
		navOnDown = _navOnDown;
		navOnLeft = _navOnLeft;
		navOnRight = _navOnRight;
	}
}
