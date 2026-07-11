using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Behaviors;

public static class PasswordBoxBehavior
{
	public static readonly DependencyProperty BoundPassword = DependencyProperty.RegisterAttached("BoundPassword", typeof(SecureString), typeof(PasswordBoxBehavior), new PropertyMetadata(null, OnBoundPasswordChanged));

	public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBoxBehavior), new PropertyMetadata(false, OnBindPasswordChanged));

	private static readonly DependencyProperty UpdatingPassword = DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxBehavior), new PropertyMetadata(false));

	private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		PasswordBox passwordBox = d as PasswordBox;
		if (d != null && GetBindPassword(d))
		{
			passwordBox.PasswordChanged -= HandlePasswordChanged;
			SecureString securePassword = (SecureString)e.NewValue;
			if (!GetUpdatingPassword(passwordBox))
			{
				passwordBox.Password = ConvertToUnsecureString(securePassword);
			}
			passwordBox.PasswordChanged += HandlePasswordChanged;
		}
	}

	private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
	{
		if (dp is PasswordBox passwordBox)
		{
			bool flag = (bool)e.OldValue;
			bool flag2 = (bool)e.NewValue;
			if (flag)
			{
				passwordBox.PasswordChanged -= HandlePasswordChanged;
			}
			if (flag2)
			{
				passwordBox.PasswordChanged += HandlePasswordChanged;
			}
		}
	}

	private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
	{
		PasswordBox passwordBox = sender as PasswordBox;
		SetUpdatingPassword(passwordBox, value: true);
		SetBoundPassword(passwordBox, passwordBox.SecurePassword);
		SetUpdatingPassword(passwordBox, value: false);
	}

	public static void SetBindPassword(DependencyObject dp, bool value)
	{
		dp.SetValue(BindPassword, value);
	}

	public static bool GetBindPassword(DependencyObject dp)
	{
		return (bool)dp.GetValue(BindPassword);
	}

	public static SecureString GetBoundPassword(DependencyObject dp)
	{
		return (SecureString)dp.GetValue(BoundPassword);
	}

	public static void SetBoundPassword(DependencyObject dp, SecureString value)
	{
		dp.SetValue(BoundPassword, value);
	}

	private static bool GetUpdatingPassword(DependencyObject dp)
	{
		return (bool)dp.GetValue(UpdatingPassword);
	}

	private static void SetUpdatingPassword(DependencyObject dp, bool value)
	{
		dp.SetValue(UpdatingPassword, value);
	}

	private static string ConvertToUnsecureString(SecureString securePassword)
	{
		if (securePassword == null)
		{
			return null;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}
}
