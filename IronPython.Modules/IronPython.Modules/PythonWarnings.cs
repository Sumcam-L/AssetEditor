using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonWarnings
{
	public const string __doc__ = "Provides low-level functionality for reporting warnings";

	private static readonly object _keyFields = new object();

	private static readonly string _keyDefaultAction = "default_action";

	private static readonly string _keyFilters = "filters";

	private static readonly string _keyOnceRegistry = "once_registry";

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		List defaultFilters = new List();
		if (context.PythonOptions.WarnPython30)
		{
			defaultFilters.AddNoLock(PythonTuple.MakeTuple("ignore", null, PythonExceptions.DeprecationWarning, null, 0));
		}
		defaultFilters.AddNoLock(PythonTuple.MakeTuple("ignore", null, PythonExceptions.PendingDeprecationWarning, null, 0));
		defaultFilters.AddNoLock(PythonTuple.MakeTuple("ignore", null, PythonExceptions.ImportWarning, null, 0));
		defaultFilters.AddNoLock(PythonTuple.MakeTuple("ignore", null, PythonExceptions.BytesWarning, null, 0));
		context.GetOrCreateModuleState(_keyFields, delegate
		{
			dict.Add(_keyDefaultAction, "default");
			dict.Add(_keyOnceRegistry, new PythonDictionary());
			dict.Add(_keyFilters, defaultFilters);
			return dict;
		});
	}

	public static void warn(CodeContext context, object message, [DefaultParameterValue(null)] PythonType category, [DefaultParameterValue(1)] int stacklevel)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		List list = context2.GetSystemStateValue("argv") as List;
		if (PythonOps.IsInstance(message, PythonExceptions.Warning))
		{
			category = DynamicHelpers.GetPythonType(message);
		}
		if (category == null)
		{
			category = PythonExceptions.UserWarning;
		}
		if (!category.IsSubclassOf(PythonExceptions.Warning))
		{
			throw PythonOps.ValueError("category is not a subclass of Warning");
		}
		TraceBackFrame traceBackFrame = null;
		if (PythonContext.GetContext(context).PythonOptions.Frames)
		{
			try
			{
				traceBackFrame = SysModule._getframeImpl(context, stacklevel);
			}
			catch (ValueErrorException)
			{
			}
		}
		PythonDictionary pythonDictionary;
		int lineno;
		if (traceBackFrame == null)
		{
			pythonDictionary = Builtin.globals(context);
			lineno = 1;
		}
		else
		{
			pythonDictionary = traceBackFrame.f_globals;
			lineno = (int)traceBackFrame.f_lineno;
		}
		string text = ((pythonDictionary == null || !pythonDictionary.ContainsKey("__name__")) ? "<string>" : ((string)pythonDictionary.get("__name__")));
		string text2 = pythonDictionary.get("__file__") as string;
		if (text2 == null || text2 == "")
		{
			if (text == "__main__")
			{
				text2 = ((list == null || list.Count <= 0) ? "__main__" : (list[0] as string));
			}
			if (text2 == null || text2 == "")
			{
				text2 = text;
			}
		}
		PythonDictionary registry = (PythonDictionary)pythonDictionary.setdefault("__warningregistry__", new PythonDictionary());
		warn_explicit(context, message, category, text2, lineno, text, registry, pythonDictionary);
	}

	public static void warn_explicit(CodeContext context, object message, PythonType category, string filename, int lineno, [DefaultParameterValue(null)] string module, [DefaultParameterValue(null)] PythonDictionary registry, [DefaultParameterValue(null)] object module_globals)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonDictionary pythonDictionary = (PythonDictionary)context2.GetModuleState(_keyFields);
		if (string.IsNullOrEmpty(module))
		{
			module = ((filename == null || filename == "") ? "<unknown>" : filename);
			if (module.EndsWith(".py"))
			{
				module = module.Substring(0, module.Length - 3);
			}
		}
		if (registry == null)
		{
			registry = new PythonDictionary();
		}
		PythonExceptions.BaseException ex;
		string text;
		if (PythonOps.IsInstance(message, PythonExceptions.Warning))
		{
			ex = (PythonExceptions.BaseException)message;
			text = ex.ToString();
			category = DynamicHelpers.GetPythonType(ex);
		}
		else
		{
			text = message.ToString();
			ex = PythonExceptions.CreatePythonThrowable(category, message.ToString());
		}
		PythonTuple key = PythonTuple.MakeTuple(text, category, lineno);
		if (registry.ContainsKey(key))
		{
			return;
		}
		string text2 = Converter.ConvertToString(pythonDictionary[_keyDefaultAction]);
		PythonTuple pythonTuple = null;
		bool flag = false;
		foreach (PythonTuple item in (List)pythonDictionary[_keyFilters])
		{
			pythonTuple = item;
			text2 = (string)item._data[0];
			PythonRegex.RE_Pattern rE_Pattern = (PythonRegex.RE_Pattern)item._data[1];
			PythonType other = (PythonType)item._data[2];
			PythonRegex.RE_Pattern rE_Pattern2 = (PythonRegex.RE_Pattern)item._data[3];
			int num = ((!(item._data[4] is int)) ? ((int)(Extensible<int>)item._data[4]) : ((int)item._data[4]));
			if ((rE_Pattern == null || rE_Pattern.match(text) != null) && category.IsSubclassOf(other) && (rE_Pattern2 == null || rE_Pattern2.match(module) != null) && (num == 0 || num == lineno))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			text2 = Converter.ConvertToString(pythonDictionary[_keyDefaultAction]);
		}
		switch (text2)
		{
		case "ignore":
			registry.Add(key, 1);
			return;
		case "error":
			throw ex.GetClrException();
		case "once":
		{
			registry.Add(key, 1);
			PythonTuple key3 = PythonTuple.MakeTuple(text, category);
			PythonDictionary pythonDictionary2 = (PythonDictionary)pythonDictionary[_keyOnceRegistry];
			if (pythonDictionary2.ContainsKey(key3))
			{
				return;
			}
			pythonDictionary2.Add(key, 1);
			break;
		}
		case "module":
		{
			registry.Add(key, 1);
			PythonTuple key2 = PythonTuple.MakeTuple(text, category, 0);
			if (registry.ContainsKey(key2))
			{
				return;
			}
			registry.Add(key2, 1);
			break;
		}
		case "default":
			registry.Add(key, 1);
			break;
		default:
			throw PythonOps.RuntimeError("Unrecognized action ({0}) in warnings.filters:\n {1}", text2, pythonTuple);
		case "always":
			break;
		}
		object warningsModule = context2.GetWarningsModule();
		if (warningsModule != null)
		{
			PythonCalls.Call(context, PythonOps.GetBoundAttr(context, warningsModule, "showwarning"), ex, category, filename, lineno, null, null);
		}
		else
		{
			showwarning(context, ex, category, filename, lineno, null, null);
		}
	}

	internal static string formatwarning(object message, PythonType category, string filename, int lineno, [DefaultParameterValue(null)] string line)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0}:{1}: {2}: {3}\n", filename, lineno, category.Name, message);
		if (line == null && lineno > 0 && File.Exists(filename))
		{
			StreamReader streamReader = new StreamReader(filename);
			for (int i = 0; i < lineno - 1; i++)
			{
				streamReader.ReadLine();
			}
			line = streamReader.ReadLine();
		}
		if (line != null)
		{
			stringBuilder.AppendFormat("  {0}\n", line.strip());
		}
		return stringBuilder.ToString();
	}

	internal static void showwarning(CodeContext context, object message, PythonType category, string filename, int lineno, [DefaultParameterValue(null)] object file, [DefaultParameterValue(null)] string line)
	{
		string text = formatwarning(message, category, filename, lineno, line);
		try
		{
			if (file == null)
			{
				PythonContext context2 = PythonContext.GetContext(context);
				if (context2.GetSystemStateValue("stderr") is PythonFile pythonFile)
				{
					pythonFile.write(text);
				}
				else
				{
					Console.Error.Write(text);
				}
			}
			else if (file is PythonFile)
			{
				((PythonFile)file).write(text);
			}
			else if (file is TextWriter)
			{
				((TextWriter)file).Write(text);
			}
		}
		catch (IOException)
		{
		}
	}
}
