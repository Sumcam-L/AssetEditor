using System;

namespace Sce.Atf.Applications;

public class SourceControlResultCodeEventArgs : EventArgs
{
	public readonly SourceControlResultCode SourceControlResult;

	public SourceControlResultCodeEventArgs(SourceControlResultCode code)
	{
		SourceControlResult = code;
	}
}
