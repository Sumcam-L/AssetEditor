using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;

namespace IronPython.Hosting;

public sealed class PythonCommandLine : CommandLine
{
	private PythonContext PythonContext => Language;

	private new PythonConsoleOptions Options => (PythonConsoleOptions)base.Options;

	protected override string Logo => GetLogoDisplay();

	protected override string Prompt
	{
		get
		{
			if (base.Engine.GetSysModule().TryGetVariable("ps1", out object value))
			{
				CodeContext globalContext = ((PythonScopeExtension)base.Scope.GetExtension(Language.ContextId)).ModuleContext.GlobalContext;
				return PythonOps.ToString(globalContext, value);
			}
			return ">>> ";
		}
	}

	public override string PromptContinuation
	{
		get
		{
			if (base.Engine.GetSysModule().TryGetVariable("ps2", out object value))
			{
				CodeContext globalContext = ((PythonScopeExtension)base.Scope.GetExtension(Language.ContextId)).ModuleContext.GlobalContext;
				return PythonOps.ToString(globalContext, value);
			}
			return "... ";
		}
	}

	protected override ErrorSink ErrorSink => ThrowingErrorSink.Default;

	private new PythonContext Language => (PythonContext)base.Language;

	public static string GetLogoDisplay()
	{
		return PythonContext.GetVersionString() + "\nType \"help\", \"copyright\", \"credits\" or \"license\" for more information.\n";
	}

	private int GetEffectiveExitCode(SystemExitException e)
	{
		object otherCode;
		int exitCode = e.GetExitCode(out otherCode);
		if (otherCode != null)
		{
			base.Console.WriteLine(otherCode.ToString(), Style.Error);
		}
		return exitCode;
	}

	protected override void Shutdown()
	{
		try
		{
			Language.Shutdown();
		}
		catch (Exception exception)
		{
			base.Console.WriteLine("", Style.Error);
			base.Console.WriteLine("Error in sys.exitfunc:", Style.Error);
			base.Console.Write(Language.FormatException(exception), Style.Error);
		}
	}

