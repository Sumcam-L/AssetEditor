using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;

namespace IronPython;

[Serializable]
[CLSCompliant(true)]
public sealed class PythonOptions : LanguageOptions
{
	private readonly ReadOnlyCollection<string> _arguments;

	private readonly ReadOnlyCollection<string> _warningFilters;

	private readonly bool _warnPy3k;

	private readonly bool _python30;

	private readonly bool _bytesWarning;

	private readonly bool _debug;

	private readonly int _recursionLimit;

	private readonly Severity _indentationInconsistencySeverity;

	private readonly PythonDivisionOptions _division;

	private readonly bool _stripDocStrings;

	private readonly bool _optimize;

	private readonly bool _inspect;

	private readonly bool _noUserSite;

	private readonly bool _noSite;

	private readonly bool _ignoreEnvironment;

	private readonly bool _verbose;

	private readonly bool _frames;

	private readonly bool _fullFrames;

	private readonly bool _tracing;

	private readonly Version _version;

	private readonly Regex _noDebug;

	private readonly int? _gcStress;

	private bool _enableProfiler;

	private readonly bool _lightweightScopes;

	public ReadOnlyCollection<string> Arguments => _arguments;

	public bool Optimize => _optimize;

	public bool StripDocStrings => _stripDocStrings;

	public ReadOnlyCollection<string> WarningFilters => _warningFilters;

	public bool WarnPython30 => _warnPy3k;

	public bool Python30 => _python30;

	public bool BytesWarning => _bytesWarning;

	public bool Debug => _debug;

	public bool Inspect => _inspect;

	public bool NoUserSite => _noUserSite;

	public bool NoSite => _noSite;

	public bool IgnoreEnvironment => _ignoreEnvironment;

	public bool Verbose => _verbose;

	public int RecursionLimit => _recursionLimit;

	public bool Frames => _frames;

	public bool FullFrames => _fullFrames;

	public bool Tracing => _tracing;

	public Severity IndentationInconsistencySeverity => _indentationInconsistencySeverity;

	[CLSCompliant(false)]
	public PythonDivisionOptions DivisionOptions => _division;

	public bool LightweightScopes => _lightweightScopes;

	public bool EnableProfiler
	{
		get
		{
			return _enableProfiler;
		}
		set
		{
			_enableProfiler = value;
		}
	}

	public int? GCStress => _gcStress;

	public Regex NoDebug => _noDebug;

	public Version PythonVersion => _version;

	public PythonOptions()
		: this(null)
	{
	}

	public PythonOptions(IDictionary<string, object> options)
		: base(EnsureSearchPaths(options))
	{
		_arguments = LanguageOptions.GetStringCollectionOption(options, "Arguments") ?? LanguageOptions.EmptyStringCollection;
		_warningFilters = LanguageOptions.GetStringCollectionOption(options, "WarningFilters", ';', ',') ?? LanguageOptions.EmptyStringCollection;
		_warnPy3k = LanguageOptions.GetOption(options, "WarnPy3k", defaultValue: false);
		_bytesWarning = LanguageOptions.GetOption(options, "BytesWarning", defaultValue: false);
		_debug = LanguageOptions.GetOption(options, "Debug", defaultValue: false);
		_inspect = LanguageOptions.GetOption(options, "Inspect", defaultValue: false);
		_noUserSite = LanguageOptions.GetOption(options, "NoUserSite", defaultValue: false);
		_noSite = LanguageOptions.GetOption(options, "NoSite", defaultValue: false);
		_ignoreEnvironment = LanguageOptions.GetOption(options, "IgnoreEnvironment", defaultValue: false);
		_verbose = LanguageOptions.GetOption(options, "Verbose", defaultValue: false);
		_optimize = LanguageOptions.GetOption(options, "Optimize", defaultValue: false);
		_stripDocStrings = LanguageOptions.GetOption(options, "StripDocStrings", defaultValue: false);
		_division = LanguageOptions.GetOption(options, "DivisionOptions", PythonDivisionOptions.Old);
		_recursionLimit = LanguageOptions.GetOption(options, "RecursionLimit", int.MaxValue);
		_indentationInconsistencySeverity = LanguageOptions.GetOption(options, "IndentationInconsistencySeverity", Severity.Ignore);
		_enableProfiler = LanguageOptions.GetOption(options, "EnableProfiler", defaultValue: false);
		_lightweightScopes = LanguageOptions.GetOption(options, "LightweightScopes", defaultValue: false);
		_fullFrames = LanguageOptions.GetOption(options, "FullFrames", defaultValue: false);
		_frames = _fullFrames || LanguageOptions.GetOption(options, "Frames", defaultValue: false);
		_gcStress = LanguageOptions.GetOption<int?>(options, "GCStress", null);
		_tracing = LanguageOptions.GetOption(options, "Tracing", defaultValue: false);
		_noDebug = LanguageOptions.GetOption<Regex>(options, "NoDebug", null);
		if (options != null && options.TryGetValue("PythonVersion", out var value))
		{
			if (value is Version)
			{
				_version = (Version)value;
			}
			else
			{
				if (!(value is string))
				{
					throw new ValueErrorException("Expected string or Version for PythonVersion");
				}
				_version = new Version((string)value);
			}
			if (_version != new Version(2, 7) && _version != new Version(3, 0))
			{
				throw new ValueErrorException("Expected Version to be 2.7 or 3.0");
			}
		}
		else
		{
			_version = new Version(2, 7);
		}
		_python30 = _version == new Version(3, 0);
	}

	private static IDictionary<string, object> EnsureSearchPaths(IDictionary<string, object> options)
	{
		if (options == null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("SearchPaths", new string[1] { "." });
			return dictionary;
		}
		if (!options.ContainsKey("SearchPaths"))
		{
			options["SearchPaths"] = new string[1] { "." };
		}
		return options;
	}
}
