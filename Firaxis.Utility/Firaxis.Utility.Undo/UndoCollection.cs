using System;
using Firaxis.Collections;

namespace Firaxis.Utility.Undo;

public class UndoCollection
{
	private class UndoStack
	{
		public IUndo Undo;

		public bool DidUndo;

		public UndoStack(IUndo undo)
		{
			Undo = undo;
			DidUndo = false;
		}
	}

	private class UndoStackCollection : ListEvent<UndoStack>
	{
	}

	private UndoStackCollection undoStack;

	private int undoPoint;

	public bool CanUndo => !IsEmpty && undoPoint != -1;

	public bool CanRedo => !IsEmpty && undoPoint < undoStack.Count - 1;

	public bool IsEmpty => undoStack.Count == 0;

	public IUndo Current => CanUndo ? undoStack[undoPoint].Undo : null;

	public event UndoEventHandler PerformedUndo;

	public event UndoEventHandler PerformedRedo;

	public event EventHandler ClearedUndo;

	public event EventHandler StackChanged;

	public UndoCollection()
	{
		undoStack = new UndoStackCollection();
		undoPoint = -1;
	}

	private void NotifyStackChanged()
	{
		this.StackChanged?.Invoke(this, EventArgs.Empty);
	}

	public virtual IUndo Add(IUndo undo)
	{
		if (CanRedo)
		{
			undoStack.RemoveRange(undoPoint + 1, undoStack.Count - (undoPoint + 1));
		}
		undoStack.Add(new UndoStack(undo));
		undo.StoreUndo();
		undoPoint = undoStack.Count - 1;
		NotifyStackChanged();
		return undo;
	}

	public virtual void PerformUndo()
	{
		if (CanUndo)
		{
			UndoStack undoStack = this.undoStack[undoPoint];
			if (!undoStack.DidUndo)
			{
				undoStack.DidUndo = true;
				undoStack.Undo.StoreRedo();
			}
			IUndo undo = undoStack.Undo;
			undo.PerformUndo();
			undoPoint--;
			NotifyStackChanged();
			this.PerformedRedo?.Invoke(this, new UndoEventArgs(undo));
		}
	}

	public virtual void PerformRedo()
	{
		if (CanRedo)
		{
			undoPoint++;
			IUndo undo = undoStack[undoPoint].Undo;
			undo.PerformRedo();
			NotifyStackChanged();
			this.PerformedUndo?.Invoke(this, new UndoEventArgs(undo));
		}
	}

	public void Clear()
	{
		undoStack.Clear();
		undoPoint = -1;
		NotifyStackChanged();
		this.ClearedUndo?.Invoke(this, EventArgs.Empty);
	}
}
