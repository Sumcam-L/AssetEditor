namespace IronPython.Compiler.Ast;

public class PythonWalker
{
	public virtual bool Walk(AndExpression node)
	{
		return true;
	}

	public virtual void PostWalk(AndExpression node)
	{
	}

	public virtual bool Walk(BackQuoteExpression node)
	{
		return true;
	}

	public virtual void PostWalk(BackQuoteExpression node)
	{
	}

	public virtual bool Walk(BinaryExpression node)
	{
		return true;
	}

	public virtual void PostWalk(BinaryExpression node)
	{
	}

	public virtual bool Walk(CallExpression node)
	{
		return true;
	}

	public virtual void PostWalk(CallExpression node)
	{
	}

	public virtual bool Walk(ConditionalExpression node)
	{
		return true;
	}

	public virtual void PostWalk(ConditionalExpression node)
	{
	}

	public virtual bool Walk(ConstantExpression node)
	{
		return true;
	}

	public virtual void PostWalk(ConstantExpression node)
	{
	}

	public virtual bool Walk(DictionaryComprehension node)
	{
		return true;
	}

	public virtual void PostWalk(DictionaryComprehension node)
	{
	}

	public virtual bool Walk(DictionaryExpression node)
	{
		return true;
	}

	public virtual void PostWalk(DictionaryExpression node)
	{
	}

	public virtual bool Walk(ErrorExpression node)
	{
		return true;
	}

	public virtual void PostWalk(ErrorExpression node)
	{
	}

	public virtual bool Walk(GeneratorExpression node)
	{
		return true;
	}

	public virtual void PostWalk(GeneratorExpression node)
	{
	}

	public virtual bool Walk(IndexExpression node)
	{
		return true;
	}

	public virtual void PostWalk(IndexExpression node)
	{
	}

	public virtual bool Walk(LambdaExpression node)
	{
		return true;
	}

	public virtual void PostWalk(LambdaExpression node)
	{
	}

	public virtual bool Walk(ListComprehension node)
	{
		return true;
	}

	public virtual void PostWalk(ListComprehension node)
	{
	}

	public virtual bool Walk(ListExpression node)
	{
		return true;
	}

	public virtual void PostWalk(ListExpression node)
	{
	}

	public virtual bool Walk(MemberExpression node)
	{
		return true;
	}

	public virtual void PostWalk(MemberExpression node)
	{
	}

	public virtual bool Walk(NameExpression node)
	{
		return true;
	}

	public virtual void PostWalk(NameExpression node)
	{
	}

	public virtual bool Walk(OrExpression node)
	{
		return true;
	}

	public virtual void PostWalk(OrExpression node)
	{
	}

	public virtual bool Walk(ParenthesisExpression node)
	{
		return true;
	}

	public virtual void PostWalk(ParenthesisExpression node)
	{
	}

	public virtual bool Walk(SetComprehension node)
	{
		return true;
	}

	public virtual void PostWalk(SetComprehension node)
	{
	}

	public virtual bool Walk(SetExpression node)
	{
		return true;
	}

	public virtual void PostWalk(SetExpression node)
	{
	}

	public virtual bool Walk(SliceExpression node)
	{
		return true;
	}

	public virtual void PostWalk(SliceExpression node)
	{
	}

	public virtual bool Walk(TupleExpression node)
	{
		return true;
	}

	public virtual void PostWalk(TupleExpression node)
	{
	}

	public virtual bool Walk(UnaryExpression node)
	{
		return true;
	}

	public virtual void PostWalk(UnaryExpression node)
	{
	}

	public virtual bool Walk(YieldExpression node)
	{
		return true;
	}

	public virtual void PostWalk(YieldExpression node)
	{
	}

	public virtual bool Walk(AssertStatement node)
	{
		return true;
	}

	public virtual void PostWalk(AssertStatement node)
	{
	}

	public virtual bool Walk(AssignmentStatement node)
	{
		return true;
	}

	public virtual void PostWalk(AssignmentStatement node)
	{
	}

	public virtual bool Walk(AugmentedAssignStatement node)
	{
		return true;
	}

	public virtual void PostWalk(AugmentedAssignStatement node)
	{
	}

	public virtual bool Walk(BreakStatement node)
	{
		return true;
	}

	public virtual void PostWalk(BreakStatement node)
	{
	}

	public virtual bool Walk(ClassDefinition node)
	{
		return true;
	}

