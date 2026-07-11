using System;

namespace Sce.Atf.Applications;

public interface IHistoryContext
{
	bool CanUndo { get; }

	bool CanRedo { get; }

	string UndoDescription { get; }

	string RedoDescription { get; }

	bool Dirty { get; set; }

	event EventHandler DirtyChanged;

	void Clear();

	void Undo();

	void Redo();
}
