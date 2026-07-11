using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(SkinServiceCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SkinServiceCommands : IInitializable, ICommandClient
{
	public enum SkinCommands
	{
		SkinEdit,
		SkinLoad,
		SkinReset
	}

	public enum SkinCommandGroup
	{
		ViewSkin
	}

	public static CommandInfo SkinEdit = new CommandInfo(SkinCommands.SkinEdit, StandardMenu.Window, StandardCommandGroup.UILayout, "Themes\\Customize\\Edit Skin...".Localize("Show Skin Editor and open current skin file."), "Edit skin.".Localize());

	public static CommandInfo SkinLoad = new CommandInfo(SkinCommands.SkinLoad, StandardMenu.Window, StandardCommandGroup.UILayout, "Themes\\Customize\\Load Skin...".Localize("Load and apply a skin file."), "Load and apply a skin file.".Localize());

	public static CommandInfo SkinReset = new CommandInfo(SkinCommands.SkinReset, StandardMenu.Window, StandardCommandGroup.UILayout, "Themes\\Customize\\Reset Skin to Default".Localize("Reset active skin to the default skin."), "Reset active skin to the default skin.".Localize());

	private string m_mruSkinFile;

	private SkinEditor m_skinEditor;

	public SkinService SkinService { get; private set; }

	public SkinServiceEditor SkinServiceEditor { get; private set; }

	public string SkinsDirectory { get; set; }

	private string MruSkinFile
	{
		get
		{
			return m_mruSkinFile;
		}
		set
		{
			SetActiveSkin(value);
		}
	}

	[Import(AllowDefault = true)]
	protected ICommandService CommandService { get; set; }

	[Import(AllowDefault = true)]
	protected IFileDialogService FileDialogService { get; set; }

	[Import(AllowDefault = true)]
	private ISettingsService SettingsService { get; set; }

	[ImportingConstructor]
	public SkinServiceCommands(SkinService skinService, SkinServiceEditor skinSvcEditor)
	{
		SkinService = skinService;
		SkinServiceEditor = skinSvcEditor;
	}

	public virtual void Initialize()
	{
		RegisterCommands();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (!(commandTag is SkinCommands))
		{
			return false;
		}
		if (m_skinEditor != null)
		{
			return false;
		}
		bool result = false;
		switch ((SkinCommands)commandTag)
		{
		case SkinCommands.SkinEdit:
			result = true;
			break;
		case SkinCommands.SkinLoad:
			result = FileDialogService != null;
			break;
		case SkinCommands.SkinReset:
			result = SkinService.ActiveSkin != null;
			break;
		}
		return result;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is SkinCommands)
		{
			switch ((SkinCommands)commandTag)
			{
			case SkinCommands.SkinEdit:
				EditSkin();
				break;
			case SkinCommands.SkinLoad:
				LoadSkin();
				break;
			case SkinCommands.SkinReset:
				ResetSkin();
				break;
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void SetActiveSkin(string activeSkin)
	{
		if (!string.IsNullOrWhiteSpace(activeSkin) && (!(m_mruSkinFile == activeSkin) || !(SkinService.ActiveSkin?.Uri.LocalPath == activeSkin)))
		{
			m_mruSkinFile = activeSkin;
			if (SkinService.ActiveSkin?.Uri.LocalPath != m_mruSkinFile)
			{
				IDocument document = SkinServiceEditor.Open(new Uri(m_mruSkinFile));
				ISkin activeSkin2 = (document as SkinDocument)?.Skin;
				SkinService.ActiveSkin = activeSkin2;
				SkinsDirectory = Directory.GetParent(m_mruSkinFile).FullName;
			}
		}
	}

	private void RegisterCommands()
	{
		if (CommandService != null)
		{
			CommandService.RegisterCommand(SkinLoad, this);
			CommandService.RegisterCommand(SkinEdit, this);
			CommandService.RegisterCommand(SkinReset, this);
		}
	}

	private void UnregisterCommands()
	{
		if (CommandService != null)
		{
			CommandService.UnregisterCommand(SkinLoad, this);
			CommandService.UnregisterCommand(SkinEdit, this);
			CommandService.UnregisterCommand(SkinReset, this);
		}
	}

	private void EditSkin()
	{
		if (m_skinEditor == null)
		{
			m_skinEditor = new SkinEditor(SkinServiceEditor, this);
			m_skinEditor.Show(SkinService.MainForm);
		}
		if (SkinService.ActiveSkin != null && SkinService.ActiveSkin.Uri != null && SkinService.ActiveSkin.Uri.IsFile)
		{
			m_skinEditor.OpenSkin(SkinService.ActiveSkin.Uri.LocalPath);
		}
		m_skinEditor.FormClosed += SkinEditor_FormClosed;
		m_skinEditor.SkinChanged += SkinEditor_SkinChanged;
		SkinService.MainForm.FormClosing += MainForm_FormClosing;
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (!e.Cancel && m_skinEditor != null)
		{
			bool flag = m_skinEditor.ConfirmCloseActiveDocument();
			e.Cancel = !flag;
			if (flag)
			{
				m_skinEditor.Close();
			}
		}
	}

	private void SkinEditor_FormClosed(object sender, FormClosedEventArgs e)
	{
		m_skinEditor = null;
		SkinService.MainForm.FormClosing -= MainForm_FormClosing;
	}

	private void SkinEditor_SkinChanged(object sender, DocumentEventArgs e)
	{
		using Stream input = m_skinEditor.GetCurrentSkin();
		using XmlTextReader xmlTextReader = new XmlTextReader(input);
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlTextReader.Namespaces = false;
			xmlDocument.Load(xmlTextReader);
			ISkin activeSkin = new Skin(xmlDocument, m_skinEditor.ActiveDocument.Uri);
			SkinService.ActiveSkin = activeSkin;
		}
		catch (Exception)
		{
			SkinService.ActiveSkin = null;
		}
	}

	private void LoadSkin()
	{
		string directory = ((SkinService.ActiveSkin == null || SkinService.ActiveSkin.Uri == null) ? SkinsDirectory : Directory.GetParent(SkinService.ActiveSkin.Uri.LocalPath)?.FullName);
		string pathName = null;
		FileDialogResult fileDialogResult = FileDialogService.OpenFileName(ref pathName, SkinServiceEditor.Info.GetFilterString(), directory);
		if (fileDialogResult == FileDialogResult.OK)
		{
			SkinDocument skinDocument = SkinServiceEditor.Open(new Uri(pathName)) as SkinDocument;
			SkinService.ActiveSkin = skinDocument?.Skin;
			SkinsDirectory = Directory.GetParent(pathName).FullName;
		}
	}

	private void ResetSkin()
	{
		SkinService.ActiveSkin = null;
	}
}
