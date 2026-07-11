using System;
using System.Collections.Generic;
using System.Data;

namespace Sce.Atf.Applications;

public interface ISourceControlService
{
	bool AllowMultipleCheckout { get; set; }

	bool AllowCheckIn { get; set; }

	bool AllowOutOfDateEdit { get; set; }

	string DefaultConnection { get; set; }

	bool Enabled { get; set; }

	event EventHandler<SourceControlEventArgs> StatusChanged;

	event EventHandler<SourceControlResultCodeEventArgs> OperationCompleted;

	void UpdateCachedStatuses(Uri rootUri, bool resetCacheFirst);

	void BroadcastStatuses(IEnumerable<Uri> uris);

	void Add(Uri uri);

	void Delete(Uri uri);

	void CheckIn(IEnumerable<Uri> uris, string description);

	void CheckOut(Uri uri);

	void GetLatestVersion(Uri uri);

	void Revert(Uri uri);

	bool GetFolderStatus(Uri uri);

	SourceControlStatus GetStatus(Uri uri);

	SourceControlStatus[] GetStatus(IEnumerable<Uri> uris);

	IEnumerable<Uri> GetModifiedFiles(IEnumerable<Uri> uris);

	bool IsSynched(Uri uri);

	bool IsLocked(Uri uri);

	void RefreshStatus(Uri uri);

	void RefreshStatus(IEnumerable<Uri> uris);

	DataTable GetRevisionLog(Uri uri);

	void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision);

	void Add(IEnumerable<Uri> uris);

	void CheckOut(IEnumerable<Uri> uris);

	void Move(Uri src, Uri dest);
}
