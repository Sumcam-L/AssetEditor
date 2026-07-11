using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(ITargetService))]
[Export(typeof(TargetService))]
[Export(typeof(ICommandClient))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TargetService : ITargetService, ICommandClient, IInitializable
{
	private enum Command
	{
		EditTargets
	}

	[ImportMany]
	private IEnumerable<Lazy<IProtocol>> m_protocols = null;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	private List<TargetViewModel> m_targets = new List<TargetViewModel>();

	public IEnumerable<IProtocol> Protocols => m_protocols.GetValues();

	public IEnumerable<ITarget> Targets => m_targets.Select((TargetViewModel x) => x.Target);

	public IEnumerable<ITarget> SelectedTargets => from x in m_targets
		where x.IsSelected
		select x.Target;

	public ITarget SelectedTarget
	{
		get
		{
			return m_targets.FirstOrDefault((TargetViewModel x) => x.IsSelected)?.Target;
		}
		set
		{
			if (value != null && !m_targets.Select((TargetViewModel x) => x.Target).Contains(value))
			{
				IProtocol protocol = Protocols.FirstOrDefault((IProtocol x) => x.Id == value.ProtocolId);
				if (protocol == null)
				{
					throw new InvalidOperationException("Could not find Protocol for slected Target");
				}
				m_targets.Add(new TargetViewModel(value, protocol));
			}
			foreach (TargetViewModel target in m_targets)
			{
				target.IsSelected = target.Target.Equals(value);
			}
		}
	}

	public TargetViewModel[] SerializableTargets
	{
		get
		{
			return m_targets.Where((TargetViewModel x) => x.Target.GetType().IsSerializable).ToArray();
		}
		set
		{
			m_targets = new List<TargetViewModel>();
			foreach (TargetViewModel target in value)
			{
				IProtocol protocol = m_protocols.GetValues().FirstOrDefault((IProtocol x) => x.Id == target.Target.ProtocolId);
				if (protocol != null)
				{
					target.Protocol = protocol;
					m_targets.Add(target);
				}
			}
		}
	}

	public string TargetsAsCsv
	{
		get
		{
			string result = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				XmlWriter xmlWriter = XmlWriter.Create(memoryStream);
				xmlWriter.WriteStartElement("Targets");
				foreach (TargetViewModel target in m_targets)
				{
					if (target.Target is IXmlSerializable xmlSerializable)
					{
						Type type = target.Target.GetType();
						xmlWriter.WriteStartElement("Target");
						xmlWriter.WriteAttributeString("selected", target.IsSelected.ToString());
						xmlWriter.WriteStartElement(type.Name, type.Namespace);
						xmlSerializable.WriteXml(xmlWriter);
						xmlWriter.WriteEndElement();
						xmlWriter.WriteEndElement();
					}
				}
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();
				memoryStream.Seek(0L, SeekOrigin.Begin);
				result = new StreamReader(memoryStream).ReadToEnd();
			}
			return result;
		}
		set
		{
			try
			{
				using MemoryStream input = new MemoryStream(Encoding.ASCII.GetBytes(value));
				using XmlReader xmlReader = XmlReader.Create(input);
				while (xmlReader.ReadToFollowing("Target"))
				{
					string attribute = xmlReader.GetAttribute("selected");
					xmlReader.ReadStartElement();
					string typeName = xmlReader.NamespaceURI + "." + xmlReader.LocalName;
					Type type = FindType(typeName);
					if (!(type == null) && Activator.CreateInstance(type) is IXmlSerializable xmlSerializable)
					{
						ITarget itarget = xmlSerializable as ITarget;
						xmlSerializable.ReadXml(xmlReader);
						TargetViewModel targetViewModel = new TargetViewModel(itarget, m_protocols.GetValues().FirstOrDefault((IProtocol x) => x.Id == itarget.ProtocolId));
						if (SelectedTarget == null && string.Compare(attribute, "True", StringComparison.OrdinalIgnoreCase) == 0)
						{
							targetViewModel.IsSelected = true;
						}
						m_targets.Add(targetViewModel);
					}
				}
			}
			catch
			{
			}
		}
	}

	public void Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => TargetsAsCsv, "Targets".Localize(), null, null));
		}
		if (m_commandService != null)
		{
			m_commandService.RegisterCommand(Command.EditTargets, StandardMenu.Edit, StandardCommandGroup.EditPreferences, "_Targets".Localize() + "...", "Edit Targets".Localize(), Keys.None, null, CommandVisibility.Menu, this);
		}
	}

	public void SelectTarget(ITarget target)
	{
		Requires.NotNull(target, "target");
		TargetViewModel targetViewModel = m_targets.FirstOrDefault((TargetViewModel x) => x.Target == target);
		if (targetViewModel == null)
		{
			throw new ArgumentException("Target not found");
		}
		foreach (TargetViewModel target2 in m_targets)
		{
			target2.IsSelected = false;
		}
		targetViewModel.IsSelected = true;
	}

	public bool? ShowTargetDialog()
	{
		TargetDialogViewModel targetDialogViewModel = new TargetDialogViewModel(m_targets, m_protocols.GetValues());
		bool? result = DialogUtils.ShowDialogWithViewModel<TargetDialog>(targetDialogViewModel);
		if (result.HasValue && result.Value)
		{
			m_targets.Clear();
			m_targets.AddRange(targetDialogViewModel.Targets);
		}
		return result;
	}

	public bool CanDoCommand(object tag)
	{
		return tag is Command;
	}

	public void DoCommand(object tag)
	{
		if (tag is Command && (Command)tag == Command.EditTargets)
		{
			ShowTargetDialog();
		}
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public static Type FindType(string typeName)
	{
		return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			select assembly.GetType(typeName)).FirstOrDefault((Type foundType) => foundType != null);
	}
}
