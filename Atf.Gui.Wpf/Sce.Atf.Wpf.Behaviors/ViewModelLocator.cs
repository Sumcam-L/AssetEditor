using System;
using System.Windows;

namespace Sce.Atf.Wpf.Behaviors;

public class ViewModelLocator
{
	public static readonly DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached("ViewModel", typeof(string), typeof(ViewModelLocator), new PropertyMetadata(string.Empty, OnViewModelChanged));

	public static readonly DependencyProperty SharedViewModelProperty = DependencyProperty.RegisterAttached("SharedViewModel", typeof(string), typeof(ViewModelLocator), new PropertyMetadata(null, OnSharedViewModelChanged));

	public static string GetViewModel(DependencyObject d)
	{
		return (string)d.GetValue(ViewModelProperty);
	}

	public static void SetViewModel(DependencyObject d, string value)
	{
		d.SetValue(ViewModelProperty, value);
	}

	private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		string vmContractName = (string)e.NewValue;
		FrameworkElement element = d as FrameworkElement;
		AttachViewModel(element, vmContractName, isShared: false);
	}

	public static string GetSharedViewModel(DependencyObject d)
	{
		return (string)d.GetValue(SharedViewModelProperty);
	}

	public static void SetSharedViewModel(DependencyObject d, string value)
	{
		d.SetValue(SharedViewModelProperty, value);
	}

	private static void OnSharedViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		string vmContractName = (string)e.NewValue;
		FrameworkElement element = d as FrameworkElement;
		AttachViewModel(element, vmContractName, isShared: true);
	}

	private static void AttachViewModel(FrameworkElement element, string vmContractName, bool isShared)
	{
		if (element == null)
		{
			throw new ArgumentException("Invalid element for attached property");
		}
		try
		{
			if (!string.IsNullOrEmpty(vmContractName))
			{
				ViewModelRepository.AttachViewModelToView(vmContractName, element, isShared);
			}
		}
		catch (Exception)
		{
		}
	}
}
