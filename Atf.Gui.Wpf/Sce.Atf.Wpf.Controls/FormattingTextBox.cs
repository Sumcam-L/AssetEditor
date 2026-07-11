using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Sce.Atf.Wpf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls;

public class FormattingTextBox : TextBox
{
	private enum LostFocusAction
	{
		None,
		Commit,
		Cancel
	}

	public static readonly DependencyProperty BeginCommandProperty;

	public static readonly DependencyProperty CancelCommandProperty;

	public static readonly DependencyProperty CommitCommandProperty;

	public static readonly DependencyProperty FinishEditingCommandProperty;

	public static readonly DependencyProperty LostFocusCommandProperty;

	public static readonly DependencyProperty TextChangedCommandProperty;

	public static readonly DependencyProperty UpdateCommandProperty;

	public static readonly DependencyProperty FormatProviderProperty;

	public static readonly DependencyProperty StringFormatProperty;

	public static readonly DependencyProperty ValueProperty;

	public static readonly DependencyProperty IsEditingProperty;

	private bool m_transactionOpen;

	private bool m_ignoreTextChanges;

	private LostFocusAction m_lostFocusAction;

	public ICommand BeginCommand
	{
		get
		{
			return (ICommand)GetValue(BeginCommandProperty);
		}
		set
		{
			SetValue(BeginCommandProperty, value);
		}
	}

	public ICommand CancelCommand
	{
		get
		{
			return (ICommand)GetValue(CancelCommandProperty);
		}
		set
		{
			SetValue(CancelCommandProperty, value);
		}
	}

	public ICommand CommitCommand
	{
		get
		{
			return (ICommand)GetValue(CommitCommandProperty);
		}
		set
		{
			SetValue(CommitCommandProperty, value);
		}
	}

	public ICommand FinishEditingCommand
	{
		get
		{
			return (ICommand)GetValue(FinishEditingCommandProperty);
		}
		set
		{
			SetValue(FinishEditingCommandProperty, value);
		}
	}

	public ICommand LostFocusCommand
	{
		get
		{
			return (ICommand)GetValue(LostFocusCommandProperty);
		}
		set
		{
			SetValue(LostFocusCommandProperty, value);
		}
	}

	public ICommand TextChangedCommand
	{
		get
		{
			return (ICommand)GetValue(TextChangedCommandProperty);
		}
		set
		{
			SetValue(TextChangedCommandProperty, value);
		}
	}

	public ICommand UpdateCommand
	{
		get
		{
			return (ICommand)GetValue(UpdateCommandProperty);
		}
		set
		{
			SetValue(UpdateCommandProperty, value);
		}
	}

	public IFormatProvider FormatProvider
	{
		get
		{
			return (IFormatProvider)GetValue(FormatProviderProperty);
		}
		set
		{
			SetValue(FormatProviderProperty, value);
		}
	}

	public string StringFormat
	{
		get
		{
			return (string)GetValue(StringFormatProperty);
		}
		set
		{
			SetValue(StringFormatProperty, value);
		}
	}

	public string Value
	{
		get
		{
			return (string)GetValue(ValueProperty);
		}
		set
		{
			SetValue(ValueProperty, value);
		}
	}

	public bool IsEditing
	{
		get
		{
			return (bool)GetValue(IsEditingProperty);
		}
		set
		{
			SetValue(IsEditingProperty, value);
		}
	}

