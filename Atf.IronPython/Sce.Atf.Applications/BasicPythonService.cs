using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Text;
using IronPython;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IScriptingService))]
[Export(typeof(ScriptingService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BasicPythonService : ScriptingService, IInitializable
{
	private static PythonDateTime forceIronPythonDotModulesToBeReferenced = new PythonDateTime();

	[ImportingConstructor]
	public BasicPythonService()
	{
		ScriptEngine engine = CreateEngine();
		SetEngine(engine);
		Initialize();
	}

	protected virtual ScriptEngine CreateEngine()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["DivisionOptions"] = PythonDivisionOptions.New;
		dictionary["PrivateBinding"] = true;
		ScriptEngine scriptEngine = Python.CreateEngine(dictionary);
		ScriptScope sysModule = scriptEngine.GetSysModule();
		sysModule.SetVariable("path_hooks", new List());
		return scriptEngine;
	}

	protected virtual void Initialize()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("import clr");
		stringBuilder.AppendLine("import System");
		stringBuilder.AppendLine("from System import *");
		stringBuilder.AppendLine("from System.Drawing import *");
		stringBuilder.AppendLine("from System.Collections.Generic import *");
		stringBuilder.AppendLine("from System.Collections.ObjectModel import *");
		stringBuilder.AppendLine("from System.Windows.Forms import *");
		stringBuilder.AppendLine("from System.Text import *");
		stringBuilder.AppendLine("from System.IO import *");
		stringBuilder.AppendLine("from System.Xml.Schema import *");
		stringBuilder.AppendLine("from System.Xml.XPath import *");
		stringBuilder.AppendLine("from System.Xml.Serialization import *");
		stringBuilder.AppendLine("from Sce.Atf import *");
		stringBuilder.AppendLine("from Sce.Atf.Applications import *");
		stringBuilder.AppendLine("from Sce.Atf.VectorMath import *");
		stringBuilder.AppendLine("from Sce.Atf.Adaptation import *");
		stringBuilder.AppendLine("from Sce.Atf.Dom import *");
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (assembly.FullName.StartsWith("Atf.") || assembly.FullName.StartsWith("Scea."))
			{
				LoadAssembly(assembly);
			}
			if (assembly.FullName.StartsWith("Atf.Gui.WinForms"))
			{
				stringBuilder.AppendLine("from Sce.Atf.Controls import *");
				stringBuilder.AppendLine("from Sce.Atf.Controls.Adaptable import *");
			}
			else if (assembly.FullName.StartsWith("Scea.Core"))
			{
				stringBuilder.AppendLine("from Scea.Editors.Host.Internal import *");
			}
			else if (assembly.FullName.StartsWith("Scea.Dom"))
			{
				stringBuilder.AppendLine("from Scea.Dom import *");
			}
		}
		ExecuteStatements(stringBuilder.ToString());
	}

	public override void ImportAllTypes(string nmspace)
	{
		ExecuteStatement($"from {nmspace} import *");
	}

	public override void ImportType(string nmspace, string typename)
	{
		ExecuteStatement($"from {nmspace} import {typename}");
	}

	void IInitializable.Initialize()
	{
	}
}
