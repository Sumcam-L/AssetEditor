using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(IScriptingService))]
[Export(typeof(ScriptingService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public abstract class ScriptingService : IScriptingService
{
	private class ScriptOutStream : Stream
	{
		private string m_output = string.Empty;

		public string Text => m_output;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		public void Reset()
		{
			m_output = string.Empty;
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0L;
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			m_output += Encoding.UTF8.GetString(buffer, offset, count);
		}
	}

	private ScriptEngine m_engine;

	private ScriptScope m_scope;

	private ScriptOutStream m_stream;

	public string DisplayName => m_engine.Setup.DisplayName;

	public virtual void LoadAssembly(Assembly assembly)
	{
		m_engine.Runtime.LoadAssembly(assembly);
	}

	public abstract void ImportAllTypes(string nmspace);

	public abstract void ImportType(string nmspace, string typename);

	public bool TryGetVariable<T>(string name, out T var)
	{
		return m_scope.TryGetVariable(name, out var);
	}

	public void SetVariable(string name, object var)
	{
		m_scope.SetVariable(name, var);
	}

	public void RemoveVariable(string name)
	{
		m_scope.RemoveVariable(name);
	}

	public string ExecuteStatement(string statement)
	{
		return ExecuteStatement(statement, multiStatements: false);
	}

	public string ExecuteStatements(string statements)
	{
		return ExecuteStatement(statements, multiStatements: true);
	}

	public dynamic ExecuteSilent(string statement)
	{
		return m_engine.Execute(statement, m_scope);
	}

	private string ExecuteStatement(string statement, bool multiStatements)
	{
		string empty = string.Empty;
		if (string.IsNullOrEmpty(statement))
		{
			return empty;
		}
		try
		{
			SourceCodeKind kind = (multiStatements ? SourceCodeKind.Statements : SourceCodeKind.SingleStatement);
			ScriptSource scriptSource = m_engine.CreateScriptSourceFromString(statement, kind);
			scriptSource.Execute(m_scope);
			empty = m_stream.Text;
		}
		catch (Exception exception)
		{
			ExceptionOperations service = m_engine.GetService<ExceptionOperations>(new object[0]);
			empty = m_stream.Text + service.FormatException(exception);
		}
		m_stream.Reset();
		return empty;
	}

	public string ExecuteFile(string fileName)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Executing script: '{0}'", fileName);
		string empty = string.Empty;
		try
		{
			FileInfo fileInfo = new FileInfo(fileName);
			if (!fileInfo.Exists)
			{
				throw new FileNotFoundException(fileInfo.FullName);
			}
			ScriptSource scriptSource = m_engine.CreateScriptSourceFromFile(fileInfo.FullName);
			scriptSource.Execute(m_scope);
			empty = m_stream.Text;
		}
		catch (SyntaxErrorException ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Script failed to execute. Syntax error at {0},{1}", ex.Line, ex.Column);
			empty = m_stream.Text + $": Syntax error at {ex.Line},{ex.Column}";
		}
		catch (Exception ex2)
		{
			ExceptionOperations service = m_engine.GetService<ExceptionOperations>(new object[0]);
			empty = m_stream.Text + service.FormatException(ex2);
			Outputs.WriteLine(OutputMessageType.Error, "Script failed to execute.  Reason:  " + ex2.Message);
		}
		m_stream.Reset();
		Outputs.WriteLine(OutputMessageType.Info, "Finished executing script: '{0}'", fileName);
		return empty;
	}

	protected void SetEngine(ScriptEngine engine)
	{
		if (m_engine != null)
		{
			throw new InvalidOperationException("engine is already set");
		}
		if (engine == null)
		{
			throw new ArgumentNullException("engine");
		}
		m_engine = engine;
		m_stream = new ScriptOutStream();
		m_engine.Runtime.IO.SetOutput(m_stream, Encoding.ASCII);
		m_scope = m_engine.CreateScope();
		LoadDefaultAssemblies(m_engine.Runtime);
	}

	protected virtual void LoadDefaultAssemblies(ScriptRuntime runtime)
	{
		runtime.LoadAssembly(typeof(string).Assembly);
		runtime.LoadAssembly(typeof(Point).Assembly);
		runtime.LoadAssembly(typeof(Uri).Assembly);
		runtime.LoadAssembly(typeof(XmlReader).Assembly);
		runtime.LoadAssembly(typeof(DomNode).Assembly);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			string name = assembly.GetName().Name;
			if (name.Equals("System.Windows.Forms") || name.Equals("Atf.Gui.WinForms") || name.Equals("Atf.Gui"))
			{
				runtime.LoadAssembly(assembly);
			}
		}
	}
}
