using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.VersionControl;

public class LocalVersionControlWorkspace : IVersionControlWorkspace
{
	public VersionControlContext Context { get; private set; }

	public string Name { get; private set; }

	public string Root { get; private set; }

	public string DepotRoot { get; private set; }

	public IEnumerable<VersionControlPath> Paths => Enumerable.Empty<VersionControlPath>();

	public IEnumerable<IVersionControlChange> PendingChanges => Enumerable.Empty<IVersionControlChange>();

	public LocalVersionControlWorkspace(VersionControlContext context, string name, string root)
	{
		Context = context;
		Name = name;
		Root = root.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		if (!string.IsNullOrEmpty(Root) && Root[Root.Length - 1] != Path.DirectorySeparatorChar)
		{
			Root += Path.DirectorySeparatorChar;
		}
		DepotRoot = "//civ6/main/";
	}

	public string GetDepotPath(string localPath)
	{
		if (localPath.StartsWith(DepotRoot, StringComparison.CurrentCultureIgnoreCase))
		{
			return localPath;
		}
		return localPath.Replace(Root, DepotRoot);
	}

	public string GetLocalPath(string depotPath)
	{
		if (depotPath.StartsWith(Root, StringComparison.CurrentCultureIgnoreCase))
		{
			return depotPath;
		}
		return depotPath.Replace(DepotRoot, Root);
	}

	public void GetStatus(IEnumerable<VersionControlPath> files, Action<FileStatusRequestResultCode> result)
	{
		IDictionary<string, FileStatusResultCode> fileResults = new ConcurrentDictionary<string, FileStatusResultCode>(new PathComparer());
		Parallel.ForEach(files, delegate(VersionControlPath file)
		{
			VersionControlStatus versionControlStatus = new VersionControlStatus();
			try
			{
				versionControlStatus.Head.Action = ((!File.GetAttributes(file.WorkspacePath).HasFlag(FileAttributes.ReadOnly)) ? VersionControlActionType.Edit : VersionControlActionType.None);
			}
			catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is NotSupportedException || ex is UnauthorizedAccessException)
			{
				versionControlStatus.Head.Action = VersionControlActionType.None;
			}
			versionControlStatus.Head.Change = 1;
			versionControlStatus.Head.Modified = File.GetLastWriteTime(file.WorkspacePath);
			versionControlStatus.Head.Revision = (File.Exists(file.WorkspacePath) ? 1 : (-1));
			versionControlStatus.Working.Action = versionControlStatus.Head.Action;
			versionControlStatus.Working.Change = versionControlStatus.Head.Change;
			versionControlStatus.Working.Modified = versionControlStatus.Head.Modified;
			versionControlStatus.Working.Revision = versionControlStatus.Head.Revision;
			versionControlStatus.LocalRevision = versionControlStatus.Working.Revision;
			fileResults[file.DepotPath] = new FileStatusResultCode(file, ResultCode.Success, versionControlStatus);
		});
		result(new FileStatusRequestResultCode(fileResults));
	}

	public void GetUncontrolledFiles(string folderPath, Action<ResultCode, IEnumerable<string>> result)
	{
		result(ResultCode.Success, Enumerable.Empty<string>());
	}

	public void GetPendingChanges(string folderPath, Action<FileStatusRequestResultCode> result)
	{
		result(new FileStatusRequestResultCode(ResultCode.Success));
	}

	public bool HasPendingChanges(string folderPath)
	{
		return true;
	}

	public bool HasOutOfDateFiles(string folderPath)
	{
		return false;
	}

	public void AddChangeList(string desc, Action<ResultCode, int> result)
	{
		result(ResultCode.Success, 0);
	}

	public void AddFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void DeleteChangeList(int number, Action<ResultCode> result)
	{
		result(ResultCode.Success);
	}

	public void DeleteFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void EditFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public IVersionControlChange FindPendingChangeByNumber(int changeListNumber)
	{
		return null;
	}

	public void GetLocalRevision(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void RemoveFromWorkspace(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void RevertFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void SyncToChangelist(int changeList, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void SyncToLatest(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public void SyncToRevision(VersionControlPath file, int revision, Action<RequestResultCode> result)
	{
		result(new RequestResultCode(ResultCode.Success, Enumerable.Empty<ItemResultCode>()));
	}

	public ResultCode UpdatePendingChangeLists()
	{
		return ResultCode.Success;
	}
}
