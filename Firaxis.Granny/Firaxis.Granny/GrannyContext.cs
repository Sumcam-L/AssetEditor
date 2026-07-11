using System;
using System.IO;
using System.Reflection;
using Firaxis.Granny.Properties;
using Firaxis.Reflection;

namespace Firaxis.Granny;

public class GrannyContext : IDisposable
{
	private Assembly GrannyAssembly { get; set; }

	private IGrannyProxy GrannyProxy { get; set; }

	private IGrannyFileLoader GrannyFileLoader { get; set; }

	private bool Is64Bit => IntPtr.Size == 8;

	public GrannyContext()
	{
		string baseAssemblyName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Resources.ProxyModule);
		GrannyAssembly = ReflectionHelper.ProxyAssembly(baseAssemblyName, bLoadDependenciesFromExePath: false, string.Empty, string.Empty, ".dll");
		GrannyProxy = ReflectionHelper.TypeLoader<IGrannyProxy>(GrannyAssembly, new object[1] { Environment.CurrentDirectory });
		GrannyFileLoader = ReflectionHelper.TypeLoader<IGrannyFileLoader>(GrannyAssembly);
	}

	public bool LoadStringDatabase(string file)
	{
		return GrannyFileLoader.LoadStringDatabase(file);
	}

	public IGrannyFile LoadGrannyFile(string file)
	{
		return GrannyFileLoader.LoadGrannyFile(file);
	}

	public IGrannyFile CreateEmptyGrannyFile(string file)
	{
		return GrannyFileLoader.CreateEmptyGrannyFile(file);
	}

	public void Dispose()
	{
		if (GrannyProxy != null)
		{
			GrannyProxy.Dispose();
		}
		if (GrannyFileLoader != null)
		{
			GrannyFileLoader.Dispose();
		}
	}
}
