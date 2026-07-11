using System.Collections.Generic;
using Firaxis.VersionControl;

namespace Firaxis.CivTech;

public interface IVersionControlService : IProjectRootProvider
{
	IVersionControlWorkspace Workspace { get; }

	IVersionControlServer Server { get; }

	bool IsConnected { get; }

	bool Reconnect(string password);

	int GetNumPendingOperations(out string errMsg);

	string GetDepotPath(string path);

	string GetLocalPath(string path);

	bool GetLatest(string filePath, out string errMsg);

	bool GetLatest(IEnumerable<string> filePaths, IList<string> errMsg);

	bool GetRevision(int revision, string filePath, out string errMsg);

	bool AddFile(string filePath, out string errMsg);

	bool AddFiles(IEnumerable<string> filePaths, IList<string> errMsg);

	bool EditFile(string filePath, out string errMsg);

	bool EditFiles(IEnumerable<string> filePaths, IList<string> errMsg);

	bool DeleteFile(string filePath, out string errMsg);

	bool DeleteFiles(IEnumerable<string> filePaths, IList<string> errMsg);

	bool RevertFile(string filePath, out string errMsg);

	bool RevertFiles(IEnumerable<string> filePaths, IList<string> errMsg);

	FileStatusRequestResultCode GetStatus(string filePath);

	FileStatusRequestResultCode GetStatus(IEnumerable<string> filePaths);

	bool AddChangeList(string description, out int changeListNo, out string errMsg);

	bool RemoveChangeList(int changeListNo, out string errMsg);

	bool AddToChangelist(IEnumerable<string> filePaths, int changeListNo, IList<string> errMsg);

	bool RemoveFromChangelist(IEnumerable<string> filePaths, IList<string> errMsg);

	bool SubmitChangeList(int changeListNo, IList<string> errMsg);

	IEnumerable<string> GetUncontrolledFiles(string folderPath);

	FileStatusRequestResultCode GetPendingChanges(string filePath);

	bool HasPendingChanges(string folderPath);

	bool HasOutOfDateFiles(string folderPath);

	bool IsVersionControlled(string filePath);

	bool IsEditible(string filePath);

	bool IsCurrentVersion(string filePath);

	bool IsMarkedForAdd(string filePath);

	bool IsMarkedForDelete(string filePath);

	bool IsAddingBackDeletedFile(string filePath);
}
