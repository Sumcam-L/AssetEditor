using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.VersionControl;

public class PerforceVersionControlWorkspace : IVersionControlWorkspace
{
	private const int kMaxRequestItemCount = 200;

	private VersionControlContext m_context;

	private string m_name;

	private string m_rootPath;

	private IList<VersionControlPath> m_pathInfo = new List<VersionControlPath>();

	private IDictionary<int, IVersionControlChange> m_pendingChanges = new Dictionary<int, IVersionControlChange>();

	public VersionControlContext Context => m_context;

	public string Name => m_name;

	public string Root => m_rootPath;

	public IEnumerable<VersionControlPath> Paths => m_pathInfo;

	public IEnumerable<IVersionControlChange> PendingChanges
	{
		get
		{
			UpdatePendingChangeLists();
			return m_pendingChanges.Values;
		}
	}

	public PerforceVersionControlWorkspace(VersionControlContext context, string name, string root, IEnumerable<VersionControlPath> paths)
	{
		m_context = context;
		m_name = name;
		m_rootPath = root.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		m_pathInfo = new List<VersionControlPath>(paths);
		ResultCode resultCode = UpdatePendingChangeLists();
		if (resultCode != ResultCode.Success)
		{
			throw new Exception(resultCode.Message);
		}
	}

	public IVersionControlChange FindPendingChangeByNumber(int changeListNumber)
	{
		if (!m_pendingChanges.ContainsKey(changeListNumber))
		{
			return null;
		}
		return m_pendingChanges[changeListNumber];
	}

