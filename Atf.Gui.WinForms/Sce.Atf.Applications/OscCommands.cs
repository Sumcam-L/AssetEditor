using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[Export(typeof(OscCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OscCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	private enum Command
	{
		OscInfo,
		CopyOscAddressOfPropertyDescriptor
	}

	private enum CommandGroups
	{
		Osc
	}

	private string m_oscAddressOfPropertyDescriptor;

	[Import(AllowDefault = true)]
	private IWin32Window m_mainWindow;

	[Import(AllowDefault = true)]
	private OscCommandReceiver m_commandReceiver;

	private static readonly string s_copyOscAddressText = "Copy OSC Address".Localize();

	protected ICommandService CommandService { get; private set; }

	protected OscService OscService { get; private set; }

	[ImportingConstructor]
	public OscCommands(ICommandService commandService, OscService oscService)
	{
		CommandService = commandService;
		OscService = oscService;
	}

	public virtual void Initialize()
	{
		CommandService.RegisterCommand(Command.OscInfo, StandardMenu.File, CommandGroups.Osc, "OSC Info", "Displays the status of the OSC service and lists the available OSC addresses and associated properties", Keys.None, null, this);
		CommandService.RegisterCommand(Command.CopyOscAddressOfPropertyDescriptor, null, null, s_copyOscAddressText, "Copies the OSC address of this property to the clipboard".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		return (Command)commandTag switch
		{
			Command.OscInfo => true, 
			Command.CopyOscAddressOfPropertyDescriptor => m_oscAddressOfPropertyDescriptor != null, 
			_ => false, 
		};
	}

	public virtual void DoCommand(object commandTag)
	{
		switch ((Command)commandTag)
		{
		case Command.OscInfo:
		{
			OscDialog oscDialog = new OscDialog(OscService, m_commandReceiver);
			oscDialog.ShowDialog(m_mainWindow);
			break;
		}
		case Command.CopyOscAddressOfPropertyDescriptor:
			Clipboard.SetText(m_oscAddressOfPropertyDescriptor);
			break;
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public IEnumerable<object> GetCommands(object context, object target)
	{
		m_oscAddressOfPropertyDescriptor = null;
		if (!(target is PropertyDescriptor descriptor))
		{
			yield break;
		}
		ISelectionContext selectionContext = context.As<ISelectionContext>();
		if (selectionContext == null)
		{
			yield break;
		}
		object selected = selectionContext.LastSelected;
		if (selected == null)
		{
			yield break;
		}
		string targetKey = descriptor.GetPropertyDescriptorKey();
		foreach (OscService.OscAddressInfo info in OscService.GetInfos(selected))
		{
			if (info.PropertyDescriptor != null && info.PropertyDescriptor.GetPropertyDescriptorKey() == targetKey)
			{
				m_oscAddressOfPropertyDescriptor = info.Address;
				yield return Command.CopyOscAddressOfPropertyDescriptor;
				yield break;
			}
		}
	}
}
