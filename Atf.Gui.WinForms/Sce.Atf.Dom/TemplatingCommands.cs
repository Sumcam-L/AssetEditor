using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Dom;

[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(IContextMenuCommandProvider))]
[InheritedExport(typeof(TemplatingCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public abstract class TemplatingCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	protected struct ImportedContent
	{
		public readonly DomNode RootNode;

		public readonly Uri Uri;

		public ImportedContent(DomNode rootNode, Uri uri)
		{
			RootNode = rootNode;
			Uri = uri;
		}
	}

	protected enum CommandTag
	{
		AddTemplateFolder,
		AddExternalTemplateFolder,
		PromoteToTemplateLibrary,
		DemoteToCopyInstance,
		ReloadExternalTemplates
	}

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService = null;

	private TemplateLister m_templateLister;

	private ICommandService m_commandService;

	private IContextRegistry m_contextRegistry;

	private WeakReference m_targetRef;

	public virtual TemplatingContext TemplatingContext => ContextRegistry.GetMostRecentContext<TemplatingContext>();

	protected abstract DomNodeType TemplateFolderType { get; }

	protected IContextRegistry ContextRegistry => m_contextRegistry;

	[ImportingConstructor]
	protected TemplatingCommands(ICommandService commandService, IContextRegistry contextRegistry, TemplateLister templateLister)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
		m_templateLister = templateLister;
	}

	public abstract bool CanPromoteToTemplateLibrary(IEnumerable<object> items);

	public abstract void PromoteToTemplateLibrary(IEnumerable<object> items);

	public abstract bool CanDemoteToCopyInstance(IEnumerable<object> items);

	public abstract DomNode[] DemoteToCopyInstance(IEnumerable<object> items);

	protected virtual ImportedContent LoadExternalTemplateLibrary(Uri uri)
	{
		return new ImportedContent(null, uri);
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(new CommandInfo(CommandTag.AddTemplateFolder, null, null, "Add Template Folder".Localize(), "Creates a new template folder".Localize()), this);
		m_commandService.RegisterCommand(new CommandInfo(CommandTag.AddExternalTemplateFolder, null, null, "Import Templates from Document...".Localize(), "Import Templates from Document".Localize()), this);
		m_commandService.RegisterCommand(CommandTag.PromoteToTemplateLibrary, StandardMenu.Edit, StandardCommandGroup.EditOther, "Promote To Template Library".Localize(), "Promote To Template Library".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(CommandTag.DemoteToCopyInstance, StandardMenu.Edit, StandardCommandGroup.EditOther, "Demote To Copy Instance".Localize(), "Demote To Copy Instance".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(CommandTag.ReloadExternalTemplates, null, null, "Rescan Template Documents".Localize(), "Rescan Template Documents".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		if (m_scriptingService != null)
		{
			m_scriptingService.SetVariable("templateCmds", this);
		}
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		if (commandTag is CommandTag)
		{
			if (CommandTag.AddTemplateFolder.Equals(commandTag))
			{
				return true;
			}
			if (CommandTag.AddExternalTemplateFolder.Equals(commandTag))
			{
				return true;
			}
			if (CommandTag.ReloadExternalTemplates.Equals(commandTag))
			{
				return true;
			}
			if (CommandTag.PromoteToTemplateLibrary.Equals(commandTag))
			{
				ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
				if (activeContext != null)
				{
					return CanPromoteToTemplateLibrary(activeContext.Selection);
				}
			}
			else if (CommandTag.DemoteToCopyInstance.Equals(commandTag))
			{
				ISelectionContext activeContext2 = m_contextRegistry.GetActiveContext<ISelectionContext>();
				if (activeContext2 != null)
				{
					return CanDemoteToCopyInstance(activeContext2.Selection);
				}
			}
		}
		return false;
	}

	protected virtual TemplateFolder CreateTemplateFolder()
	{
		TemplateFolder templateFolder = new DomNode(TemplateFolderType).As<TemplateFolder>();
		templateFolder.Name = "New Template Folder".Localize();
		TemplateFolder templateFolder2 = m_targetRef.Target.As<TemplateFolder>();
		if (templateFolder2 == null)
		{
			TemplatingContext templatingContext = m_targetRef.Target.As<TemplatingContext>();
			if (templatingContext != null)
			{
				templateFolder2 = templatingContext.RootFolder;
			}
		}
		templateFolder2?.Folders.Add(templateFolder);
		return templateFolder;
	}

	protected virtual void AddExternalTemplateFolder()
	{
		ImportedContent importedContent = LoadExternalTemplateLibrary(null);
		if (importedContent.RootNode != null)
		{
			importedContent.RootNode.InitializeExtensions();
			TemplateFolder templateFolder = importedContent.RootNode.Cast<TemplateFolder>();
			templateFolder.Url = importedContent.Uri;
			templateFolder.Name = Path.GetFileNameWithoutExtension(importedContent.Uri.LocalPath);
		}
	}

	protected virtual void ReloadExternalTemplates()
	{
	}

	public virtual void DoCommand(object commandTag)
	{
		ISelectionContext context = m_contextRegistry.GetActiveContext<ISelectionContext>();
		ITransactionContext context2 = context.As<ITransactionContext>();
		if (CommandTag.AddTemplateFolder.Equals(commandTag))
		{
			context2.DoTransaction(delegate
			{
				CreateTemplateFolder();
			}, "Add Template Folder".Localize());
		}
		else if (CommandTag.AddExternalTemplateFolder.Equals(commandTag))
		{
			context2.DoTransaction(AddExternalTemplateFolder, "Add External Template Folder".Localize());
		}
		else if (CommandTag.ReloadExternalTemplates.Equals(commandTag))
		{
			context2.DoTransaction(ReloadExternalTemplates, "Reload External Templates".Localize());
		}
		else if (CommandTag.PromoteToTemplateLibrary.Equals(commandTag))
		{
			context2.DoTransaction(delegate
			{
				PromoteToTemplateLibrary(context.Selection);
			}, "Promote To Template Library".Localize());
		}
		else if (CommandTag.DemoteToCopyInstance.Equals(commandTag))
		{
			context2.DoTransaction(delegate
			{
				DemoteToCopyInstance(context.Selection);
			}, "Demote To Copy Instance".Localize());
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public virtual IEnumerable<object> GetCommands(object context, object target)
	{
		m_targetRef = null;
		if (context.Is<TemplatingContext>() && m_templateLister.TreeControl.Focused)
		{
			m_targetRef = new WeakReference(target);
			yield return CommandTag.AddTemplateFolder;
			yield return CommandTag.AddExternalTemplateFolder;
			yield return CommandTag.ReloadExternalTemplates;
		}
	}
}
