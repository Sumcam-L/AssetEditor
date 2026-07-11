using System.Threading;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

public static class DefaultContext
{
	internal static CodeContext _default;

	internal static CodeContext _defaultCLS;

	public static ContextId Id => Default.LanguageContext.ContextId;

	public static CodeContext Default => _default;

	public static PythonContext DefaultPythonContext => _default.LanguageContext;

	public static CodeContext DefaultCLS => _defaultCLS;

	internal static CodeContext CreateDefaultCLSContext(PythonContext context)
	{
		ModuleContext moduleContext = new ModuleContext(new PythonDictionary(), context);
		moduleContext.ShowCls = true;
		return moduleContext.GlobalContext;
	}

	internal static void InitializeDefaults(CodeContext defaultContext, CodeContext defaultClsCodeContext)
	{
		Interlocked.CompareExchange(ref _default, defaultContext, null);
		Interlocked.CompareExchange(ref _defaultCLS, defaultClsCodeContext, null);
	}
}
