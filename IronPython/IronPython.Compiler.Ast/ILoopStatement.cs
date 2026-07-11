using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

internal interface ILoopStatement
{
	LabelTarget BreakLabel { get; set; }

	LabelTarget ContinueLabel { get; set; }
}
