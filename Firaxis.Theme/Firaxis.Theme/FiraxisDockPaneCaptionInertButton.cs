using System.ComponentModel;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

[ToolboxItem(false)]
public class FiraxisDockPaneCaptionInertButton : InertButtonBase
{
	private Bitmap _hovered;

	private Bitmap _normal;

	private Bitmap _active;

	private Bitmap _pressed;

	private Bitmap _hoveredActive;

	private Bitmap _hoveredAutoHide;

	private Bitmap _autoHide;

	private Bitmap _pressedAutoHide;

	private DockPaneCaptionBase m_dockPaneCaption;

	private DockPaneCaptionBase DockPaneCaption => m_dockPaneCaption;

	public bool IsAutoHide => DockPaneCaption.DockPane.IsAutoHide;

	public bool IsActive => DockPaneCaption.DockPane.IsActivePane;

	public override Bitmap Image => (!IsActive) ? _normal : (IsAutoHide ? _autoHide : _active);

	public override Bitmap HoverImage => (!IsActive) ? _hovered : (IsAutoHide ? _hoveredAutoHide : _hoveredActive);

	public override Bitmap PressImage => IsAutoHide ? _pressedAutoHide : _pressed;

	public FiraxisDockPaneCaptionInertButton(DockPaneCaptionBase dockPaneCaption, Bitmap hovered, Bitmap normal, Bitmap pressed, Bitmap hoveredActive, Bitmap active, Bitmap hoveredAutoHide = null, Bitmap autoHide = null, Bitmap pressedAutoHide = null)
	{
		m_dockPaneCaption = dockPaneCaption;
		_hovered = hovered;
		_normal = normal;
		_pressed = pressed;
		_hoveredActive = hoveredActive;
		_active = active;
		_hoveredAutoHide = hoveredAutoHide ?? hoveredActive;
		_autoHide = autoHide ?? active;
		_pressedAutoHide = pressedAutoHide ?? pressed;
		RefreshChanges();
	}
}
