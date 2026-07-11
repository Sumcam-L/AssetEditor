using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Hosting;

public sealed class PythonOptionsParser : OptionsParser<PythonConsoleOptions>
{
	private List<string> _warningFilters;

	protected override void ParseArgument(string arg)
	{
		ContractUtils.RequiresNotNull(arg, "arg");
		switch (arg)
		{
		case "-b":
			base.LanguageSetup.Options["BytesWarning"] = ScriptingRuntimeHelpers.True;
			break;
		case "-c":
		{
			base.ConsoleOptions.Command = PeekNextArg();
			string[] array = PopRemainingArgs();
			array[0] = arg;
			base.LanguageSetup.Options["Arguments"] = array;
			break;
		}
		case "-?":
			base.ConsoleOptions.PrintUsage = true;
			base.ConsoleOptions.Exit = true;
			break;
		case "-i":
			base.ConsoleOptions.Introspection = true;
			base.LanguageSetup.Options["Inspect"] = ScriptingRuntimeHelpers.True;
			break;
		case "-m":
			base.ConsoleOptions.ModuleToRun = PeekNextArg();
			base.LanguageSetup.Options["Arguments"] = PopRemainingArgs();
			break;
		case "-x":
			base.ConsoleOptions.SkipFirstSourceLine = true;
			break;
		case "-v":
			base.LanguageSetup.Options["Verbose"] = ScriptingRuntimeHelpers.True;
			break;
		case "-S":
			base.ConsoleOptions.SkipImportSite = true;
			base.LanguageSetup.Options["NoSite"] = ScriptingRuntimeHelpers.True;
			break;
		case "-s":
			base.LanguageSetup.Options["NoUserSite"] = ScriptingRuntimeHelpers.True;
			break;
		case "-E":
			base.ConsoleOptions.IgnoreEnvironmentVariables = true;
			base.LanguageSetup.Options["IgnoreEnvironment"] = ScriptingRuntimeHelpers.True;
			break;
		case "-t":
			base.LanguageSetup.Options["IndentationInconsistencySeverity"] = Severity.Warning;
			break;
		case "-tt":
			base.LanguageSetup.Options["IndentationInconsistencySeverity"] = Severity.Error;
			break;
		case "-O":
			base.LanguageSetup.Options["Optimize"] = ScriptingRuntimeHelpers.True;
			break;
		case "-OO":
			base.LanguageSetup.Options["Optimize"] = ScriptingRuntimeHelpers.True;
			base.LanguageSetup.Options["StripDocStrings"] = ScriptingRuntimeHelpers.True;
			break;
		case "-Q":
			base.LanguageSetup.Options["DivisionOptions"] = ToDivisionOptions(PopNextArg());
			break;
		case "-Qold":
		case "-Qnew":
		case "-Qwarn":
		case "-Qwarnall":
			base.LanguageSetup.Options["DivisionOptions"] = ToDivisionOptions(arg.Substring(2));
			break;
		case "-V":
			base.ConsoleOptions.PrintVersion = true;
			base.ConsoleOptions.Exit = true;
			IgnoreRemainingArgs();
			break;
		case "-W":
			if (_warningFilters == null)
			{
				_warningFilters = new List<string>();
			}
			_warningFilters.Add(PopNextArg());
			break;
		case "-3":
			base.LanguageSetup.Options["WarnPy3k"] = ScriptingRuntimeHelpers.True;
			break;
		case "-":
			PushArgBack();
			base.LanguageSetup.Options["Arguments"] = PopRemainingArgs();
			break;
		case "-X:Frames":
			base.LanguageSetup.Options["Frames"] = ScriptingRuntimeHelpers.True;
			break;
		case "-X:FullFrames":
		{
			IDictionary<string, object> options = base.LanguageSetup.Options;
			object value = (base.LanguageSetup.Options["FullFrames"] = ScriptingRuntimeHelpers.True);
			options["Frames"] = value;
			break;
		}
		case "-X:Tracing":
			base.LanguageSetup.Options["Tracing"] = ScriptingRuntimeHelpers.True;
			break;
		case "-X:GCStress":
		{
			if (!StringUtils.TryParseInt32(PopNextArg(), out var result2) || result2 < 0 || result2 > GC.MaxGeneration)
			{
				throw new InvalidOptionException($"The argument for the {arg} option must be between 0 and {GC.MaxGeneration}.");
			}
			base.LanguageSetup.Options["GCStress"] = result2;
			break;
		}
		case "-X:MaxRecursion":
		{
			if (!StringUtils.TryParseInt32(PopNextArg(), out var result) || result < 10)
			{
				throw new InvalidOptionException($"The argument for the {arg} option must be an integer >= 10.");
			}
			base.LanguageSetup.Options["RecursionLimit"] = result;
			break;
		}
		case "-X:EnableProfiler":
			base.LanguageSetup.Options["EnableProfiler"] = ScriptingRuntimeHelpers.True;
			break;
		case "-X:LightweightScopes":
			base.LanguageSetup.Options["LightweightScopes"] = ScriptingRuntimeHelpers.True;
			break;
		case "-X:MTA":
			base.ConsoleOptions.IsMta = true;
			break;
		case "-X:Python30":
			base.LanguageSetup.Options["PythonVersion"] = new Version(3, 0);
			break;
		case "-X:Debug":
			base.RuntimeSetup.DebugMode = true;
			base.LanguageSetup.Options["Debug"] = ScriptingRuntimeHelpers.True;
			break;
		case "-X:NoDebug":
		{
			string text = PopNextArg();
			try
			{
				base.LanguageSetup.Options["NoDebug"] = new Regex(text);
				break;
			}
			catch
			{
				throw OptionsParser.InvalidOptionValue("-X:NoDebug", text);
			}
		}
		case "-X:BasicConsole":
			base.ConsoleOptions.BasicConsole = true;
			break;
		default:
			base.ParseArgument(arg);
			if (base.ConsoleOptions.FileName != null)
			{
				PushArgBack();
				base.LanguageSetup.Options["Arguments"] = PopRemainingArgs();
			}
			break;
		case "-B":
		case "-U":
		case "-d":
		case "-u":
			break;
		}
	}

