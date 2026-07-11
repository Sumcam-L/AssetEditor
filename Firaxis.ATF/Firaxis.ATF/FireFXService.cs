using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.FireFX;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IFireFXService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FireFXService : IFireFXService, IDisposable
{
	private IToolHostLoaderService ToolHostLoader { get; set; }

	private IFireFXCompilerService FireFXCompiler { get; set; }

	private ICivTechService CivTechService { get; set; }

	[ImportingConstructor]
	public FireFXService(ICivTechService civTechSvc, IToolHostLoaderService toolHostLoader)
	{
		CivTechService = civTechSvc;
		ToolHostLoader = toolHostLoader;
		ToolHostLoader.Loaded += ToolHostLoader_Loaded;
		ToolHostLoader.Unloaded += ToolHostLoader_Unloaded;
		if (ToolHostLoader.ToolHostInterface != null && ToolHostLoader.ToolHostInterface.IsLoaded)
		{
			StartupFireFXCompiler();
		}
	}

	private void ToolHostLoader_Loaded(object sender, ToolHostEventArgs e)
	{
		StartupFireFXCompiler();
	}

	private void ToolHostLoader_Unloaded(object sender, ToolHostEventArgs e)
	{
		ShutdownFireFXCompiler();
	}

	public ResultCode CompileResource(IFireFXScriptResource scriptResource)
	{
		if (FireFXCompiler == null)
		{
			scriptResource.Effect = null;
			return new ResultCode("FireFX compiler not available. ToolHost DLL \"{0}\" {1} loaded", ToolHostLoader.ToolHostDllPath, ToolHostLoader.ToolHostInterface.IsLoaded ? "was" : "was not");
		}
		scriptResource.Issues.Clear();
		string fileName = Path.GetFileName(scriptResource.Uri?.LocalPath ?? string.Empty);
		IFireFXEffect effect;
		ResultCode resultCode = FireFXCompiler.CompileText(fileName, scriptResource.Text, out effect, scriptResource.Issues);
		if (!resultCode)
		{
			scriptResource.Effect = null;
			return resultCode;
		}
		scriptResource.Effect = effect;
		return ResultCode.Success;
	}

	public ResultCode Compile(string scriptPath, out IFireFXEffect effect, IList<CompileIssue> issues)
	{
		if (FireFXCompiler == null)
		{
			effect = null;
			return new ResultCode("FireFX compiler not available. ToolHost DLL \"{0}\" {1} loaded", ToolHostLoader.ToolHostDllPath, ToolHostLoader.ToolHostInterface.IsLoaded ? "was" : "was not");
		}
		return FireFXCompiler.Compile(scriptPath, out effect, issues);
	}

	public ResultCode Compile(string scriptPath, string byteCodePath, IList<CompileIssue> issues)
	{
		if (FireFXCompiler == null)
		{
			return new ResultCode("FireFX compiler not available. ToolHost DLL \"{0}\" {1} loaded", ToolHostLoader.ToolHostDllPath, ToolHostLoader.ToolHostInterface.IsLoaded ? "was" : "was not");
		}
		return FireFXCompiler.Compile(scriptPath, byteCodePath, issues);
	}

	public ResultCode CompileText(string scriptName, string scriptText, out IFireFXEffect effect, IList<CompileIssue> issues)
	{
		if (FireFXCompiler == null)
		{
			effect = null;
			return new ResultCode("FireFX compiler not available. ToolHost DLL \"{0}\" {1} loaded", ToolHostLoader.ToolHostDllPath, ToolHostLoader.ToolHostInterface.IsLoaded ? "was" : "was not");
		}
		return FireFXCompiler.CompileText(scriptName, scriptText, out effect, issues);
	}

	public ResultCode CompileText(string scriptName, string scriptText, string byteCodePath, IList<CompileIssue> issues)
	{
		if (FireFXCompiler == null)
		{
			return new ResultCode("FireFX compiler not available. ToolHost DLL \"{0}\" {1} loaded", ToolHostLoader.ToolHostDllPath, ToolHostLoader.ToolHostInterface.IsLoaded ? "was" : "was not");
		}
		return FireFXCompiler.CompileText(scriptName, scriptText, byteCodePath, issues);
	}

	public void Dispose()
	{
		ShutdownFireFXCompiler();
	}

	private void StartupFireFXCompiler()
	{
		IFireFXCompilerService fireFXCompilerService = Context.EnsureCreated<CivTechContext>().CreateInstance<IFireFXCompilerService>();
		if (fireFXCompilerService != null)
		{
			ResultCode resultCode = fireFXCompilerService.Startup(CivTechService.ProjectMapService.LayeredPantry);
			if ((bool)resultCode)
			{
				FireFXCompiler = fireFXCompilerService;
				return;
			}
			Outputs.WriteLine(OutputMessageType.Error, "Failed to startup FireFX compiler service! Error: {0}", resultCode.Message);
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Failed to construct FireFX compiler service!");
		}
	}

	private void ShutdownFireFXCompiler()
	{
		FireFXCompiler?.Shutdown();
		FireFXCompiler = null;
	}
}
