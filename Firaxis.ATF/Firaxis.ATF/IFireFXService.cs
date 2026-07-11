using System.Collections.Generic;
using Firaxis.CivTech.FireFX;
using Firaxis.Error;

namespace Firaxis.ATF;

public interface IFireFXService
{
	ResultCode CompileResource(IFireFXScriptResource scriptResource);

	ResultCode Compile(string scriptPath, out IFireFXEffect effect, IList<CompileIssue> issues);

	ResultCode Compile(string scriptPath, string byteCodePath, IList<CompileIssue> issues);

	ResultCode CompileText(string scriptName, string scriptText, out IFireFXEffect effect, IList<CompileIssue> issues);

	ResultCode CompileText(string scriptName, string scriptText, string byteCodePath, IList<CompileIssue> issues);
}
