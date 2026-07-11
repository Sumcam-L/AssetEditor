using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(IOutputWriter))]
[Export(typeof(ErrorDialogService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ErrorDialogService : IOutputWriter, IPartImportsSatisfiedNotification
{
	[Import(AllowDefault = true)]
	private IWin32Window m_owner;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	private ErrorDialog m_errorDialog;

	private readonly HashSet<string> m_suppressedMessages = new HashSet<string>();

	public string SuppressedMessages
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string suppressedMessage in m_suppressedMessages)
			{
				stringBuilder.Append(suppressedMessage);
				stringBuilder.Append(Environment.NewLine);
			}
			return stringBuilder.ToString();
		}
		set
		{
			m_suppressedMessages.Clear();
			string[] array = value.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string item in array2)
			{
				m_suppressedMessages.Add(item);
			}
		}
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		if (m_settingsService == null)
		{
		}
	}

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (type == OutputMessageType.Error || type == OutputMessageType.Warning)
		{
			ShowError(message, message, type);
		}
	}

	public void Clear()
	{
	}

	private void ShowError(string messageId, string message, OutputMessageType messageType)
	{
		messageId = messageId.Replace(Environment.NewLine, string.Empty);
		if (m_suppressedMessages.Contains(messageId))
		{
			return;
		}
		(m_owner as Control).InvokeIfRequired(delegate
		{
			if (m_errorDialog == null)
			{
				m_errorDialog = new ErrorDialog();
				m_errorDialog.StartPosition = FormStartPosition.CenterScreen;
				m_errorDialog.SuppressMessageClicked += errorDialog_SuppressMessageClicked;
				m_errorDialog.FormClosed += errorDialog_FormClosed;
			}
			if (messageType == OutputMessageType.Error)
			{
				m_errorDialog.Text = "Error!".Localize();
			}
			else if (messageType == OutputMessageType.Warning)
			{
				m_errorDialog.Text = "Warning".Localize();
			}
			else if (messageType == OutputMessageType.Info)
			{
				m_errorDialog.Text = "Info".Localize();
			}
			m_errorDialog.MessageId = messageId;
			m_errorDialog.Message = message;
			m_errorDialog.Visible = false;
			m_errorDialog.Show(m_owner);
		});
	}

	private void errorDialog_SuppressMessageClicked(object sender, EventArgs e)
	{
		if (m_errorDialog.SuppressMessage)
		{
			m_suppressedMessages.Add(m_errorDialog.MessageId);
		}
	}

	private void errorDialog_FormClosed(object sender, FormClosedEventArgs e)
	{
		m_errorDialog.Dispose();
		m_errorDialog = null;
	}
}