	protected override void AfterParse()
	{
		if (_warningFilters != null)
		{
			base.LanguageSetup.Options["WarningFilters"] = _warningFilters.ToArray();
		}
	}

	private static PythonDivisionOptions ToDivisionOptions(string value)
	{
		return value switch
		{
			"old" => PythonDivisionOptions.Old, 
			"new" => PythonDivisionOptions.New, 
			"warn" => PythonDivisionOptions.Warn, 
			"warnall" => PythonDivisionOptions.WarnAll, 
			_ => throw OptionsParser.InvalidOptionValue("-Q", value), 
		};
	}

	public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments)
	{
		base.GetHelp(out commandLine, out string[,] options2, out environmentVariables, out comments);
		commandLine = "Usage: ipy [options] [file.py|- [arguments]]";
		string[,] array = new string[26, 2]
		{
			{ "-v", "Verbose (trace import statements) (also PYTHONVERBOSE=x)" },
			{ "-m module", "run library module as a script" },
			{ "-x", "Skip first line of the source" },
			{ "-u", "Unbuffered stdout & stderr" },
			{ "-O", "generate optimized code" },
			{ "-OO", "remove doc strings and apply -O optimizations" },
			{ "-E", "Ignore environment variables" },
			{ "-Q arg", "Division options: -Qold (default), -Qwarn, -Qwarnall, -Qnew" },
			{ "-S", "Don't imply 'import site' on initialization" },
			{ "-s", "Don't add user site directory to sys.path" },
			{ "-t", "Issue warnings about inconsistent tab usage" },
			{ "-tt", "Issue errors for inconsistent tab usage" },
			{ "-W arg", "Warning control (arg is action:message:category:module:lineno)" },
			{ "-3", "Warn about Python 3.x incompatibilities" },
			{ "-X:Frames", "Enable basic sys._getframe support" },
			{ "-X:FullFrames", "Enable sys._getframe with access to locals" },
			{ "-X:Tracing", "Enable support for tracing all methods even before sys.settrace is called" },
			{ "-X:GCStress", "Specifies the GC stress level (the generation to collect each statement)" },
			{ "-X:MaxRecursion", "Set the maximum recursion level" },
			{ "-X:Debug", "Enable application debugging (preferred over -D)" },
			{ "-X:NoDebug <regex>", "Provides a regular expression of files which should not be emitted in debug mode" },
			{ "-X:MTA", "Run in multithreaded apartment" },
			{ "-X:Python30", "Enable available Python 3.0 features" },
			{ "-X:EnableProfiler", "Enables profiling support in the compiler" },
			{ "-X:LightweightScopes", "Generate optimized scopes that can be garbage collected" },
			{ "-X:BasicConsole", "Use only the basic console features" }
		};
		string[,] array2 = ArrayUtils.Concatenate(array, options2);
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < array2.Length / 2; i++)
		{
			list.Add(array2[i, 0]);
			list2.Add(i);
		}
		int[] array3 = list2.ToArray();
		Array.Sort(list.ToArray(), array3, StringComparer.InvariantCulture);
		options = new string[array2.Length / 2, 2];
		for (int j = 0; j < array3.Length; j++)
		{
			options[j, 0] = array2[array3[j], 0];
			options[j, 1] = array2[array3[j], 1];
		}
		environmentVariables = new string[2, 2]
		{
			{ "IRONPYTHONPATH", "Path to search for module" },
			{ "IRONPYTHONSTARTUP", "Startup module" }
		};
	}
}