	private VersionControlPath GetPertinentDepotPath(string depotPath)
	{
		VersionControlPath versionControlPath = null;
		string text = depotPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		foreach (VersionControlPath item in m_pathInfo)
		{
			if (!text.StartsWith(item.DepotPath, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			if (versionControlPath != null)
			{
				if (item.DepotPath.Length > versionControlPath.DepotPath.Length)
				{
					versionControlPath = item;
				}
			}
			else
			{
				versionControlPath = item;
			}
		}
		return versionControlPath;
	}

	private VersionControlPath GetPertinentLocalPath(string localPath)
	{
		VersionControlPath versionControlPath = null;
		string text = localPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		foreach (VersionControlPath item in m_pathInfo)
		{
			if (!text.StartsWith(item.WorkspacePath, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			if (versionControlPath != null)
			{
				if (item.WorkspacePath.Length > versionControlPath.WorkspacePath.Length)
				{
					versionControlPath = item;
				}
			}
			else
			{
				versionControlPath = item;
			}
		}
		return versionControlPath;
	}

	public string GetLocalPath(string depotPath)
	{
		VersionControlPath pertinentDepotPath = GetPertinentDepotPath(depotPath);
		if (pertinentDepotPath == null)
		{
			pertinentDepotPath = GetPertinentLocalPath(depotPath);
			if (pertinentDepotPath != null)
			{
				return depotPath;
			}
			return string.Empty;
		}
		string text = Path.Combine(pertinentDepotPath.WorkspacePath, depotPath.Substring(pertinentDepotPath.DepotPath.Length).TrimStart('/'));
		return text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	public string GetDepotPath(string localPath)
	{
		VersionControlPath pertinentLocalPath = GetPertinentLocalPath(localPath);
		if (pertinentLocalPath == null)
		{
			pertinentLocalPath = GetPertinentDepotPath(localPath);
			if (pertinentLocalPath != null)
			{
				return localPath;
			}
			return string.Empty;
		}
		string text = pertinentLocalPath.DepotPath + localPath.Substring(pertinentLocalPath.WorkspacePath.Length);
		return text.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	public void AddChangeList(string desc, Action<ResultCode, int> result)
	{
		Regex changeExtraction = new Regex("^(Change)\\s+(\\d+)\\s+(created)\\.", RegexOptions.Compiled);
		string stdinData = GenerateChangelistSpecification(desc);
		PerforceRequestHelper.Run(Context, $"-c {m_name} change -i", stdinData, delegate(string res, string err)
		{
			ResultCode res2 = ResultCode.Success;
			if (PerforceRequestHelper.IsGlobalError(err, ref res2))
			{
				result(res2, 0);
			}
			else
			{
				ResultCode resultCode = UpdatePendingChangeLists();
				if (resultCode == ResultCode.Success)
				{
					MatchCollection matchCollection = changeExtraction.Matches(res);
					if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 4)
					{
						int result2 = 0;
						if (int.TryParse(matchCollection[0].Groups[2].Value, out result2))
						{
							result(ResultCode.Success, result2);
						}
						else
						{
							result(new ResultCode("Failed to parse change list number"), 0);
						}
					}
					else
					{
						result(new ResultCode(err), 0);
					}
				}
				else
				{
					result(resultCode, 0);
				}
			}
		});
	}

	public void DeleteChangeList(int number, Action<ResultCode> result)
	{
		PerforceRequestHelper.Run(Context, $"-c {m_name} change -d {number}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				ResultCode resultCode = UpdatePendingChangeLists();
				if (resultCode == ResultCode.Success)
				{
					if (res.StartsWith($"Change {number} deleted."))
					{
						result(ResultCode.Success);
					}
					else
					{
						result(new ResultCode((err.Length > 0) ? err : res));
					}
				}
				else
				{
					result(resultCode);
				}
			}
		});
	}

	private void SplitPerforceFileRequest(IEnumerable<VersionControlPath> files, string requestPreamble, Func<IEnumerable<VersionControlPath>, string> fileJoiner, Func<string, string, RequestResultCode> parser, Action<RequestResultCode> result)
	{
		IList<ISet<VersionControlPath>> list = new List<ISet<VersionControlPath>>();
		int num = 200;
		int num2 = -1;
		foreach (VersionControlPath file in files)
		{
			if (++num >= 200)
			{
				list.Add(new SortedSet<VersionControlPath>());
				num2++;
				num = 0;
			}
			list[num2].Add(file);
		}
		List<ItemResultCode> itemResults = new List<ItemResultCode>();
		ResultCode globalResult = ResultCode.Success;
		try
		{
			list.AsParallel().ForAll(delegate(ISet<VersionControlPath> partitionedFileList)
			{
				StringBuilder stringBuilder2 = new StringBuilder(requestPreamble);
				stringBuilder2.Append(fileJoiner(partitionedFileList));
				string reqStr = stringBuilder2.ToString();
				PerforceRequestHelper.Run(Context, reqStr, delegate(string res, string err)
				{
					if (!PerforceRequestHelper.IsGlobalError(err, ref globalResult))
					{
						RequestResultCode requestResultCode = parser(res, err);
						if (requestResultCode.ItemResults.Count() < partitionedFileList.Count())
						{
							string text = $"REQUEST:\n{reqStr}\n\nRESULT:\n{res}\n\nERROR:\n{err}\n\n@summary Perforce Request Oddity @assign bwhitman";
						}
						else if (requestResultCode.Result == PerforceResultCode.PartialSuccess)
						{
							string text2 = $"REQUEST:\n{reqStr}\n\nRESULT:\n{res}\n\nERROR:\n{err}\n\n@summary Perforce Partial Success @assign bwhitman";
						}
						lock (itemResults)
						{
							itemResults.AddRange(requestResultCode.ItemResults);
						}
					}
				});
			});
		}
		catch (AggregateException ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("@summary perforce request error @assign bwhitman");
			foreach (Exception innerException in ex.InnerExceptions)
			{
				stringBuilder.AppendLine(innerException.Message);
			}
			PlatformAssert.If(condition: true, stringBuilder.ToString());
		}
		if (!globalResult)
		{
			result(new RequestResultCode(globalResult, itemResults));
		}
		else
		{
			result(new RequestResultCode(itemResults));
		}
	}

	private void SplitPerforceFileStatusRequest(IEnumerable<VersionControlPath> files, string requestPreamble, Func<IEnumerable<VersionControlPath>, string> fileJoiner, Func<string, string, FileStatusRequestResultCode> parser, Action<FileStatusRequestResultCode> result)
	{
		IList<ISet<VersionControlPath>> list = new List<ISet<VersionControlPath>>();
		int num = 200;
		int num2 = -1;
		foreach (VersionControlPath file in files)
		{
			if (++num >= 200)
			{
				list.Add(new SortedSet<VersionControlPath>());
				num2++;
				num = 0;
			}
			list[num2].Add(file);
		}
		IDictionary<string, FileStatusResultCode> itemResults = new ConcurrentDictionary<string, FileStatusResultCode>(new PathComparer());
		ResultCode globalResult = ResultCode.Success;
		try
		{
			list.AsParallel().ForAll(delegate(ISet<VersionControlPath> partitionedFileList)
			{
				StringBuilder stringBuilder2 = new StringBuilder(requestPreamble);
				stringBuilder2.Append(fileJoiner(partitionedFileList));
				string reqStr = stringBuilder2.ToString();
				PerforceRequestHelper.Run(Context, reqStr, delegate(string res, string err)
				{
					if (!PerforceRequestHelper.IsGlobalError(err, ref globalResult))
					{
						FileStatusRequestResultCode fileStatusRequestResultCode = parser(res, err);
						if (fileStatusRequestResultCode.Status.Count < partitionedFileList.Count())
						{
							IEnumerable<string> values = (from x in partitionedFileList
								select x.DepotPath into x
								orderby x
								select x).ToArray();
							string text = string.Join("\n", values);
							IEnumerable<string> values2 = fileStatusRequestResultCode.Status.Keys.OrderBy((string x) => x).ToArray();
							string arg = string.Join("\n", values2);
							string message = $"Perforce Request Oddity:\n\nREQUEST:\n{reqStr}\n\nRESULT:\n{arg}\n\nERROR:\n{err}";
							PlatformAssert.If(condition: true, message);
						}
						fileStatusRequestResultCode.Status.ToList().AsParallel().ForAll(delegate(KeyValuePair<string, FileStatusResultCode> x)
						{
							itemResults[x.Key] = x.Value;
						});
					}
				});
			});
		}
		catch (AggregateException ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("@summary perforce request error @assign bwhitman");
			foreach (Exception innerException in ex.InnerExceptions)
			{
				stringBuilder.AppendLine(innerException.Message);
			}
			PlatformAssert.If(condition: true, stringBuilder.ToString());
		}
		if (!globalResult)
		{
			result(new FileStatusRequestResultCode(globalResult, itemResults));
		}
		else
		{
			result(new FileStatusRequestResultCode(itemResults));
		}
	}

	public void GetLocalRevision(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} files ", GenerateFileListString, ParseFileResult, result);
	}

	public void GetStatus(IEnumerable<VersionControlPath> files, Action<FileStatusRequestResultCode> result)
	{
		SplitPerforceFileStatusRequest(files, $"-c {m_name} fstat ", GenerateFileListString, ParseStatusResult, result);
	}

	public void GetUncontrolledFiles(string folderPath, Action<ResultCode, IEnumerable<string>> result)
	{
		string depotPath = GetDepotPath(folderPath);
		if (!depotPath.EndsWith("/"))
		{
			depotPath += "/";
		}
		depotPath += "...";
		string request = $"-c {m_name} fstat -T \"depotFile, clientFile, movedFile, headAction\" {depotPath}";
		PerforceRequestHelper.Run(Context, request, delegate(string res, string err)
		{
			ResultCode res2 = ResultCode.Success;
			if (!PerforceRequestHelper.IsGlobalError(err, ref res2))
			{
				FileStatusRequestResultCode fileStatusRequestResultCode = PerforceRequestHelper.ParseStatusOperationResult(this, res, err, new string[0]);
				List<string> first = Directory.GetFiles(folderPath.TrimEnd('.'), "*.*", SearchOption.AllDirectories).Except(Directory.GetFiles(folderPath.TrimEnd('.'), "*.*")).ToList();
				if (fileStatusRequestResultCode.Status.ContainsKey(depotPath))
				{
					fileStatusRequestResultCode.Status.Remove(depotPath);
				}
				result(res2, first.Except(from p4w in fileStatusRequestResultCode.Status.Values
					where p4w.Status.Head.Action != VersionControlActionType.Delete
					select p4w.File.WorkspacePath));
			}
		});
	}

	public void GetPendingChanges(string folderPath, Action<FileStatusRequestResultCode> result)
	{
		string depotPath = GetDepotPath(folderPath);
		if (!depotPath.EndsWith("/"))
		{
			depotPath += "/";
		}
		depotPath += "...";
		string request = $"-c {m_name} fstat -Rco -e default {depotPath}";
		PerforceRequestHelper.Run(Context, request, delegate(string res, string err)
		{
			ResultCode res2 = ResultCode.Success;
			if (!PerforceRequestHelper.IsGlobalError(err, ref res2))
			{
				FileStatusRequestResultCode fileStatusRequestResultCode = PerforceRequestHelper.ParseStatusOperationResult(this, res, err, new string[0]);
				if (fileStatusRequestResultCode.Status.ContainsKey(depotPath))
				{
					fileStatusRequestResultCode.Status.Remove(depotPath);
				}
				result(fileStatusRequestResultCode);
			}
		});
	}

	public bool HasPendingChanges(string folderPath)
	{
		string depotPath = GetDepotPath(folderPath);
		if (!depotPath.EndsWith("/"))
		{
			depotPath += "/";
		}
		depotPath += "...";
		string request = $"-c {m_name} fstat -Rco -m 1 {depotPath}";
		bool anyChanges = false;
		PerforceRequestHelper.Run(Context, request, delegate(string res, string err)
		{
			ResultCode res2 = ResultCode.Success;
			if (!PerforceRequestHelper.IsGlobalError(err, ref res2))
			{
				FileStatusRequestResultCode fileStatusRequestResultCode = PerforceRequestHelper.ParseStatusOperationResult(this, res, err, new string[1] { "not opened on this client" });
				if (fileStatusRequestResultCode.Status.Values.Any() && fileStatusRequestResultCode.Status.Values.First().File.DepotPath != depotPath)
				{
					anyChanges = true;
				}
			}
		});
		return anyChanges;
	}

	public bool HasOutOfDateFiles(string folderPath)
	{
		string depotPath = GetDepotPath(folderPath);
		if (!depotPath.EndsWith("/"))
		{
			depotPath += "/";
		}
		depotPath += "...";
		string request = $"-c {m_name} sync -n -m 1 {depotPath}";
		bool anyOutOfDate = false;
		PerforceRequestHelper.Run(Context, request, delegate(string res, string err)
		{
			ResultCode res2 = ResultCode.Success;
			if (!PerforceRequestHelper.IsGlobalError(err, ref res2))
			{
				RequestResultCode requestResultCode = ParseSyncResult(res, err);
				if (requestResultCode.ItemResults.Any() && requestResultCode.ItemResults.First().File.DepotPath != depotPath)
				{
					anyOutOfDate = true;
				}
			}
		});
		return anyOutOfDate;
	}

	public void AddFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} add ", GenerateFileListString, ParseAddResult, result);
	}

