namespace IronPython.Compiler.Ast;

public class PythonWalkerNonRecursive : PythonWalker
{
	public override bool Walk(AndExpression node)
	{
		return false;
	}

	public override void PostWalk(AndExpression node)
	{
	}

	public override bool Walk(BackQuoteExpression node)
	{
		return false;
	}

	public override void PostWalk(BackQuoteExpression node)
	{
	}

	public override bool Walk(BinaryExpression node)
	{
		return false;
	}

	public override void PostWalk(BinaryExpression node)
	{
	}

	public override bool Walk(CallExpression node)
	{
		return false;
	}

	public override void PostWalk(CallExpression node)
	{
	}

	public override bool Walk(ConditionalExpression node)
	{
		return false;
	}

	public override void PostWalk(ConditionalExpression node)
	{
	}

	public override bool Walk(ConstantExpression node)
	{
		return false;
	}

	public override void PostWalk(ConstantExpression node)
	{
	}

	public override bool Walk(DictionaryComprehension node)
	{
		return false;
	}

	public override void PostWalk(DictionaryComprehension node)
	{
	}

	public override bool Walk(DictionaryExpression node)
	{
		return false;
	}

	public override void PostWalk(DictionaryExpression node)
	{
	}

	public override bool Walk(ErrorExpression node)
	{
		return false;
	}

	public override void PostWalk(ErrorExpression node)
	{
	}

	public override bool Walk(GeneratorExpression node)
	{
		return false;
	}

	public override void PostWalk(GeneratorExpression node)
	{
	}

	public override bool Walk(IndexExpression node)
	{
		return false;
	}

	public override void PostWalk(IndexExpression node)
	{
	}

	public override bool Walk(LambdaExpression node)
	{
		return false;
	}

	public override void PostWalk(LambdaExpression node)
	{
	}

	public override bool Walk(ListComprehension node)
	{
		return false;
	}

	public override void PostWalk(ListComprehension node)
	{
	}

	public override bool Walk(ListExpression node)
	{
		return false;
	}

	public override void PostWalk(ListExpression node)
	{
	}

	public override bool Walk(MemberExpression node)
	{
		return false;
	}

	public override void PostWalk(MemberExpression node)
	{
	}

	public override bool Walk(NameExpression node)
	{
		return false;
	}

	public override void PostWalk(NameExpression node)
	{
	}

	public override bool Walk(OrExpression node)
	{
		return false;
	}

	public override void PostWalk(OrExpression node)
	{
	}

	public override bool Walk(ParenthesisExpression node)
	{
		return false;
	}

	public override void PostWalk(ParenthesisExpression node)
	{
	}

	public override bool Walk(SetComprehension node)
	{
		return false;
	}

	public override void PostWalk(SetComprehension node)
	{
	}

	public override bool Walk(SetExpression node)
	{
		return false;
	}

	public override void PostWalk(SetExpression node)
	{
	}

	public override bool Walk(SliceExpression node)
	{
		return false;
	}

	public override void PostWalk(SliceExpression node)
	{
	}

	public override bool Walk(TupleExpression node)
	{
		return false;
	}

	public override void PostWalk(TupleExpression node)
	{
	}

	public override bool Walk(UnaryExpression node)
	{
		return false;
	}

	public override void PostWalk(UnaryExpression node)
	{
	}

	public override bool Walk(YieldExpression node)
	{
		return false;
	}

	public override void PostWalk(YieldExpression node)
	{
	}

	public override bool Walk(AssertStatement node)
	{
		return false;
	}

	public override void PostWalk(AssertStatement node)
	{
	}

	public override bool Walk(AssignmentStatement node)
	{
		return false;
	}

	public override void PostWalk(AssignmentStatement node)
	{
	}

	public override bool Walk(AugmentedAssignStatement node)
	{
		return false;
	}

	public override void PostWalk(AugmentedAssignStatement node)
	{
	}

	public override bool Walk(BreakStatement node)
	{
		return false;
	}

	public override void PostWalk(BreakStatement node)
	{
	}

	public override bool Walk(ClassDefinition node)
	{
		return false;
	}

