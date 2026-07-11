using System.ComponentModel;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012;

[ToolboxItem(false)]
public class VS2012DockPaneCaptionInertButton : InertButtonBase
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

	public override Bitmap Image
	{
		get
		{
			if (!IsActive)
			{
				return _normal;
			}
			if (!IsAutoHide)
			{
				return _active;
			}
			return _autoHide;
		}
	}

	public override Bitmap HoverImage
	{
		get
		{
			if (!IsActive)
			{
				return _hovered;
			}
			if (!IsAutoHide)
			{
				return _hoveredActive;
			}
			return _hoveredAutoHide;
		}
	}

	public override Bitmap PressImage
	{
		get
		{
			if (!IsAutoHide)
			{
				return _pressed;
			}
			return _pressedAutoHide;
		}
	}

	public VS2012DockPaneCaptionInertButton(DockPaneCaptionBase dockPaneCaption, Bitmap hovered, Bitmap normal, Bitmap pressed, Bitmap hoveredActive, Bitmap active, Bitmap hoveredAutoHide = null, Bitmap autoHide = null, Bitmap pressedAutoHide = null)
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
