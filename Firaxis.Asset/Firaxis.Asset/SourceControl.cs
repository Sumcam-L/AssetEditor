using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.Asset.Properties;
using Firaxis.Error;
using Firaxis.IO;
using Firaxis.Utility.Converters;

namespace Firaxis.Asset;

public class SourceControl : ISourceControl
{
	[Flags]
	public enum FileState
	{
		Add = 2,
		Edit = 4,
		Delete = 8,
		Editable = 6,
		NotEditable = -15
	}

	public enum RevRange
	{
		Revision,
		Changelist
	}

	public enum Changelist
	{
		DontSync,
		SyncToLatest
	}

	private class SourceControlLabel : ISourceControlLabel
	{
		public string Name { get; private set; }

		public DateTime Date { get; private set; }

		public SourceControlLabel(string name, DateTime date)
		{
			Name = name;
			Date = date;
		}
	}

	private string user;

	private string password;

	private string port;

	private string client;

	public string User
	{
		get
		{
			return user;
		}
		set
		{
			user = value;
		}
	}

	public string Password
	{
		get
		{
			return password;
		}
		set
		{
			password = value;
		}
	}

	public string Port
	{
		get
		{
			return port;
		}
		set
		{
			port = value;
		}
	}

	public string Client
	{
		get
		{
			return client;
		}
		set
		{
			client = value;
		}
	}

	public bool IsConnected => false;

	public bool VerifyConnection => false;

	public int LatestChange => -1;

	public int HaveChange => -1;

	public SourceControl()
		: this(string.Empty, string.Empty, Resources.P4Port, string.Empty)
	{
	}

	public SourceControl(string client)
		: this(string.Empty, string.Empty, Resources.P4Port, client)
	{
	}

	public SourceControl(string user, string password, string port, string client)
	{
		this.user = user;
		this.password = password;
		Port = port;
		this.client = client;
	}

	public P4Connection Connect()
	{
		P4Connection p4Connection = new P4Connection();
		if (user.Length > 0)
		{
			p4Connection.User = user;
		}
		p4Connection.Password = password;
		p4Connection.Port = Port;
		if (client.Length > 0)
		{
			p4Connection.Client = client;
		}
		p4Connection.Connect();
		return p4Connection;
	}

	public void Submit(string sDescription, string[] asFiles)
	{
	}

	public int GetChangelistNumber(Changelist Changelist)
	{
		return Changelist switch
		{
			Changelist.SyncToLatest => LatestChange, 
			Changelist.DontSync => HaveChange, 
			_ => (int)Changelist, 
		};
	}

