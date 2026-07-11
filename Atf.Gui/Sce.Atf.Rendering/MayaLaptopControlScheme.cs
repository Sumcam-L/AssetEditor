using Sce.Atf.Input;

namespace Sce.Atf.Rendering;

public class MayaLaptopControlScheme : ControlScheme
{
	public override bool IsRotating(Keys modifierKeys, MouseEventArgs e)
	{
		return e.Button == MouseButtons.Left && (modifierKeys & (Keys.Control | Keys.Alt)) == Keys.Alt;
	}

	public override bool IsZooming(Keys modifierKeys, MouseEventArgs e)
	{
		return (e.Button == MouseButtons.Right && (modifierKeys & Keys.Alt) == Keys.Alt) || e.Delta != 0;
	}

	public override bool IsPanning(Keys modifierKeys, MouseEventArgs e)
	{
		return e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && (modifierKeys & (Keys.Control | Keys.Alt)) == (Keys.Control | Keys.Alt));
	}

	public override bool IsTurning(Keys modifierKeys, MouseEventArgs e)
	{
		return (e.Button == MouseButtons.Middle && (modifierKeys & Keys.Alt) == 0) || (e.Button == MouseButtons.Left && (modifierKeys & (Keys.Control | Keys.Alt)) == Keys.Alt);
	}

	public override bool IsElevating(Keys modifierKeys, MouseEventArgs e)
	{
		return (e.Button == MouseButtons.Middle && (modifierKeys & Keys.Alt) == Keys.Alt) || (e.Button == MouseButtons.Left && (modifierKeys & (Keys.Control | Keys.Alt)) == (Keys.Control | Keys.Alt));
	}
}