	protected virtual void OnIsEditingChanged(DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			if (base.IsInitialized)
			{
				Focus();
			}
			else
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(SetFocus));
			}
		}
	}

	protected virtual void OnIsReadOnlyChanged(DependencyPropertyChangedEventArgs e)
	{
		CoerceValue(IsEditingProperty);
	}

	protected override void OnTextChanged(TextChangedEventArgs e)
	{
		base.OnTextChanged(e);
		if (!m_ignoreTextChanges)
		{
			Value = UnFormat(base.Text);
			ValueEditorUtil.ExecuteCommand(TextChangedCommand, this, base.Text);
			if (IsEditing)
			{
				m_lostFocusAction = LostFocusAction.Commit;
			}
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		bool handlesCommitKeys = ValueEditorUtil.GetHandlesCommitKeys(this);
		if (e.Key == Key.Return)
		{
			LostFocusAction lostFocusAction = m_lostFocusAction;
			m_lostFocusAction = LostFocusAction.None;
			bool flag = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0;
			if (lostFocusAction == LostFocusAction.Commit)
			{
				if (flag)
				{
					CommitChange();
				}
				else
				{
					UpdateChange();
				}
			}
			if (flag)
			{
				OnFinishEditing();
			}
			e.Handled |= handlesCommitKeys;
		}
		else if (e.Key == Key.Escape && IsEditing)
		{
			LostFocusAction lostFocusAction2 = m_lostFocusAction;
			m_lostFocusAction = LostFocusAction.None;
			if (lostFocusAction2 != LostFocusAction.None)
			{
				CancelChange();
			}
			OnFinishEditing();
			e.Handled |= handlesCommitKeys;
		}
		base.OnKeyDown(e);
	}

	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		if (!base.IsReadOnly)
		{
			IsEditing = true;
			SelectAll();
		}
		base.OnGotKeyboardFocus(e);
	}

	protected override void OnLostFocus(RoutedEventArgs e)
	{
		base.OnLostFocus(e);
		IsEditing = false;
		ValueEditorUtil.ExecuteCommand(LostFocusCommand, this, null);
		e.Handled = true;
	}

	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		InternalLostFocus();
	}

	protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnPreviewLostKeyboardFocus(e);
		InternalLostFocus();
	}

	static FormattingTextBox()
	{
		BeginCommandProperty = DependencyProperty.Register("BeginCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		CommitCommandProperty = DependencyProperty.Register("CommitCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		FinishEditingCommandProperty = DependencyProperty.Register("FinishEditingCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		LostFocusCommandProperty = DependencyProperty.Register("LostFocusCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		TextChangedCommandProperty = DependencyProperty.Register("TextChangedCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		UpdateCommandProperty = DependencyProperty.Register("UpdateCommand", typeof(ICommand), typeof(FormattingTextBox), new PropertyMetadata(null));
		FormatProviderProperty = DependencyProperty.Register("FormatProvider", typeof(IFormatProvider), typeof(FormattingTextBox), new UIPropertyMetadata(CultureInfo.InvariantCulture));
		StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(FormattingTextBox), new UIPropertyMetadata(null, StringFormatChanged));
		ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(FormattingTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValueChangedCallback, null, isAnimationProhibited: false, UpdateSourceTrigger.PropertyChanged));
		IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(FormattingTextBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, IsEditingChanged, CoerceIsEditing));
		TextBoxBase.IsReadOnlyProperty.OverrideMetadata(typeof(FormattingTextBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, IsReadOnlyChanged));
	}

	private static object CoerceIsEditing(DependencyObject target, object value)
	{
		if (target is FormattingTextBox formattingTextBox && (bool)value && formattingTextBox.IsReadOnly)
		{
			return false;
		}
		return value;
	}

	private static void IsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FormattingTextBox formattingTextBox)
		{
			formattingTextBox.OnIsEditingChanged(e);
		}
	}

	private void SetFocus()
	{
		if (base.Visibility == Visibility.Visible)
		{
			Focus();
		}
	}

	private static void StringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FormattingTextBox formattingTextBox)
		{
			formattingTextBox.UpdateTextFromValue();
		}
	}

	private static void IsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FormattingTextBox formattingTextBox)
		{
			formattingTextBox.OnIsReadOnlyChanged(e);
		}
	}

	private string UnFormat(string s)
	{
		if (StringFormat == null)
		{
			return s;
		}
		Regex regex = new Regex("(.*)(\\{0.*\\})(.*)");
		Match match = regex.Match(StringFormat);
		if (!match.Success)
		{
			return s;
		}
		if (match.Groups.Count > 1 && !string.IsNullOrEmpty(match.Groups[1].Value))
		{
			s = s.Replace(match.Groups[1].Value, "");
		}
		if (match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[3].Value))
		{
			s = s.Replace(match.Groups[3].Value, "");
		}
		return s;
	}

	private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FormattingTextBox { m_ignoreTextChanges: false } formattingTextBox)
		{
			formattingTextBox.UpdateTextFromValue();
		}
	}

	private void UpdateChange()
	{
		if (!m_transactionOpen)
		{
			ValueEditorUtil.ExecuteCommand(BeginCommand, this, null);
		}
		Value = UnFormat(base.Text);
		ValueEditorUtil.ExecuteCommand(UpdateCommand, this, null);
		m_transactionOpen = true;
		ValueEditorUtil.UpdateBinding(this, ValueProperty, UpdateBindingType.Target);
		UpdateTextFromValue();
	}

	private void UpdateTextFromValue()
	{
		m_ignoreTextChanges = true;
		string text = StringFormat;
		if (text != null)
		{
			if (!text.Contains("{0"))
			{
				text = $"{{0:{text}}}";
			}
			base.Text = string.Format(FormatProvider, text, new object[1] { Value });
		}
		else
		{
			base.Text = ((Value != null) ? string.Format(FormatProvider, "{0}", new object[1] { Value }) : null);
		}
		m_ignoreTextChanges = false;
	}

	private void OnFinishEditing()
	{
		ICommand finishEditingCommand = FinishEditingCommand;
		if (finishEditingCommand != null)
		{
			ValueEditorUtil.ExecuteCommand(finishEditingCommand, this, null);
		}
		else
		{
			MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}
	}

	private void CancelChange()
	{
		if (!m_transactionOpen)
		{
			ValueEditorUtil.ExecuteCommand(BeginCommand, this, null);
		}
		ValueEditorUtil.UpdateBinding(this, ValueProperty, updateSource: false);
		ValueEditorUtil.ExecuteCommand(CancelCommand, this, null);
		m_transactionOpen = false;
		ValueEditorUtil.UpdateBinding(this, ValueProperty, UpdateBindingType.Target);
		UpdateTextFromValue();
	}

	private void CommitChange()
	{
		if (!m_transactionOpen)
		{
			ValueEditorUtil.ExecuteCommand(BeginCommand, this, null);
		}
		ValueEditorUtil.UpdateBinding(this, ValueProperty, updateSource: false);
		ValueEditorUtil.ExecuteCommand(CommitCommand, this, null);
		m_transactionOpen = false;
		ValueEditorUtil.UpdateBinding(this, ValueProperty, UpdateBindingType.Target);
		UpdateTextFromValue();
	}

	private void InternalLostFocus()
	{
		LostFocusAction lostFocusAction = m_lostFocusAction;
		m_lostFocusAction = LostFocusAction.None;
		switch (lostFocusAction)
		{
		case LostFocusAction.Commit:
			CommitChange();
			break;
		case LostFocusAction.Cancel:
			CancelChange();
			break;
		}
	}
}
