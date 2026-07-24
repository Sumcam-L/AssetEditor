using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;

namespace Sce.Atf;

public static class MefUtil
{
	public static void InitializeAll(this CompositionContainer container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		ImportDefinition definition = new ImportDefinition((ExportDefinition contraint) => true, string.Empty, ImportCardinality.ZeroOrMore, isRecomposable: true, isPrerequisite: true);
		try
		{
			var composeTimer = Stopwatch.StartNew();
			foreach (Export export in container.GetExports(definition))
			{
				object value = export.Value;
			}
			LogTiming("Startup: init-all compose elapsed={0}ms", composeTimer.ElapsedMilliseconds);

			var slowServices = new List<string>();
			foreach (IInitializable exportedValue in container.GetExportedValues<IInitializable>())
			{
				var svcTimer = Stopwatch.StartNew();
				exportedValue.Initialize();
				long ms = svcTimer.ElapsedMilliseconds;
				if (ms >= 50)
				{
					string name = exportedValue.GetType().Name;
					slowServices.Add(string.Format("Startup: init-svc {0} elapsed={1}ms", name, ms));
				}
			}
			foreach (var line in slowServices)
			{
				LogTiming(line);
			}
		}
		catch (CompositionException ex)
		{
			foreach (CompositionError error in ex.Errors)
			{
				Outputs.WriteLine(OutputMessageType.Error, "MEF CompositionException: {0}", error.Description);
			}
			throw;
		}
	}

	private static void LogTiming(string format, params object[] args)
	{
#if DEBUG
		try
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");
			string message = args == null || args.Length == 0 ? format : string.Format(format, args);
			File.AppendAllText(path, string.Format("{0:O} {1}{2}", DateTime.Now, message, Environment.NewLine));
		}
		catch { }
#endif
	}
}
