using System;

namespace Sce.Atf;

public interface IDocument : IResource
{
	bool IsReadOnly { get; }

	bool Dirty { get; set; }

	event EventHandler DirtyChanged;
}
