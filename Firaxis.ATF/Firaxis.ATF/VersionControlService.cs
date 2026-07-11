using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.Error;
using Firaxis.VersionControl;
using Sce.Atf;

namespace Firaxis.ATF;

public class VersionControlService : IVersionControlService, IProjectRootProvider
{
	private IVersionControlServer m_server;

	private VersionControlInfo m_versionControlInfo;

	private IVersionControlWorkspace m_workspace;

	public bool IsConnected => m_server.IsConnected;

	public IVersionControlServer Server => m_server;

	public VersionControlInfo VersionControlInfo => m_versionControlInfo;

	public IVersionControlWorkspace Workspace => m_workspace;

	public string WorkspaceRoot => m_workspace.Root;

	public int GetNumPendingOperations(out string errMsg)
	{
		string errLocal = string.Empty;
		int pendingOps = 0;
		m_server.GetNumPendingOperations(delegate(ResultCode res, int pendingCnt)
		{
			if (res == ResultCode.Success)
			{
				pendingOps = pendingCnt;
			}
			else
			{
				errLocal = res.Message;
			}
		});
		errMsg = errLocal;
		return pendingOps;
	}

	public VersionControlService(VersionControlInfo vcsInfo)
	{
		m_versionControlInfo = vcsInfo;
		ResultCode resultCode = Connect();
		if (!resultCode)
		{
			throw new ResultCodeException(resultCode, "Failed to connect to version control server\n\n" + resultCode.Message);
		}
	}

	public bool AddChangeList(string description, out int changeListNo, out string errMsg)
	{
		bool result = false;
		string errLocal = string.Empty;
		int chgLocal = 0;
		Workspace.AddChangeList(description, delegate(ResultCode res, int chgNo)
		{
			if (res == ResultCode.Success)
			{
				result = true;
				chgLocal = chgNo;
			}
			else
			{
				errLocal = res.Message;
			}
		});
		errMsg = errLocal;
		changeListNo = chgLocal;
		return result;
	}

	public bool AddFile(string filePath, out string errMsg)
	{
		IList<string> list = new List<string>();
		bool result = AddFiles(new string[1] { filePath }, list);
		if (list.Any())
		{
			errMsg = string.Join("\n", list);
			return result;
		}
		errMsg = string.Empty;
		return result;
	}

	public bool AddFiles(IEnumerable<string> filePaths, IList<string> errMsg)
	{
		bool result = false;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		m_workspace.AddFiles(files, delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					ActionResultCode actionResultCode = itemRes as ActionResultCode;
					string text = string.Join("\n\t", actionResultCode.AdditionalInfo);
					if (!string.IsNullOrEmpty(text))
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message + "\n\t" + text);
					}
					else
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	public bool AddToChangelist(IEnumerable<string> filePaths, int changeListNo, IList<string> errMsg)
	{
		IVersionControlChange versionControlChange = Workspace.FindPendingChangeByNumber(changeListNo);
		if (versionControlChange == null)
		{
			errMsg.Add($"Changelist #{changeListNo} not found in current workspace.");
			return false;
		}
		bool result = false;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		versionControlChange.AddFiles(files, delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					ActionResultCode actionResultCode = itemRes as ActionResultCode;
					string text = string.Join("\n\t", actionResultCode.AdditionalInfo);
					if (!string.IsNullOrEmpty(text))
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message + "\n\t" + text);
					}
					else
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	public bool DeleteFile(string filePath, out string errMsg)
	{
		IList<string> list = new List<string>();
		bool result = DeleteFiles(new string[1] { filePath }, list);
		if (list.Any())
		{
			errMsg = string.Join("\n", list);
			return result;
		}
		errMsg = string.Empty;
		return result;
	}

	public bool DeleteFiles(IEnumerable<string> filePaths, IList<string> errMsg)
	{
		bool result = false;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		m_workspace.DeleteFiles(files, delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					ActionResultCode actionResultCode = itemRes as ActionResultCode;
					string text = string.Join("\n\t", actionResultCode.AdditionalInfo);
					if (!string.IsNullOrEmpty(text))
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message + "\n\t" + text);
					}
					else
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	public bool EditFile(string filePath, out string errMsg)
	{
		IList<string> list = new List<string>();
		bool result = EditFiles(new string[1] { filePath }, list);
		if (list.Any())
		{
			errMsg = string.Join("\n", list);
			return result;
		}
		errMsg = string.Empty;
		return result;
	}

	public bool EditFiles(IEnumerable<string> filePaths, IList<string> errMsg)
	{
		bool result = false;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		m_workspace.EditFiles(files, delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					ActionResultCode actionResultCode = itemRes as ActionResultCode;
					string text = string.Join("\n\t", actionResultCode.AdditionalInfo);
					if (!string.IsNullOrEmpty(text))
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message + "\n\t" + text);
					}
					else
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	public string GetDepotPath(string path)
	{
		return m_workspace.GetDepotPath(path);
	}

