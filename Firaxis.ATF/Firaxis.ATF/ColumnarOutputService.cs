using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Firaxis.Controls;
using Firaxis.Threading;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;


namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(ICategorizedOutputWriter))]
[Export(typeof(ColumnarOutputService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ColumnarOutputService : IControlHostClient, ICommandContext, ICommandClient, ICategorizedOutputWriter, IInitializable
{
	public enum OutputCommandGroups
	{
		VisibilityCommands,
		VerbosityCommands
	}

	private enum ColumnarOutputCommands
	{
		ShowTextOutput,
		ShowGridOutput,
		ToggleVerboseToolOutput,
		ToggleEngineLogOutput
	}

	private const int kMaxOutputLines = 1000000;

	private const int kLinesToRemoveAtMax = 10000;

	private const int kMaxTextLength = 4194304;

	private const int kTextToRemoveAtMax = 262144;

	public static CommandInfo ShowTextOutput;

	public static CommandInfo ShowGridOutput;

	public static CommandInfo ToggleVerboseToolOutput;

	public static CommandInfo ToggleEngineLogOutput;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService;

	private uint m_nextOutputTick;

	private ReaderWriterLockSlim m_logQueueLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private List<OutputMessageData> m_logQueue = new List<OutputMessageData>();

	private IQuietTimeAction m_logFlusher;

	private BindingList<OutputMessageData> m_gridData = new BindingList<OutputMessageData>();

	private const int kMaxLogItemsPerOutputUpdate = 100;

	private const uint kMinimumOutputQuietPeriodMS = 100u;

	private readonly IControlHostService m_controlHostService;

	private readonly ISettingsService m_settingsService;

	private readonly CommandControl m_outputControl;

	private readonly DataGridView m_gridOutputControl;

	private string m_textOutputData = string.Empty;

	private readonly RichTextBox m_textOutputControl;

	private Color m_textForeColor = SystemColors.ControlText;

	public bool VerboseToolOutputEnabled { get; set; }

	public bool VerboseEngineOutputEnabled { get; set; }

	public ICommandClient CommandClient => this;

	public IEnumerable<CommandInfo> Commands => new CommandInfo[4] { ShowTextOutput, ShowGridOutput, ToggleVerboseToolOutput, ToggleEngineLogOutput };

	static ColumnarOutputService()
	{
		ShowTextOutput = new CommandInfo(ColumnarOutputCommands.ShowTextOutput, StandardMenu.Window, OutputCommandGroups.VisibilityCommands, "Output\\Show Text Output".Localize(), "Show standard text output window".Localize("Show text output"), Sce.Atf.Input.Keys.None, Resources.ShowTextOutput, CommandVisibility.ControlDefault);
		ShowGridOutput = new CommandInfo(ColumnarOutputCommands.ShowGridOutput, StandardMenu.Window, OutputCommandGroups.VisibilityCommands, "Output\\Show Grid Output".Localize(), "Show grid based categorized output window".Localize("Show grid output"), Sce.Atf.Input.Keys.None, Resources.ShowGridOutput, CommandVisibility.ControlDefault);
		ToggleVerboseToolOutput = new CommandInfo(ColumnarOutputCommands.ToggleVerboseToolOutput, StandardMenu.Window, OutputCommandGroups.VerbosityCommands, "Output\\Toggle Verbose Tool Output".Localize(), "Toggle verbose tool output".Localize("Toggle verbose tool output"), Sce.Atf.Input.Keys.None, Resources.ToggleVerboseToolOuput, CommandVisibility.ControlDefault);
		ToggleEngineLogOutput = new CommandInfo(ColumnarOutputCommands.ToggleEngineLogOutput, StandardMenu.Window, OutputCommandGroups.VerbosityCommands, "Output\\Toggle Engine Log Output".Localize(), "Toggle verbose engine log output".Localize("Toggle engine log output"), Sce.Atf.Input.Keys.None, Resources.ToggleVerboseEngineOuput, CommandVisibility.ControlDefault);
	}

	[ImportingConstructor]
	public ColumnarOutputService(IControlHostService controlHostService, ISettingsService settingsService)
	{
		m_controlHostService = controlHostService;
		m_settingsService = settingsService;
		m_textOutputControl = new RichTextBox();
		m_textOutputControl.Dock = DockStyle.Fill;
		m_textOutputControl.Multiline = true;
		m_textOutputControl.ScrollBars = RichTextBoxScrollBars.Both;
		m_textOutputControl.WordWrap = false;
		m_textOutputControl.ReadOnly = true;
		m_textOutputControl.MouseUp += TextOutputControl_MouseUp;
		m_textOutputControl.BorderStyle = BorderStyle.None;
		m_textOutputControl.ForeColorChanged += TextOutputControl_ForeColorChanged;
		m_gridOutputControl = new DataGridView();
		m_gridOutputControl.Dock = DockStyle.Fill;
		m_gridOutputControl.BorderStyle = BorderStyle.None;
		m_gridOutputControl.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		m_gridOutputControl.ReadOnly = true;
		m_gridOutputControl.RowHeadersVisible = false;
		m_gridOutputControl.AllowUserToResizeRows = false;
		m_gridOutputControl.AllowUserToAddRows = false;
		m_gridOutputControl.AllowUserToDeleteRows = false;
		m_gridOutputControl.AllowUserToResizeColumns = true;
		m_gridData.AllowNew = true;
		m_gridData.AllowRemove = false;
		m_gridData.AllowEdit = false;
		m_gridOutputControl.ColumnCount = 4;
		m_gridOutputControl.Columns[0].Name = "Type";
		m_gridOutputControl.Columns[0].DataPropertyName = "Type";
		m_gridOutputControl.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
		m_gridOutputControl.Columns[1].Name = "Time";
		m_gridOutputControl.Columns[1].DataPropertyName = "Time";
		m_gridOutputControl.Columns[1].DefaultCellStyle.Format = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
		m_gridOutputControl.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
		m_gridOutputControl.Columns[2].Name = "Category";
		m_gridOutputControl.Columns[2].DataPropertyName = "Category";
		m_gridOutputControl.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
		m_gridOutputControl.Columns[3].Name = "Message";
		m_gridOutputControl.Columns[3].DataPropertyName = "Message";
		m_gridOutputControl.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		m_gridOutputControl.AutoGenerateColumns = false;
		m_gridOutputControl.DataSource = m_gridData;
		m_gridOutputControl.CellFormatting += GridOutputControl_CellFormatting;
		m_outputControl = new CommandControl();
		m_outputControl.Dock = DockStyle.Fill;
		_ = m_outputControl.Handle;
		_ = m_gridOutputControl.Handle;
		_ = m_textOutputControl.Handle;
		m_outputControl.ChildControls.Add(m_textOutputControl);
		m_outputControl.ChildControls.Add(m_gridOutputControl);
		m_outputControl.Bind(this);
		m_textOutputControl.Visible = false;
		m_logFlusher = new QuietTimeAction(125, delegate
		{
			WriteQueueOutput();
		});
	}

	private void TextOutputControl_ForeColorChanged(object sender, EventArgs e)
	{
		m_textForeColor = m_textOutputControl.ForeColor;
	}

	private void GridOutputControl_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		OutputMessageData outputMessageData = m_gridData[e.RowIndex];
		DataGridViewCell dataGridViewCell = m_gridOutputControl[e.ColumnIndex, e.RowIndex];
		Color messageTypeColor = GetMessageTypeColor(outputMessageData.Type);
		if (dataGridViewCell.Style.ForeColor != messageTypeColor)
		{
			dataGridViewCell.Style.ForeColor = messageTypeColor;
			dataGridViewCell.Style.SelectionForeColor = messageTypeColor;
		}
	}

	private void TextOutputControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (e.Button == System.Windows.Forms.MouseButtons.Right)
		{
			Point p = new Point(e.X, e.Y);
			List<object> commandTags = new List<object>(GetPopupCommandTags(this, null));
			Point screenPoint = m_textOutputControl.PointToScreen(p);
			m_commandService.RunContextMenu(commandTags, screenPoint);
		}
	}

	public virtual void Initialize()
	{
		RegisterUserSettings();
		m_controlHostService.RegisterControl(m_outputControl, "Output".Localize("title of Output window"), "View errors, warnings, and informative messages".Localize(), StandardControlGroup.Bottom, this);
		RegisterCommands();
		ApplySkinToOutputControl();
	}

	public void Write(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (type >= OutputMessageType.Diagnostic || verbosity == OutputMessageVerbosity.ExtremelyVerbose)
		{
			return;
		}
		bool flag = IsToolOutput(category);
		if ((!flag || VerboseToolOutputEnabled || verbosity == OutputMessageVerbosity.Normal) && (flag || VerboseEngineOutputEnabled))
		{
			QueueLogOutput(category, type, verbosity, message);
			if (TimeForMoreOutput())
			{
				WriteQueueOutput();
			}
		}
	}

	public void Clear()
	{
		m_outputControl.SafeBeginInvoke(delegate
		{
			m_gridData.Clear();
			if (!m_textOutputControl.IsDisposed)
			{
				m_textOutputControl.Clear();
			}
		});
	}

	public void Activate(Control control)
	{
		if (m_commandService != null)
		{
			m_commandService.SetActiveClient(this);
		}
	}

	public void Deactivate(Control control)
	{
		if (m_commandService != null)
		{
			m_commandService.SetActiveClient(null);
		}
	}

	public bool Close(Control control)
	{
		return true;
	}

	private void RegisterCommands()
	{
		if (m_commandService != null)
		{
			m_commandService.RegisterCommand(StandardCommand.EditCopy, CommandVisibility.All, this);
			m_commandService.RegisterCommand(StandardCommand.EditSelectAll, CommandVisibility.Menu, this);
			m_commandService.RegisterCommand(ShowTextOutput, this);
			m_commandService.RegisterCommand(ShowGridOutput, this);
			m_commandService.RegisterCommand(ToggleVerboseToolOutput, this);
			m_commandService.RegisterCommand(ToggleEngineLogOutput, this);
		}
	}

	public bool CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand standardCommand)
		{
			switch (standardCommand)
			{
			case StandardCommand.EditCopy:
				result = m_textOutputControl.Focused && m_textOutputControl.SelectionLength > 0;
				break;
			case StandardCommand.EditSelectAll:
				result = m_textOutputControl.Focused && m_textOutputControl.TextLength > 0;
				break;
			}
		}
		else if (commandTag is ColumnarOutputCommands)
		{
			switch ((ColumnarOutputCommands)commandTag)
			{
			case ColumnarOutputCommands.ShowGridOutput:
				result = true;
				break;
			case ColumnarOutputCommands.ShowTextOutput:
				result = true;
				break;
			case ColumnarOutputCommands.ToggleVerboseToolOutput:
			case ColumnarOutputCommands.ToggleEngineLogOutput:
				result = true;
				break;
			}
		}
		return result;
	}

	public void DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand standardCommand)
		{
			switch (standardCommand)
			{
			case StandardCommand.EditCopy:
				m_textOutputControl.Copy();
				break;
			case StandardCommand.EditSelectAll:
				m_textOutputControl.SelectAll();
				break;
			}
		}
		else if (commandTag is ColumnarOutputCommands)
		{
			switch ((ColumnarOutputCommands)commandTag)
			{
			case ColumnarOutputCommands.ShowGridOutput:
				m_gridOutputControl.Visible = true;
				m_textOutputControl.Visible = false;
				break;
			case ColumnarOutputCommands.ShowTextOutput:
				m_gridOutputControl.Visible = false;
				m_textOutputControl.Visible = true;
				break;
			case ColumnarOutputCommands.ToggleVerboseToolOutput:
				VerboseToolOutputEnabled = !VerboseToolOutputEnabled;
				break;
			case ColumnarOutputCommands.ToggleEngineLogOutput:
				VerboseEngineOutputEnabled = !VerboseEngineOutputEnabled;
				break;
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
		if (commandTag is ColumnarOutputCommands)
		{
			switch ((ColumnarOutputCommands)commandTag)
			{
			case ColumnarOutputCommands.ShowGridOutput:
				UpdateVisibilityCommandState(m_gridOutputControl, state);
				break;
			case ColumnarOutputCommands.ShowTextOutput:
				UpdateVisibilityCommandState(m_textOutputControl, state);
				break;
			case ColumnarOutputCommands.ToggleVerboseToolOutput:
				UpdateCommandStateChecked(VerboseToolOutputEnabled, state);
				break;
			case ColumnarOutputCommands.ToggleEngineLogOutput:
				UpdateCommandStateChecked(VerboseEngineOutputEnabled, state);
				break;
			}
		}
	}

	public IEnumerable<object> GetPopupCommandTags(object target, object clicked)
	{
		return new object[6]
		{
			StandardCommand.EditCopy,
			StandardCommand.EditSelectAll,
			ColumnarOutputCommands.ShowGridOutput,
			ColumnarOutputCommands.ShowTextOutput,
			ColumnarOutputCommands.ToggleEngineLogOutput,
			ColumnarOutputCommands.ToggleVerboseToolOutput
		};
	}

	private void EnqueueQuietTimeFlush()
	{
		m_logFlusher?.UpdateLastChangeTime();
	}

	private bool TimeForMoreOutput()
	{
		return NativeMethods.GetTickCount() >= m_nextOutputTick;
	}

	private void UpdateNextOutputTime()
	{
		m_nextOutputTick = NativeMethods.GetTickCount() + 100;
	}

	private void WriteQueueOutput()
	{
		OutputMessageData[] messages = new OutputMessageData[0];
		using (ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_logQueueLock))
		{
			int num = Math.Min(m_logQueue.Count, 100);
			if (num > 0)
			{
				messages = m_logQueue.GetRange(0, num).ToArray();
				using (new ScopedWriterLock(upgrableReadLock))
				{
					m_logQueue.RemoveRange(0, num);
				}
			}
		}
		WriteLogMessages(messages);
		UpdateNextOutputTime();
		EnqueueQuietTimeFlush();
	}

	private void QueueLogOutput(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		using (new ScopedWriterLock(m_logQueueLock))
		{
			m_logQueue.Add(new OutputMessageData(DateTime.Now, category, type, verbosity, message));
		}
	}

	private void WriteLogMessages(OutputMessageData[] messages)
	{
		if (messages.Length != 0)
		{
			m_outputControl.SafeBeginInvoke(delegate
			{
				WriteLogMessageImpl(messages);
			});
		}
	}

	private void WriteLogMessageImpl(OutputMessageData[] messages)
	{
		bool clearSelection = m_gridData.Count == 0;
		for (int i = 0; i < messages.Length; i++)
		{
			OutputMessageData item = messages[i];
			m_gridData.Add(item);
			WriteToText(item.Category, item.Type, item.Verbosity, item.Message);
		}
		AdvanceRowsIfAppropriate(clearSelection);
	}

	private bool IsToolOutput(string category)
	{
		if (!category.Equals("Tool") && !category.Equals("FireFX") && !category.Equals("Previewer"))
		{
			return category.Equals("Cooker");
		}
		return true;
	}

	private void RegisterUserSettings()
	{
		BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => VerboseToolOutputEnabled, "Enable Verbose Tool Output", "Output".Localize(), "If true, verbose output from the tool will be displayed in the output window".Localize());
		BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => VerboseEngineOutputEnabled, "Enable Verbose Engine Output", "Output".Localize(), "If true, verbose output from the engine will be displayed in the output window".Localize());
		m_settingsService.RegisterSettings("Application", boundPropertyDescriptor);
		m_settingsService.RegisterUserSettings("Application", boundPropertyDescriptor);
		m_settingsService.RegisterSettings("Application", boundPropertyDescriptor2);
		m_settingsService.RegisterUserSettings("Application", boundPropertyDescriptor2);
	}

	private void UpdateVisibilityCommandState(Control ctl, CommandState state)
	{
		if (!ctl.IsDisposed && ctl.IsHandleCreated)
		{
			UpdateCommandStateChecked(ctl.Visible, state);
		}
	}

	private void UpdateCommandStateChecked(bool chk, CommandState state)
	{
		if (state.Check != chk)
		{
			state.Check = chk;
		}
	}

	private string GetOutputString(OutputMessageType type, string message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (type)
		{
		case OutputMessageType.Diagnostic:
			stringBuilder.AppendFormat("{0,-5}", "DIAG");
			break;
		case OutputMessageType.Error:
			stringBuilder.AppendFormat("{0,-5}", "ERROR");
			break;
		case OutputMessageType.Warning:
			stringBuilder.AppendFormat("{0,-5}", "WARN");
			break;
		case OutputMessageType.Bug:
			stringBuilder.AppendFormat("{0,-5}", "BUG");
			break;
		default:
			stringBuilder.AppendFormat("{0,-5}", "INFO");
			break;
		}
		stringBuilder.AppendFormat(" {0:HH:mm:ss}Z", DateTime.Now.ToUniversalTime());
		uint num = (FiraxisATFRegistry.AssetPreviewerService?.AssetPreviewer?.FrameNumber).GetValueOrDefault() % 65535;
		stringBuilder.AppendFormat("({0:X5})", num);
		stringBuilder.AppendFormat(" {0}", message);
		return stringBuilder.ToString();
	}

	private void WriteToText(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (!m_textOutputControl.IsDisposed)
		{
			Color red;
			switch (type)
			{
			case OutputMessageType.Diagnostic:
				return;
			case OutputMessageType.Error:
				red = Color.Red;
				break;
			}
			red = ((type != OutputMessageType.Warning) ? m_textForeColor : Color.DarkOrange);
			string outputString = GetOutputString(type, message);
			m_textOutputData += outputString;
			m_textOutputControl.SelectionColor = red;
			m_textOutputControl.AppendText(outputString);
			if (m_textOutputData.Length >= 4194304)
			{
				m_textOutputData = m_textOutputData.Substring(262144);
				m_textOutputControl.Select(0, 262144);
				m_textOutputControl.Cut();
				m_textOutputControl.SelectionStart = m_textOutputData.Length;
				m_textOutputControl.SelectionLength = 0;
			}
		}
	}

	private void ApplySkinToOutputControl()
	{
		SkinService.ApplyActiveSkin(m_outputControl);
		foreach (DataGridViewColumn column in m_gridOutputControl.Columns)
		{
			SkinService.ApplyActiveSkin(column.HeaderCell);
			SkinService.ApplyActiveSkin(column.DefaultCellStyle);
		}
	}

	private void AdvanceRowsIfAppropriate(bool clearSelection)
	{
		if (m_gridOutputControl.IsDisposed)
		{
			return;
		}
		if (clearSelection)
		{
			m_gridOutputControl.ClearSelection();
		}
		if (m_gridOutputControl.Visible && m_gridOutputControl.SelectedRows.Count <= 0 && m_gridOutputControl.RowCount != 0 && m_gridOutputControl.Height != 0)
		{
			DataGridViewCell dataGridViewCell = m_gridOutputControl[0, m_gridOutputControl.RowCount - 1];
			if (dataGridViewCell != null)
			{
				m_gridOutputControl.FirstDisplayedCell = dataGridViewCell;
			}
		}
	}

	private Color GetMessageTypeColor(OutputMessageType type)
	{
		return type switch
		{
			OutputMessageType.Error => Color.FromArgb(255, 255, 90, 90), 
			OutputMessageType.Warning => Color.DarkOrange, 
			_ => m_gridOutputControl.DefaultCellStyle.ForeColor, 
		};
	}
}