	public WindowsPath GetLocalPathFromDepot(SourceControlPath depot)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("where", depot.ToString());
		ThrowExceptionOnError("where", depot.ToString(), p4RecordSet);
		if (p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("path"))
		{
			ThrowException("where", depot.ToString(), p4RecordSet);
		}
		return new WindowsPath(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["path"]);
	}

	public SourceControlPath GetDepotPathFromLocal(WindowsPath local)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("where", local.ToString());
		ThrowExceptionOnError("where", local.ToString(), p4RecordSet);
		if (p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("depotFile"))
		{
			ThrowException("where", local.ToString(), p4RecordSet);
		}
		return new SourceControlPath(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["depotFile"]);
	}

	public bool IsLocalPathInClientView(WindowsPath local)
	{
		if (!IsInClientRootPath(local) || WindowsPath.Equals(local, GetClientRootPath()))
		{
			return false;
		}
		using (P4Connection p4Connection = Connect())
		{
			P4RecordSet p4RecordSet = p4Connection.Run("fstat", local.ToString());
			ThrowExceptionOnError("fstat", local.ToString(), p4RecordSet);
			if (p4RecordSet.HasWarnings())
			{
				if (p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("no such file(s)"))
				{
					return true;
				}
				if (p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("file(s) not in client view"))
				{
					return false;
				}
				if (p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("no permission for operation on file(s)"))
				{
					return false;
				}
			}
			if (p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("depotFile") || string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["depotFile"]))
			{
				ThrowException("fstat", local.ToString(), p4RecordSet);
			}
		}
		return true;
	}

	public bool IsClientViewPathInDepot(WindowsPath local)
	{
		if (!IsLocalPathInClientView(local))
		{
			throw new Exception("Cannot call IsClientViewPathInDepot on a path outside the client view.");
		}
		using (P4Connection p4Connection = Connect())
		{
			P4RecordSet p4RecordSet = p4Connection.Run("fstat", local.ToString());
			ThrowExceptionOnError("fstat", local.ToString(), p4RecordSet);
			if (p4RecordSet.HasWarnings() && p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("no such file(s)"))
			{
				return false;
			}
			if (p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("depotFile") || string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["depotFile"]))
			{
				ThrowException("fstat", local.ToString(), p4RecordSet);
			}
		}
		return true;
	}

	public bool IsInDepot(IPath path, bool obeyClientView, bool includeDeleted)
	{
		if (!IsSourceControlPath(path.ToString()) && (!IsInClientRootPath(path) || WindowsPath.Equals(path as WindowsPath, GetClientRootPath())))
		{
			return false;
		}
		return IsInDepot(path.ToString(), obeyClientView, includeDeleted);
	}

	private bool IsInDepot(string path, bool obeyClientView, bool includeDeleted)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("fstat", path);
		ThrowExceptionOnError("fstat", path, p4RecordSet);
		if (p4RecordSet.HasWarnings())
		{
			if (p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("no such file(s)"))
			{
				return false;
			}
			if (p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("file(s) not in client view"))
			{
				return false;
			}
			if (p4RecordSet.Warnings[p4RecordSet.Warnings.Length - 1].Contains("no permission for operation on file(s)"))
			{
				p4RecordSet = p4Connection.Run("where", path);
				ThrowExceptionOnError("where", path, p4RecordSet);
				if (!p4RecordSet.HasWarnings() && p4RecordSet.Records.Length != 0 && p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("depotFile"))
				{
					return IsInDepot(new WindowsPath(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["depotFile"]), obeyClientView, includeDeleted);
				}
				return false;
			}
		}
		if (p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("depotFile") || string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["depotFile"]))
		{
			ThrowException("fstat", path, p4RecordSet);
		}
		if (!p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("headAction") || string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["headAction"]))
		{
			return false;
		}
		if (obeyClientView && !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("isMapped"))
		{
			return false;
		}
		if (!includeDeleted && p4RecordSet.Records[p4RecordSet.Records.Length - 1]["headAction"].Contains("delete"))
		{
			return false;
		}
		if (!includeDeleted && p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("action") && string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["action"]) && p4RecordSet.Records[p4RecordSet.Records.Length - 1]["action"].Contains("delete"))
		{
			return false;
		}
		return true;
	}

	public bool IsOpenFor(IPath path, FileState desiredState)
	{
		return IsOpenFor(path.ToString(), desiredState);
	}

	private bool IsOpenFor(string path, FileState desiredState)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("fstat", path);
		ThrowExceptionOnError("fstat", path, p4RecordSet);
		if (p4RecordSet.HasWarnings() || p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("action"))
		{
			return false;
		}
		FileState fileState = (FileState)0;
		if (p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields["action"].Contains("add"))
		{
			fileState |= FileState.Add;
		}
		if (p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields["action"].Contains("edit"))
		{
			fileState |= FileState.Edit;
		}
		if (p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields["action"].Contains("delete"))
		{
			fileState |= FileState.Delete;
		}
		return (desiredState & fileState) != 0;
	}

	public int HaveRevisionNumber(IPath path)
	{
		return HaveRevisionNumber(path.ToString());
	}

	private int HaveRevisionNumber(string path)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("files", path);
		ThrowExceptionOnError("files", path, p4RecordSet);
		if (p4RecordSet.Records.Length != 0 && p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("depotFile") && p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("rev") && !string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["depotFile"]) && !string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["rev"]))
		{
			return Convert.ToInt32(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["rev"]);
		}
		ThrowException("files", path, p4RecordSet);
		return -1;
	}

	public DateTime GetHeadTime(IPath path)
	{
		return GetHeadTime(path.ToString());
	}

	private DateTime GetHeadTime(string path)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("fstat", path);
		if (!p4RecordSet.HasErrors() && !p4RecordSet.HasWarnings() && p4RecordSet.Records.Length != 0 && p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("headTime"))
		{
			return DateTimeConverter.ConvertFromUnixTimestamp(Convert.ToInt32(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["headTime"]));
		}
		ThrowExceptionOnError("fstat", path, p4RecordSet);
		return new DateTime(1970, 1, 1, 0, 0, 0, 0);
	}

	public DateTime GetHeadModTime(IPath path)
	{
		return GetHeadModTime(path.ToString());
	}

	private DateTime GetHeadModTime(string path)
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("fstat", path);
		if (!p4RecordSet.HasErrors() && !p4RecordSet.HasWarnings() && p4RecordSet.Records.Length != 0 && p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("headModTime"))
		{
			return DateTimeConverter.ConvertFromUnixTimestamp(Convert.ToInt32(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["headModTime"]));
		}
		ThrowExceptionOnError("fstat", path, p4RecordSet);
		return new DateTime(1970, 1, 1, 0, 0, 0, 0);
	}

	public static bool IsSourceControlPath(string depot)
	{
		return depot.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).StartsWith("//depot", StringComparison.OrdinalIgnoreCase);
	}

	public WindowsPath GetClientRootPath()
	{
		using P4Connection p4Connection = Connect();
		P4RecordSet p4RecordSet = p4Connection.Run("info");
		ThrowExceptionOnError("info", string.Empty, p4RecordSet);
		if (p4RecordSet.Records.Length == 0 || !p4RecordSet.Records[p4RecordSet.Records.Length - 1].Fields.ContainsKey("clientRoot"))
		{
			ThrowException("info", string.Empty, p4RecordSet);
		}
		if (string.IsNullOrEmpty(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["clientRoot"]) || p4RecordSet.Records[p4RecordSet.Records.Length - 1]["clientRoot"] == "null")
		{
			return null;
		}
		return new WindowsPath(p4RecordSet.Records[p4RecordSet.Records.Length - 1]["clientRoot"]);
	}

	public bool IsInClientRootPath(IPath path)
	{
		WindowsPath clientRootPath = GetClientRootPath();
		if (clientRootPath == null)
		{
			return true;
		}
		return path.ToString().StartsWith(clientRootPath.ToString(), StringComparison.OrdinalIgnoreCase);
	}

	public static string CreateFileRevRange(IPath path, Changelist changelist, RevRange revRange)
	{
		ExceptionLogger.Log(changelist != Changelist.DontSync, "Cannot create file[revRange] string because the provided changelist is not valid.", null, throwException: true);
		if (changelist == Changelist.SyncToLatest)
		{
			return path.ToString();
		}
		return string.Format("{0}{1}{2}", path.ToString(), (revRange == RevRange.Revision) ? "#" : "@", (int)changelist);
	}

	public static void ThrowExceptionOnError(string command, string parameters, P4RecordSet result)
	{
		if (result.HasErrors())
		{
			ThrowException(command, parameters, result);
		}
	}

	private static void ThrowException(string command, string parameters, P4RecordSet result)
	{
		string text = string.Format("P4 command '{0} {1}' {2}:\n", command, parameters, result.HasErrors() ? "has errors" : "failed");
		string[] errors = result.Errors;
		foreach (string text2 in errors)
		{
			text = text + "\n  " + text2;
		}
		throw new Exception(text);
	}

	public List<ISourceControlLabel> CollectLabels()
	{
		return CollectLabels("");
	}

	public List<ISourceControlLabel> CollectLabels(string pattern)
	{
		List<ISourceControlLabel> list = null;
		using (P4Connection p4Connection = Connect())
		{
			P4RecordSet p4RecordSet = p4Connection.Run("labels");
			if (!p4RecordSet.HasErrors())
			{
				list = new List<ISourceControlLabel>();
				P4Record[] records = p4RecordSet.Records;
				foreach (P4Record p4Record in records)
				{
					string text = p4Record.Fields["label"];
					string p4Date = p4Record.Fields["Update"];
					if (string.IsNullOrEmpty(pattern) || text.Contains(pattern))
					{
						SourceControlLabel item = new SourceControlLabel(text, p4Connection.ConvertDate(p4Date));
						list.Add(item);
					}
				}
			}
			else
			{
				ThrowExceptionOnError("labels", string.Empty, p4RecordSet);
			}
		}
		return list;
	}
}
