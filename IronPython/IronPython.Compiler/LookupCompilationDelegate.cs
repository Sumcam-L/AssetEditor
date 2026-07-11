using IronPython.Runtime;

namespace IronPython.Compiler;

public delegate object LookupCompilationDelegate(CodeContext context, FunctionCode code);
