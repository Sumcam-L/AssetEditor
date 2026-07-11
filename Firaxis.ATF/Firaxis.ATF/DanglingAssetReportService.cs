using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IDanglingAssetReportService))]
public class DanglingAssetReportService : IDanglingAssetReportService
{
	private readonly IArtDefRegistry m_artDefRegistry;

	private readonly ICivTechService m_civTechService;

	[ImportingConstructor]
	public DanglingAssetReportService(IArtDefRegistry artDefRegistry, ICivTechService civTechService)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_artDefRegistry = artDefRegistry;
			m_civTechService = civTechService;
		}
	}

	public string GenerateDanglingAssetReport()
	{
		IEnumerable<string> projectXLPPantries = GetProjectXLPPantries();
		IDictionary<string, IXLP> dictionary = LoadAllXLPs(projectXLPPantries);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, IXLP> item in dictionary.OrderBy((KeyValuePair<string, IXLP> x) => x.Key))
		{
			IXLP value = item.Value;
			if (value.ClassName == "UITexture")
			{
				continue;
			}
			IEnumerable<IXLPEntry> orphanedEntries = m_artDefRegistry.GetOrphanedEntries(value);
			if (!orphanedEntries.Any())
			{
				continue;
			}
			stringBuilder.AppendFormat("XLP at {0} has the following dangling entries:\n\n", item.Key);
			foreach (string item2 in from x in orphanedEntries
				select x.ID into x
				orderby x
				select x)
			{
				stringBuilder.AppendFormat("\t- {0}\n", item2);
			}
			stringBuilder.Append("\n\n\n");
		}
		foreach (IXLP value2 in dictionary.Values)
		{
			value2.Dispose();
		}
		dictionary.Clear();
		return stringBuilder.ToString();
	}

	private IEnumerable<string> GetProjectXLPPantries()
	{
		ICollection<string> collection = new List<string>();
		foreach (string dependency in m_civTechService.PrimaryProject.Dependencies)
		{
			ProjectEnvironment project = null;
			if (m_civTechService.ActiveProjectMap.GetProject(dependency, ref project))
			{
				string xLPRoot = project.Paths.XLPRoot;
				collection.Add(xLPRoot);
			}
		}
		collection.Add(m_civTechService.PrimaryProject.Paths.XLPRoot);
		return collection;
	}

	private IDictionary<string, IXLP> LoadAllXLPs(IEnumerable<string> xlpPantries)
	{
		List<string> list = new List<string>();
		foreach (string xlpPantry in xlpPantries)
		{
			list.AddRange(Directory.GetFiles(xlpPantry, "*.xlp", SearchOption.AllDirectories));
		}
		CivTechContext context = Context.EnsureCreated<CivTechContext>();
		Func<IXLP> factoryFunction = () => context.CreateInstance<IXLP>();
		using RegistryLoader<IXLP> registryLoader = new RegistryLoader<IXLP>(list, factoryFunction);
		registryLoader.StartLoad();
		registryLoader.Wait();
		return new Dictionary<string, IXLP>(registryLoader.Result);
	}
}
