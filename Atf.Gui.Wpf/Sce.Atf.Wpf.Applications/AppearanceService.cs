using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Xml;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Skins;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(AppearanceService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AppearanceService : IInitializable
{
	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	private const string SettingsDocumentElementName = "AppearanceSettings";

	private const string SettingsCurrentAttributeName = "current";

	private const string SettingsSkinElementName = "Skin";

	private const string SettingsSkinAttributeName = "name";

	private const string SettingsSkinAttributeUriName = "uri";

	private string m_currentSkin;

	private readonly Dictionary<string, Sce.Atf.Wpf.Skins.Skin> m_registeredSkins = new Dictionary<string, Sce.Atf.Wpf.Skins.Skin>();

	public IEnumerable<Sce.Atf.Wpf.Skins.Skin> RegisteredSkins => m_registeredSkins.Values;

	public string CurrentSkin
	{
		get
		{
			return m_currentSkin;
		}
		set
		{
			if (!(m_currentSkin != value))
			{
				return;
			}
			OnAppearanceChanging();
			try
			{
				Application.Current.Resources.BeginInit();
				if (!string.IsNullOrEmpty(m_currentSkin))
				{
					m_registeredSkins.TryGetValue(m_currentSkin, out var value2);
					value2?.Unload();
				}
				m_currentSkin = value;
				if (!string.IsNullOrEmpty(m_currentSkin))
				{
					m_registeredSkins.TryGetValue(m_currentSkin, out var value3);
					value3?.Load();
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				Application.Current.Resources.EndInit();
			}
			OnAppearanceChanged();
		}
	}

	public string PersistedSettings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("AppearanceSettings");
			xmlDocument.AppendChild(xmlElement);
			try
			{
				if (!string.IsNullOrEmpty(CurrentSkin))
				{
					xmlElement.SetAttribute("current", CurrentSkin);
				}
				foreach (KeyValuePair<string, Sce.Atf.Wpf.Skins.Skin> registeredSkin in m_registeredSkins)
				{
					Sce.Atf.Wpf.Skins.Skin value = registeredSkin.Value;
					string key = registeredSkin.Key;
					XmlElement xmlElement2 = xmlDocument.CreateElement("Skin");
					xmlElement2.SetAttribute("name", key);
					if (value is ReferencedAssemblySkin referencedAssemblySkin)
					{
						xmlElement2.SetAttribute("uri", referencedAssemblySkin.Uri.ToString());
					}
					xmlElement.AppendChild(xmlElement2);
				}
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Exception saving appearance persisted settings: {0}", ex.Message);
				xmlDocument.RemoveAll();
			}
			return xmlDocument.InnerXml.Trim();
		}
		set
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				if (xmlDocument.DocumentElement == null)
				{
					return;
				}
				string attribute = xmlDocument.DocumentElement.GetAttribute("current");
				foreach (XmlElement childNode in xmlDocument.DocumentElement.ChildNodes)
				{
					string attribute2 = childNode.GetAttribute("name");
					string attribute3 = childNode.GetAttribute("uri");
					if (!string.IsNullOrEmpty(attribute3) && Uri.TryCreate(attribute3, UriKind.RelativeOrAbsolute, out var result))
					{
						RegisterSkin(new ReferencedAssemblySkin(attribute2, result));
					}
				}
				CurrentSkin = ((!string.IsNullOrEmpty(attribute)) ? attribute : m_registeredSkins.Keys.FirstOrDefault());
			}
			catch (Exception)
			{
			}
		}
	}

	public void Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => PersistedSettings, "Skins".Localize(), null, null));
			m_settingsService.RegisterUserSettings("Appearance".Localize(), new BoundPropertyDescriptor(this, () => CurrentSkin, "Theme".Localize(), "Theme".Localize(), "Theme".Localize(), new ThemesValueEditor(this), null));
		}
	}

	public void RegisterSkin(Sce.Atf.Wpf.Skins.Skin skin)
	{
		if (!m_registeredSkins.ContainsKey(skin.Name))
		{
			m_registeredSkins[skin.Name] = skin;
		}
		if (string.IsNullOrEmpty(m_currentSkin))
		{
			CurrentSkin = skin.Name;
		}
	}

	protected void OnAppearanceChanging()
	{
	}

	protected void OnAppearanceChanged()
	{
	}
}
