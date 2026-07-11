using System;
using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.CivTech.FireFX;

public interface IFireFXCompilerService : IAssemblyInstance, IDisposable
{
	IEnumerable<string> IntrinsicFunctionNames { get; }

	ResultCode Startup(IVirtualPantry pantry);

	void Shutdown();

	ResultCode Compile(string scriptPath, out IFireFXEffect effect, IList<CompileIssue> issues);

	ResultCode Compile(string scriptPath, string byteCodePath, IList<CompileIssue> issues);

	ResultCode CompileText(string scriptName, string scriptText, out IFireFXEffect effect, IList<CompileIssue> issues);

	ResultCode CompileText(string scriptName, string scriptText, string byteCodePath, IList<CompileIssue> issues);
}
