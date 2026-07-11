using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Firaxis.MVVMBase;

public class MessageBoxViewModel : DialogViewModel
{
	private string _title;

	private ObservableCollection<string> _messages;

	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged("Title");
			}
		}
	}

	public ObservableCollection<string> Messages
	{
		get
		{
			return _messages;
		}
		set
		{
			if (_messages != value)
			{
				_messages = value;
				OnPropertyChanged("Messages");
			}
		}
	}

	public MessageBoxViewModel(string title, IEnumerable<string> messages)
	{
		Title = title;
		Messages = new ObservableCollection<string>(messages);
	}

	public MessageBoxViewModel(string title, string messages)
		: this(title, messages.Replace('\r', ' ').Split('\n'))
	{
	}
}
