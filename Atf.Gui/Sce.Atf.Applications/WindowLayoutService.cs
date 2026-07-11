using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IWindowLayoutService))]
[Export(typeof(WindowLayoutService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WindowLayoutService : IInitializable, IWindowLayoutService
{
	private class LayoutInformation
	{
		public object DockState { get; set; }

		public List<Pair<IWindowLayoutClient, object>> LayoutData { get; private set; }

		public LayoutInformation()
		{
			LayoutData = new List<Pair<IWindowLayoutClient, object>>();
		}
	}

	private const string DocumentContentPrefix = "Sce.Atf.DockPanel.DocumentContent,";

	private bool m_changing;

	private string m_current = s_defaultLayoutName;

	private bool m_ResetLayout = false;

	private const string SettingsDisplayName = "WindowLayouts";

	private const string SettingsDocumentElementName = "WindowLayoutSettings";

	private const string SettingsCurrentAttributeName = "current";

	private const string SettingsLayoutElementName = "Layout";

	private const string SettingsLayoutAttributeName = "name";

	private const string SettingsDockStateElementName = "DockState";

	private const string SettingsLayoutDataElementName = "LayoutData";

	private const string SettingsLayoutDataAttributeName = "type";

	[ImportMany]
	private IEnumerable<Lazy<IWindowLayoutClient>> m_clients;

	private readonly Dictionary<string, LayoutInformation> m_layouts = new Dictionary<string, LayoutInformation>(StringComparer.CurrentCulture);

	private static readonly string s_defaultLayoutName = string.Empty;

	public string CurrentLayout
	{
		get
		{
			return m_current;
		}
		set
		{
			if (string.IsNullOrEmpty(value) || !IsValidLayoutName(value))
			{
				return;
			}
			OnLayoutsChanging();
			if (m_layouts.TryGetValue(value, out var value2))
			{
				DockStateProvider.DockState = value2.DockState.ToString();
				foreach (Pair<IWindowLayoutClient, object> layoutDatum in value2.LayoutData)
				{
					IWindowLayoutClient first = layoutDatum.First;
					object second = layoutDatum.Second;
					if (first != null)
					{
						first.LayoutData = second;
					}
				}
			}
			else
			{
				value2 = new LayoutInformation
				{
					DockState = FixupXmlData(DockStateProvider.DockState.ToString())
				};
				string dockState = SanitizeLayoutData(value2.DockState.ToString());
				value2.DockState = dockState;
				foreach (IWindowLayoutClient value3 in m_clients.GetValues())
				{
					value2.LayoutData.Add(new Pair<IWindowLayoutClient, object>(value3, value3.LayoutData));
				}
				m_layouts[value] = value2;
			}
			m_current = value;
			OnLayoutsChanged();
		}
	}

	public bool ResetLayout
	{
		get
		{
			return m_ResetLayout;
		}
		set
		{
			m_ResetLayout = value;
			if (m_ResetLayout)
			{
				m_current = s_defaultLayoutName;
				OnLayoutsChanging();
			}
			else
			{
				m_changing = false;
				this.LayoutsChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public bool LayoutChanging => m_changing;

	public IEnumerable<string> Layouts => m_layouts.Keys;

	public IDockStateProvider DockStateProvider { get; private set; }

	[Import(AllowDefault = true)]
	public ISettingsService SettingsService { get; set; }

	public string PersistedSettings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("WindowLayoutSettings");
			xmlDocument.AppendChild(xmlElement);
			try
			{
				xmlElement.SetAttribute("current", string.IsNullOrEmpty(m_current) ? s_defaultLayoutName : m_current);
				foreach (KeyValuePair<string, LayoutInformation> layout in m_layouts)
				{
					string key = layout.Key;
					LayoutInformation value = layout.Value;
					XmlElement xmlElement2 = xmlDocument.CreateElement("Layout");
					xmlElement2.SetAttribute("name", key);
					XmlElement xmlElement3 = xmlDocument.CreateElement("DockState");
					xmlElement3.InnerXml = FixupXmlData(value.DockState.ToString());
					xmlElement2.AppendChild(xmlElement3);
					foreach (Pair<IWindowLayoutClient, object> layoutDatum in value.LayoutData)
					{
						IWindowLayoutClient first = layoutDatum.First;
						object second = layoutDatum.Second;
						if (first != null && second != null)
						{
							string fullName = first.GetType().FullName;
							try
							{
								XmlElement xmlElement4 = xmlDocument.CreateElement("LayoutData");
								xmlElement4.SetAttribute("type", fullName);
								xmlElement4.InnerXml = FixupXmlData(second.ToString());
								xmlElement2.AppendChild(xmlElement4);
							}
							catch (Exception ex)
							{
								Outputs.WriteLine(OutputMessageType.Error, "Exception persisting window layout user data: {0}", ex.Message);
							}
						}
					}
					xmlElement.AppendChild(xmlElement2);
				}
				if (xmlDocument.DocumentElement == null)
				{
					xmlDocument.RemoveAll();
				}
				else if (xmlDocument.DocumentElement.ChildNodes.Count == 0)
				{
					xmlDocument.RemoveAll();
				}
			}
			catch (Exception ex2)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Exception saving layout persisted settings: {0}", ex2.Message);
				xmlDocument.RemoveAll();
			}
			return xmlDocument.InnerXml.Trim();
		}
		set
		{
			try
			{
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				if (xmlDocument.DocumentElement == null)
				{
					return;
				}
				OnLayoutsChanging();
				m_current = ((!xmlDocument.DocumentElement.HasAttribute("current")) ? s_defaultLayoutName : xmlDocument.DocumentElement.GetAttribute("current"));
				foreach (XmlElement childNode in xmlDocument.DocumentElement.ChildNodes)
				{
					string attribute = childNode.GetAttribute("name");
					if (string.IsNullOrEmpty(attribute))
					{
						continue;
					}
					LayoutInformation layoutInformation = new LayoutInformation();
					foreach (XmlElement childNode2 in childNode.ChildNodes)
					{
						if (string.Compare(childNode2.Name, "DockState") == 0)
						{
							layoutInformation.DockState = childNode2.InnerXml;
						}
						else
						{
							if (string.Compare(childNode2.Name, "LayoutData") != 0)
							{
								continue;
							}
							string type = childNode2.GetAttribute("type");
							if (!string.IsNullOrEmpty(type))
							{
								IWindowLayoutClient windowLayoutClient = m_clients.GetValues().FirstOrDefault((IWindowLayoutClient t) => string.Compare(type, t.GetType().FullName) == 0);
								if (windowLayoutClient != null)
								{
									string second = (string)(windowLayoutClient.LayoutData = childNode2.InnerXml);
									layoutInformation.LayoutData.Add(new Pair<IWindowLayoutClient, object>(windowLayoutClient, second));
								}
							}
						}
					}
					if (layoutInformation.DockState != null)
					{
						m_layouts[attribute] = layoutInformation;
					}
				}
				OnLayoutsChanged();
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Exception loading layout persisted settings: {0}", ex.Message);
			}
		}
	}

	public event EventHandler<EventArgs> LayoutsChanging;

	public event EventHandler<EventArgs> LayoutsChanged;

	[ImportingConstructor]
	public WindowLayoutService(IDockStateProvider dockStateProvider)
	{
		DockStateProvider = dockStateProvider;
	}

	public virtual void Initialize()
	{
		if (DockStateProvider != null)
		{
			DockStateProvider.DockStateChanged += DockStateProviderDockStateChanged;
		}
		if (SettingsService != null)
		{
			SettingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => PersistedSettings, "WindowLayouts", null, null));
		}
	}

	public void AddLayout(string newLayoutName, object dockState)
	{
		LayoutInformation layoutInformation = new LayoutInformation
		{
			DockState = dockState
		};
		foreach (IWindowLayoutClient value in m_clients.GetValues())
		{
			layoutInformation.LayoutData.Add(new Pair<IWindowLayoutClient, object>(value, value.LayoutData));
		}
		m_layouts[newLayoutName] = layoutInformation;
	}

	public bool RenameLayout(string oldLayoutName, string newLayoutName)
	{
		if (string.IsNullOrEmpty(oldLayoutName) || string.IsNullOrEmpty(newLayoutName))
		{
			return false;
		}
		if (!m_layouts.TryGetValue(oldLayoutName, out var value))
		{
			return false;
		}
		if (!IsValidLayoutName(newLayoutName))
		{
			return false;
		}
		OnLayoutsChanging();
		m_layouts.Remove(oldLayoutName);
		m_layouts.Add(newLayoutName, value);
		OnLayoutsChanged();
		return true;
	}

	public bool RemoveLayout(string layoutName)
	{
		if (string.IsNullOrEmpty(layoutName))
		{
			return false;
		}
		if (!m_layouts.ContainsKey(layoutName))
		{
			return false;
		}
		OnLayoutsChanging();
		m_layouts.Remove(layoutName);
		if (this.IsCurrent(layoutName))
		{
			m_current = s_defaultLayoutName;
		}
		OnLayoutsChanged();
		return true;
	}

	public static bool IsValidLayoutName(string layoutName)
	{
		if (string.IsNullOrEmpty(layoutName))
		{
			return false;
		}
		char[] anyOf = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct()
			.ToArray();
		return layoutName.IndexOfAny(anyOf) < 0;
	}

	private void OnLayoutsChanging()
	{
		m_changing = true;
		this.LayoutsChanging.Raise(this, EventArgs.Empty);
	}

	private void OnLayoutsChanged()
	{
		try
		{
			this.LayoutsChanged.Raise(this, EventArgs.Empty);
		}
		finally
		{
			m_changing = false;
			m_ResetLayout = false;
		}
	}

	private void DockStateProviderDockStateChanged(object sender, EventArgs e)
	{
		if (!m_changing)
		{
			m_current = s_defaultLayoutName;
		}
	}

	private string SanitizeLayoutData(string xmlData)
	{
		int num = 0;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlData);
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Content");
		foreach (XmlNode item in elementsByTagName)
		{
			if (item.Attributes["PersistString"] != null && item.Attributes["PersistString"].Value.Contains("Sce.Atf.DockPanel.DocumentContent,"))
			{
				item.Attributes["PersistString"].Value = "Ignore" + num;
				num++;
			}
		}
		return xmlDocument.OuterXml;
	}

	private static string FixupXmlData(string xmlData)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlData);
		XmlNode xmlDeclaration = GetXmlDeclaration(xmlDocument);
		if (xmlDeclaration != null)
		{
			xmlDocument.RemoveChild(xmlDeclaration);
		}
		IEnumerable<XmlNode> enumerable = GetXmlComments(xmlDocument).ToList();
		foreach (XmlNode item in enumerable)
		{
			xmlDocument.RemoveChild(item);
		}
		return xmlDocument.InnerXml;
	}

	private static XmlNode GetXmlDeclaration(XmlDocument xmlDoc)
	{
		return xmlDoc.ChildNodes.Cast<XmlNode>().FirstOrDefault((XmlNode n) => n.NodeType == XmlNodeType.XmlDeclaration);
	}

	private static IEnumerable<XmlNode> GetXmlComments(XmlDocument xmlDoc)
	{
		return from XmlNode n in xmlDoc.ChildNodes
			where n.NodeType == XmlNodeType.Comment
			select n;
	}
}