	public override void PostWalk(ClassDefinition node)
	{
	}

	public override bool Walk(ContinueStatement node)
	{
		return false;
	}

	public override void PostWalk(ContinueStatement node)
	{
	}

	public override bool Walk(DelStatement node)
	{
		return false;
	}

	public override void PostWalk(DelStatement node)
	{
	}

	public override bool Walk(EmptyStatement node)
	{
		return false;
	}

	public override void PostWalk(EmptyStatement node)
	{
	}

	public override bool Walk(ExecStatement node)
	{
		return false;
	}

	public override void PostWalk(ExecStatement node)
	{
	}

	public override bool Walk(ExpressionStatement node)
	{
		return false;
	}

	public override void PostWalk(ExpressionStatement node)
	{
	}

	public override bool Walk(ForStatement node)
	{
		return false;
	}

	public override void PostWalk(ForStatement node)
	{
	}

	public override bool Walk(FromImportStatement node)
	{
		return false;
	}

	public override void PostWalk(FromImportStatement node)
	{
	}

	public override bool Walk(FunctionDefinition node)
	{
		return false;
	}

	public override void PostWalk(FunctionDefinition node)
	{
	}

	public override bool Walk(GlobalStatement node)
	{
		return false;
	}

	public override void PostWalk(GlobalStatement node)
	{
	}

	public override bool Walk(IfStatement node)
	{
		return false;
	}

	public override void PostWalk(IfStatement node)
	{
	}

	public override bool Walk(ImportStatement node)
	{
		return false;
	}

	public override void PostWalk(ImportStatement node)
	{
	}

	public override bool Walk(PrintStatement node)
	{
		return false;
	}

	public override void PostWalk(PrintStatement node)
	{
	}

	public override bool Walk(PythonAst node)
	{
		return false;
	}

	public override void PostWalk(PythonAst node)
	{
	}

	public override bool Walk(RaiseStatement node)
	{
		return false;
	}

	public override void PostWalk(RaiseStatement node)
	{
	}

	public override bool Walk(ReturnStatement node)
	{
		return false;
	}

	public override void PostWalk(ReturnStatement node)
	{
	}

	public override bool Walk(SuiteStatement node)
	{
		return false;
	}

	public override void PostWalk(SuiteStatement node)
	{
	}

	public override bool Walk(TryStatement node)
	{
		return false;
	}

	public override void PostWalk(TryStatement node)
	{
	}

	public override bool Walk(WhileStatement node)
	{
		return false;
	}

	public override void PostWalk(WhileStatement node)
	{
	}

	public override bool Walk(WithStatement node)
	{
		return false;
	}

	public override void PostWalk(WithStatement node)
	{
	}

	public override bool Walk(Arg node)
	{
		return false;
	}

	public override void PostWalk(Arg node)
	{
	}

	public override bool Walk(ComprehensionFor node)
	{
		return false;
	}

	public override void PostWalk(ComprehensionFor node)
	{
	}

	public override bool Walk(ComprehensionIf node)
	{
		return false;
	}

	public override void PostWalk(ComprehensionIf node)
	{
	}

	public override bool Walk(DottedName node)
	{
		return false;
	}

	public override void PostWalk(DottedName node)
	{
	}

	public override bool Walk(IfStatementTest node)
	{
		return false;
	}

	public override void PostWalk(IfStatementTest node)
	{
	}

	public override bool Walk(ModuleName node)
	{
		return false;
	}

	public override void PostWalk(ModuleName node)
	{
	}

	public override bool Walk(Parameter node)
	{
		return false;
	}

	public override void PostWalk(Parameter node)
	{
	}

	public override bool Walk(RelativeModuleName node)
	{
		return false;
	}

	public override void PostWalk(RelativeModuleName node)
	{
	}

	public override bool Walk(SublistParameter node)
	{
		return false;
	}

	public override void PostWalk(SublistParameter node)
	{
	}

	public override bool Walk(TryStatementHandler node)
	{
		return false;
	}

	public override void PostWalk(TryStatementHandler node)
	{
	}
}
