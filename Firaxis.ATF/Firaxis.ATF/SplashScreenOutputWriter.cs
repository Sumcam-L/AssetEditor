using System.ComponentModel.Composition;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(ICategorizedOutputWriter))]
[Export(typeof(ISplashScreenOutputWriter))]
[Export(typeof(SplashScreenOutputWriter))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SplashScreenOutputWriter : ICategorizedOutputWriter, ISplashScreenOutputWriter
{
	public event CategorizedOutputEventHandler Logger;

	public void Clear()
	{
	}

	public void Write(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (type < OutputMessageType.Diagnostic)
		{
			this.Logger?.Invoke(category, type, verbosity, message);
		}
	}
}