	protected override int Run()
	{
		if (Options.ModuleToRun != null)
		{
			object o;
			try
			{
				o = Importer.Import(PythonContext.SharedContext, "runpy", PythonTuple.EMPTY, 0);
			}
			catch (Exception)
			{
				base.Console.WriteLine("Could not import runpy module", Style.Error);
				return -1;
			}
			object boundAttr;
			try
			{
				boundAttr = PythonOps.GetBoundAttr(PythonContext.SharedContext, o, "run_module");
			}
			catch (Exception)
			{
				base.Console.WriteLine("Could not access runpy.run_module", Style.Error);
				return -1;
			}
			try
			{
				PythonOps.CallWithKeywordArgs(PythonContext.SharedContext, boundAttr, new object[3]
				{
					Options.ModuleToRun,
					"__main__",
					ScriptingRuntimeHelpers.True
				}, new string[2] { "run_name", "alter_sys" });
			}
			catch (SystemExitException ex3)
			{
				object otherCode;
				return ex3.GetExitCode(out otherCode);
			}
			return 0;
		}
		int result = base.Run();
		string environmentVariable = Environment.GetEnvironmentVariable("IRONPYTHONINSPECT");
		if (environmentVariable != null && !Options.Introspection)
		{
			result = RunInteractiveLoop();
		}
		return result;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Console.Output = new OutputWriter(PythonContext, isErrorOutput: false);
		base.Console.ErrorOutput = new OutputWriter(PythonContext, isErrorOutput: true);
		int pathIndex = PythonContext.PythonOptions.SearchPaths.Count;
		Language.DomainManager.LoadAssembly(typeof(string).Assembly);
		Language.DomainManager.LoadAssembly(typeof(Debug).Assembly);
		InitializePath(ref pathIndex);
		InitializeModules();
		InitializeExtensionDLLs();
		ImportSite();
		string environmentVariable = Environment.GetEnvironmentVariable("IRONPYTHONINSPECT");
		if (environmentVariable != null)
		{
			Options.Introspection = true;
		}
		string directory = ".";
		if (Options.Command == null && Options.FileName != null)
		{
			if (Options.FileName == "-")
			{
				Options.FileName = "<stdin>";
			}
			else
			{
				if (Directory.Exists(Options.FileName))
				{
					Options.FileName = Path.Combine(Options.FileName, "__main__.py");
				}
				if (!File.Exists(Options.FileName))
				{
					base.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "File {0} does not exist.", new object[1] { Options.FileName }), Style.Error);
					Environment.Exit(1);
				}
				directory = Path.GetDirectoryName(Language.DomainManager.Platform.GetFullPath(Options.FileName));
			}
		}
		PythonContext.InsertIntoPath(0, directory);
		PythonContext.MainThread = Thread.CurrentThread;
	}

	protected override Scope CreateScope()
	{
		ModuleOptions features = ((PythonContext.PythonOptions.DivisionOptions == PythonDivisionOptions.New) ? ModuleOptions.TrueDivision : ModuleOptions.None);
		ModuleContext moduleContext = new ModuleContext(new PythonDictionary(), PythonContext);
		moduleContext.Features = features;
		moduleContext.InitializeBuiltins(moduleBuiltins: true);
		PythonContext.PublishModule("__main__", moduleContext.Module);
		moduleContext.Globals["__doc__"] = null;
		moduleContext.Globals["__name__"] = "__main__";
		return moduleContext.GlobalScope;
	}

	private void InitializePath(ref int pathIndex)
	{
		if (Options.IgnoreEnvironmentVariables)
		{
			return;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
		if (environmentVariable != null && environmentVariable.Length > 0)
		{
			string[] array = environmentVariable.Split(Path.PathSeparator);
			string[] array2 = array;
			foreach (string directory in array2)
			{
				PythonContext.InsertIntoPath(pathIndex++, directory);
			}
		}
	}

	private void InitializeModules()
	{
		string text = "";
		string prefix = "";
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly != null)
		{
			text = entryAssembly.Location;
			prefix = Path.GetDirectoryName(text);
		}
		PythonContext.SetHostVariables(prefix, text, null);
	}

	private void InitializeExtensionDLLs()
	{
		string path = Path.Combine(PythonContext.InitialPrefix, "DLLs");
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] files = Directory.GetFiles(path);
		foreach (string text in files)
		{
			if (text.ToLower().EndsWith(".dll"))
			{
				try
				{
					ClrModule.AddReference(PythonContext.SharedContext, new FileInfo(text).Name);
				}
				catch
				{
				}
			}
		}
	}

	private void ImportSite()
	{
		if (Options.SkipImportSite)
		{
			return;
		}
		try
		{
			Importer.ImportModule(PythonContext.SharedContext, null, "site", bottom: false, -1);
		}
		catch (Exception exception)
		{
			base.Console.Write(Language.FormatException(exception), Style.Error);
		}
	}

	protected override int RunInteractive()
	{
		PrintLogo();
		if (base.Scope == null)
		{
			base.Scope = CreateScope();
		}
		int num = 1;
		try
		{
			RunStartup();
			num = 0;
		}
		catch (SystemExitException e)
		{
			return GetEffectiveExitCode(e);
		}
		catch (Exception)
		{
		}
		ScriptScope sysModule = base.Engine.GetSysModule();
		sysModule.SetVariable("ps1", ">>> ");
		sysModule.SetVariable("ps2", "... ");
		return RunInteractiveLoop();
	}

	private void RunStartup()
	{
		if (Options.IgnoreEnvironmentVariables)
		{
			return;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("IRONPYTHONSTARTUP");
		if (environmentVariable == null || environmentVariable.Length <= 0)
		{
			return;
		}
		if (Options.HandleExceptions)
		{
			try
			{
				ExecuteCommand(base.Engine.CreateScriptSourceFromFile(environmentVariable));
				return;
			}
			catch (Exception ex)
			{
				if (ex is SystemExitException)
				{
					throw;
				}
				base.Console.Write(Language.FormatException(ex), Style.Error);
				return;
			}
		}
		ExecuteCommand(base.Engine.CreateScriptSourceFromFile(environmentVariable));
	}

	protected override int? TryInteractiveAction()
	{
		try
		{
			try
			{
				return TryInteractiveActionWorker();
			}
			finally
			{
				PythonOps.ClearCurrentException();
			}
		}
		catch (SystemExitException e)
		{
			return GetEffectiveExitCode(e);
		}
	}

	private int? TryInteractiveActionWorker()
	{
		int? result = null;
		try
		{
			result = RunOneInteraction();
			return result;
		}
		catch (ThreadAbortException ex)
		{
			if (ex.ExceptionState is KeyboardInterruptException)
			{
				base.Console.WriteLine(Language.FormatException(ex), Style.Error);
				Thread.ResetAbort();
			}
		}
		return result;
	}

	private int? RunOneInteraction()
	{
		bool continueInteraction;
		string text = ReadStatement(out continueInteraction);
		if (!continueInteraction)
		{
			PythonContext.DispatchCommand(null);
			return 0;
		}
		if (string.IsNullOrEmpty(text))
		{
			base.Console.Write(string.Empty, Style.Out);
			return null;
		}
		SourceUnit su = Language.CreateSnippet(text, "<stdin>", SourceCodeKind.InteractiveCode);
		PythonCompilerOptions pco = (PythonCompilerOptions)Language.GetCompilerOptions(base.Scope);
		pco.Module |= ModuleOptions.ExecOrEvalCode;
		Action command = delegate
		{
			try
			{
				su.Compile(pco, ErrorSink).Run(base.Scope);
			}
			catch (Exception ex2)
			{
				if (ex2 is SystemExitException)
				{
					throw;
				}
				UnhandledException(ex2);
			}
		};
		try
		{
			PythonContext.DispatchCommand(command);
		}
		catch (SystemExitException ex)
		{
			object otherCode;
			return ex.GetExitCode(out otherCode);
		}
		return null;
	}

	protected override int GetNextAutoIndentSize(string text)
	{
		return Parser.GetNextAutoIndentSize(text, Options.AutoIndentSize);
	}

	protected override int RunCommand(string command)
	{
		if (Options.HandleExceptions)
		{
			try
			{
				return RunCommandWorker(command);
			}
			catch (Exception exception)
			{
				base.Console.Write(Language.FormatException(exception), Style.Error);
				return 1;
			}
		}
		return RunCommandWorker(command);
	}

	private int RunCommandWorker(string command)
	{
		int num = 1;
		try
		{
			base.Scope = CreateScope();
			ExecuteCommand(base.Engine.CreateScriptSourceFromString(command, SourceCodeKind.File));
			return 0;
		}
		catch (SystemExitException e)
		{
			return GetEffectiveExitCode(e);
		}
	}

	protected override int RunFile(string fileName)
	{
		int result = 1;
		if (Options.HandleExceptions)
		{
			try
			{
				result = RunFileWorker(fileName);
			}
			catch (Exception exception)
			{
				base.Console.Write(Language.FormatException(exception), Style.Error);
			}
		}
		else
		{
			result = RunFileWorker(fileName);
		}
		return result;
	}

	private int RunFileWorker(string fileName)
	{
		ModuleOptions moduleOptions = ModuleOptions.Optimized | ModuleOptions.ModuleBuiltins;
		if (Options.SkipFirstSourceLine)
		{
			moduleOptions |= ModuleOptions.SkipFirstLine;
		}
		ScriptCode scriptCode;
		PythonModule pythonModule = PythonContext.CompileModule(fileName, "__main__", PythonContext.CreateFileUnit(string.IsNullOrEmpty(fileName) ? null : fileName, PythonContext.DefaultEncoding), moduleOptions, out scriptCode);
		PythonContext.PublishModule("__main__", pythonModule);
		base.Scope = pythonModule.Scope;
		try
		{
			scriptCode.Run(base.Scope);
		}
		catch (SystemExitException e)
		{
			Options.Introspection = false;
			return GetEffectiveExitCode(e);
		}
		return 0;
	}

	public override IList<string> GetGlobals(string name)
	{
		IList<string> globals = base.GetGlobals(name);
		foreach (object key in PythonContext.BuiltinModuleInstance.__dict__.Keys)
		{
			if (key is string text && text.StartsWith(name))
			{
				globals.Add(text);
			}
		}
		return globals;
	}

	protected override void UnhandledException(Exception e)
	{
		PythonOps.PrintException(PythonContext.SharedContext, e, base.Console);
	}
}
