using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class PerforceVersionControlChange : IVersionControlChange
{
	private IVersionControlWorkspace m_workspace;

	private int m_number;

	private DateTime m_date;

	private string m_email;

	private IList<VersionControlFileChange> m_files = new List<VersionControlFileChange>();

	private Regex m_successFileMatcher = new Regex("edit\\s+(//.*)$", RegexOptions.Compiled);

	private Regex m_failureFileMatcher = new Regex("^\\s*(//.*)\\s+-\\s+(.*)$", RegexOptions.Compiled);

	private Regex m_fileVersionMatcher = new Regex("^(//.*)\\#(.+)$", RegexOptions.Compiled);

	private Regex m_errorFileMatcher = new Regex("^open for read:\\s+(.*):\\s+(.*)$", RegexOptions.Compiled);

	private string[] m_errorStrings = new string[4] { "File(s) couldn't be locked.", "Some file(s) could not be transferred from client.", "Out of date files must be resolved or reverted.", "No files to submit." };

	public int Number => m_number;

	public DateTime Date => m_date;

	public string Email => m_email;

	public string Description { get; set; }

	public IEnumerable<VersionControlFileChange> Files => m_files;

	public PerforceVersionControlChange(IVersionControlWorkspace workspace, int num, DateTime dt, string email, string desc)
	{
		m_workspace = workspace;
		m_number = num;
		m_date = dt;
		m_email = email;
		Description = desc;
		ResultCode resultCode = UpdateChangeFiles();
		if (resultCode != ResultCode.Success)
		{
			throw new Exception(resultCode.Message);
		}
	}

	public void AddFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		PerforceRequestHelper.Run(m_workspace.Context, $"-c {m_workspace.Name} reopen -c {m_number} {GenerateFileListString(files)}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				ResultCode resultCode = UpdateChangeFiles();
				if (resultCode != ResultCode.Success)
				{
					result(new RequestResultCode(resultCode, Enumerable.Empty<ActionResultCode>()));
				}
				else if (res.StartsWith($"Change {m_number}"))
				{
					result(new RequestResultCode(new ResultCode(res), Enumerable.Empty<ActionResultCode>()));
				}
				else
				{
					result(PerforceRequestHelper.ParseFileOperationResult(m_workspace, res, err, new string[2] { "nothing changed", "reopened; change" }));
				}
			}
		});
	}

	public void RemoveFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result)
	{
		PerforceRequestHelper.Run(m_workspace.Context, $"-c {m_workspace.Name} reopen -c default {GenerateFileListString(files)}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				ResultCode resultCode = UpdateChangeFiles();
				if (resultCode != ResultCode.Success)
				{
					result(new RequestResultCode(resultCode, Enumerable.Empty<ActionResultCode>()));
				}
				else
				{
					result(PerforceRequestHelper.ParseFileOperationResult(m_workspace, res, err, new string[2] { "nothing changed", "reopened; default" }));
				}
			}
		});
	}

	public void Submit(Action<RequestResultCode> result)
	{
		Regex lineSplitter = new Regex(".*\\r\\n");
		PerforceRequestHelper.Run(m_workspace.Context, $"-c {m_workspace.Name} submit -c {m_number}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				MatchCollection matchCollection = lineSplitter.Matches(res);
				MatchCollection failure = lineSplitter.Matches(err);
				IEnumerable<ActionResultCode> fileResults;
				ResultCode resultCode = ParseFileResults(matchCollection, failure, out fileResults);
				if (resultCode == ResultCode.Success)
				{
					if (WasSuccessfulSubmit(matchCollection))
					{
						ResultCode overallResult = m_workspace.UpdatePendingChangeLists();
						result(new RequestResultCode(overallResult, fileResults));
					}
					else
					{
						string msg = FindErrorMessage(matchCollection, failure);
						result(new RequestResultCode(new ResultCode(msg), fileResults));
					}
				}
				else
				{
					result(new RequestResultCode(resultCode, fileResults));
				}
			}
		});
	}

	public void Revert(Action<RequestResultCode> result)
	{
		PerforceRequestHelper.Run(m_workspace.Context, $"-c {m_workspace.Name} revert -c {m_number} {GenerateFileListString(m_files)}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, result))
			{
				ResultCode resultCode = UpdateChangeFiles();
				if (resultCode != ResultCode.Success)
				{
					result(new RequestResultCode(resultCode, Enumerable.Empty<ActionResultCode>()));
				}
				else if (res.StartsWith($"Change {m_number}"))
				{
					result(new RequestResultCode(new ResultCode(res), Enumerable.Empty<ActionResultCode>()));
				}
				else
				{
					result(PerforceRequestHelper.ParseFileOperationResult(m_workspace, res, err, new string[3] { "abandoned", "cleared", "reverted" }));
				}
			}
		});
	}

	private string GenerateFileListString(IEnumerable<VersionControlPath> files)
	{
		return string.Join(" ", files.Select((VersionControlPath file) => $"\"{file.DepotPath}\""));
	}

	private string GenerateFileListString(IEnumerable<VersionControlFileChange> files)
	{
		return string.Join(" ", files.Select((VersionControlFileChange file) => $"\"{file.Path.DepotPath}\""));
	}

	private VersionControlChangeType FindVersionControlChangeType(string changeTypeStr)
	{
		return changeTypeStr switch
		{
			"add" => VersionControlChangeType.Add, 
			"edit" => VersionControlChangeType.Edit, 
			"delete" => VersionControlChangeType.Delete, 
			"move/add" => VersionControlChangeType.MoveAdd, 
			"move/delete" => VersionControlChangeType.MoveDelete, 
			_ => VersionControlChangeType.Invalid, 
		};
	}

	private ResultCode UpdateChangeFiles()
	{
		m_files.Clear();
		Regex fileSplit = new Regex("^\\s+(//.*)\\s+#\\s+(.*)$", RegexOptions.Compiled);
		ResultCode updateResult = ResultCode.Success;
		PerforceRequestHelper.Run(m_workspace.Context, $"-c {m_workspace.Name} change -o {m_number}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, ref updateResult))
			{
				using (StringReader stringReader = new StringReader(res))
				{
					while (updateResult == ResultCode.Success)
					{
						string text = stringReader.ReadLine();
						if (text == null)
						{
							break;
						}
						if (!text.StartsWith("#") && text.StartsWith("Files:"))
						{
							while (true)
							{
								string text2 = stringReader.ReadLine();
								if (string.IsNullOrEmpty(text2))
								{
									break;
								}
								MatchCollection matchCollection = fileSplit.Matches(text2);
								if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 3)
								{
									string value = matchCollection[0].Groups[1].Value;
									string localPath = m_workspace.GetLocalPath(value);
									VersionControlPath path = new VersionControlPath(value, localPath);
									VersionControlChangeType change = FindVersionControlChangeType(matchCollection[0].Groups[2].Value);
									m_files.Add(new VersionControlFileChange(change, path));
								}
								else
								{
									updateResult = new ResultCode("Failed update local cache of the changelist's files");
								}
							}
						}
					}
				}
			}
		});
		return updateResult;
	}

	private bool WasSuccessfulSubmit(MatchCollection results)
	{
		return results.Count > 0 && results[results.Count - 1].Value.StartsWith($"Change {m_number} submitted.");
	}

	private ResultCode ParseFileResults(MatchCollection success, MatchCollection failure, out IEnumerable<ActionResultCode> fileResults)
	{
		IDictionary<string, ActionResultCode> dictionary = new Dictionary<string, ActionResultCode>(StringComparer.CurrentCultureIgnoreCase);
		ResultCode resultCode = ParseFileResults(success, dictionary);
		if (resultCode != ResultCode.Success)
		{
			fileResults = Enumerable.Empty<ActionResultCode>();
			return resultCode;
		}
		ResultCode resultCode2 = ParseFileErrors(failure, dictionary);
		if (resultCode2 != ResultCode.Success)
		{
			fileResults = Enumerable.Empty<ActionResultCode>();
			return resultCode2;
		}
		fileResults = dictionary.Values;
		return ResultCode.Success;
	}

	private ResultCode ParseFileResults(MatchCollection results, IDictionary<string, ActionResultCode> fileResults)
	{
		for (int i = 0; i < results.Count; i++)
		{
			string result = results[i].Value.TrimEnd('\r', '\n');
			ResultCode resultCode = ParseFileSuccess(result, fileResults);
			if (resultCode != ResultCode.Success)
			{
				return resultCode;
			}
			ResultCode resultCode2 = ParseFileFailure(result, fileResults);
			if (resultCode2 != ResultCode.Success)
			{
				return resultCode2;
			}
		}
		return ResultCode.Success;
	}

	private ResultCode ParseFileSuccess(string result, IDictionary<string, ActionResultCode> fileResults)
	{
		Match match = m_successFileMatcher.Match(result);
		if (match.Success)
		{
			if (match.Groups.Count != 2)
			{
				return new ResultCode("Failed to parse file results.");
			}
			string value = match.Groups[1].Value;
			int result2 = -1;
			if (value.Contains("#"))
			{
				MatchCollection matchCollection = m_fileVersionMatcher.Matches(value);
				if (matchCollection.Count != 1 || matchCollection[0].Groups.Count != 3)
				{
					return new ResultCode("Failed to parse file results.");
				}
				value = matchCollection[0].Groups[1].Value;
				if (!int.TryParse(matchCollection[0].Groups[2].Value, out result2))
				{
					return new ResultCode("Failed to parse file revision results.");
				}
			}
			string localPath = m_workspace.GetLocalPath(value);
			fileResults[value] = new ActionResultCode(new VersionControlPath(value, localPath), ResultCode.Success, result2);
		}
		return ResultCode.Success;
	}

	private ResultCode ParseFileFailure(string result, IDictionary<string, ActionResultCode> fileResults)
	{
		Match match = m_failureFileMatcher.Match(result);
		if (match.Success)
		{
			if (match.Groups.Count != 3)
			{
				return new ResultCode("Failed to parse file results.");
			}
			string value = match.Groups[1].Value;
			int result2 = -1;
			if (value.Contains("#"))
			{
				MatchCollection matchCollection = m_fileVersionMatcher.Matches(value);
				if (matchCollection.Count != 1 || matchCollection[0].Groups.Count != 3)
				{
					return new ResultCode("Failed to parse file results.");
				}
				value = matchCollection[0].Groups[1].Value;
				if (!int.TryParse(matchCollection[0].Groups[2].Value, out result2))
				{
					return new ResultCode("Failed to parse file revision results.");
				}
			}
			string localPath = m_workspace.GetLocalPath(value);
			string value2 = match.Groups[2].Value;
			fileResults[value] = new ActionResultCode(new VersionControlPath(value, localPath), new ResultCode(value2), result2);
		}
		return ResultCode.Success;
	}

	private ResultCode ParseFileErrors(MatchCollection errors, IDictionary<string, ActionResultCode> fileResults)
	{
		for (int i = 0; i < errors.Count; i++)
		{
			string input = errors[i].Value.TrimEnd('\r', '\n');
			Match match = m_errorFileMatcher.Match(input);
			if (match.Success)
			{
				if (match.Groups.Count != 3)
				{
					return new ResultCode("Failed to parse file results.");
				}
				string value = match.Groups[1].Value;
				string depotPath = m_workspace.GetDepotPath(value);
				string value2 = match.Groups[2].Value;
				fileResults[depotPath] = new ActionResultCode(new VersionControlPath(depotPath, value), new ResultCode(value2), -1);
			}
		}
		return ResultCode.Success;
	}

	private string FindErrorMessage(MatchCollection success, MatchCollection failure)
	{
		for (int i = 0; i < success.Count; i++)
		{
			string value = success[i].Value;
			string[] errorStrings = m_errorStrings;
			foreach (string text in errorStrings)
			{
				if (value.Contains(text))
				{
					return text;
				}
			}
		}
		for (int k = 0; k < failure.Count; k++)
		{
			string value2 = failure[k].Value;
			string[] errorStrings2 = m_errorStrings;
			foreach (string text2 in errorStrings2)
			{
				if (value2.Contains(text2))
				{
					return text2;
				}
			}
		}
		if (failure.Count > 0)
		{
			return failure[failure.Count - 1].Value;
		}
		return "Unknown error.";
	}
}
