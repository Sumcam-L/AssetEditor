using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[Export(typeof(TargetCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TargetCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	private enum CommandTag
	{
		VitaNeighborhood
	}

	[ImportMany]
	private IEnumerable<ITargetProvider> m_targetProviders = null;

	private const string AddNewString = "Add New ";

	private const string RemoveTargetString = "Remove ";

	private List<object> m_addTargetsCmdTags = new List<object>();

	private List<object> m_removeTargetsCmdTags = new List<object>();

	private ITargetConsumer m_targetConsumer;

	private IEnumerable<TargetInfo> m_selectedTargets;

	[Import(AllowDefault = true)]
	public ICommandService CommandService { get; set; }

	public IEnumerable<ITargetProvider> TargetProviders
	{
		get
		{
			return m_targetProviders;
		}
		set
		{
			m_targetProviders = value;
		}
	}

	void IInitializable.Initialize()
	{
		if (CommandService == null)
		{
			return;
		}
		if (Deci4pTargetProvider.SdkInstalled)
		{
			CommandInfo commandInfo = new CommandInfo(CommandTag.VitaNeighborhood, null, null, "Edit Vita Target in Neighborhood".Localize(), "Edit Vita Target in Neighborhood".Localize());
			commandInfo.ShortcutsEditable = false;
			CommandService.RegisterCommand(commandInfo, this);
		}
		foreach (ITargetProvider targetProvider in TargetProviders)
		{
			if (targetProvider.CanCreateNew)
			{
				string text = "Add New ".Localize() + targetProvider.Name;
				CommandService.RegisterCommand(new CommandInfo(text, null, null, text, "Creates a new target".Localize()), this);
				m_addTargetsCmdTags.Add(text);
				string text2 = "Remove ".Localize() + targetProvider.Name;
				CommandService.RegisterCommand(new CommandInfo(text2, null, null, text2, "Remove selected target".Localize()), this);
				m_removeTargetsCmdTags.Add(text2);
			}
		}
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		if (m_targetProviders == null)
		{
			return false;
		}
		if (CommandTag.VitaNeighborhood.Equals(commandTag))
		{
			if (m_selectedTargets != null && m_selectedTargets.Any())
			{
				return m_selectedTargets.All((TargetInfo target) => target.Protocol == "Deci4p");
			}
			return false;
		}
		if (m_addTargetsCmdTags.Contains(commandTag))
		{
			foreach (ITargetProvider targetProvider2 in TargetProviders)
			{
				string text = "Add New ".Localize() + targetProvider2.Name;
				if (text.Equals(commandTag))
				{
					return true;
				}
			}
			return false;
		}
		if (m_removeTargetsCmdTags.Contains(commandTag))
		{
			foreach (ITargetProvider targetProvider in TargetProviders)
			{
				string text2 = "Remove ".Localize() + targetProvider.Name;
				if (text2.Equals(commandTag) && m_selectedTargets != null && m_selectedTargets.Any())
				{
					return m_selectedTargets.All((TargetInfo target) => targetProvider.GetTargets(m_targetConsumer).Contains(target));
				}
			}
			return false;
		}
		return false;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (CommandTag.VitaNeighborhood.Equals(commandTag))
		{
			Process.Start("Explorer.exe", "/e,/root,::{BA414141-28C6-7F3C-45FF-14C28C11EE88}");
			return;
		}
		if (m_addTargetsCmdTags.Contains(commandTag))
		{
			foreach (ITargetProvider targetProvider in TargetProviders)
			{
				string text = "Add New ".Localize() + targetProvider.Name;
				if (text.Equals(commandTag))
				{
					targetProvider.AddTarget(targetProvider.CreateNew());
					break;
				}
			}
			return;
		}
		if (!m_removeTargetsCmdTags.Contains(commandTag))
		{
			return;
		}
		foreach (TargetInfo selectedTarget in m_selectedTargets)
		{
			foreach (ITargetProvider targetProvider2 in TargetProviders)
			{
				targetProvider2.Remove(selectedTarget);
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
		if (m_removeTargetsCmdTags.Contains(commandTag))
		{
			commandState.Text = "Remove ".Localize() + m_selectedTargets.First().Name;
		}
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object selectedTargets)
	{
		m_selectedTargets = null;
		if (!context.Is<ITargetConsumer>())
		{
			yield break;
		}
		m_targetConsumer = context.Cast<ITargetConsumer>();
		m_selectedTargets = selectedTargets as IEnumerable<TargetInfo>;
		foreach (object addTargetsCmdTag in m_addTargetsCmdTags)
		{
			yield return addTargetsCmdTag;
		}
		if (m_selectedTargets != null && m_selectedTargets.Any())
		{
			foreach (object removeTargetsCmdTag in m_removeTargetsCmdTags)
			{
				yield return removeTargetsCmdTag;
			}
		}
		yield return CommandTag.VitaNeighborhood;
	}
}
