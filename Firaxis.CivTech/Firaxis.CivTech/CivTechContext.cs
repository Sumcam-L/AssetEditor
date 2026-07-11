using System;
using System.IO;
using System.Reflection;
using Firaxis.CivTech.Properties;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.CivTech;

public class CivTechContext : IDisposable
{
	private bool disposedValue = false;

	private Assembly CivTechAssembly { get; set; }

	private ICivTechProxy CivTechProxy { get; set; }

	private ICivTechAssert CivTechAssert { get; set; }

	public ICivTechLogger CivTechLogger { get; set; }

	public string CivTechPath => CivTechAssembly.Location;

	public CivTechContext()
		: this(AssertionConfiguration.eOutput)
	{
	}

	public CivTechContext(AssertionConfiguration assCfg)
	{
		string baseAssemblyName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Resources.ProxyModule);
		CivTechAssembly = ReflectionHelper.ProxyAssembly(baseAssemblyName, bLoadDependenciesFromExePath: false, string.Empty, string.Empty, ".dll");
		CivTechProxy = ReflectionHelper.TypeLoader<ICivTechProxy>(CivTechAssembly, new object[1] { Environment.CurrentDirectory });
		CivTechLogger = ReflectionHelper.TypeLoader<ICivTechLogger>(CivTechAssembly);
		CivTechAssert = CreateInstance<ICivTechAssert>(new object[2]
		{
			Environment.CurrentDirectory,
			assCfg
		});
	}

	public T CreateInstance<T>() where T : IAssemblyInstance
	{
		return ReflectionHelper.TypeLoader<T>(CivTechAssembly);
	}

	public T CreateInstance<T>(params object[] args) where T : IAssemblyInstance
	{
		return ReflectionHelper.TypeLoader<T>(CivTechAssembly, args);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			Context.Remove(this);
			if (disposing)
			{
				CivTechAssembly = null;
				CivTechLogger?.Dispose();
				CivTechProxy?.Dispose();
				CivTechAssert?.Dispose();
			}
			CivTechLogger = null;
			CivTechProxy = null;
			CivTechAssert = null;
			disposedValue = true;
		}
	}

	~CivTechContext()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
