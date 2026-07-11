using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class SourceControlFileInfo
{
	public readonly Uri Uri;

	public readonly SourceControlStatus Status;

	public readonly SourceControlStatus HeadStatus;

	public readonly int HeadRevision;

	public readonly int Revision;

	public readonly bool IsLocked;

	public readonly IEnumerable<string> OtherUsers;

	public readonly string OtherLock;

	public SourceControlFileInfo(Uri uri, SourceControlStatus status)
	{
		Uri = uri;
		Status = status;
		OtherUsers = EmptyEnumerable<string>.Instance;
	}

	public SourceControlFileInfo(Uri uri, SourceControlStatus status, int headRevision, int revision, bool isLocked, IEnumerable<string> otherUsers)
	{
		Uri = uri;
		Status = status;
		HeadRevision = headRevision;
		Revision = revision;
		IsLocked = isLocked;
		OtherUsers = otherUsers;
	}

	public SourceControlFileInfo(Uri uri, SourceControlStatus status, SourceControlStatus headStatus, int headRevision, int revision, bool isLocked, IEnumerable<string> otherUsers, string otherLock)
	{
		Uri = uri;
		Status = status;
		HeadStatus = headStatus;
		HeadRevision = headRevision;
		Revision = revision;
		IsLocked = isLocked;
		OtherUsers = otherUsers;
		OtherLock = otherLock;
	}
}
