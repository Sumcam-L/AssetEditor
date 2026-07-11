using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Firaxis.ATF;

public class BugSubmissionHeader
{
	public string Application { get; set; }

	public IList<ulong> Callstack { get; set; }

	public Catalog Catalog { get; set; }

	public uint Changelist { get; set; }

	public Exception Exception { get; set; }

	public string File { get; set; }

	public string Host { get; set; }

	public int Line { get; set; }

	public IList<ModuleInfo> ModuleInfo { get; set; }

	public SessionInfo Session { get; set; }

	public long Size { get; set; }

	public string SubmissionType { get; set; }

	public string Time { get; set; }

	public VersionInfo Version { get; set; }

	public BugSubmissionHeader()
		: this(0L)
	{
	}

	public BugSubmissionHeader(long dataSize)
	{
		Host = Environment.MachineName;
		Size = dataSize;
		Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		Catalog = new Catalog();
		Exception = new Exception();
		Callstack = new List<ulong>();
		ModuleInfo = new List<ModuleInfo>();
		Session = new SessionInfo();
		Version = new VersionInfo();
		Application = Path.GetFileName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
	}
}
