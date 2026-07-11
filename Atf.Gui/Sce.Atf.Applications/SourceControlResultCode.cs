using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class SourceControlResultCode
{
	public readonly IEnumerable<Uri> Uris;

	public readonly string Message;

	public string ResultInformation { get; set; }

	public static implicit operator bool(SourceControlResultCode rc)
	{
		return string.IsNullOrWhiteSpace(rc.Message);
	}

	public SourceControlResultCode(Uri uri)
		: this(uri, "")
	{
	}

	public SourceControlResultCode(Uri uri, string message, params object[] args)
		: this(uri, string.Format(message, args))
	{
	}

	public SourceControlResultCode(Uri uri, string msg)
		: this(new Uri[1] { uri }, msg)
	{
	}

	public SourceControlResultCode(IEnumerable<Uri> uris)
		: this(uris, "")
	{
	}

	public SourceControlResultCode(IEnumerable<Uri> uris, string message, params object[] args)
		: this(uris, string.Format(message, args))
	{
	}

	public SourceControlResultCode(IEnumerable<Uri> uris, string msg)
	{
		Uris = uris;
		Message = msg;
	}
}
