using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IOutputWriter))]
[Export(typeof(ErrorDialogService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ErrorDialogService : IOutputWriter, IPartImportsSatisfiedNotification
{
	private HashSet<string> m_suppressedMessages = new HashSet<string>();

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
	}

	public void Write(OutputMessageType type, string message)
	{
		if (type == OutputMessageType.Error || type == OutputMessageType.Warning)
		{
			ShowError(message, message);
		}
	}

	public void Clear()
	{
	}

	private void ShowError(string messageId, string message)
	{
		messageId = messageId.Replace(Environment.NewLine, string.Empty);
		if (m_suppressedMessages.Contains(messageId))
		{
			return;
		}
		Application.Current.Dispatcher.BeginInvokeIfRequired(delegate
		{
			if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
			{
				ErrorDialog errorDialog = new ErrorDialog();
				ErrorDialogViewModel errorDialogViewModel = new ErrorDialogViewModel
				{
					Message = message
				};
				errorDialog.DataContext = errorDialogViewModel;
				errorDialog.Owner = Application.Current.MainWindow;
				errorDialog.ShowDialog();
				if (errorDialogViewModel.SuppressMessage)
				{
					m_suppressedMessages.Add(messageId);
				}
			}
		});
	}

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		throw new NotImplementedException();
	}
}
