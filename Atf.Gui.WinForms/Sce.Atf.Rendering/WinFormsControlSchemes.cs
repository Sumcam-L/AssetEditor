using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf.Rendering;

public static class WinFormsControlSchemes
{
	public static bool IsRotating(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.MouseEventArgs e)
	{
		return controlScheme.IsRotating(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsRotating(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.MouseEventArgs e)
	{
		return controlScheme.IsRotating(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
	}

	public static bool IsZooming(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.MouseEventArgs e)
	{
		return controlScheme.IsZooming(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsZooming(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.MouseEventArgs e)
	{
		return controlScheme.IsZooming(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
	}

	public static bool IsPanning(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.MouseEventArgs e)
	{
		return controlScheme.IsPanning(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsPanning(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.MouseEventArgs e)
	{
		return controlScheme.IsPanning(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
	}

	public static bool IsTurning(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.MouseEventArgs e)
	{
		return controlScheme.IsTurning(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsTurning(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.MouseEventArgs e)
	{
		return controlScheme.IsTurning(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
	}

	public static bool IsElevating(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.MouseEventArgs e)
	{
		return controlScheme.IsElevating(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsElevating(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.MouseEventArgs e)
	{
		return controlScheme.IsElevating(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
	}

	public static bool IsControllingCamera(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.MouseEventArgs e)
	{
		return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsControllingCamera(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.MouseEventArgs e)
	{
		return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
	}

	public static bool IsControllingCamera(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, Sce.Atf.Input.KeyEventArgs e)
	{
		return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), e);
	}

	public static bool IsControllingCamera(this ControlScheme controlScheme, System.Windows.Forms.Keys modifierKeys, System.Windows.Forms.KeyEventArgs e)
	{
		return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), KeyEventArgsInterop.ToAtf(e));
	}

	public static bool IsInputKey(this ControlScheme controlScheme, System.Windows.Forms.Keys key)
	{
		return controlScheme.IsInputKey(KeysInterop.ToAtf(key));
	}
}
