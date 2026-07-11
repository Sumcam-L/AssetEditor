using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(IContextMenuCommandProvider))]
[InheritedExport(typeof(LayeringCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public abstract class LayeringCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	private enum CommandTag
	{
		AddLayerFolder
	}

	private readonly LayerLister m_layerLister;

	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	private WeakReference m_targetRef;

	protected abstract DomNodeType LayerFolderType { get; }

	[ImportingConstructor]
	public LayeringCommands(ICommandService commandService, IContextRegistry contextRegistry, LayerLister layerLister)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
		m_layerLister = layerLister;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(new CommandInfo(CommandTag.AddLayerFolder, null, null, "Add Layer".Localize(), "Creates a new layer folder".Localize()), this);
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		return CommandTag.AddLayerFolder.Equals(commandTag) && m_targetRef != null && m_targetRef.Target != null && (m_targetRef.Target.Is<LayerFolder>() || m_targetRef.Target.Is<ILayeringContext>());
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (!CommandTag.AddLayerFolder.Equals(commandTag))
		{
			return;
		}
		LayerFolder newLayer = new DomNode(LayerFolderType).As<LayerFolder>();
		newLayer.Name = "New Layer".Localize();
		IList<LayerFolder> layerList = null;
		object target = m_targetRef.Target;
		if (target != null)
		{
			LayerFolder layerFolder = target.As<LayerFolder>();
			if (layerFolder != null)
			{
				layerList = layerFolder.Folders;
			}
			else
			{
				LayeringContext layeringContext = target.As<LayeringContext>();
				if (layeringContext != null)
				{
					layerList = layeringContext.Layers;
				}
			}
		}
		if (layerList != null)
		{
			ILayeringContext mostRecentContext = m_contextRegistry.GetMostRecentContext<ILayeringContext>();
			ITransactionContext context = mostRecentContext.As<ITransactionContext>();
			context.DoTransaction(delegate
			{
				layerList.Add(newLayer);
			}, "Add Layer".Localize());
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
	{
		m_targetRef = null;
		if (context.Is<LayeringContext>() && m_layerLister.TreeControl.Focused)
		{
			m_targetRef = new WeakReference(target);
			yield return CommandTag.AddLayerFolder;
		}
	}
}