	public void EditFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} edit ", GenerateFileListString, ParseEditResult, result);
	}

	public void DeleteFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} delete ", GenerateFileListString, ParseDeleteResult, result);
	}

	public void RevertFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} revert ", GenerateFileListString, ParseRevertResult, result);
	}

	public void SyncToLatest(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} sync ", GenerateFileListString, ParseSyncResult, result);
	}

	public void SyncToRevision(VersionControlPath file, int revision, Action<RequestResultCode> result)
	{
		PerforceRequestHelper.Run(Context, $"-c {m_name} sync {file.DepotPath}#{revision}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				result(ParseSyncResult(res, err));
			}
		});
	}

	public void SyncToChangelist(int changeList, Action<RequestResultCode> result)
	{
		PerforceRequestHelper.Run(Context, string.Format("-c {0} sync {1}", m_name, string.Join(" ", m_pathInfo.Select((VersionControlPath path) => $"{path.DepotPath}...@{changeList}"))), delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				result(ParseSyncResult(res, err));
			}
		});
	}

	public void RemoveFromWorkspace(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		SplitPerforceFileRequest(files, $"-c {m_name} sync ", GenerateFileListStringRev0, ParseRemoveResult, result);
	}

	private FileStatusRequestResultCode ParseStatusResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseStatusOperationResult(this, resultStr, errorStr, Enumerable.Empty<string>().ToArray());
	}

	private RequestResultCode ParseAddResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[3] { "opened for add", "currently opened for add", "missing" });
	}

	private RequestResultCode ParseEditResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[2] { "opened for edit", "currently opened for edit" });
	}

	private RequestResultCode ParseFileResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[1] { "edit change" });
	}

	private RequestResultCode ParseDeleteResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[2] { "opened for delete", "currently opened for delete" });
	}

	private RequestResultCode ParseSyncResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[3] { "up-to-date", "updating", "added as" });
	}

	private RequestResultCode ParseRemoveResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[3] { "up-to-date", "updating", "deleted as" });
	}

	private RequestResultCode ParseRevertResult(string resultStr, string errorStr)
	{
		return PerforceRequestHelper.ParseFileOperationResult(this, resultStr, errorStr, new string[3] { "abandoned", "cleared", "reverted" });
	}

	public ResultCode UpdatePendingChangeLists()
	{
		Regex sdsSplit = new Regex("Change\\s(\\d+)\\son\\s(\\d\\d\\d\\d/\\d\\d/\\d\\d)\\sby\\s(.+)\\s\\*pending\\*\\s'(.*)'$", RegexOptions.Compiled);
		ResultCode result = ResultCode.Success;
		PerforceRequestHelper.Run(Context, $"changes -c {m_name} -s pending", delegate(string res, string err)
		{
			if (PerforceRequestHelper.IsGlobalError(err, ref result))
			{
				return;
			}
			using StringReader stringReader = new StringReader(res);
			while (true)
			{
				string text = stringReader.ReadLine();
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				MatchCollection matchCollection = sdsSplit.Matches(text);
				if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 5)
				{
					GroupCollection groups = matchCollection[0].Groups;
					int result2 = 0;
					int.TryParse(groups[1].Value, out result2);
					DateTime.TryParse(groups[2].Value, out var result3);
					string value = groups[3].Value;
					string value2 = groups[4].Value;
					if (m_pendingChanges.ContainsKey(result2))
					{
						m_pendingChanges[result2].Description = value2;
					}
					else
					{
						try
						{
							m_pendingChanges[result2] = new PerforceVersionControlChange(this, result2, result3, value, value2);
						}
						catch (Exception ex)
						{
							result = new ResultCode($"Failed to create local changelist cache. Error=\"{ex.Message}\"");
						}
					}
				}
				else
				{
					result = new ResultCode("Failed to parse workspace change list results. Result={0}", text);
				}
			}
		});
		return result;
	}

	private string GenerateFileListStringRev0(IEnumerable<VersionControlPath> files)
	{
		return string.Join(" ", files.Select((VersionControlPath path) => $"{path.DepotPath}#0"));
	}

	private string GenerateFileListString(IEnumerable<VersionControlPath> files)
	{
		return string.Join(" ", files.Select((VersionControlPath file) => $"\"{file.DepotPath}\""));
	}

	private string GenerateChangelistSpecification(string desc)
	{
		return $"Change: new\nClient: {m_name}\nUser: {m_context.Username}\nStatus: new\nDescription: {desc}\n\u001a";
	}
}
