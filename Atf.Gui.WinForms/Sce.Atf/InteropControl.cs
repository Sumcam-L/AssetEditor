using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

public class InteropControl : Control
{
	protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
	{
		OnKeyDown(KeyEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnKeyDown(Sce.Atf.Input.KeyEventArgs e)
	{
		base.OnKeyDown(KeyEventArgsInterop.ToWf(e));
	}

	protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
	{
		OnKeyUp(KeyEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnKeyUp(Sce.Atf.Input.KeyEventArgs e)
	{
		base.OnKeyUp(KeyEventArgsInterop.ToWf(e));
	}

	protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
	{
		return IsInputKey(KeysInterop.ToAtf(keyData));
	}

	protected virtual bool IsInputKey(Sce.Atf.Input.Keys keyData)
	{
		return base.IsInputKey(KeysInterop.ToWf(keyData));
	}

	protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
	{
		Sce.Atf.Input.Message msg2 = MessageInterop.ToAtf(msg);
		return ProcessCmdKey(ref msg2, KeysInterop.ToAtf(keyData));
	}

	protected virtual bool ProcessCmdKey(ref Sce.Atf.Input.Message msg, Sce.Atf.Input.Keys keyData)
	{
		System.Windows.Forms.Message msg2 = MessageInterop.ToWf(msg);
		return base.ProcessCmdKey(ref msg2, KeysInterop.ToWf(keyData));
	}

	protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
	{
		OnMouseWheel(MouseEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnMouseWheel(Sce.Atf.Input.MouseEventArgs e)
	{
		base.OnMouseWheel(MouseEventArgsInterop.ToWf(e));
	}

	protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
	{
		OnMouseDown(MouseEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnMouseDown(Sce.Atf.Input.MouseEventArgs e)
	{
		base.OnMouseDown(MouseEventArgsInterop.ToWf(e));
	}

	protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
	{
		OnMouseMove(MouseEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnMouseMove(Sce.Atf.Input.MouseEventArgs e)
	{
		base.OnMouseMove(MouseEventArgsInterop.ToWf(e));
	}

	protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
	{
		OnMouseUp(MouseEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnMouseUp(Sce.Atf.Input.MouseEventArgs e)
	{
		base.OnMouseUp(MouseEventArgsInterop.ToWf(e));
	}

	protected override void OnDragEnter(System.Windows.Forms.DragEventArgs e)
	{
		OnDragEnter(DragEventArgsInterop.ToAtf(e));
	}

	protected virtual void OnDragEnter(Sce.Atf.Input.DragEventArgs e)
	{
		base.OnDragEnter(DragEventArgsInterop.ToWf(e));
	}
}
