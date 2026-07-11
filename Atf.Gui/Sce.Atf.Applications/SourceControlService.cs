using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;

namespace Sce.Atf.Applications;

[InheritedExport(typeof(SourceControlService))]
public abstract class SourceControlService : ISourceControlService
{
	private bool m_enabled;

	public virtual bool IsConnected { get; set; }

	public virtual bool CanConfigure => true;

	public virtual bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			if (value != Enabled)
			{
				m_enabled = value;
				OnEnabledChanged(EventArgs.Empty);
			}
		}
	}

	public bool ThrowExceptions { get; set; }

	public bool AllowMultipleCheckout { get; set; }

	public bool AllowCheckIn { get; set; }

	public bool AllowOutOfDateEdit { get; set; }

	public virtual string DefaultConnection { get; set; }

	public event EventHandler ConnectionChanged;

	public event EventHandler EnabledChanged;

	public event EventHandler<SourceControlEventArgs> StatusChanged;

	public event EventHandler<SourceControlResultCodeEventArgs> OperationCompleted;

	protected SourceControlService()
	{
		AllowCheckIn = true;
	}

	public virtual bool Connect()
	{
		return true;
	}

	public virtual void Disconnect()
	{
	}

	public abstract void UpdateCachedStatuses(Uri rootUri, bool resetCacheFirst);

	public abstract void BroadcastStatuses(IEnumerable<Uri> uris);

	public abstract void Add(Uri uri);

	public abstract void Delete(Uri uri);

	public abstract void CheckIn(IEnumerable<Uri> uris, string description);

	public abstract void CheckOut(Uri uri);

	public abstract void GetLatestVersion(Uri uri);

	public abstract void Revert(Uri uri);

	public abstract bool GetFolderStatus(Uri uri);

	public abstract SourceControlStatus GetStatus(Uri uri);

	public abstract SourceControlStatus[] GetStatus(IEnumerable<Uri> uris);

	public abstract IEnumerable<Uri> GetModifiedFiles(IEnumerable<Uri> uris);

	public abstract bool IsSynched(Uri uri);

	public abstract bool IsLocked(Uri uri);

	public abstract void RefreshStatus(Uri uri);

	public abstract void RefreshStatus(IEnumerable<Uri> uris);

	public abstract DataTable GetRevisionLog(Uri uri);

	public abstract void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision);

	public abstract SourceControlFileInfo GetFileInfo(Uri uri);

	public virtual Image GetSourceControlStatusIcon(Uri uri, SourceControlStatus status)
	{
		return null;
	}

	protected virtual void OnStatusChanged(SourceControlEventArgs e)
	{
		this.StatusChanged.Raise(this, e);
	}

	protected virtual void OnConnectionChanged(EventArgs e)
	{
		this.ConnectionChanged.Raise(this, e);
	}

	protected virtual void OnEnabledChanged(EventArgs e)
	{
		this.EnabledChanged.Raise(this, e);
	}

	public abstract void Add(IEnumerable<Uri> uris);

	public abstract void CheckOut(IEnumerable<Uri> uris);

	public virtual bool ConnectSilently()
	{
		return true;
	}

	protected virtual void OnOperationCompleted(SourceControlResultCodeEventArgs e)
	{
		this.OperationCompleted.Raise(this, e);
	}

	public abstract void Move(Uri src, Uri dest);
}
