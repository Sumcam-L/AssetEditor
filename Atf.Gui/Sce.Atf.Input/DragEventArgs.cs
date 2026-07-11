using System;

namespace Sce.Atf.Input;

public class DragEventArgs : EventArgs
{
	private readonly object data;

	private readonly int keyState;

	private readonly int x;

	private readonly int y;

	private readonly DragDropEffects allowedEffect;

	private DragDropEffects effect;

	public object Data => data;

	public int KeyState => keyState;

	public int X => x;

	public int Y => y;

	public DragDropEffects AllowedEffect => allowedEffect;

	public DragDropEffects Effect
	{
		get
		{
			return effect;
		}
		set
		{
			effect = value;
		}
	}

	public DragEventArgs(object data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect)
	{
		this.data = data;
		this.keyState = keyState;
		this.x = x;
		this.y = y;
		this.allowedEffect = allowedEffect;
		this.effect = effect;
	}
}
