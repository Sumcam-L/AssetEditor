using Firaxis.Error;

namespace Firaxis.VersionControl;

public class FileStatusResultCode : ItemResultCode
{
	public VersionControlStatus Status { get; internal set; }

	public FileStatusResultCode()
	{
		Status = new VersionControlStatus();
	}

	public FileStatusResultCode(VersionControlPath f, ResultCode r)
		: base(f, r)
	{
		Status = new VersionControlStatus();
	}

	public FileStatusResultCode(VersionControlPath f, ResultCode r, VersionControlStatus status)
		: base(f, r)
	{
		Status = status;
	}
}
