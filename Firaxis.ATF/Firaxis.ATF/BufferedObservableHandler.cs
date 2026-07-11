using System;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

public class BufferedObservableHandler
{
	public class IgnoreUpdatesReceipt : IDisposable
	{
		private BufferedObservableHandler m_handler;

		internal IgnoreUpdatesReceipt(BufferedObservableHandler handler)
		{
			m_handler = handler;
			m_handler.SuspendUpdates();
		}

		public void Dispose()
		{
			m_handler.ResumeUpdates();
			m_handler = null;
		}
	}

	private enum UpdateStatus
	{
		NoUpdateNeeded,
		UpdateNeeded,
		RebuildNeeded,
		ReloadNeeded,
		UpdatesIgnored
	}

	private UpdateStatus m_updateStatus;

	private int m_updateDepth;

	private IObservableContext ObservableContext { get; set; }

	private IValidationContext ValidationContext { get; set; }

	private Action UpdateFunctor { get; set; }

	private Action ReloadFunctor { get; set; }

	private Action RebuildFunctor { get; set; }

	public BufferedObservableHandler()
	{
	}

	public BufferedObservableHandler(IValidationContext validationContext, IObservableContext observableContext, Action updateFunctor, Action rebuildFunctor, Action reloadFunctor)
	{
		Bind(validationContext, observableContext, updateFunctor, rebuildFunctor, reloadFunctor);
	}

	public void Bind(IValidationContext validationContext, IObservableContext observableContext, Action updateFunctor, Action rebuildFunctor, Action reloadFunctor)
	{
		ObservableContext = observableContext;
		ValidationContext = validationContext;
		UpdateFunctor = updateFunctor;
		RebuildFunctor = rebuildFunctor;
		ReloadFunctor = reloadFunctor;
		ObservableContext.ItemChanged += ObservableContext_ItemChanged;
		ObservableContext.ItemInserted += ObservableContext_ItemInserted;
		ObservableContext.ItemRemoved += ObservableContext_ItemRemoved;
		ObservableContext.Reloaded += ObservableContext_Reloaded;
		ValidationContext.Beginning += ValidationContext_Beginning;
		ValidationContext.Ended += ValidationContext_Ended;
		ValidationContext.Cancelled += ValidationContext_Cancelled;
	}

	public IgnoreUpdatesReceipt ScopedSuspendUpdates()
	{
		return new IgnoreUpdatesReceipt(this);
	}

	private void SuspendUpdates()
	{
		BugSubmitter.SilentAssert(m_updateStatus != UpdateStatus.UpdatesIgnored, "m_updateStatus was left as UpdateStatus.UpdatesIgnored with m_updateDepth of {0} before suspending updates @summary m_updateStatus was left as UpdateStatus.UpdatesIgnored before suspending updates @assign bwhitman", m_updateDepth);
		m_updateStatus = UpdateStatus.UpdatesIgnored;
	}

	private void ResumeUpdates()
	{
		BugSubmitter.SilentAssert(m_updateStatus == UpdateStatus.UpdatesIgnored, "m_updateStatus was left as {0} with an m_updateDepth of {1} when attempting to resume updates @summary m_updateStatus was not UpdateStatus.UpdatesIgnored when attempting to resume updates @assign bwhitman", m_updateStatus, m_updateDepth);
		m_updateStatus = UpdateStatus.NoUpdateNeeded;
	}

	private void ObservableContext_Reloaded(object sender, EventArgs e)
	{
		DoReload();
	}

	protected void ObservableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		DoUpdate();
	}

	private void ObservableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		DoRebuild();
	}

	private void ObservableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		DoRebuild();
	}

	private void ValidationContext_Cancelled(object sender, EventArgs e)
	{
		EndUpdate();
	}

	private void ValidationContext_Ended(object sender, EventArgs e)
	{
		EndUpdate();
	}

	private void ValidationContext_Beginning(object sender, EventArgs e)
	{
		BeginUpdate();
	}

	private void BeginUpdate()
	{
		m_updateDepth++;
	}

	private void DoUpdate()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus < UpdateStatus.UpdateNeeded)
			{
				m_updateStatus = UpdateStatus.UpdateNeeded;
			}
		}
		else
		{
			UpdateFunctor?.Invoke();
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}

	private void DoRebuild()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus < UpdateStatus.RebuildNeeded)
			{
				m_updateStatus = UpdateStatus.RebuildNeeded;
			}
		}
		else
		{
			RebuildFunctor?.Invoke();
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}

	private void DoReload()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus < UpdateStatus.ReloadNeeded)
			{
				m_updateStatus = UpdateStatus.ReloadNeeded;
			}
		}
		else
		{
			ReloadFunctor?.Invoke();
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}

	private void EndUpdate()
	{
		m_updateDepth--;
		if (m_updateDepth < 0)
		{
			m_updateDepth = 0;
			m_updateStatus = UpdateStatus.ReloadNeeded;
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.ExtremelyVerbose, "Ended update while not updating in {0}!", GetType().Name);
		}
		if (m_updateDepth == 0)
		{
			switch (m_updateStatus)
			{
			case UpdateStatus.UpdateNeeded:
				DoUpdate();
				break;
			case UpdateStatus.RebuildNeeded:
				DoRebuild();
				break;
			case UpdateStatus.ReloadNeeded:
				DoReload();
				break;
			}
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}
}
