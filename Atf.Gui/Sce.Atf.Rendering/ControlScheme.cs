using Sce.Atf.Input;

namespace Sce.Atf.Rendering;

public abstract class ControlScheme
{
	public virtual Keys Left1 => Keys.A;

	public virtual Keys Left2 => Keys.Left;

	public virtual Keys Right1 => Keys.D;

	public virtual Keys Right2 => Keys.Right;

	public virtual Keys Forward1 => Keys.W;

	public virtual Keys Forward2 => Keys.Up;

	public virtual Keys Back1 => Keys.S;

	public virtual Keys Back2 => Keys.Down;

	public virtual Keys AddSelection => Keys.Shift;

	public virtual Keys ToggleSelection => Keys.Control;

	public virtual Keys RemoveSelection => Keys.None;

	public abstract bool IsRotating(Keys modifierKeys, MouseEventArgs e);

	public abstract bool IsZooming(Keys modifierKeys, MouseEventArgs e);

	public abstract bool IsPanning(Keys modifierKeys, MouseEventArgs e);

	public abstract bool IsTurning(Keys modifierKeys, MouseEventArgs e);

	public abstract bool IsElevating(Keys modifierKeys, MouseEventArgs e);

	public virtual bool IsControllingCamera(Keys modifierKeys, MouseEventArgs e)
	{
		return IsRotating(modifierKeys, e) || IsZooming(modifierKeys, e) || IsPanning(modifierKeys, e) || IsTurning(modifierKeys, e) || IsElevating(modifierKeys, e);
	}

	public virtual bool IsControllingCamera(Keys modifierKeys, KeyEventArgs e)
	{
		return IsInputKey(e.KeyCode);
	}

	public virtual bool IsInputKey(Keys key)
	{
		return key == Left1 || key == Left2 || key == Right1 || key == Right2 || key == Forward1 || key == Forward2 || key == Back1 || key == Back2;
	}
}
