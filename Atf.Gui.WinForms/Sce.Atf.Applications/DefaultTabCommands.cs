using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

[Export(typeof(DefaultTabCommands))]
[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DefaultTabCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
{
	private enum Command
	{
		CloseCurrentTab,
		CloseOtherTabs,
		CloseAllTabs,
		CopyFullPath,
		OpenContainingFolder
	}

	public Func<ControlInfo, bool> IsDocumentControl = (ControlInfo controlInfo) => controlInfo.Client.Is<IDocumentClient>();

	private readonly ICommandService m_commandService;

	private readonly IControlHostService m_controlHostService;

	private readonly IControlRegistry m_controlRegistry;

	private static readonly char[] s_invalidFileNameChars = Path.GetInvalidFileNameChars();

	protected ICommandService CommandService => m_commandService;

	[ImportingConstructor]
	public DefaultTabCommands(ICommandService commandService, IControlHostService controlHostService, IControlRegistry controlRegistry)
	{
		m_commandService = commandService;
		m_controlHostService = controlHostService;
		m_controlRegistry = controlRegistry;
	}

	public virtual void Initialize()
	{
		m_commandService.RegisterCommand(Command.CloseCurrentTab, null, null, "Close".Localize(), "Closes the current Tab panel".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.CloseOtherTabs, null, null, "Close All But This".Localize(), "Closes all but the current Tab panel".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.CloseAllTabs, null, null, "Close All".Localize(), "Closes all tabs".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.CopyFullPath, null, null, "Copy Full Path".Localize(), "Copies the file path for the document".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.OpenContainingFolder, null, null, "Open Containing Folder".Localize(), "Opens the folder containing the document in Windows Explorer".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
	}

	public virtual IEnumerable<object> GetCommands(object context, object target)
	{
		if (target is ControlInfo arg && IsDocumentControl(arg))
		{
			return new object[5]
			{
				Command.CloseCurrentTab,
				Command.CloseOtherTabs,
				Command.CloseAllTabs,
				Command.CopyFullPath,
				Command.OpenContainingFolder
			};
		}
		return EmptyEnumerable<object>.Instance;
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		ControlInfo[] documentControls = GetDocumentControls();
		if (documentControls.Length != 0)
		{
			switch ((Command)commandTag)
			{
			case Command.CloseCurrentTab:
				return m_controlRegistry.ActiveControl.Group != StandardControlGroup.CenterPermanent;
			case Command.CloseOtherTabs:
				return documentControls.Length > 1;
			case Command.CloseAllTabs:
				return documentControls.Length != 0;
			case Command.CopyFullPath:
			case Command.OpenContainingFolder:
			{
				string documentPath = GetDocumentPath(m_controlRegistry.ActiveControl);
				bool result = false;
				try
				{
					if (PathUtil.IsValidPath(documentPath))
					{
						result = File.Exists(documentPath);
					}
				}
				catch (IOException)
				{
				}
				return result;
			}
			}
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		switch ((Command)commandTag)
		{
		case Command.CloseCurrentTab:
			Close(m_controlRegistry.ActiveControl);
			break;
		case Command.CloseOtherTabs:
			CloseOthers(m_controlRegistry.ActiveControl);
			break;
		case Command.CloseAllTabs:
			CloseAll();
			break;
		case Command.CopyFullPath:
			Clipboard.SetDataObject(m_controlRegistry.ActiveControl.Description, copy: true);
			break;
		case Command.OpenContainingFolder:
			Process.Start("explorer.exe", "/e,/select," + m_controlRegistry.ActiveControl.Description);
			break;
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void Close(ControlInfo info)
	{
		if (info.Client.Close(info.Control))
		{
			m_controlHostService.UnregisterControl(info.Control);
		}
	}

	private void CloseOthers(ControlInfo info)
	{
		ControlInfo[] documentControls = GetDocumentControls();
		ControlInfo[] array = documentControls;
		foreach (ControlInfo controlInfo in array)
		{
			if (controlInfo != info)
			{
				Close(controlInfo);
			}
		}
	}

	private void CloseAll()
	{
		ControlInfo[] documentControls = GetDocumentControls();
		ControlInfo[] array = documentControls;
		foreach (ControlInfo info in array)
		{
			Close(info);
		}
	}

	private ControlInfo[] GetDocumentControls()
	{
		List<ControlInfo> list = new List<ControlInfo>();
		foreach (ControlInfo control in m_controlHostService.Controls)
		{
			if (IsDocumentControl(control))
			{
				list.Add(control);
			}
		}
		return list.ToArray();
	}

	private string GetDocumentPath(ControlInfo info)
	{
		string description = info.Description;
		return description.Trim(s_invalidFileNameChars);
	}
}