	public bool GetLatest(IEnumerable<string> filePaths, IList<string> errMsg)
	{
		bool result = false;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		m_workspace.SyncToLatest(files, delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					if (!itemRes.Result)
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	public bool GetLatest(string filePath, out string errMsg)
	{
		IList<string> list = new List<string>();
		bool latest = GetLatest(new string[1] { filePath }, list);
		if (list.Any())
		{
			errMsg = string.Join("\n", list);
			return latest;
		}
		errMsg = string.Empty;
		return latest;
	}

	public string GetLocalPath(string path)
	{
		string localPath = m_workspace.GetLocalPath(path);
		if (string.IsNullOrEmpty(localPath) && !IsChildWorkspaceRoot(path))
		{
			return path;
		}
		return localPath;
	}

	public FileStatusRequestResultCode GetPendingChanges(string folderPath)
	{
		if (!IsChildWorkspaceRoot(folderPath))
		{
			return new FileStatusRequestResultCode(new ResultCode("Path not part of workspace"));
		}
		FileStatusRequestResultCode result = null;
		m_workspace.GetPendingChanges(folderPath, delegate(FileStatusRequestResultCode res)
		{
			result = res;
		});
		return result;
	}

	public bool GetRevision(int revision, string filePath, out string errMsg)
	{
		string msg = string.Empty;
		m_workspace.SyncToRevision(new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)), revision, delegate(RequestResultCode res)
		{
			if (res.Result != ResultCode.Success)
			{
				msg = res.ItemResults.First().Result.Message;
			}
		});
		errMsg = msg;
		return string.IsNullOrEmpty(msg);
	}

	public FileStatusRequestResultCode GetStatus(IEnumerable<string> filePaths)
	{
		FileStatusRequestResultCode result = null;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		m_workspace.GetStatus(files, delegate(FileStatusRequestResultCode res)
		{
			result = res;
		});
		return result;
	}

	public FileStatusRequestResultCode GetStatus(string filePath)
	{
		return GetStatus(new string[1] { filePath });
	}

	public IEnumerable<string> GetUncontrolledFiles(string folderPath)
	{
		if (!IsChildWorkspaceRoot(folderPath))
		{
			return Enumerable.Empty<string>();
		}
		IEnumerable<string> files = Enumerable.Empty<string>();
		m_workspace.GetUncontrolledFiles(folderPath, delegate(ResultCode resCode, IEnumerable<string> resFiles)
		{
			if (resCode == ResultCode.Success)
			{
				files = resFiles;
			}
		});
		return files;
	}

	public bool HasOutOfDateFiles(string folderPath)
	{
		return m_workspace.HasOutOfDateFiles(folderPath);
	}

	public bool HasPendingChanges(string folderPath)
	{
		if (!IsChildWorkspaceRoot(folderPath))
		{
			return false;
		}
		return m_workspace.HasPendingChanges(folderPath);
	}

	public bool IsAddingBackDeletedFile(string filePath)
	{
		if (!IsChildWorkspaceRoot(filePath))
		{
			return true;
		}
		FileStatusRequestResultCode status = GetStatus(filePath);
		if (!status.Result)
		{
			return false;
		}
		VersionControlStatus status2 = status.Status.First().Value.Status;
		if (status2.Working.Action == VersionControlActionType.Add || status2.Working.Action == VersionControlActionType.MoveAdd)
		{
			return status2.Head.Action == VersionControlActionType.Delete;
		}
		return false;
	}

	public bool IsCurrentVersion(string filePath)
	{
		if (!IsChildWorkspaceRoot(filePath))
		{
			return true;
		}
		FileStatusRequestResultCode status = GetStatus(filePath);
		if (!status.Result)
		{
			return false;
		}
		return status.Status.First().Value.Status.LocalRevision == status.Status.First().Value.Status.Head.Revision;
	}

	public bool IsEditible(string filePath)
	{
		if (!IsChildWorkspaceRoot(filePath))
		{
			if (!File.Exists(filePath))
			{
				return false;
			}
			if (new FileInfo(filePath).IsReadOnly)
			{
				return false;
			}
			return true;
		}
		FileStatusRequestResultCode status = GetStatus(filePath);
		if (!status.Result)
		{
			return false;
		}
		if (status.Status.First().Value.Status.Working.Action != VersionControlActionType.Edit)
		{
			return status.Status.First().Value.Status.Working.Action == VersionControlActionType.Add;
		}
		return true;
	}

	public bool IsMarkedForAdd(string filePath)
	{
		if (!IsChildWorkspaceRoot(filePath))
		{
			return false;
		}
		FileStatusRequestResultCode status = GetStatus(filePath);
		if (!status.Result)
		{
			return false;
		}
		if (status.Status.Any())
		{
			return status.Status.First().Value.Status.Working.Action == VersionControlActionType.Add;
		}
		return false;
	}

