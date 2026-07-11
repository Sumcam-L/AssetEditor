using System;
using System.ComponentModel;

namespace SharpDX;

public abstract class ComponentBase : IComponent, INotifyPropertyChanged
{
	private string name;

	private readonly bool isNameImmutable;

	private object tag;

	[DefaultValue(null)]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (isNameImmutable)
			{
				throw new ArgumentException("Name property is immutable for this instance", "value");
			}
			if (!(name == value))
			{
				name = value;
				OnPropertyChanged("Name");
			}
		}
	}

	[DefaultValue(null)]
	[Browsable(false)]
	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			if (!object.ReferenceEquals(tag, value))
			{
				tag = value;
				OnPropertyChanged("Tag");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected ComponentBase()
	{
	}

	protected ComponentBase(string name)
	{
		if (name != null)
		{
			this.name = name;
			isNameImmutable = true;
		}
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
