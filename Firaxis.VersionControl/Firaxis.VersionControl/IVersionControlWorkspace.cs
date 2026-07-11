using System;
using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public interface IVersionControlWorkspace
{
	VersionControlContext Context { get; }

	string Name { get; }

	string Root { get; }

	IEnumerable<VersionControlPath> Paths { get; }

	IEnumerable<IVersionControlChange> PendingChanges { get; }

	IVersionControlChange FindPendingChangeByNumber(int changeListNumber);

	ResultCode UpdatePendingChangeLists();

	string GetLocalPath(string depotPath);

	string GetDepotPath(string localPath);

	void AddFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void EditFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void DeleteFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void RevertFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void GetLocalRevision(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void GetStatus(IEnumerable<VersionControlPath> files, Action<FileStatusRequestResultCode> result);

	void GetPendingChanges(string folderPath, Action<FileStatusRequestResultCode> result);

	bool HasPendingChanges(string folderPath);

	bool HasOutOfDateFiles(string folderPath);

	void GetUncontrolledFiles(string folderPath, Action<ResultCode, IEnumerable<string>> result);

	void AddChangeList(string desc, Action<ResultCode, int> result);

	void DeleteChangeList(int number, Action<ResultCode> result);

	void SyncToLatest(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void SyncToRevision(VersionControlPath file, int revision, Action<RequestResultCode> result);

	void SyncToChangelist(int changeList, Action<RequestResultCode> result);

	void RemoveFromWorkspace(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);
}
