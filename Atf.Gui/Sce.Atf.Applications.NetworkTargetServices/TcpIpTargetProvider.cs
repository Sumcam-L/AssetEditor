using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetProvider))]
[Export(typeof(TcpIpTargetProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TcpIpTargetProvider : ITargetProvider, IInitializable
{
	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	private List<TargetInfo> m_targets = new List<TargetInfo>();

	private bool m_targetsLoaded;

	public virtual string Name => "TCP Target".Localize();

	public bool CanCreateNew => true;

	public virtual string Id => "Sce.Atf.TcpIpTargetProvider";

	public string PersistedTargets
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Targets");
			xmlDocument.AppendChild(xmlElement);
			try
			{
				string innerXml = SerializeTargets(TargetScope.PerApp);
				xmlElement.InnerXml = innerXml;
				if (xmlDocument.DocumentElement.ChildNodes.Count == 0)
				{
					xmlDocument.RemoveAll();
				}
			}
			catch
			{
				xmlDocument.RemoveAll();
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			try
			{
				DeserializeTargets(value, TargetScope.PerApp);
			}
			catch
			{
			}
		}
	}

	[ImportMany]
	protected IEnumerable<ITargetConsumer> TargetConsumers { get; set; }

	public IEnumerable<TargetInfo> GetTargets(ITargetConsumer targetConsumer)
	{
		foreach (TargetInfo target in m_targets)
		{
			yield return target;
		}
	}

	public virtual TargetInfo CreateNew()
	{
		return new TcpIpTargetInfo();
	}

	public bool AddTarget(TargetInfo target)
	{
		if (target is TcpIpTargetInfo && !m_targets.Contains(target))
		{
			m_targets.Add(target);
			foreach (ITargetConsumer targetConsumer in TargetConsumers)
			{
				targetConsumer.TargetsChanged(this, m_targets);
			}
			return true;
		}
		return false;
	}

	public bool Remove(TargetInfo target)
	{
		TargetInfo targetInfo = m_targets.FirstOrDefault((TargetInfo n) => n == target);
		if (targetInfo != null)
		{
			m_targets.Remove(targetInfo);
			foreach (ITargetConsumer targetConsumer in TargetConsumers)
			{
				targetConsumer.TargetsChanged(this, m_targets);
			}
			return true;
		}
		return false;
	}

	void IInitializable.Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.Saving += settingsService_Saving;
			m_settingsService.Reloaded += settingsService_Reloaded;
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => PersistedTargets, "Targets".Localize(), null, null));
		}
	}

	protected void settingsService_Reloaded(object sender, EventArgs e)
	{
		if (m_targetsLoaded)
		{
			return;
		}
		string text = string.Empty;
		try
		{
			string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SCE\\ATF\\TargetSettings.xml");
			string settingsPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SCE\\ATF\\TargetSettings.xml");
			var array = new[]
			{
				new
				{
					SettingsPath = settingsPath,
					Scope = TargetScope.PerUser
				},
				new
				{
					SettingsPath = settingsPath2,
					Scope = TargetScope.AllUsers
				}
			};
			var array2 = array;
			foreach (var anon in array2)
			{
				text = anon.SettingsPath;
				XmlElement xmlElement = null;
				if (File.Exists(text))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(text);
					if (xmlDocument.SelectSingleNode("Targets") is XmlElement)
					{
						DeserializeTargets(xmlDocument.InnerXml, anon.Scope);
					}
				}
			}
			m_targetsLoaded = true;
		}
		catch (Exception ex)
		{
			Outputs.Write(OutputMessageType.Error, text + ": ");
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
	}

	protected void settingsService_Saving(object sender, EventArgs e)
	{
		string text = string.Empty;
		try
		{
			string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SCE\\ATF\\TargetSettings.xml");
			string settingsPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SCE\\ATF\\TargetSettings.xml");
			var array = new[]
			{
				new
				{
					SettingsPath = settingsPath,
					Scope = TargetScope.PerUser
				},
				new
				{
					SettingsPath = settingsPath2,
					Scope = TargetScope.AllUsers
				}
			};
			var array2 = array;
			foreach (var anon in array2)
			{
				text = anon.SettingsPath;
				string directoryName = Path.GetDirectoryName(text);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				XmlElement xmlElement = null;
				XmlDocument xmlDocument = new XmlDocument();
				if (File.Exists(text))
				{
					xmlDocument.Load(text);
					xmlElement = xmlDocument.SelectSingleNode("Targets") as XmlElement;
				}
				else
				{
					xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
					xmlElement = xmlDocument.CreateElement("Targets");
					xmlDocument.AppendChild(xmlElement);
				}
				if (xmlElement == null)
				{
					break;
				}
				List<XmlNode> list = new List<XmlNode>();
				XmlNode xmlNode = xmlElement.SelectSingleNode("TcpTargets");
				if (xmlNode != null)
				{
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{
						if (childNode is XmlElement xmlElement2)
						{
							string attribute = xmlElement2.GetAttribute("provider");
							if (attribute == Id)
							{
								list.Add(childNode);
							}
						}
					}
					foreach (XmlNode item in list)
					{
						xmlNode.RemoveChild(item);
					}
				}
				else
				{
					xmlNode = xmlDocument.CreateElement("TcpTargets");
					xmlElement.AppendChild(xmlNode);
				}
				string innerXml = SerializeTargets(anon.Scope);
				XmlElement xmlElement3 = xmlDocument.CreateElement("TcpTargets");
				xmlElement3.InnerXml = innerXml;
				if (xmlElement3.FirstChild != null)
				{
					foreach (XmlNode childNode2 in xmlElement3.FirstChild.ChildNodes)
					{
						XmlNode newChild = childNode2.CloneNode(deep: false);
						xmlNode.AppendChild(newChild);
					}
				}
				xmlDocument.Save(text);
			}
		}
		catch (Exception ex)
		{
			Outputs.Write(OutputMessageType.Error, text + ": ");
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
	}

	protected virtual string SerializeTargets(TargetScope scope)
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("TcpTargets");
		xmlDocument.AppendChild(xmlElement);
		try
		{
			foreach (TcpIpTargetInfo target in m_targets)
			{
				if (target.Scope == scope)
				{
					XmlElement xmlElement2 = xmlDocument.CreateElement("TcpTarget");
					xmlElement2.SetAttribute("name", target.Name);
					xmlElement2.SetAttribute("platform", target.Platform);
					xmlElement2.SetAttribute("endpoint", target.Endpoint);
					xmlElement2.SetAttribute("protocol", target.Protocol);
					if (scope != TargetScope.PerApp)
					{
						xmlElement2.SetAttribute("provider", Id);
					}
					if (target.FixedPort > 0)
					{
						xmlElement2.SetAttribute("fixedport", target.FixedPort.ToString());
					}
					xmlElement.AppendChild(xmlElement2);
				}
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
		return xmlDocument.InnerXml;
	}

	protected virtual void DeserializeTargets(string value, TargetScope scope)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(value);
		XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("TcpTargets/TcpTarget");
		if (xmlNodeList == null || xmlNodeList.Count == 0)
		{
			return;
		}
		foreach (XmlElement item in xmlNodeList)
		{
			if (scope != TargetScope.PerApp)
			{
				string attribute = item.GetAttribute("provider");
				if (attribute != Id)
				{
					continue;
				}
			}
			TcpIpTargetInfo tcpIpTargetInfo = new TcpIpTargetInfo
			{
				Name = item.GetAttribute("name"),
				Platform = item.GetAttribute("platform"),
				Endpoint = item.GetAttribute("endpoint"),
				Protocol = item.GetAttribute("protocol"),
				Scope = scope
			};
			int result = 0;
			if (int.TryParse(item.GetAttribute("fixedport"), out result))
			{
				tcpIpTargetInfo.FixedPort = result;
			}
			m_targets.Add(tcpIpTargetInfo);
		}
		foreach (ITargetConsumer targetConsumer in TargetConsumers)
		{
			targetConsumer.TargetsChanged(this, m_targets);
		}
	}
}