	public bool IsMarkedForDelete(string filePath)
	{
		if (!IsChildWorkspaceRoot(filePath))
		{
			return false;
		}
		FileStatusRequestResultCode status = GetStatus(filePath);
		if (!status.Result)
		{
			return false;
		}
		if (status.Status.Any())
		{
			VersionControlStatus status2 = status.Status.First().Value.Status;
			if (status2.Working.Action != VersionControlActionType.Add && status2.Working.Action != VersionControlActionType.MoveAdd)
			{
				if (status2.Working.Action != VersionControlActionType.Delete)
				{
					return status2.Head.Action == VersionControlActionType.Delete;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public bool IsVersionControlled(string filePath)
	{
		if (!IsChildWorkspaceRoot(filePath))
		{
			return false;
		}
		FileStatusRequestResultCode status = GetStatus(filePath);
		if (!status.Result)
		{
			return false;
		}
		if (status.Status.First().Value.Status.Working.Action != VersionControlActionType.Add)
		{
			if (status.Status.First().Value.Status.Head.Revision >= 0)
			{
				return status.Status.First().Value.Status.Head.Action != VersionControlActionType.Delete;
			}
			return false;
		}
		return true;
	}

	public bool Reconnect(string password)
	{
		return m_server.Reconnect(password);
	}

	public bool RemoveChangeList(int changeListNo, out string errMsg)
	{
		bool result = false;
		string errLocal = string.Empty;
		Workspace.DeleteChangeList(changeListNo, delegate(ResultCode res)
		{
			if (res == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errLocal = res.Message;
			}
		});
		errMsg = errLocal;
		return result;
	}

	public bool RemoveFromChangelist(IEnumerable<string> filePaths, IList<string> errMsg)
	{
		return AddToChangelist(filePaths, 0, errMsg);
	}

	public bool RevertFile(string filePath, out string errMsg)
	{
		IList<string> list = new List<string>();
		bool result = RevertFiles(new string[1] { filePath }, list);
		if (list.Any())
		{
			errMsg = string.Join("\n", list);
			return result;
		}
		errMsg = string.Empty;
		return result;
	}

	public bool RevertFiles(IEnumerable<string> filePaths, IList<string> errMsg)
	{
		bool result = false;
		IEnumerable<VersionControlPath> files = filePaths.Select((string filePath) => new VersionControlPath(m_workspace.GetDepotPath(filePath), m_workspace.GetLocalPath(filePath)));
		m_workspace.RevertFiles(files, delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					ActionResultCode actionResultCode = itemRes as ActionResultCode;
					string text = string.Join("\n\t", actionResultCode.AdditionalInfo);
					if (!string.IsNullOrEmpty(text))
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message + "\n\t" + text);
					}
					else
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	public bool SubmitChangeList(int changeListNo, IList<string> errMsg)
	{
		IVersionControlChange versionControlChange = Workspace.FindPendingChangeByNumber(changeListNo);
		if (versionControlChange == null)
		{
			errMsg.Add($"Changelist #{changeListNo} not found in current workspace.");
			return false;
		}
		bool result = false;
		versionControlChange.Submit(delegate(RequestResultCode res)
		{
			if (res.Result == ResultCode.Success)
			{
				result = true;
			}
			else
			{
				errMsg.Add(res.Result.Message);
				res.ItemResults.ForEach(delegate(ItemResultCode itemRes)
				{
					ActionResultCode actionResultCode = itemRes as ActionResultCode;
					string text = string.Join("\n\t", actionResultCode.AdditionalInfo);
					if (!string.IsNullOrEmpty(text))
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message + "\n\t" + text);
					}
					else
					{
						errMsg.Add(itemRes.File.DepotPath + " - " + itemRes.Result.Message);
					}
				});
			}
		});
		return result;
	}

	private ResultCode Connect()
	{
		if (m_workspace != null)
		{
			return ResultCode.Success;
		}
		ResultCode result = ResultCode.Success;
		if (GetVersionControlInformation(out var connectionURI, out var workspace))
		{
			VersionControlConnector.Connect(connectionURI, out m_server, delegate(ResultCode res)
			{
				if (res == ResultCode.Success)
				{
					m_workspace = m_server.FindWorkspaceByName(workspace);
					if (m_workspace == null)
					{
						result = new ResultCode("Could not find workspace \"" + workspace + "\" associated with server \"" + m_server.Context.Host + "\"");
					}
				}
				else
				{
					result = new ResultCode("Failed to connect to server \"" + connectionURI + "\"\n\n" + res.Message);
				}
			});
		}
		else
		{
			result = new ResultCode("Failed to get version control configuration information from windows registry");
		}
		return result;
	}

	private bool GetVersionControlInformation(out string uri, out string workspace)
	{
		uri = VersionControlInfo.Uri;
		workspace = VersionControlInfo.Workspace;
		return true;
	}

	private bool IsChildWorkspaceRoot(string filePath)
	{
		if (!filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).StartsWith(m_workspace.Root, StringComparison.CurrentCultureIgnoreCase) && !m_workspace.GetLocalPath(filePath).StartsWith(m_workspace.Root, StringComparison.CurrentCultureIgnoreCase))
		{
			return false;
		}
		return true;
	}
}
