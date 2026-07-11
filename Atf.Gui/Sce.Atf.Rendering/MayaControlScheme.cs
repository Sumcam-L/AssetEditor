using Sce.Atf.Input;

namespace Sce.Atf.Rendering;

public class MayaControlScheme : ControlScheme
{
	public override bool IsRotating(Keys modifierKeys, MouseEventArgs e)
	{
		return e.Button == MouseButtons.Left && (modifierKeys & Keys.Alt) == Keys.Alt;
	}

	public override bool IsZooming(Keys modifierKeys, MouseEventArgs e)
	{
		return (e.Button == MouseButtons.Right && (modifierKeys & Keys.Alt) == Keys.Alt) || e.Delta != 0;
	}

	public override bool IsPanning(Keys modifierKeys, MouseEventArgs e)
	{
		return e.Button == MouseButtons.Middle;
	}

	public override bool IsTurning(Keys modifierKeys, MouseEventArgs e)
	{
		return e.Button == MouseButtons.Middle && (modifierKeys & Keys.Alt) == 0;
	}

	public override bool IsElevating(Keys modifierKeys, MouseEventArgs e)
	{
		return e.Button == MouseButtons.Middle && (modifierKeys & Keys.Alt) == Keys.Alt;
	}
}
