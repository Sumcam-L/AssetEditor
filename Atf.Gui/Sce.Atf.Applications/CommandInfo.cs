using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

public class CommandInfo
{
	public readonly object CommandTag;

	public readonly object MenuTag;

	public readonly object GroupTag;

	public readonly string MenuText;

	public string ImageName;

	public object ImageKey;

	public string DisplayedMenuText;

	public readonly string Description;

	public string HelpUrl;

	public readonly int Index = s_count++;

	public static CommandInfo FileSave = new CommandInfo(StandardCommand.FileSave, StandardMenu.File, StandardCommandGroup.FileSave, "Save".Localize("Save the active file"), "Save the active file".Localize(), Keys.S | Keys.Control, Resources.SaveImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FileSaveAs = new CommandInfo(StandardCommand.FileSaveAs, StandardMenu.File, StandardCommandGroup.FileSave, "Save As ...".Localize("Save the active file under a new name"), "Save the active file under a new name".Localize(), Keys.None, Resources.SaveAsImage, CommandVisibility.Menu, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FileSaveAll = new CommandInfo(StandardCommand.FileSaveAll, StandardMenu.File, StandardCommandGroup.FileSave, "Save All".Localize("Saves all open files"), "Save all open files".Localize(), Keys.S | Keys.Shift | Keys.Control, Resources.SaveAllImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FileClose = new CommandInfo(StandardCommand.FileClose, StandardMenu.File, StandardCommandGroup.FileSave, "Close".Localize("Close the active file"), "Close the active file".Localize(), new Keys[2]
	{
		Keys.W | Keys.Control,
		Keys.F4 | Keys.Control
	}, null, CommandVisibility.Menu);

	public static CommandInfo FilePrint = new CommandInfo(StandardCommand.Print, StandardMenu.File, StandardCommandGroup.FilePrint, "Print...".Localize("Print the active document"), "Print the active document".Localize(), Keys.P | Keys.Control, Resources.PrinterImage);

	public static CommandInfo FilePageSetup = new CommandInfo(StandardCommand.PageSetup, StandardMenu.File, StandardCommandGroup.FilePrint, "Page Setup...".Localize("Set up page for printing"), "Set up page for printing".Localize(), Keys.None, Resources.PrinterPreferencesImage);

	public static CommandInfo FilePrintPreview = new CommandInfo(StandardCommand.PrintPreview, StandardMenu.File, StandardCommandGroup.FilePrint, "Print Preview...".Localize("Show a print preview of the active document"), "Show a print preview of the active document".Localize(), Keys.None, Resources.PrinterViewImage);

	public static CommandInfo FileExit = new CommandInfo(StandardCommand.FileExit, StandardMenu.File, StandardCommandGroup.FileExit, "Exit".Localize("Exit the application"), "Exit Application".Localize());

	public static CommandInfo EditUndo = new CommandInfo(StandardCommand.EditUndo, StandardMenu.Edit, StandardCommandGroup.EditUndo, "Undo".Localize("Undo the last change"), "Undo the last change".Localize(), Keys.Z | Keys.Control, Resources.UndoImage);

	public static CommandInfo EditRedo = new CommandInfo(StandardCommand.EditRedo, StandardMenu.Edit, StandardCommandGroup.EditUndo, "Redo".Localize("Redo the last edit"), "Redo the last edit".Localize(), Keys.Y | Keys.Control, Resources.RedoImage);

	public static CommandInfo EditCut = new CommandInfo(StandardCommand.EditCut, StandardMenu.Edit, StandardCommandGroup.EditCut, "Cut".Localize("Cut the selection and place it on the clipboard"), "Cut the selection and place it on the clipboard".Localize(), Keys.X | Keys.Control, Resources.CutImage);

	public static CommandInfo EditCopy = new CommandInfo(StandardCommand.EditCopy, StandardMenu.Edit, StandardCommandGroup.EditCut, "Copy".Localize("Copy the selection and place it on the clipboard"), "Copy the selection and place it on the clipboard".Localize(), Keys.C | Keys.Control, Resources.CopyImage);

	public static CommandInfo EditPaste = new CommandInfo(StandardCommand.EditPaste, StandardMenu.Edit, StandardCommandGroup.EditCut, "Paste".Localize("Paste the contents of the clipboard and make that the new selection"), "Paste the contents of the clipboard and make that the new selection".Localize(), Keys.V | Keys.Control, Resources.PasteImage);

	public static CommandInfo EditDelete = new CommandInfo(StandardCommand.EditDelete, StandardMenu.Edit, StandardCommandGroup.EditCut, "Delete".Localize("Delete the selection"), "Delete the selection".Localize(), Keys.Delete, Resources.DeleteImage, CommandVisibility.All);

	public static CommandInfo EditSelectAll = new CommandInfo(StandardCommand.EditSelectAll, StandardMenu.Edit, StandardCommandGroup.EditSelectAll, "Select All".Localize("Select all items"), "Select all items".Localize(), Keys.A | Keys.Control);

	public static CommandInfo EditDeselectAll = new CommandInfo(StandardCommand.EditDeselectAll, StandardMenu.Edit, StandardCommandGroup.EditSelectAll, "Deselect All".Localize("Deselect all items"), "Deselect all items".Localize());

	public static CommandInfo EditInvertSelection = new CommandInfo(StandardCommand.EditInvertSelection, StandardMenu.Edit, StandardCommandGroup.EditSelectAll, "Invert Selection".Localize("Select unselected items and deselect selected items"), "Select unselected items and deselect selected items".Localize());

	public static CommandInfo EditLock = new CommandInfo(StandardCommand.EditLock, StandardMenu.Edit, StandardCommandGroup.EditOther, "Lock".Localize(), "Lock the selection to disable editing".Localize(), Keys.L | Keys.Control, Resources.LockImage);

	public static CommandInfo EditUnlock = new CommandInfo(StandardCommand.EditUnlock, StandardMenu.Edit, StandardCommandGroup.EditOther, "Unlock".Localize("Unlock the selection to enable editing"), "Unlock the selection to enable editing".Localize(), Keys.L | Keys.Shift | Keys.Control, Resources.UnlockImage);

	public static CommandInfo UILock = new CommandInfo(StandardCommand.UILock, StandardMenu.Window, StandardCommandGroup.UILayout, "Lock UI Layout".Localize(), "Lock UI Layout".Localize(), Keys.None, Resources.UnlockUIImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo EditGroup = new CommandInfo(StandardCommand.EditGroup, StandardMenu.Edit, StandardCommandGroup.EditOther, "Group".Localize("Group the selection into a single item"), "Group the selection into a single item".Localize(), Keys.G | Keys.Control, Resources.GroupImage);

	public static CommandInfo EditUngroup = new CommandInfo(StandardCommand.EditUngroup, StandardMenu.Edit, StandardCommandGroup.EditOther, "Ungroup".Localize("Ungroup any selected groups"), "Ungroup any selected groups".Localize(), Keys.G | Keys.Shift | Keys.Control, Resources.UngroupImage);

	public static CommandInfo ViewHide = new CommandInfo(StandardCommand.ViewHide, StandardMenu.View, StandardCommandGroup.ViewShow, "Hide".Localize("Hide all selected objects"), "Hide all selected objects".Localize(), Keys.H, Resources.HideImage);

	public static CommandInfo ViewShow = new CommandInfo(StandardCommand.ViewShow, StandardMenu.View, StandardCommandGroup.ViewShow, "Show".Localize("Show all selected objects"), "Show all selected objects".Localize(), Keys.H | Keys.Shift, Resources.ShowImage);

	public static CommandInfo ViewShowLast = new CommandInfo(StandardCommand.ViewShowLast, StandardMenu.View, StandardCommandGroup.ViewShow, "Show Last Hidden".Localize("Show the last hidden object"), "Show the last hidden object".Localize(), Keys.H | Keys.Control, Resources.ShowLastImage);

	public static CommandInfo ViewShowAll = new CommandInfo(StandardCommand.ViewShowAll, StandardMenu.View, StandardCommandGroup.ViewShow, "Show All".Localize("Show all hidden objects"), "Show all hidden objects".Localize(), Keys.R | Keys.Shift, Resources.ShowAllImage);

	public static CommandInfo ViewIsolate = new CommandInfo(StandardCommand.ViewIsolate, StandardMenu.View, StandardCommandGroup.ViewShow, "Isolate".Localize("Show only the selected objects and hide all others"), "Show only the selected objects and hide all others".Localize(), Keys.I, Resources.IsolateImage);

	public static CommandInfo ViewFrameSelection = new CommandInfo(StandardCommand.ViewFrameSelection, StandardMenu.View, StandardCommandGroup.ViewZoomIn, "Frame Selection".Localize("Frame selection in view"), "Frames all selected objects in the current view".Localize(), Keys.F, Resources.FitToSizeImage);

	public static CommandInfo ViewFrameAll = new CommandInfo(StandardCommand.ViewFrameAll, StandardMenu.View, StandardCommandGroup.ViewZoomIn, "Frame All".Localize("Frame all objects in view"), "Frames all objects in the current view".Localize(), Keys.F | Keys.Shift, null);

	public static CommandInfo ViewZoomIn = new CommandInfo(StandardCommand.ViewZoomIn, StandardMenu.View, StandardCommandGroup.ViewZoomIn, "Zoom In".Localize("Zoom In"), "Zoom In".Localize(), Keys.Oemplus | Keys.Control, null);

	public static CommandInfo ViewZoomOut = new CommandInfo(StandardCommand.ViewZoomOut, StandardMenu.View, StandardCommandGroup.ViewZoomIn, "Zoom Out".Localize("Zoom Out"), "Zoom Out".Localize(), Keys.OemMinus | Keys.Control, null);

	public static CommandInfo ViewZoomReset = new CommandInfo(StandardCommand.ViewZoomReset, StandardMenu.View, StandardCommandGroup.ViewZoomIn, "Zoom Reset".Localize("Zoom Reset"), "Zoom Reset".Localize(), Keys.D0 | Keys.Control, null);

	public static CommandInfo ViewZoomExtents = new CommandInfo(StandardCommand.ViewZoomExtents, StandardMenu.View, StandardCommandGroup.ViewZoomIn, "Fit In Active View".Localize("Pan and Zoom to center selection"), "Pan and Zoom to center selection".Localize(), Keys.F, Resources.FitToSizeImage);

	public static CommandInfo FormatAlignLefts = new CommandInfo(StandardCommand.FormatAlignLefts, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/Lefts".Localize("Align left sides of selected items"), "Align left sides of selected items".Localize(), Keys.None, Resources.AlignLeftsImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FormatAlignRights = new CommandInfo(StandardCommand.FormatAlignRights, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/Rights".Localize("Align right sides of selected items"), "Align right sides of selected items".Localize(), Keys.None, Resources.AlignRightsImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FormatAlignCenters = new CommandInfo(StandardCommand.FormatAlignCenters, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/Centers".Localize("Align centers of selected items"), "Align centers of selected items".Localize(), Keys.None, Resources.AlignCentersImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FormatAlignTops = new CommandInfo(StandardCommand.FormatAlignTops, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/Tops".Localize("Align tops of selected items"), "Align tops of selected items".Localize(), Keys.None, Resources.AlignTopsImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FormatAlignBottoms = new CommandInfo(StandardCommand.FormatAlignBottoms, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/Bottoms".Localize("Align bottoms of selected items"), "Align bottoms of selected items".Localize(), Keys.None, Resources.AlignBottomsImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FormatAlignMiddles = new CommandInfo(StandardCommand.FormatAlignMiddles, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/Middles".Localize(), "Align middles of selected items".Localize(), Keys.None, Resources.AlignMiddlesImage, CommandVisibility.Default, "https://github.com/SonyWWS/ATF/wiki/Using-Standard-Command-Components".Localize());

	public static CommandInfo FormatAlignToGrid = new CommandInfo(StandardCommand.FormatAlignToGrid, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Align/To Grid".Localize("Align selected items to x/y grid"), "Align selected items to x/y grid".Localize());

	public static CommandInfo FormatMakeSizeEqual = new CommandInfo(StandardCommand.FormatMakeSizeEqual, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Size/Make Equal".Localize("Make selected items have the same size"), "Make selected items have the same size".Localize());

	public static CommandInfo FormatMakeWidthEqual = new CommandInfo(StandardCommand.FormatMakeWidthEqual, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Size/Make Widths Equal".Localize("Make selected items have the same width"), "Make selected items have the same width".Localize());

	public static CommandInfo FormatMakeHeightEqual = new CommandInfo(StandardCommand.FormatMakeHeightEqual, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Size/Make Heights Equal".Localize("Make selected items have the same height"), "Make selected items have the same height".Localize());

	public static CommandInfo FormatSizeToGrid = new CommandInfo(StandardCommand.FormatSizeToGrid, StandardMenu.Format, StandardCommandGroup.FormatAlign, "Size/Size to Grid".Localize("Make selected items sizes align to x/y grid"), "Make selected items sizes align to x/y grid".Localize());

	public static CommandInfo WindowSplitHoriz = new CommandInfo(StandardCommand.WindowSplitHoriz, StandardMenu.Window, StandardCommandGroup.WindowSplit, "Split Horizontal".Localize("Split the window horizontally"), "Split the window horizontally".Localize());

	public static CommandInfo WindowSplitVert = new CommandInfo(StandardCommand.WindowSplitVert, StandardMenu.Window, StandardCommandGroup.WindowSplit, "Split Vertical".Localize("Split the window vertically"), "Split the window vertically".Localize());

	public static CommandInfo WindowRemoveSplit = new CommandInfo(StandardCommand.WindowRemoveSplit, StandardMenu.Window, StandardCommandGroup.WindowSplit, "Remove Split".Localize("Remove the split"), "Remove the split".Localize());

	public static CommandInfo WindowTileHorizontal = new CommandInfo(StandardCommand.WindowTileHorizontal, StandardMenu.Window, StandardCommandGroup.WindowTile, "Tile Horizontal".Localize("Tile Documents Horizontally"), "Tile the documents, as separate visible items, horizontally".Localize());

	public static CommandInfo WindowTileVertical = new CommandInfo(StandardCommand.WindowTileVertical, StandardMenu.Window, StandardCommandGroup.WindowTile, "Tile Vertical".Localize("Tile Documents Vertically"), "Tile the documents, as separate visible items, vertically".Localize());

	public static CommandInfo WindowTileTabbed = new CommandInfo(StandardCommand.WindowTileTabbed, StandardMenu.Window, StandardCommandGroup.WindowTile, "Tile Overlapping".Localize("Tile Documents Overlapping"), "Tile the documents, all together as tabbed items, in the central region of the application".Localize());

	public static CommandInfo HelpAbout = new CommandInfo(StandardCommand.HelpAbout, StandardMenu.Help, StandardCommandGroup.HelpAbout, "&About".Localize("Get information about application"), "Get information about application".Localize());

	private static int s_count;

	private List<Keys> m_shortcuts;

	private List<Keys> m_defaultShortcuts;

	private CommandVisibility m_visibility;

	private ICommandService m_commandService;

	private readonly HashSet<ICommandClient> m_checkCanDoClients = new HashSet<ICommandClient>();

	public IEnumerable<ICommandClient> CheckCanDoClients => m_checkCanDoClients;

	public IEnumerable<Keys> Shortcuts
	{
		get
		{
			return m_shortcuts;
		}
		set
		{
			m_shortcuts = new List<Keys>(value);
			this.ShortcutsChanged.Raise(this, EventArgs.Empty);
		}
	}

	public bool ShortcutsAreDefault
	{
		get
		{
			if (IsEmptyOrNone(m_shortcuts) && IsEmptyOrNone(m_defaultShortcuts))
			{
				return true;
			}
			return m_defaultShortcuts.SequenceEqual(m_shortcuts);
		}
	}

	public IEnumerable<Keys> DefaultShortcuts
	{
		get
		{
			return m_defaultShortcuts;
		}
		private set
		{
			m_defaultShortcuts = new List<Keys>(value);
		}
	}

	public ICommandService CommandService
	{
		get
		{
			return m_commandService;
		}
		set
		{
			if (m_commandService != null && value != null)
			{
				throw new InvalidOperationException("CommandInfo already has been registered");
			}
			m_commandService = value;
		}
	}

	public CommandVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			if (m_visibility != value)
			{
				m_visibility = value;
				this.VisibilityChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public bool CheckOnClick { get; set; }

	public bool ShortcutsEditable { get; set; }

	public event EventHandler ShortcutsChanged;

	public event EventHandler VisibilityChanged;

	public event EventHandler CheckCanDo;

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description)
		: this(commandTag, menuTag, groupTag, menuText, description, Keys.None, null, CommandVisibility.Menu)
	{
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut)
		: this(commandTag, menuTag, groupTag, menuText, description, shortcut, null, CommandVisibility.Menu)
	{
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, string imageName)
		: this(commandTag, menuTag, groupTag, menuText, description, shortcut, imageName, CommandVisibility.Default)
	{
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, IEnumerable<Keys> shortcuts, string imageName)
		: this(commandTag, menuTag, groupTag, menuText, description, shortcuts, imageName, CommandVisibility.Default)
	{
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, string imageName, CommandVisibility visibility, string helpUrl = null)
	{
		CommandTag = commandTag;
		MenuTag = menuTag;
		GroupTag = groupTag;
		MenuText = menuText;
		Description = description;
		DefaultShortcuts = new Keys[1] { shortcut };
		Shortcuts = new Keys[1] { shortcut };
		ImageName = imageName;
		Visibility = visibility;
		HelpUrl = helpUrl;
		ShortcutsEditable = true;
		this.ShortcutsChanged.Raise(this, EventArgs.Empty);
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, IEnumerable<Keys> shortcuts, string imageName, CommandVisibility visibility, string helpUrl = null)
	{
		CommandTag = commandTag;
		MenuTag = menuTag;
		GroupTag = groupTag;
		MenuText = menuText;
		Description = description;
		DefaultShortcuts = shortcuts;
		Shortcuts = shortcuts;
		ImageName = imageName;
		Visibility = visibility;
		HelpUrl = helpUrl;
		ShortcutsEditable = true;
		this.ShortcutsChanged.Raise(this, EventArgs.Empty);
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, object imageKey)
		: this(commandTag, menuTag, groupTag, menuText, description, shortcut, imageKey, CommandVisibility.Default)
	{
	}

	public CommandInfo(object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, object imageKey, CommandVisibility visibility)
	{
		CommandTag = commandTag;
		MenuTag = menuTag;
		GroupTag = groupTag;
		MenuText = menuText;
		Description = description;
		DefaultShortcuts = new Keys[1] { shortcut };
		Shortcuts = new Keys[1] { shortcut };
		ImageName = imageKey as string;
		ImageKey = imageKey;
		Visibility = visibility;
		ShortcutsEditable = true;
		this.ShortcutsChanged.Raise(this, EventArgs.Empty);
	}

	public void EnableCheckCanDoEvent(ICommandClient client)
	{
		m_checkCanDoClients.Add(client);
	}

	public void OnCheckCanDo(ICommandClient client)
	{
		if (!m_checkCanDoClients.Contains(client))
		{
			throw new InvalidOperationException("Call EnableCheckCanDoEvent before calling OnCheckCanDo");
		}
		this.CheckCanDo.Raise(this, EventArgs.Empty);
	}

	public void ClearShortcuts()
	{
		m_shortcuts.Clear();
		this.ShortcutsChanged.Raise(this, EventArgs.Empty);
	}

	public void AddShortcut(Keys shortcut)
	{
		if (shortcut != Keys.None)
		{
			m_shortcuts.Remove(Keys.None);
			if (!m_shortcuts.Contains(shortcut))
			{
				m_shortcuts.Add(shortcut);
			}
			this.ShortcutsChanged.Raise(this, EventArgs.Empty);
		}
	}

	public void RemoveShortcut(Keys shortcut)
	{
		if (shortcut != Keys.None && m_shortcuts.Remove(shortcut))
		{
			this.ShortcutsChanged.Raise(this, EventArgs.Empty);
		}
	}

	public static CommandInfo GetStandardCommand(StandardCommand command)
	{
		return command switch
		{
			StandardCommand.FileClose => FileClose, 
			StandardCommand.FileSave => FileSave, 
			StandardCommand.FileSaveAs => FileSaveAs, 
			StandardCommand.FileSaveAll => FileSaveAll, 
			StandardCommand.FileExit => FileExit, 
			StandardCommand.PageSetup => FilePageSetup, 
			StandardCommand.PrintPreview => FilePrintPreview, 
			StandardCommand.Print => FilePrint, 
			StandardCommand.EditUndo => EditUndo, 
			StandardCommand.EditRedo => EditRedo, 
			StandardCommand.EditCut => EditCut, 
			StandardCommand.EditCopy => EditCopy, 
			StandardCommand.EditPaste => EditPaste, 
			StandardCommand.EditDelete => EditDelete, 
			StandardCommand.EditSelectAll => EditSelectAll, 
			StandardCommand.EditDeselectAll => EditDeselectAll, 
			StandardCommand.EditInvertSelection => EditInvertSelection, 
			StandardCommand.EditLock => EditLock, 
			StandardCommand.EditUnlock => EditUnlock, 
			StandardCommand.EditGroup => EditGroup, 
			StandardCommand.EditUngroup => EditUngroup, 
			StandardCommand.ViewFrameSelection => ViewFrameSelection, 
			StandardCommand.ViewFrameAll => ViewFrameAll, 
			StandardCommand.ViewZoomIn => ViewZoomIn, 
			StandardCommand.ViewZoomOut => ViewZoomOut, 
			StandardCommand.ViewZoomExtents => ViewZoomExtents, 
			StandardCommand.FormatAlignLefts => FormatAlignLefts, 
			StandardCommand.FormatAlignRights => FormatAlignRights, 
			StandardCommand.FormatAlignCenters => FormatAlignCenters, 
			StandardCommand.FormatAlignTops => FormatAlignTops, 
			StandardCommand.FormatAlignBottoms => FormatAlignBottoms, 
			StandardCommand.FormatAlignMiddles => FormatAlignMiddles, 
			StandardCommand.FormatAlignToGrid => FormatAlignToGrid, 
			StandardCommand.FormatMakeSizeEqual => FormatMakeSizeEqual, 
			StandardCommand.FormatMakeWidthEqual => FormatMakeWidthEqual, 
			StandardCommand.FormatMakeHeightEqual => FormatMakeHeightEqual, 
			StandardCommand.FormatSizeToGrid => FormatSizeToGrid, 
			StandardCommand.WindowSplitHoriz => WindowSplitHoriz, 
			StandardCommand.WindowSplitVert => WindowSplitVert, 
			StandardCommand.WindowRemoveSplit => WindowRemoveSplit, 
			StandardCommand.HelpAbout => HelpAbout, 
			StandardCommand.UILock => UILock, 
			_ => throw new NotImplementedException(), 
		};
	}

	private bool IsEmptyOrNone(IList<Keys> shortcuts)
	{
		foreach (Keys shortcut in shortcuts)
		{
			if (shortcut != Keys.None)
			{
				return false;
			}
		}
		return true;
	}
}
