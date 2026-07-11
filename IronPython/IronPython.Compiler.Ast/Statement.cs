using System;

namespace IronPython.Compiler.Ast;

public abstract class Statement : Node
{
	public virtual string Documentation => null;

	public override Type Type => typeof(void);
}
