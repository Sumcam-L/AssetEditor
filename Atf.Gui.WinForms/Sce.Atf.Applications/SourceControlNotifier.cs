using System;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(ISourceControlNotifier))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SourceControlNotifier : ISourceControlNotifier
{
	public event EventHandler SourceControlReady;

	public void Notify()
	{
		this.SourceControlReady?.Invoke(this, EventArgs.Empty);
	}
}
