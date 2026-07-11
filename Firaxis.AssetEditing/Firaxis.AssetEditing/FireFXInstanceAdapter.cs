using System;
using System.Collections.Generic;
using System.IO;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.FireFX;
using Firaxis.Error;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class FireFXInstanceAdapter : InstanceEntityAdapter
{
	public IFireFXInstance FireFX => InstanceEntity as IFireFXInstance;

	public IFireFXInstanceData FireFXInstanceData => FireFX.InstanceData;

	public bool HasScript => ClassName == "FireFXScript";

	public IFireFXScriptResource ScriptResource { get; private set; }

	protected override AttributeInfo ClassNameAttribute => EntitySchema.FireFXInstanceType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.FireFXInstanceType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.FireFXInstanceType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.FireFXInstanceType.DescriptionAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.FireFXInstanceType.NameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.FireFXInstanceType.TagsChild;

	public string ScriptText
	{
		get
		{
			return GetAttribute<string>(EntitySchema.FireFXInstanceType.ScriptTextAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.FireFXInstanceType.ScriptTextAttribute, value);
		}
	}

	protected override void AssignBasicPropertiesFromEntity()
	{
		base.AssignBasicPropertiesFromEntity();
		ScriptText = FireFX.InstanceData.As<IFireFXScriptData>()?.Script ?? string.Empty;
	}

	protected override void AssignCookParametersFromEntity(bool updateUI)
	{
		base.AssignCookParametersFromEntity(updateUI);
		global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, InstanceType, ClassName);
		UpdateInstanceData();
	}

	protected override void OnClassChange()
	{
	}

	protected override void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		base.HandleDomNodeAttributeChanged(sender, e);
		if (e.AttributeInfo == EntitySchema.FireFXInstanceType.ScriptTextAttribute && ScriptResource != null)
		{
			string text = (string)e.NewValue;
			if (ScriptResource.Text != text)
			{
				ScriptResource.Text = text;
			}
		}
	}

	public override void PostClassNameChange()
	{
		base.PostClassNameChange();
		FireFX.HandleClassChange();
		IClassEntity classEntity = global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, InstanceType, ClassName);
		FireFX.PopulateDataFiles(classEntity);
		UpdateInstanceData();
	}

	private void UpdateEmitterCookParameters(IClassEntity entityClass)
	{
		ScriptResource.As<IFireFXAdapterCompileHandler>()?.UpdateCookParameters(entityClass);
	}

	private void UpdateInstanceData()
	{
		if (ScriptResource != null)
		{
			ScriptResource.TextChanged -= ScriptResource_TextChanged;
			ScriptResource.EffectChanged -= ScriptResource_EffectChanged;
		}
		ScriptResource = null;
		if (ClassName == "FireFXScript")
		{
			ScriptResource = new FireFXScriptData(this);
			ScriptResource.EffectChanged += ScriptResource_EffectChanged;
			ScriptResource.TextChanged += ScriptResource_TextChanged;
			DoCompile(ScriptResource);
		}
	}

	private void ScriptResource_TextChanged(object sender, EventArgs e)
	{
		if (!(ScriptText == ScriptResource?.Text))
		{
			BaseEntityPropertyContext baseEntityPropertyContext = base.DomNode.As<BaseEntityPropertyContext>();
			baseEntityPropertyContext.Cancelled -= EntityContext_Cancelled;
			baseEntityPropertyContext.Cancelled += EntityContext_Cancelled;
			baseEntityPropertyContext.DoTransaction(delegate
			{
				ScriptText = ScriptResource?.Text ?? string.Empty;
			}, "Edit Script".Localize());
			baseEntityPropertyContext.Cancelled -= EntityContext_Cancelled;
		}
	}

	private void EntityContext_Cancelled(object sender, EventArgs e)
	{
		if (ScriptResource != null)
		{
			ScriptResource.Text = ScriptText;
		}
	}

	private void ScriptResource_EffectChanged(object sender, EventArgs e)
	{
		IClassEntity entityClass = global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, InstanceType, ClassName);
		UpdateEmitterCookParameters(entityClass);
		base.DomNode.As<BaseEntityPropertyContext>().OnReloaded();
	}

	private void DoCompile(IFireFXScriptResource docToCompile)
	{
		IFireFXService fireFXService = base.DomNode.As<FireFXDocument>().FireFXService;
		Uri uri = docToCompile.Uri;
		string fileName = Path.GetFileName(((uri != null) ? uri.LocalPath : null) ?? string.Empty);
		CategorizedOutputs.WriteLine("FireFX", OutputMessageType.Info, "Compiling FireFX script: {0}", fileName);
		ResultCode resultCode = fireFXService.CompileResource(docToCompile);
		if ((bool)resultCode)
		{
			CategorizedOutputs.WriteLine("FireFX", OutputMessageType.Info, "Compile succeeded with no errors");
		}
		else
		{
			ReportCompileIssues(resultCode, docToCompile.Issues);
		}
	}

	private void ReportCompileIssues(ResultCode compileResult, IList<CompileIssue> compileIssues)
	{
		CategorizedOutputs.WriteLine("FireFX", OutputMessageType.Error, OutputMessageVerbosity.Verbose, compileResult.Message);
		int num = 0;
		int num2 = 0;
		string text = "succeeded";
		OutputMessageType type = OutputMessageType.Warning;
		foreach (CompileIssue compileIssue in compileIssues)
		{
			OutputMessageType type2 = OutputMessageType.Info;
			if (compileIssue.Type == CompileIssueType.Warning)
			{
				type2 = OutputMessageType.Warning;
				num++;
			}
			else if (compileIssue.Type == CompileIssueType.Error)
			{
				text = "failed";
				type = OutputMessageType.Error;
				type2 = OutputMessageType.Error;
				num2++;
			}
			CategorizedOutputs.WriteLine("FireFX", type2, "{0}({1}): {2}", compileIssue.File, compileIssue.LineNo, compileIssue.Message);
		}
		CategorizedOutputs.WriteLine("FireFX", type, "Compile {0} with {1} warnings and {2} errors. Overall={3}", text, num, num2, compileResult.Message);
	}
}
