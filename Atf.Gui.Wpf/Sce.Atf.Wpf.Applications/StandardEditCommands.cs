using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(StandardEditCommands))]
[Export(typeof(IContextMenuCommandProvider))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardEditCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	private class ClipboardLoggingData
	{
		private readonly List<string> m_formats = new List<string>();

		private const string kLogFileFormatString = "AtfClipboardLogging_{0}.log";

		public string LogFile { get; private set; }

		public ClipboardLoggingData()
		{
			LogFile = string.Format("AtfClipboardLogging_{0}.log", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}

		public bool AddFormat(string format)
		{
			if (m_formats.Contains(format))
			{
				return false;
			}
			m_formats.Add(format);
			return true;
		}
	}

	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	private static ClipboardLoggingData s_clipboardLoggingData;

	private static bool s_useSystemClipboard;

	private static IDataObject s_clipboard;

	private static uint s_clipboardNum;

	private static ICollection<string> s_clipboardFormatsToIgnore;

	private static ICollection<string> s_clipboardAcceptableFormats;

	public static bool UseSystemClipboard
	{
		get
		{
			return s_useSystemClipboard;
		}
		set
		{
			s_useSystemClipboard = value;
		}
	}

	public static ICollection<string> SystemClipboardFormatsToIgnore
	{
		get
		{
			return s_clipboardFormatsToIgnore;
		}
		set
		{
			s_clipboardFormatsToIgnore = value ?? new List<string>();
		}
	}

	public static ICollection<string> AcceptableSystemClipboardFormats
	{
		get
		{
			return s_clipboardAcceptableFormats;
		}
		set
		{
			s_clipboardAcceptableFormats = value ?? new List<string>();
		}
	}

	public IDataObject Clipboard
	{
		get
		{
			if (s_useSystemClipboard)
			{
				try
				{
					uint clipboardSequenceNumber = User32.GetClipboardSequenceNumber();
					if (clipboardSequenceNumber != s_clipboardNum)
					{
						s_clipboardNum = clipboardSequenceNumber;
						IDataObject dataObject = System.Windows.Clipboard.GetDataObject();
						if (dataObject != null)
						{
							string[] formats = dataObject.GetFormats();
							foreach (string text in formats)
							{
								if (ShouldWeCopySystemClipboardObjectToLocalClipboard(text))
								{
									object data;
									try
									{
										data = dataObject.GetData(text, autoConvert: true);
									}
									finally
									{
									}
									s_clipboard.SetData(text, data);
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
			return s_clipboard;
		}
		set
		{
			if (s_useSystemClipboard)
			{
				try
				{
					System.Windows.Clipboard.SetDataObject(value);
					s_clipboardNum = User32.GetClipboardSequenceNumber();
				}
				catch (Exception)
				{
				}
			}
			s_clipboard = value;
		}
	}

	protected ICommandService CommandService => m_commandService;

	protected IContextRegistry ContextRegistry => m_contextRegistry;

	public static event EventHandler Copying;

	public static event EventHandler Copied;

	public static event EventHandler Pasted;

	public static event EventHandler Deleting;

	public static event EventHandler Deleted;

	[ImportingConstructor]
	public StandardEditCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(StandardCommand.EditCut, StandardMenu.Edit, StandardCommandGroup.EditCut, "Cut".Localize("Cut the selection and place it on the clipboard"), "Cut the selection and place it on the clipboard".Localize(), Keys.X | Keys.Control, Sce.Atf.Resources.CutImage, CommandVisibility.Menu, this);
		m_commandService.RegisterCommand(StandardCommand.EditCopy, StandardMenu.Edit, StandardCommandGroup.EditCut, "Copy".Localize("Copy the selection and place it on the clipboard"), "Copy the selection and place it on the clipboard".Localize(), Keys.C | Keys.Control, Sce.Atf.Resources.CopyImage, CommandVisibility.Menu, this);
		m_commandService.RegisterCommand(StandardCommand.EditPaste, StandardMenu.Edit, StandardCommandGroup.EditCut, "Paste".Localize("Paste the contents of the clipboard and make that the new selection"), "Paste the contents of the clipboard and make that the new selection".Localize(), Keys.V | Keys.Control, Sce.Atf.Resources.PasteImage, CommandVisibility.Menu, this);
		m_commandService.RegisterCommand(StandardCommand.EditDelete, StandardMenu.Edit, StandardCommandGroup.EditCut, "Delete".Localize("Delete the selection"), "Delete the selection".Localize(), Keys.Delete, Sce.Atf.Resources.DeleteImage, CommandVisibility.Menu, this);
	}

	public bool CanPaste()
	{
		return m_contextRegistry.GetActiveContext<IInstancingContext>()?.CanInsert(Clipboard) ?? false;
	}

	public bool CanCopy()
	{
		return m_contextRegistry.GetActiveContext<IInstancingContext>()?.CanCopy() ?? false;
	}

	public bool CanDelete()
	{
		return m_contextRegistry.GetActiveContext<IInstancingContext>()?.CanDelete() ?? false;
	}

	public void Copy()
	{
		IInstancingContext activeContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
		if (activeContext != null && activeContext.CanCopy())
		{
			object obj = activeContext.Copy();
			IDataObject clipboard = (obj as IDataObject) ?? new DataObject(obj);
			OnCopying(EventArgs.Empty);
			Clipboard = clipboard;
			OnCopied(EventArgs.Empty);
		}
	}

	public void Cut()
	{
		Copy();
		Delete("Cut".Localize("Cut the selection"));
	}

	public void Delete()
	{
		Delete("Delete".Localize());
	}

	public void Paste()
	{
		IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
		if (instancingContext != null && instancingContext.CanInsert(Clipboard))
		{
			ITransactionContext context = instancingContext.As<ITransactionContext>();
			context.DoTransaction(delegate
			{
				instancingContext.Insert(Clipboard);
			}, CommandInfo.EditPaste.MenuText);
			OnPasted(EventArgs.Empty);
		}
	}

	bool ICommandClient.CanDoCommand(object tag)
	{
		bool result = false;
		IInstancingContext activeContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
		if (!(tag is StandardCommand))
		{
			return false;
		}
		switch ((StandardCommand)tag)
		{
		case StandardCommand.EditCut:
			result = activeContext != null && activeContext.CanCopy() && activeContext.CanDelete();
			break;
		case StandardCommand.EditDelete:
			result = activeContext?.CanDelete() ?? false;
			break;
		case StandardCommand.EditCopy:
			result = activeContext?.CanCopy() ?? false;
			break;
		case StandardCommand.EditPaste:
			result = activeContext != null && CanPaste();
			break;
		}
		return result;
	}

	void ICommandClient.DoCommand(object tag)
	{
		if (tag is StandardCommand)
		{
			switch ((StandardCommand)tag)
			{
			case StandardCommand.EditCut:
				Cut();
				break;
			case StandardCommand.EditDelete:
				Delete();
				break;
			case StandardCommand.EditCopy:
				Copy();
				break;
			case StandardCommand.EditPaste:
				Paste();
				break;
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object clicked)
	{
		ISelectionContext selectionContext = context.As<ISelectionContext>();
		IInstancingContext instancingContext = context.As<IInstancingContext>();
		if (selectionContext != null && instancingContext != null)
		{
			return new object[4]
			{
				StandardCommand.EditCut,
				StandardCommand.EditCopy,
				StandardCommand.EditPaste,
				StandardCommand.EditDelete
			};
		}
		return EmptyEnumerable<object>.Instance;
	}

	protected virtual void OnCopying(EventArgs e)
	{
		StandardEditCommands.Copying.Raise(this, e);
	}

	protected virtual void OnCopied(EventArgs e)
	{
		StandardEditCommands.Copied.Raise(this, e);
	}

	protected virtual void OnPasted(EventArgs e)
	{
		StandardEditCommands.Pasted.Raise(this, e);
	}

	protected virtual void OnDeleting(EventArgs e)
	{
		StandardEditCommands.Deleting.Raise(this, e);
	}

	protected virtual void OnDeleted(EventArgs e)
	{
		StandardEditCommands.Deleted.Raise(this, e);
	}

	private void Delete(string commandName)
	{
		OnDeleting(EventArgs.Empty);
		IInstancingContext instancingContext = m_contextRegistry.GetActiveContext<IInstancingContext>();
		if (instancingContext != null && instancingContext.CanDelete())
		{
			ITransactionContext context = instancingContext.As<ITransactionContext>();
			context.DoTransaction(delegate
			{
				instancingContext.Delete();
				instancingContext.As<ISelectionContext>()?.Clear();
			}, commandName);
		}
		OnDeleted(EventArgs.Empty);
	}

	[Conditional("CLIPBOARD_LOGGING")]
	private static void LogClipboardFormat(string format, bool start)
	{
		if (s_clipboardLoggingData == null)
		{
			s_clipboardLoggingData = new ClipboardLoggingData();
		}
		string text = string.Format("{0} Format: {1}", start ? "[BEG]" : "[END]", format);
		if (!s_clipboardLoggingData.AddFormat(text))
		{
			return;
		}
		try
		{
			using FileStream stream = File.Open(s_clipboardLoggingData.LogFile, FileMode.Append, FileAccess.Write, FileShare.Write);
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.WriteLine(text);
			streamWriter.Flush();
		}
		catch (Exception ex)
		{
			ex.ToString();
		}
	}

	static StandardEditCommands()
	{
		s_clipboard = new DataObject();
		s_clipboardFormatsToIgnore = new string[7] { "EnhancedMetafile", "Link Source", "Hyperlink", "MetaFilePict", "Embed Source", "Link Source Descriptor", "ObjectLink" };
		s_clipboardAcceptableFormats = new string[8]
		{
			DataFormats.FileDrop,
			DataFormats.CommaSeparatedValue,
			DataFormats.Html,
			DataFormats.OemText,
			DataFormats.Rtf,
			DataFormats.Serializable,
			DataFormats.Text,
			DataFormats.UnicodeText
		};
		s_clipboardNum = User32.GetClipboardSequenceNumber() - 1;
	}

	private static bool ShouldWeCopySystemClipboardObjectToLocalClipboard(string dataFormat)
	{
		if (!UseSystemClipboard)
		{
			return false;
		}
		if (AcceptableSystemClipboardFormats.Count > 0 && AcceptableSystemClipboardFormats.Contains(dataFormat))
		{
			return true;
		}
		return !SystemClipboardFormatsToIgnore.Contains(dataFormat);
	}
}
