using System.CodeDom;
using System.Collections.Generic;
using Microsoft.Scripting.Runtime;

namespace IronPython.Hosting;

internal class PythonCodeDomCodeGen : CodeDomCodeGen
{
	private Stack<int> _indents = new Stack<int>(new int[1]);

	private int _generatedIndent;

	protected override void WriteExpressionStatement(CodeExpressionStatement s)
	{
		base.Writer.Write(new string(' ', _generatedIndent + _indents.Peek()));
		WriteExpression(s.Expression);
		base.Writer.Write("\n");
	}

	protected override void WriteSnippetExpression(CodeSnippetExpression e)
	{
		base.Writer.Write(IndentSnippet(e.Value));
	}

	protected override void WriteSnippetStatement(CodeSnippetStatement s)
	{
		string value = s.Value;
		base.Writer.Write(IndentSnippetStatement(value));
		base.Writer.Write('\n');
		string text = value.Substring(value.LastIndexOf('\n') + 1);
		if (!string.IsNullOrEmpty(text.Trim('\t', ' ')))
		{
			return;
		}
		text = text.Replace("\t", "        ");
		int length = text.Length;
		if (length > _indents.Peek())
		{
			_indents.Push(length);
			return;
		}
		while (length < _indents.Peek())
		{
			_indents.Pop();
		}
	}

	protected override void WriteFunctionDefinition(CodeMemberMethod func)
	{
		base.Writer.Write("def ");
		base.Writer.Write(func.Name);
		base.Writer.Write("(");
		for (int i = 0; i < func.Parameters.Count; i++)
		{
			if (i != 0)
			{
				base.Writer.Write(",");
			}
			base.Writer.Write(func.Parameters[i].Name);
		}
		base.Writer.Write("):\n");
		int num = _indents.Peek();
		_generatedIndent += 4;
		foreach (CodeStatement statement in func.Statements)
		{
			WriteStatement(statement);
		}
		_generatedIndent -= 4;
		while (_indents.Peek() > num)
		{
			_indents.Pop();
		}
	}

	protected override string QuoteString(string val)
	{
		return "'''" + val.Replace("\\", "\\\\").Replace("'''", "\\'''") + "'''";
	}

	private string IndentSnippet(string block)
	{
		return block.Replace("\n", "\n" + new string(' ', _generatedIndent));
	}

	private string IndentSnippetStatement(string block)
	{
		return new string(' ', _generatedIndent) + IndentSnippet(block);
	}
}
