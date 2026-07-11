using System;

namespace Firaxis.CivTech;

public interface IWorkspaceDependencyWatcher : IDisposable
{
	event EventHandler<WorkspaceItemChangedEvent> WorkspaceItemChanged;

	event EventHandler<WorkspaceItemChangedEvent> WorkspaceItemRemoved;

	event EventHandler<WorkspaceItemChangedEvent> WorkspaceItemAdded;

	event EventHandler<WorkspaceItemRenamedEvent> WorkspaceItemRenamed;

	void DisableFileWatches();

	void EnableFileWatches();
}
