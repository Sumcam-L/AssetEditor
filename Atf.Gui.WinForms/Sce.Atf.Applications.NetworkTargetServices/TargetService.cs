using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TargetService : IInitializable, ITargetService, ICommandClient
{
	private enum Commands
	{
		EditTarget
	}

	protected Dictionary<string, Target> m_targets = new Dictionary<string, Target>();

	private readonly ICommandService m_commandService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	private bool m_singleSelectionMode = true;

	private bool m_canEditPortNumber = true;

	private int m_defaultPortNumber = -1;

	private string[] m_protocols;

	private readonly IMainWindow m_mainWindow;

	public bool SingleSelectionMode
	{
		get
		{
			return m_singleSelectionMode;
		}
		set
		{
			m_singleSelectionMode = value;
		}
	}

	public bool CanEditPortNumber
	{
		get
		{
			return m_canEditPortNumber;
		}
		set
		{
			m_canEditPortNumber = value;
		}
	}

	public int DefaultPortNumber
	{
		get
		{
			return m_defaultPortNumber;
		}
		set
		{
			if (value < 0 || value > 65535)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_defaultPortNumber = value;
		}
	}

	public string Targets
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Targets");
			xmlDocument.AppendChild(xmlElement);
			try
			{
				foreach (Target value in m_targets.Values)
				{
					XmlElement xmlElement2 = xmlDocument.CreateElement("Target");
					xmlElement2.SetAttribute("name", value.Name);
					xmlElement2.SetAttribute("host", value.Host);
					xmlElement2.SetAttribute("protocol", value.Protocol);
					xmlElement2.SetAttribute("port", value.Port.ToString());
					xmlElement2.SetAttribute("selected", value.Selected.ToString());
					xmlElement.AppendChild(xmlElement2);
				}
				if (xmlDocument.DocumentElement.ChildNodes.Count == 0)
				{
					xmlDocument.RemoveAll();
				}
			}
			catch
			{
				xmlDocument.RemoveAll();
			}
			return xmlDocument.InnerXml.Trim();
		}
		set
		{
			try
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				if (m_protocols != null)
				{
					string[] protocols = m_protocols;
					foreach (string key in protocols)
					{
						dictionary[key] = null;
					}
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("Target");
				if (xmlNodeList == null || xmlNodeList.Count == 0)
				{
					return;
				}
				foreach (XmlElement item in xmlNodeList)
				{
					Target target = new Target(item.GetAttribute("name"), item.GetAttribute("host"), int.Parse(item.GetAttribute("port")));
					target.Selected = bool.Parse(item.GetAttribute("selected"));
					string attribute = item.GetAttribute("protocol");
					if (!string.IsNullOrEmpty(attribute) && dictionary.ContainsKey(attribute))
					{
						target.Protocol = attribute;
					}
					m_targets[target.Name] = target;
				}
				Target selectedTarget = GetSelectedTarget();
				if (selectedTarget != null)
				{
					SelectTarget(selectedTarget.Name);
				}
			}
			catch
			{
			}
		}
	}

	[ImportingConstructor]
	public TargetService(ICommandService commandService, IMainWindow main)
	{
		m_commandService = commandService;
		m_mainWindow = main;
	}

	public TargetService()
	{
	}

	void IInitializable.Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => Targets, "Targets".Localize(), null, null));
		}
		RegisterCommand(m_commandService);
	}

	protected void RegisterCommand(ICommandService commandService)
	{
		commandService?.RegisterCommand(Commands.EditTarget, StandardMenu.Edit, StandardCommandGroup.EditPreferences, "Target ...".Localize(), "Edit targets".Localize(), this);
	}

	public Target[] GetAllTargets()
	{
		Target[] result = null;
		List<Target> list = new List<Target>();
		if (m_targets.Count > 0)
		{
			foreach (Target value in m_targets.Values)
			{
				list.Add((Target)value.Clone());
			}
			result = list.ToArray();
		}
		return result;
	}

	public Target[] GetSelectedTargets()
	{
		Target[] result = null;
		List<Target> list = new List<Target>();
		if (m_targets.Count > 0)
		{
			foreach (Target value in m_targets.Values)
			{
				if (value.Selected)
				{
					list.Add((Target)value.Clone());
				}
			}
			if (list.Count > 0)
			{
				result = list.ToArray();
			}
		}
		return result;
	}

	public Target GetSelectedTarget()
	{
		Target result = null;
		if (m_targets.Count > 0)
		{
			foreach (Target value in m_targets.Values)
			{
				if (value.Selected)
				{
					result = (Target)value.Clone();
					break;
				}
			}
		}
		return result;
	}

	public void SelectTarget(string name)
	{
		if (StringUtil.IsNullOrEmptyOrWhitespace(name))
		{
			throw new ArgumentNullException();
		}
		if (m_targets.Count == 0)
		{
			throw new InvalidOperationException();
		}
		Target value = null;
		if (!m_targets.TryGetValue(name, out value))
		{
			throw new Exception(name + " target not found");
		}
		if (m_singleSelectionMode)
		{
			foreach (Target value2 in m_targets.Values)
			{
				value2.Selected = false;
			}
		}
		value.Selected = true;
	}

	public void AddTarget(string name, string host, int port)
	{
		if (StringUtil.IsNullOrEmptyOrWhitespace(name) || StringUtil.IsNullOrEmptyOrWhitespace(host))
		{
			throw new ArgumentNullException();
		}
		if (port < 0 || port > 65535)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (m_targets.ContainsKey(name))
		{
			throw new Exception(name + " already exist");
		}
		Target target = new Target(name.Trim(), host.Trim(), port);
		m_targets.Add(target.Name, target);
	}

	public virtual DialogResult ShowTargetDialog()
	{
		return ShowTargetDialog(m_mainWindow.DialogOwner);
	}

	public void SetProtocols(string[] protocols)
	{
		if (protocols != null && protocols.Length != 0)
		{
			if (m_protocols != null)
			{
				throw new Exception("protocols already been set");
			}
			m_protocols = protocols;
		}
	}

	protected DialogResult ShowTargetDialog(IWin32Window dialogOwner)
	{
		TargetDialog targetDialog = new TargetDialog(m_targets, m_singleSelectionMode, m_defaultPortNumber, m_canEditPortNumber, m_protocols);
		return targetDialog.ShowDialog(dialogOwner);
	}

	public bool CanDoCommand(object commandTag)
	{
		return Commands.EditTarget.Equals(commandTag);
	}

	public void DoCommand(object commandTag)
	{
		if (Commands.EditTarget.Equals(commandTag))
		{
			ShowTargetDialog();
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}
}
