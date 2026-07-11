using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public class SizeBasedTemplateSelector : DataTemplateSelector
{
	private readonly Dictionary<ContentPresenter, TemplateSize> m_elementSizeDictionary = new Dictionary<ContentPresenter, TemplateSize>();

	public ObservableCollection<TemplateSize> TemplateSizes { get; set; }

	public SizeBasedTemplateSelector()
	{
		TemplateSizes = new ObservableCollection<TemplateSize>();
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (container is ContentPresenter contentPresenter)
		{
			TemplateSize templateSize = FindTemplateBySize(contentPresenter.ActualWidth, contentPresenter.ActualHeight);
			if (!m_elementSizeDictionary.ContainsKey(contentPresenter))
			{
				contentPresenter.SizeChanged += TemplateContainerSizeChanged;
				contentPresenter.Unloaded += RemoveContentControl;
				m_elementSizeDictionary.Add(contentPresenter, templateSize);
			}
			else
			{
				m_elementSizeDictionary[contentPresenter] = templateSize;
			}
			return templateSize.DataTemplate;
		}
		return null;
	}

	private void RemoveContentControl(object sender, RoutedEventArgs e)
	{
		if (sender is ContentPresenter contentPresenter)
		{
			contentPresenter.SizeChanged += TemplateContainerSizeChanged;
			contentPresenter.Unloaded += RemoveContentControl;
			m_elementSizeDictionary.Remove(contentPresenter);
		}
	}

	private TemplateSize FindTemplateBySize(double actualWidth, double actualHeight)
	{
		foreach (TemplateSize templateSize in TemplateSizes)
		{
			if (templateSize.IsRightSize(actualWidth, actualHeight))
			{
				return templateSize;
			}
		}
		return TemplateSizes.First();
	}

	private void TemplateContainerSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (sender is ContentPresenter contentPresenter && m_elementSizeDictionary.TryGetValue(contentPresenter, out var value) && !value.IsRightSize(contentPresenter.ActualWidth, contentPresenter.ActualHeight))
		{
			contentPresenter.ContentTemplateSelector = null;
			contentPresenter.ContentTemplate = SelectTemplate(contentPresenter.DataContext, contentPresenter);
		}
	}
}
