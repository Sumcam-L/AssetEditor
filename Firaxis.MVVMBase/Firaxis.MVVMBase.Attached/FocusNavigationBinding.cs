using System;
using System.Windows;
using System.Windows.Input;

namespace Firaxis.MVVMBase.Attached;

public class FocusNavigationBinding : Freezable
{
	public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(Key), typeof(FocusNavigationBinding), new PropertyMetadata(Key.None));

	public static readonly DependencyProperty ElementNameProperty = DependencyProperty.Register("ElementName", typeof(string), typeof(FocusNavigationBinding), new PropertyMetadata((object)null));

	public static readonly DependencyProperty CanNavigateProperty = DependencyProperty.Register("CanNavigate", typeof(Func<bool>), typeof(FocusNavigationBinding), new PropertyMetadata((object)null));

	public Key Key
	{
		get
		{
			return (Key)GetValue(KeyProperty);
		}
		set
		{
			SetValue(KeyProperty, value);
		}
	}

	public string ElementName
	{
		get
		{
			return (string)GetValue(ElementNameProperty);
		}
		set
		{
			SetValue(ElementNameProperty, value);
		}
	}

	public Func<bool> CanNavigate
	{
		get
		{
			return (Func<bool>)GetValue(CanNavigateProperty);
		}
		set
		{
			SetValue(CanNavigateProperty, value);
		}
	}

	public FocusNavigationBinding()
	{
	}

	public FocusNavigationBinding(Key key, string elementName, Func<bool> canNavigate)
	{
		Key = key;
		ElementName = elementName;
		CanNavigate = canNavigate;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new FocusNavigationBinding();
	}
}
