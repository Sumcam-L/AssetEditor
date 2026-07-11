using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

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
			foreach (Export export in container.GetExports(definition))
			{
				object value = export.Value;
			}
			foreach (IInitializable exportedValue in container.GetExportedValues<IInitializable>())
			{
				exportedValue.Initialize();
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
}
