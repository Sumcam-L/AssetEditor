using System.ComponentModel.Composition;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(RenameCommand))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class RenameCommand : ICommandClient, IInitializable
{
	private enum Command
	{
		Rename
	}

	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	private string Settings { get; set; }

	[ImportingConstructor]
	public RenameCommand(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	public static string Rename(string original, string prefix, string baseName, string suffix, long numericSuffix = -1L)
	{
		if (baseName != null)
		{
			original = baseName;
		}
		else
		{
			if (!string.IsNullOrEmpty(prefix) && original.StartsWith(prefix))
			{
				original = original.Remove(0, prefix.Length);
			}
			if (numericSuffix >= 0)
			{
				int num = original.Length - 1;
				while (num >= 0 && char.IsDigit(original[num]))
				{
					num--;
				}
				if (num < original.Length - 1)
				{
					original = original.Remove(num + 1);
				}
			}
			if (!string.IsNullOrEmpty(suffix) && original.EndsWith(suffix))
			{
				original = original.Remove(original.Length - suffix.Length, suffix.Length);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(prefix))
		{
			stringBuilder.Append(prefix);
		}
		if (!string.IsNullOrEmpty(original))
		{
			stringBuilder.Append(original);
		}
		if (!string.IsNullOrEmpty(suffix))
		{
			stringBuilder.Append(suffix);
		}
		if (numericSuffix >= 0)
		{
			stringBuilder.Append(numericSuffix.ToString(CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(new CommandInfo(Command.Rename, StandardMenu.Edit, StandardCommandGroup.EditOther, "Rename...".Localize("Rename selected objects"), "Rename selected objects".Localize()), this);
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => Settings, "Settings", null, null));
		}
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		if (Command.Rename.Equals(commandTag))
		{
			ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
			INamingContext activeContext2 = m_contextRegistry.GetActiveContext<INamingContext>();
			if (activeContext != null && activeContext2 != null)
			{
				foreach (object item in activeContext.Selection)
				{
					if (activeContext2.CanSetName(item))
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		INamingContext activeContext2 = m_contextRegistry.GetActiveContext<INamingContext>();
		ITransactionContext activeContext3 = m_contextRegistry.GetActiveContext<ITransactionContext>();
		using RenameCommandDialog renameCommandDialog = new RenameCommandDialog();
		renameCommandDialog.Set(activeContext, activeContext2, activeContext3);
		renameCommandDialog.Settings = Settings;
		renameCommandDialog.ShowDialog(GetDialogOwner());
		Settings = renameCommandDialog.Settings;
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private IWin32Window GetDialogOwner()
	{
		if (m_mainWindow != null)
		{
			return m_mainWindow.DialogOwner;
		}
		if (m_mainForm != null)
		{
			return m_mainForm;
		}
		return null;
	}
}
