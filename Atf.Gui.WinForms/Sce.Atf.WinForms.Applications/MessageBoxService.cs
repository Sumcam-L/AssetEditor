using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.WinForms.Applications;

[Export(typeof(IMessageBoxService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MessageBoxService : IMessageBoxService
{
	private readonly Dictionary<DialogResult, MessageBoxResult> m_resultMap = new Dictionary<DialogResult, MessageBoxResult>
	{
		{
			DialogResult.Cancel,
			MessageBoxResult.Cancel
		},
		{
			DialogResult.No,
			MessageBoxResult.No
		},
		{
			DialogResult.None,
			MessageBoxResult.None
		},
		{
			DialogResult.OK,
			MessageBoxResult.OK
		},
		{
			DialogResult.Yes,
			MessageBoxResult.Yes
		}
	};

	private readonly Dictionary<MessageBoxButton, MessageBoxButtons> m_buttonMap = new Dictionary<MessageBoxButton, MessageBoxButtons>
	{
		{
			MessageBoxButton.OK,
			MessageBoxButtons.OK
		},
		{
			MessageBoxButton.OKCancel,
			MessageBoxButtons.OKCancel
		},
		{
			MessageBoxButton.YesNo,
			MessageBoxButtons.YesNo
		},
		{
			MessageBoxButton.YesNoCancel,
			MessageBoxButtons.YesNoCancel
		}
	};

	private readonly Dictionary<MessageBoxImage, MessageBoxIcon> m_imageMap = new Dictionary<MessageBoxImage, MessageBoxIcon>
	{
		{
			MessageBoxImage.Error,
			MessageBoxIcon.Hand
		},
		{
			MessageBoxImage.Information,
			MessageBoxIcon.Asterisk
		},
		{
			MessageBoxImage.None,
			MessageBoxIcon.None
		},
		{
			MessageBoxImage.Question,
			MessageBoxIcon.Question
		},
		{
			MessageBoxImage.Exclamation,
			MessageBoxIcon.Exclamation
		}
	};

	public MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
	{
		Form parent = FindFirstVisibleForm();
		if (parent != null)
		{
			if (parent.InvokeRequired)
			{
				return (MessageBoxResult)parent.Invoke((Func<MessageBoxResult>)(() => m_resultMap[MessageBox.Show(parent, message, title, m_buttonMap[button], m_imageMap[image])]));
			}
			return m_resultMap[MessageBox.Show(parent, message, title, m_buttonMap[button], m_imageMap[image])];
		}
		return m_resultMap[MessageBox.Show(message, title, m_buttonMap[button], m_imageMap[image])];
	}

	private Form FindFirstVisibleForm()
	{
		return Application.OpenForms.OfType<Form>().FirstOrDefault((Form fod) => fod.Visible);
	}
}
