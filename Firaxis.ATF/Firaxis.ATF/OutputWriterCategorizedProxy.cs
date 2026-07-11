using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IOutputWriter))]
[Export(typeof(OutputWriterCategorizedProxy))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OutputWriterCategorizedProxy : IOutputWriter
{
	[ImportMany(AllowRecomposition = true)]
	private IEnumerable<Lazy<ICategorizedOutputWriter>> m_categorizedOutputWriters;

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		foreach (ICategorizedOutputWriter item in m_categorizedOutputWriters?.GetValues())
		{
			item.Write("Tool", type, verbosity, message);
		}
	}

	public void Clear()
	{
		foreach (ICategorizedOutputWriter item in m_categorizedOutputWriters?.GetValues())
		{
			item.Clear();
		}
	}
}
