using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Sce.Atf.Applications;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Models;

[Export(typeof(IMenuItem))]
[Export(typeof(IToolBarItem))]
internal class CommandItem : NotifyPropertyChangedBase, ICommandItem, IMenuItem, INotifyPropertyChanged, ICommand, IToolBarItem
{
	private bool m_isChecked;

	private object m_imageSourceKey;

	private readonly CommandVisibility m_visibility;

	private List<InputBinding> m_inputBindings = new List<InputBinding>();

	private readonly CommandInfo m_info;

	private readonly Func<ICommandItem, bool> m_canExecuteMethod;

	private readonly Action<ICommandItem> m_executeMethod;

	private string m_text;

	private string m_description;

	private static readonly char[] s_pathDelimiters = new char[2] { '/', '\\' };

	private static readonly PropertyChangedEventArgs s_textArgs = ObservableUtil.CreateArgs((CommandItem x) => x.Text);

	private static readonly PropertyChangedEventArgs s_descriptionArgs = ObservableUtil.CreateArgs((CommandItem x) => x.Description);

	private static readonly PropertyChangedEventArgs s_shortcutArgs = ObservableUtil.CreateArgs((CommandItem x) => x.Shortcuts);

	private static readonly PropertyChangedEventArgs s_imageSourceKeyArgs = ObservableUtil.CreateArgs((CommandItem x) => x.ImageSourceKey);

	private static readonly PropertyChangedEventArgs s_isCheckedArgs = ObservableUtil.CreateArgs((CommandItem x) => x.IsChecked);

	public ComposablePart ComposablePart { get; set; }

	public object CommandTag => m_info.CommandTag;

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			m_isChecked = value;
			OnPropertyChanged(s_isCheckedArgs);
		}
	}

	public object ImageSourceKey
	{
		get
		{
			return m_imageSourceKey;
		}
		set
		{
			m_imageSourceKey = value;
			OnPropertyChanged(s_imageSourceKeyArgs);
		}
	}

	public IEnumerable<Keys> Shortcuts
	{
		get
		{
			return m_info.Shortcuts;
		}
		set
		{
			m_info.Shortcuts = value;
		}
	}

	public CommandVisibility Visibility => m_visibility;

	public int Index => m_info.Index;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
			OnPropertyChanged(s_textArgs);
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
			OnPropertyChanged(s_descriptionArgs);
		}
	}

	public bool IsVisible => this.IsVisible(CommandVisibility.Menu);

	public object MenuTag => m_info.MenuTag;

	public object GroupTag => m_info.GroupTag;

	public IEnumerable<string> MenuPath { get; private set; }

	object IToolBarItem.Tag => CommandTag;

	object IToolBarItem.ToolBarTag => MenuTag;

	bool IToolBarItem.IsVisible => this.IsVisible(CommandVisibility.Toolbar);

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (m_canExecuteMethod != null)
			{
				CommandManager.RequerySuggested += value;
			}
		}
		remove
		{
			if (m_canExecuteMethod != null)
			{
				CommandManager.RequerySuggested -= value;
			}
		}
	}

	public CommandItem(CommandInfo info, Func<ICommandItem, bool> canExecuteMethod, Action<ICommandItem> executeMethod)
	{
		m_info = info;
		m_text = info.DisplayedMenuText;
		MenuPath = ParseMenuPath(info.MenuText);
		m_description = info.Description;
		m_imageSourceKey = FixUpLegacyImage(info.ImageKey);
		info.ShortcutsChanged += Info_ShortcutsChanged;
		m_visibility = ((info.MenuTag != null) ? info.Visibility : CommandVisibility.None);
		m_canExecuteMethod = canExecuteMethod;
		m_executeMethod = executeMethod;
		RebuildBinding();
	}

	public bool CanExecute(object parameter)
	{
		return m_canExecuteMethod == null || m_canExecuteMethod(this);
	}

	public void Execute(object parameter)
	{
		m_executeMethod(this);
	}

	private void Info_ShortcutsChanged(object sender, EventArgs e)
	{
		RebuildBinding();
		OnPropertyChanged(s_shortcutArgs);
	}

	private IEnumerable<string> ParseMenuPath(string menuText)
	{
		if (!string.IsNullOrEmpty(menuText))
		{
			string[] array = ((menuText[0] != '@') ? menuText.Split(s_pathDelimiters, 8) : new string[1] { menuText.Substring(1, menuText.Length - 1) });
			if (array.Length > 1)
			{
				string[] array2 = new string[array.Length - 1];
				Array.Copy(array, array2, array.Length - 1);
				return array2;
			}
		}
		return EmptyArray<string>.Instance;
	}

	private void RebuildBinding()
	{
		Window mainWindow = Application.Current.MainWindow;
		if (mainWindow == null)
		{
			return;
		}
		foreach (InputBinding inputBinding2 in m_inputBindings)
		{
			mainWindow.InputBindings.Remove(inputBinding2);
		}
		m_inputBindings = new List<InputBinding>();
		foreach (Keys shortcut in Shortcuts)
		{
			if (shortcut != Keys.None)
			{
				KeyGesture gesture = ToWpfKeyGesture(shortcut);
				InputBinding inputBinding = new InputBinding(this, gesture);
				m_inputBindings.Add(inputBinding);
				mainWindow.InputBindings.Add(inputBinding);
			}
		}
	}

	private static object FixUpLegacyImage(object imageKey)
	{
		object result = imageKey;
		string text = imageKey as string;
		if (!string.IsNullOrEmpty(text))
		{
			object obj = Application.Current.TryFindResource(text);
			if (obj == null)
			{
				Image image = Sce.Atf.ResourceUtil.GetImage(text);
				if (image == null)
				{
					throw new InvalidOperationException("Could not find embedded image: " + text);
				}
				Util.GetOrCreateResourceForEmbeddedImage(image);
				result = image;
			}
		}
		return result;
	}

	private static KeyGesture ToWpfKeyGesture(Keys atfKeys)
	{
		ModifierKeys modifiers = Sce.Atf.Wpf.Interop.KeysInterop.ToWpfModifiers(atfKeys);
		Key key = Sce.Atf.Wpf.Interop.KeysInterop.ToWpf(atfKeys);
		return new KeyGesture(key, modifiers);
	}
}