	public virtual void PostWalk(ClassDefinition node)
	{
	}

	public virtual bool Walk(ContinueStatement node)
	{
		return true;
	}

	public virtual void PostWalk(ContinueStatement node)
	{
	}

	public virtual bool Walk(DelStatement node)
	{
		return true;
	}

	public virtual void PostWalk(DelStatement node)
	{
	}

	public virtual bool Walk(EmptyStatement node)
	{
		return true;
	}

	public virtual void PostWalk(EmptyStatement node)
	{
	}

	public virtual bool Walk(ExecStatement node)
	{
		return true;
	}

	public virtual void PostWalk(ExecStatement node)
	{
	}

	public virtual bool Walk(ExpressionStatement node)
	{
		return true;
	}

	public virtual void PostWalk(ExpressionStatement node)
	{
	}

	public virtual bool Walk(ForStatement node)
	{
		return true;
	}

	public virtual void PostWalk(ForStatement node)
	{
	}

	public virtual bool Walk(FromImportStatement node)
	{
		return true;
	}

	public virtual void PostWalk(FromImportStatement node)
	{
	}

	public virtual bool Walk(FunctionDefinition node)
	{
		return true;
	}

	public virtual void PostWalk(FunctionDefinition node)
	{
	}

	public virtual bool Walk(GlobalStatement node)
	{
		return true;
	}

	public virtual void PostWalk(GlobalStatement node)
	{
	}

	public virtual bool Walk(IfStatement node)
	{
		return true;
	}

	public virtual void PostWalk(IfStatement node)
	{
	}

	public virtual bool Walk(ImportStatement node)
	{
		return true;
	}

	public virtual void PostWalk(ImportStatement node)
	{
	}

	public virtual bool Walk(PrintStatement node)
	{
		return true;
	}

	public virtual void PostWalk(PrintStatement node)
	{
	}

	public virtual bool Walk(PythonAst node)
	{
		return true;
	}

	public virtual void PostWalk(PythonAst node)
	{
	}

	public virtual bool Walk(RaiseStatement node)
	{
		return true;
	}

	public virtual void PostWalk(RaiseStatement node)
	{
	}

	public virtual bool Walk(ReturnStatement node)
	{
		return true;
	}

	public virtual void PostWalk(ReturnStatement node)
	{
	}

	public virtual bool Walk(SuiteStatement node)
	{
		return true;
	}

	public virtual void PostWalk(SuiteStatement node)
	{
	}

	public virtual bool Walk(TryStatement node)
	{
		return true;
	}

	public virtual void PostWalk(TryStatement node)
	{
	}

	public virtual bool Walk(WhileStatement node)
	{
		return true;
	}

	public virtual void PostWalk(WhileStatement node)
	{
	}

	public virtual bool Walk(WithStatement node)
	{
		return true;
	}

	public virtual void PostWalk(WithStatement node)
	{
	}

	public virtual bool Walk(Arg node)
	{
		return true;
	}

	public virtual void PostWalk(Arg node)
	{
	}

	public virtual bool Walk(ComprehensionFor node)
	{
		return true;
	}

	public virtual void PostWalk(ComprehensionFor node)
	{
	}

	public virtual bool Walk(ComprehensionIf node)
	{
		return true;
	}

	public virtual void PostWalk(ComprehensionIf node)
	{
	}

	public virtual bool Walk(DottedName node)
	{
		return true;
	}

	public virtual void PostWalk(DottedName node)
	{
	}

	public virtual bool Walk(IfStatementTest node)
	{
		return true;
	}

	public virtual void PostWalk(IfStatementTest node)
	{
	}

	public virtual bool Walk(ModuleName node)
	{
		return true;
	}

	public virtual void PostWalk(ModuleName node)
	{
	}

	public virtual bool Walk(Parameter node)
	{
		return true;
	}

	public virtual void PostWalk(Parameter node)
	{
	}

	public virtual bool Walk(RelativeModuleName node)
	{
		return true;
	}

	public virtual void PostWalk(RelativeModuleName node)
	{
	}

	public virtual bool Walk(SublistParameter node)
	{
		return true;
	}

	public virtual void PostWalk(SublistParameter node)
	{
	}

	public virtual bool Walk(TryStatementHandler node)
	{
		return true;
	}

	public virtual void PostWalk(TryStatementHandler node)
	{
	}
}
