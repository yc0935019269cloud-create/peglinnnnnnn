using UnityEngine;

[CreateAssetMenu(menuName = "UI/Controller Icon Mapping")]
public class ControllerIconMapping : ScriptableObject
{
	[SerializeField]
	private Sprite _switchIcon;

	[SerializeField]
	private Sprite _switchIconPressed;

	[SerializeField]
	private Sprite _xboxIcon;

	[SerializeField]
	private Sprite _xboxIconPressed;

	[SerializeField]
	private Sprite _psIcon;

	[SerializeField]
	private Sprite _psIconPressed;

	[SerializeField]
	private Sprite _keyboardIcon;

	[SerializeField]
	private Sprite _keyboardIconPressed;

	public Sprite switchIcon => _switchIcon;

	public Sprite switchIconPressed => _switchIconPressed;

	public Sprite xboxIcon => _xboxIcon;

	public Sprite xboxIconPressed => _xboxIconPressed;

	public Sprite psIcon => _psIcon;

	public Sprite psIconPressed => _psIconPressed;

	public Sprite keyboardIcon => _keyboardIcon;

	public Sprite keyboardIconPressed => _keyboardIconPressed;
}
