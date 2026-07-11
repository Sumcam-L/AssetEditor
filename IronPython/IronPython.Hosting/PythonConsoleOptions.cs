using System;
using Microsoft.Scripting.Hosting.Shell;

namespace IronPython.Hosting;

[CLSCompliant(true)]
public class PythonConsoleOptions : ConsoleOptions
{
	private bool _ignoreEnvironmentVariables;

	private bool _skipImportSite;

	private bool _skipFistSourceLine;

	private string _runAsModule;

	private bool _basicConsole;

	public bool IgnoreEnvironmentVariables
	{
		get
		{
			return _ignoreEnvironmentVariables;
		}
		set
		{
			_ignoreEnvironmentVariables = value;
		}
	}

	public bool SkipImportSite
	{
		get
		{
			return _skipImportSite;
		}
		set
		{
			_skipImportSite = value;
		}
	}

	public string ModuleToRun
	{
		get
		{
			return _runAsModule;
		}
		set
		{
			_runAsModule = value;
		}
	}

	public bool SkipFirstSourceLine
	{
		get
		{
			return _skipFistSourceLine;
		}
		set
		{
			_skipFistSourceLine = value;
		}
	}

	public bool BasicConsole
	{
		get
		{
			return _basicConsole;
		}
		set
		{
			_basicConsole = value;
		}
	}
}
