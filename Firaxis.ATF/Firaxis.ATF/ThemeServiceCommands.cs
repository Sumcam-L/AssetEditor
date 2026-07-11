using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(ThemeServiceCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ThemeServiceCommands : IInitializable, ICommandClient
{
	public enum ThemeCommands
	{
		NoTheme,
		PickDarkTheme,
		PickLightTheme,
		PickCustomTheme
	}

	public enum ThemeCommandGroup
	{
		DefaultThemes,
		CustomThemes,
		ImportThemes
	}

	public static CommandInfo PickDarkTheme = new CommandInfo(ThemeCommands.PickDarkTheme, StandardMenu.Window, StandardCommandGroup.UILayout, "Themes\\Firaxis Dark Theme".Localize(), "Select Firaxis Dark Theme".Localize("Select Firaxis Dark Theme"));

	public static CommandInfo PickLightTheme = new CommandInfo(ThemeCommands.PickLightTheme, StandardMenu.Window, StandardCommandGroup.UILayout, "Themes\\Firaxis Light Theme".Localize(), "Select Firaxis Light Theme".Localize("Select Firaxis Light Theme"));

	public static CommandInfo PickCustomTheme = new CommandInfo(ThemeCommands.PickCustomTheme, StandardMenu.Window, StandardCommandGroup.UILayout, "Themes\\Custom Theme".Localize(), "Select Custom Theme 1".Localize("Select Custom Theme 1"));

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	private ThemeCommands m_mruTheme;

	private ICommandService CommandService { get; set; }

	private IThemeService ThemeService { get; set; }

	private ISkinService SkinService { get; set; }

	private SkinServiceEditor SkinServiceEditor { get; set; }

	private ThemeCommands MruTheme
	{
		get
		{
			return m_mruTheme;
		}
		set
		{
			if (m_mruTheme != value)
			{
				m_mruTheme = value;
				UpdateThemeService();
			}
		}
	}

	[ImportingConstructor]
	public ThemeServiceCommands(ICommandService cmdSvc, IThemeService themeSvc, ISkinService skinSvc, SkinServiceEditor skinEditorSvc)
	{
		CommandService = cmdSvc;
		ThemeService = themeSvc;
		SkinService = skinSvc;
		SkinServiceEditor = skinEditorSvc;
		MruTheme = ThemeCommands.PickDarkTheme;
	}

	public void Initialize()
	{
		RegisterCommands();
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => MruTheme, "Most Recently Used Theme".Localize(), null, null));
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (!(commandTag is ThemeCommands themeCommands))
		{
			return false;
		}
		if ((uint)(themeCommands - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is ThemeCommands)
		{
			MruTheme = (ThemeCommands)commandTag;
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void UpdateThemeService()
	{
		switch (m_mruTheme)
		{
		case ThemeCommands.PickLightTheme:
			SkinService.ActiveSkin = LoadThemeFromResource("Firaxis.ATF.Resources.Light.skn");
			break;
		case ThemeCommands.PickDarkTheme:
			SkinService.ActiveSkin = LoadThemeFromResource("Firaxis.ATF.Resources.Dark.skn");
			break;
		case ThemeCommands.PickCustomTheme:
			SkinService.ActiveSkin = LoadUserTheme();
			break;
		default:
			SkinService.ActiveSkin = null;
			break;
		}
	}

	private ISkin LoadThemeFromFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return null;
		}
		if (!(SkinServiceEditor.Open(new Uri(filePath)) is SkinDocument skinDocument))
		{
			return null;
		}
		return skinDocument.Skin;
	}

	private ISkin LoadThemeFromResource(string resName)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
			{
				using XmlTextReader xmlTextReader = new XmlTextReader(input);
				xmlTextReader.Namespaces = false;
				xmlDocument.Load(xmlTextReader);
			}
			return new Skin(xmlDocument, new Uri("res://" + resName));
		}
		catch (System.Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
			return null;
		}
	}

	private ISkin LoadUserTheme()
	{
		string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Custom.skn");
		return LoadThemeFromFile(filePath);
	}

	private void RegisterCommands()
	{
		CommandService.RegisterCommand(PickDarkTheme, this);
		CommandService.RegisterCommand(PickLightTheme, this);
	}

	private void UnregisterCommands()
	{
		CommandService.UnregisterCommand(PickDarkTheme, this);
		CommandService.UnregisterCommand(PickLightTheme, this);
	}
}
