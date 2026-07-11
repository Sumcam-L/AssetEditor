using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IOutputWriter))]
[Export(typeof(OutputService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OutputService : IControlHostClient, ICommandClient, IOutputWriter, IInitializable
{
	private readonly CommandInfo[] m_commandsToDeactivate = new CommandInfo[5]
	{
		CommandInfo.EditCut,
		CommandInfo.EditDelete,
		CommandInfo.EditPaste,
		CommandInfo.EditDeselectAll,
		CommandInfo.EditInvertSelection
	};

	private readonly IControlHostService m_controlHostService;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService;

	private readonly RichTextBox m_textBox;

	public Font Font
	{
		get
		{
			return m_textBox.Font;
		}
		set
		{
			Font font = m_textBox.Font;
			m_textBox.Font = value;
			font?.Dispose();
		}
	}

	public Color BackColor
	{
		get
		{
			return m_textBox.BackColor;
		}
		set
		{
			m_textBox.BackColor = value;
		}
	}

	public RichTextBox TextBox => m_textBox;

	[ImportingConstructor]
	public OutputService(IControlHostService controlHostService)
	{
		m_controlHostService = controlHostService;
		m_textBox = new RichTextBox();
		m_textBox.Multiline = true;
		m_textBox.ScrollBars = RichTextBoxScrollBars.Both;
		m_textBox.WordWrap = false;
		m_textBox.ReadOnly = true;
		m_textBox.MouseUp += textBox_MouseUp;
		m_textBox.BorderStyle = BorderStyle.None;
		IntPtr handle = m_textBox.Handle;
	}

	public virtual void Initialize()
	{
		m_controlHostService.RegisterControl(m_textBox, "Output".Localize("title of Output window"), "View errors, warnings, and informative messages".Localize(), StandardControlGroup.Bottom, this);
		RegisterCommands();
	}

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		OutputMessage(type, verbosity, message);
	}

	public void Write(OutputMessageType type, string formatString, params object[] args)
	{
		string formatString2 = string.Format(formatString, args);
		Write(type, formatString2);
	}

	public void Clear()
	{
		if (m_textBox.InvokeRequired)
		{
			m_textBox.BeginInvoke(new ThreadStart(Clear));
			return;
		}
		m_textBox.Text = string.Empty;
		m_textBox.ScrollToCaret();
	}

	protected virtual void OutputMessage(OutputMessageType messageType, OutputMessageVerbosity verbosity, string message, RichTextBox textBox)
	{
		if (verbosity == OutputMessageVerbosity.Normal)
		{
			Color color;
			string value;
			switch (messageType)
			{
			case OutputMessageType.Diagnostic:
				return;
			case OutputMessageType.Error:
				color = Color.Red;
				value = "Error".Localize("Label for error message");
				break;
			case OutputMessageType.Warning:
				color = Color.DarkOrange;
				value = "Warning".Localize("Label for warning message");
				break;
			default:
				color = textBox.ForeColor;
				value = "Info".Localize("Label for informative message");
				break;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value);
			stringBuilder.Append(": ");
			stringBuilder.Append(message);
			if (textBox.SelectionColor != color)
			{
				textBox.SelectionColor = color;
			}
			textBox.AppendText(stringBuilder.ToString());
		}
	}

	public void OutputMessage(OutputMessageType messageType, OutputMessageVerbosity verbosity, string message)
	{
		if (verbosity != OutputMessageVerbosity.Normal)
		{
			return;
		}
		if (m_textBox.InvokeRequired)
		{
			m_textBox.BeginInvoke(new Action<OutputMessageType, OutputMessageVerbosity, string>(OutputMessage), messageType, verbosity, message);
		}
		else if (!m_textBox.IsDisposed)
		{
			if (m_textBox.TextLength > 1048576)
			{
				m_textBox.Text = string.Empty;
			}
			OutputMessage(messageType, verbosity, message, m_textBox);
			try
			{
				m_textBox.ScrollToCaret();
			}
			catch (Exception)
			{
			}
		}
	}

	public void Activate(Control control)
	{
		if (m_commandService == null)
		{
			return;
		}
		m_commandService.SetActiveClient(this);
		CommandInfo[] commandsToDeactivate = m_commandsToDeactivate;
		foreach (CommandInfo commandInfo in commandsToDeactivate)
		{
			if (commandInfo.CommandService != null)
			{
				m_commandService.RegisterCommand(commandInfo, this);
			}
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

	public bool CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand standardCommand)
		{
			switch (standardCommand)
			{
			case StandardCommand.EditCopy:
				result = m_textBox.Focused && m_textBox.SelectionLength > 0;
				break;
			case StandardCommand.EditSelectAll:
				result = m_textBox.Focused && m_textBox.TextLength > 0;
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
				m_textBox.Copy();
				break;
			case StandardCommand.EditSelectAll:
				m_textBox.SelectAll();
				break;
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public IEnumerable<object> GetPopupCommandTags(object target, object clicked)
	{
		return new object[2]
		{
			StandardCommand.EditCopy,
			StandardCommand.EditSelectAll
		};
	}

	private void RegisterCommands()
	{
		if (m_commandService != null)
		{
			m_commandService.RegisterCommand(StandardCommand.EditCopy, CommandVisibility.All, this);
			m_commandService.RegisterCommand(StandardCommand.EditSelectAll, CommandVisibility.Menu, this);
		}
	}

	private void textBox_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			Point p = new Point(e.X, e.Y);
			List<object> commandTags = new List<object>(GetPopupCommandTags(this, null));
			Point screenPoint = m_textBox.PointToScreen(p);
			m_commandService.RunContextMenu(commandTags, screenPoint);
		}
	}
}
