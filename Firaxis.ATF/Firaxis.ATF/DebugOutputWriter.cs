using System.ComponentModel.Composition;
using System.Diagnostics;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(ICategorizedOutputWriter))]
[Export(typeof(DebugOutputWriter))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DebugOutputWriter : ICategorizedOutputWriter
{
	private static object m_logLocker = new object();

	public void Clear()
	{
	}

	public void Write(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (type != OutputMessageType.Debug)
		{
			return;
		}
		lock (m_logLocker)
		{
			Debugger.Log((int)verbosity, category, message.Trim('\r', '\n'));
		}
	}
}
