using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public static class PerforceRequestHelper
{
	private static IList<IFileStatusParser> s_parsers;

	public static string kTimeout;

	private static int kMaxWindowsPath;

	private static Regex s_depotFileSplit;

	private static Regex s_versionSplit;

	private static Regex s_depotFileMatcher;

	private static Regex s_localFileMatcher;

	private static Regex s_movedFileMatcher;

	static PerforceRequestHelper()
	{
		s_parsers = new List<IFileStatusParser>();
		kTimeout = "timeout";
		kMaxWindowsPath = 240;
		s_depotFileSplit = new Regex("^\\s*(//.*)\\s+-\\s+(.*)$", RegexOptions.Compiled);
		s_versionSplit = new Regex("^(//.*)\\#(.+)$", RegexOptions.Compiled);
		s_depotFileMatcher = new Regex("^\\.\\.\\.\\s+depotFile\\s*(//.*)$", RegexOptions.Compiled);
		s_localFileMatcher = new Regex("^\\.\\.\\.\\s+clientFile\\s*(.*)$", RegexOptions.Compiled);
		s_movedFileMatcher = new Regex("^\\.\\.\\.\\s+movedFile\\s*(//.*)$", RegexOptions.Compiled);
		RegisterParser(new IgnoreLineParser("^\\.\\.\\.\\s+isMapped.*$"));
		RegisterParser(new IgnoreLineParser("^\\.\\.\\.\\s+headTime\\s+\\d+$"));
		RegisterParser(new IgnoreLineParser("^\\.\\.\\.\\s+type\\s+.+$"));
		RegisterParser(new HeadActionLineParser());
		RegisterParser(new HeadTypeLineParser());
		RegisterParser(new HeadRevisionLineParser());
		RegisterParser(new HeadChangeLineParser());
		RegisterParser(new HeadModTimeLineParser());
		RegisterParser(new HaveRevisionLineParser());
		RegisterParser(new LocalActionLineParser());
		RegisterParser(new LocalRevisionLineParser());
		RegisterParser(new LocalChangeLineParser());
		RegisterParser(new LocalOwnerLineParser());
		RegisterParser(new OtherOpenLineParser());
		RegisterParser(new OtherActionLineParser());
		RegisterParser(new OtherChangeLineParser());
		RegisterParser(new IgnoreLineParser("^\\.\\.\\.\\s+\\.\\.\\.\\s+otherOpen.+$"));
	}

	private static void RegisterParser(IFileStatusParser parser)
	{
		s_parsers.Add(parser);
	}

	private static IFileStatusParser FindFileStatusParser(string line)
	{
		foreach (IFileStatusParser s_parser in s_parsers)
		{
			if (s_parser.MatchesLine(line))
			{
				return s_parser;
			}
		}
		return null;
	}

	public static ResultCode DetermineOverallResult(IEnumerable<ItemResultCode> itemResults)
	{
		int num = 0;
		int num2 = 0;
		ResultCode result = ResultCode.Success;
		foreach (ItemResultCode itemResult in itemResults)
		{
			num2++;
			if (itemResult.Result != ResultCode.Success)
			{
				result = PerforceResultCode.PartialSuccess;
				num++;
			}
		}
		if (num2 > 0 && num == num2)
		{
			result = new ResultCode("Operation failed on all files");
		}
		return result;
	}

	public static bool IsGlobalError(string err, ref ResultCode res)
	{
		if (err == kTimeout)
		{
			if ((bool)res)
			{
				res = PerforceResultCode.Timeout;
			}
			return true;
		}
		if (err.Contains("Perforce password (P4PASSWD) invalid or unset.") || err.Contains("Your session has expired, please login again."))
		{
			if ((bool)res)
			{
				res = PerforceResultCode.NotSignedIn;
			}
			return true;
		}
		if (err.Contains("Connect to server failed") || err.Contains("Broker connection error"))
		{
			if ((bool)res)
			{
				res = PerforceResultCode.FailedToConnect;
			}
			return true;
		}
		if (err.Contains("Unicode clients require a unicode enabled server.") && (bool)res)
		{
			res = new PerforceResultCode("Client unicode configuration mismatch. Server expects non-unicode configuration and client is using unicode.");
		}
		return false;
	}

	public static bool IsGlobalError(string err, Action<RequestResultCode> result)
	{
		ResultCode res = ResultCode.Success;
		if (!IsGlobalError(err, ref res))
		{
			return false;
		}
		result(new RequestResultCode(res, Enumerable.Empty<ActionResultCode>()));
		return true;
	}

	public static bool IsGlobalError(string err, Action<ResultCode> result)
	{
		ResultCode res = ResultCode.Success;
		if (!IsGlobalError(err, ref res))
		{
			return false;
		}
		result(res);
		return true;
	}

	public static void Run(VersionControlContext context, string request, string stdinData, Action<string, string> responseHandler)
	{
		using Process process = new Process();
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.FileName = "p4.exe";
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardInput = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.Arguments = $"-p {context.Host} {request}";
		int milliseconds = context.Timeout * (1 + (int)((float)request.Length / (float)kMaxWindowsPath));
		bool stdOutDone = false;
		bool stdErrDone = false;
		StringBuilder stdout = new StringBuilder();
		process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs evt)
		{
			if (evt.Data == null)
			{
				stdOutDone = true;
				return;
			}
			lock (stdout)
			{
				stdout.AppendLine(evt.Data);
			}
		};
		StringBuilder stderr = new StringBuilder();
		process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs evt)
		{
			if (evt.Data == null)
			{
				stdErrDone = true;
				return;
			}
			lock (stderr)
			{
				stderr.AppendLine(evt.Data);
			}
		};
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.StandardInput.Write(stdinData);
		if (process.WaitForExit(milliseconds))
		{
			while (!stdErrDone || !stdOutDone)
			{
				Thread.Sleep(1);
			}
			string arg;
			string arg2;
			lock (stdout)
			{
				lock (stderr)
				{
					arg = stdout.ToString();
					arg2 = stderr.ToString();
				}
			}
			responseHandler(arg, arg2);
		}
		else
		{
			string arg3;
			lock (stdout)
			{
				arg3 = stdout.ToString();
			}
			responseHandler(arg3, kTimeout);
		}
	}

	public static void Run(VersionControlContext context, string request, Action<string, string> responseHandler)
	{
		using Process process = new Process();
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.FileName = "p4.exe";
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.Arguments = $"-p {context.Host} {request}";
		int milliseconds = context.Timeout * (1 + (int)((float)request.Length / (float)kMaxWindowsPath));
		bool stdOutDone = false;
		bool stdErrDone = false;
		StringBuilder stdout = new StringBuilder();
		process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs evt)
		{
			if (evt.Data == null)
			{
				stdOutDone = true;
				return;
			}
			lock (stdout)
			{
				stdout.AppendLine(evt.Data);
			}
		};
		StringBuilder stderr = new StringBuilder();
		process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs evt)
		{
			if (evt.Data == null)
			{
				stdErrDone = true;
				return;
			}
			lock (stderr)
			{
				stderr.AppendLine(evt.Data);
			}
		};
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		if (process.WaitForExit(milliseconds))
		{
			while (!stdErrDone || !stdOutDone)
			{
				Thread.Sleep(1);
			}
			string arg;
			string arg2;
			lock (stdout)
			{
				lock (stderr)
				{
					arg = stdout.ToString();
					arg2 = stderr.ToString();
				}
			}
			responseHandler(arg, arg2);
		}
		else
		{
			string arg3;
			lock (stdout)
			{
				arg3 = stdout.ToString();
			}
			responseHandler(arg3, kTimeout);
		}
	}

	private static bool ParseDepotPathResult(IVersionControlWorkspace workspace, string resultStr, string[] successStrings, ref ActionResultCode result)
	{
		MatchCollection matchCollection = s_depotFileSplit.Matches(resultStr);
		if (matchCollection.Count != 1 || matchCollection[0].Groups.Count != 3)
		{
			return false;
		}
		result.Revision = -1;
		string value = matchCollection[0].Groups[1].Value;
		if (value.Contains("#"))
		{
			MatchCollection matchCollection2 = s_versionSplit.Matches(value);
			if (matchCollection2.Count != 1 || matchCollection2[0].Groups.Count != 3)
			{
				return false;
			}
			value = matchCollection2[0].Groups[1].Value;
			int result2 = 1;
			int.TryParse(matchCollection2[0].Groups[2].Value, out result2);
			result.Revision = result2;
		}
		string localPath = workspace.GetLocalPath(value);
		result.File = new VersionControlPath(value, localPath);
		string value2 = matchCollection[0].Groups[2].Value;
		ResultCode result3 = new ResultCode(value2);
		foreach (string value3 in successStrings)
		{
			if (value2.Contains(value3))
			{
				result3 = ResultCode.Success;
				break;
			}
		}
		result.Result = result3;
		return true;
	}

	private static bool ParseLocalPathResult(IVersionControlWorkspace workspace, string resultStr, string[] successStrings, ref ActionResultCode result)
	{
		string arg = workspace.Root.Replace("\\", "\\\\");
		Regex regex = new Regex($"^\\s*({arg}\\\\.+)\\s+-\\s+(.*)$", RegexOptions.Compiled);
		MatchCollection matchCollection = regex.Matches(resultStr);
		if (matchCollection.Count != 1 || matchCollection[0].Groups.Count != 3)
		{
			return false;
		}
		string value = matchCollection[0].Groups[1].Value;
		string depotPath = workspace.GetDepotPath(value);
		result.File = new VersionControlPath(depotPath, value);
		string value2 = matchCollection[0].Groups[2].Value;
		ResultCode result2 = new ResultCode(value2);
		foreach (string value3 in successStrings)
		{
			if (value2.Contains(value3))
			{
				result2 = ResultCode.Success;
				break;
			}
		}
		result.Result = result2;
		return true;
	}

	private static bool ParseAdditionalInfoResult(IVersionControlWorkspace workspace, string resultStr, out string vcPath, out string addtInfo)
	{
		ActionResultCode result = new ActionResultCode();
		if (ParseDepotPathResult(workspace, resultStr, Enumerable.Empty<string>().ToArray(), ref result))
		{
			vcPath = result.File.DepotPath;
			addtInfo = result.Result.Message;
			return true;
		}
		if (ParseLocalPathResult(workspace, resultStr, Enumerable.Empty<string>().ToArray(), ref result))
		{
			vcPath = result.File.WorkspacePath;
			addtInfo = result.Result.Message;
			return true;
		}
		vcPath = string.Empty;
		addtInfo = string.Empty;
		return false;
	}

	private static ResultCode ParseFileOperationResultString(IVersionControlWorkspace workspace, string resultStr, string[] successStrings, IDictionary<string, ItemResultCode> itemResults)
	{
		ResultCode res = ResultCode.Success;
		if (IsGlobalError(resultStr, ref res))
		{
			return res;
		}
		using (StringReader stringReader = new StringReader(resultStr))
		{
			while (true)
			{
				string text = stringReader.ReadLine();
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				string vcPath;
				string addtInfo;
				if (!text.StartsWith("..."))
				{
					ActionResultCode result = new ActionResultCode();
					if (ParseDepotPathResult(workspace, text, successStrings, ref result))
					{
						itemResults[result.File.DepotPath] = result;
						continue;
					}
					if (!ParseLocalPathResult(workspace, text, successStrings, ref result))
					{
						return new ResultCode("Failed to parse command result");
					}
					itemResults[result.File.DepotPath] = result;
				}
				else if (ParseAdditionalInfoResult(workspace, text.Substring(4), out vcPath, out addtInfo) && itemResults.ContainsKey(vcPath) && itemResults[vcPath] is ActionResultCode actionResultCode)
				{
					actionResultCode.AdditionalInfo.Add(addtInfo);
				}
			}
		}
		return ResultCode.Success;
	}

	public static RequestResultCode ParseFileOperationResult(IVersionControlWorkspace workspace, string resultStr, string errorStr, string[] successStrings)
	{
		IDictionary<string, ItemResultCode> dictionary = new Dictionary<string, ItemResultCode>();
		ResultCode res = ResultCode.Success;
		if (IsGlobalError(errorStr, ref res))
		{
			return new RequestResultCode(res, dictionary.Values);
		}
		ResultCode resultCode = ParseFileOperationResultString(workspace, errorStr, successStrings, dictionary);
		ResultCode resultCode2 = ParseFileOperationResultString(workspace, resultStr, successStrings, dictionary);
		if (resultCode != ResultCode.Success)
		{
			return new RequestResultCode(resultCode, dictionary.Values);
		}
		if (resultCode2 != ResultCode.Success)
		{
			return new RequestResultCode(resultCode2, dictionary.Values);
		}
		ResultCode overallResult = DetermineOverallResult(dictionary.Values);
		return new RequestResultCode(overallResult, dictionary.Values);
	}

	private static bool ParseDepotFilePath(IVersionControlWorkspace workspace, StringReader reader, ref string depotPath)
	{
		string text = reader.ReadLine();
		if (text == null)
		{
			return false;
		}
		MatchCollection matchCollection = s_depotFileMatcher.Matches(text);
		if (matchCollection.Count == 0)
		{
			return false;
		}
		depotPath = matchCollection[0].Groups[1].Value;
		return true;
	}

	private static bool ParseLocalFilePath(IVersionControlWorkspace workspace, string line, ref string localPath)
	{
		MatchCollection matchCollection = s_localFileMatcher.Matches(line);
		if (matchCollection.Count == 0)
		{
			return false;
		}
		localPath = matchCollection[0].Groups[1].Value;
		return true;
	}

	private static bool ParseMovedFilePath(IVersionControlWorkspace workspace, string line, ref string movedPath)
	{
		MatchCollection matchCollection = s_movedFileMatcher.Matches(line);
		if (matchCollection.Count == 0)
		{
			return false;
		}
		movedPath = matchCollection[0].Groups[1].Value;
		return true;
	}

	private static bool ParseMovedAndLocalFilePaths(IVersionControlWorkspace workspace, StringReader reader, ref string movedPath, ref string localPath)
	{
		string text = reader.ReadLine();
		if (text == null)
		{
			return false;
		}
		if (ParseMovedFilePath(workspace, text, ref movedPath))
		{
			text = reader.ReadLine();
			if (text == null)
			{
				return false;
			}
		}
		return ParseLocalFilePath(workspace, text, ref localPath);
	}

	private static void ParseFileStatusLines(IVersionControlWorkspace workspace, StringReader reader, ref FileStatusResultCode itemResults)
	{
		while (true)
		{
			string text = reader.ReadLine();
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			for (IFileStatusParser fileStatusParser = FindFileStatusParser(text); fileStatusParser != null; fileStatusParser = FindFileStatusParser(text))
			{
				if (!fileStatusParser.ParseStatusLine(workspace, text, ref itemResults))
				{
					return;
				}
				text = reader.ReadLine();
				if (string.IsNullOrEmpty(text))
				{
					return;
				}
			}
		}
	}

	private static void ParseSingleFileStatusResultString(IVersionControlWorkspace workspace, StringReader reader, ref FileStatusResultCode itemResults)
	{
		string localPath = string.Empty;
		string movedPath = string.Empty;
		string depotPath = string.Empty;
		if (ParseDepotFilePath(workspace, reader, ref depotPath) && ParseMovedAndLocalFilePaths(workspace, reader, ref movedPath, ref localPath))
		{
			itemResults.File = new VersionControlPath(depotPath, localPath);
			ParseFileStatusLines(workspace, reader, ref itemResults);
		}
	}

	private static ResultCode ParseStatusOperationResultString(IVersionControlWorkspace workspace, string resultStr, IDictionary<string, FileStatusResultCode> itemResults)
	{
		using (StringReader stringReader = new StringReader(resultStr))
		{
			while (stringReader.Peek() > -1)
			{
				FileStatusResultCode itemResults2 = new FileStatusResultCode();
				ParseSingleFileStatusResultString(workspace, stringReader, ref itemResults2);
				if (!string.IsNullOrEmpty(itemResults2.File.DepotPath))
				{
					itemResults[itemResults2.File.DepotPath] = itemResults2;
				}
			}
		}
		return ResultCode.Success;
	}

	public static FileStatusRequestResultCode ParseStatusOperationResult(IVersionControlWorkspace workspace, string resultStr, string errorStr, string[] successErrors)
	{
		IDictionary<string, FileStatusResultCode> dictionary = new Dictionary<string, FileStatusResultCode>();
		ResultCode res = ResultCode.Success;
		if (IsGlobalError(errorStr, ref res))
		{
			return new FileStatusRequestResultCode(res, dictionary);
		}
		IDictionary<string, ItemResultCode> dictionary2 = new Dictionary<string, ItemResultCode>();
		ResultCode resultCode = ParseFileOperationResultString(workspace, errorStr, successErrors, dictionary2);
		foreach (KeyValuePair<string, ItemResultCode> item in dictionary2)
		{
			dictionary[item.Key] = new FileStatusResultCode(item.Value.File, item.Value.Result);
		}
		ResultCode resultCode2 = ParseStatusOperationResultString(workspace, resultStr, dictionary);
		if (resultCode != ResultCode.Success)
		{
			return new FileStatusRequestResultCode(resultCode, dictionary);
		}
		if (resultCode2 != ResultCode.Success)
		{
			return new FileStatusRequestResultCode(resultCode2, dictionary);
		}
		ResultCode result = DetermineOverallResult(dictionary.Values);
		return new FileStatusRequestResultCode(result, dictionary);
	}
}
