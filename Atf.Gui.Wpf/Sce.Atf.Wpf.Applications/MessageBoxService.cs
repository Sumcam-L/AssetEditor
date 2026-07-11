using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IMessageBoxService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MessageBoxService : IMessageBoxService
{
	private readonly Dictionary<System.Windows.MessageBoxResult, Sce.Atf.Applications.MessageBoxResult> m_resultMap = new Dictionary<System.Windows.MessageBoxResult, Sce.Atf.Applications.MessageBoxResult>
	{
		{
			System.Windows.MessageBoxResult.Cancel,
			Sce.Atf.Applications.MessageBoxResult.Cancel
		},
		{
			System.Windows.MessageBoxResult.No,
			Sce.Atf.Applications.MessageBoxResult.No
		},
		{
			System.Windows.MessageBoxResult.None,
			Sce.Atf.Applications.MessageBoxResult.None
		},
		{
			System.Windows.MessageBoxResult.OK,
			Sce.Atf.Applications.MessageBoxResult.OK
		},
		{
			System.Windows.MessageBoxResult.Yes,
			Sce.Atf.Applications.MessageBoxResult.Yes
		}
	};

	private readonly Dictionary<Sce.Atf.Applications.MessageBoxButton, System.Windows.MessageBoxButton> m_buttonMap = new Dictionary<Sce.Atf.Applications.MessageBoxButton, System.Windows.MessageBoxButton>
	{
		{
			Sce.Atf.Applications.MessageBoxButton.OK,
			System.Windows.MessageBoxButton.OK
		},
		{
			Sce.Atf.Applications.MessageBoxButton.OKCancel,
			System.Windows.MessageBoxButton.OKCancel
		},
		{
			Sce.Atf.Applications.MessageBoxButton.YesNo,
			System.Windows.MessageBoxButton.YesNo
		},
		{
			Sce.Atf.Applications.MessageBoxButton.YesNoCancel,
			System.Windows.MessageBoxButton.YesNoCancel
		}
	};

	private readonly Dictionary<Sce.Atf.Applications.MessageBoxImage, System.Windows.MessageBoxImage> m_imageMap = new Dictionary<Sce.Atf.Applications.MessageBoxImage, System.Windows.MessageBoxImage>
	{
		{
			Sce.Atf.Applications.MessageBoxImage.Error,
			System.Windows.MessageBoxImage.Hand
		},
		{
			Sce.Atf.Applications.MessageBoxImage.Information,
			System.Windows.MessageBoxImage.Asterisk
		},
		{
			Sce.Atf.Applications.MessageBoxImage.None,
			System.Windows.MessageBoxImage.None
		},
		{
			Sce.Atf.Applications.MessageBoxImage.Question,
			System.Windows.MessageBoxImage.Question
		},
		{
			Sce.Atf.Applications.MessageBoxImage.Exclamation,
			System.Windows.MessageBoxImage.Exclamation
		}
	};

	public Sce.Atf.Applications.MessageBoxResult Show(string message, string title, Sce.Atf.Applications.MessageBoxButton button, Sce.Atf.Applications.MessageBoxImage image)
	{
		return m_resultMap[WpfMessageBox.Show(message, title, m_buttonMap[button], m_imageMap[image])];
	}
}
