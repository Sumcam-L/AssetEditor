using System;
using System.Windows.Forms;

namespace Sce.Atf.Rendering;

public abstract class Controller
{
	public const float PI = (float)Math.PI;

	public const float TwoPI = (float)Math.PI * 2f;

	public virtual bool KeyDown(object sender, KeyEventArgs e)
	{
		return false;
	}

	public virtual bool KeyUp(object sender, KeyEventArgs e)
	{
		return false;
	}

	public virtual bool MouseWheel(object sender, MouseEventArgs e)
	{
		return false;
	}

	public virtual bool MouseDown(object sender, MouseEventArgs e)
	{
		return false;
	}

	public virtual bool MouseMove(object sender, MouseEventArgs e)
	{
		return false;
	}

	public virtual bool MouseUp(object sender, MouseEventArgs e)
	{
		return false;
	}
}
