using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Firaxis.CivTech;
using Firaxis.MVVMBase.Helpers;

namespace Firaxis.MVVMBase;

[Serializable]
public abstract class BaseViewModel : IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>
{
	public static Dictionary<Type, Dictionary<string, HashSet<string>>> InternalDependencies = new Dictionary<Type, Dictionary<string, HashSet<string>>>();

	[NonSerialized]
	private List<IPropertyChangedDependency> PropertyChangedDependencies;

	[NonSerialized]
	protected bool _disposed;

	protected string _displayName;

	public virtual string DisplayName
	{
		get
		{
			return _displayName;
		}
		protected set
		{
			if (!(_displayName == value))
			{
				_displayName = value;
				OnPropertyChanged("DisplayName");
			}
		}
	}

	[field: NonSerialized]
	public event PropertyChangedEventHandler PropertyChanged;

	~BaseViewModel()
	{
		ApplicationHelper.BeginInvokeIfNeeded(InternalDispose);
	}

	private static void EstablishTypePropertyDependencies(Type t)
	{
		if (InternalDependencies.ContainsKey(t))
		{
			return;
		}
		Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
		PropertyInfo[] properties = t.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!dictionary.ContainsKey(propertyInfo.Name))
			{
				dictionary[propertyInfo.Name] = new HashSet<string> { propertyInfo.Name };
			}
			foreach (DependsOnAttribute customAttribute in propertyInfo.GetCustomAttributes<DependsOnAttribute>(inherit: true))
			{
				AddDependency(t, propertyInfo.Name, customAttribute.DependencyName, dictionary);
			}
		}
		InternalDependencies.Add(t, dictionary);
	}

	private static void AddDependency(Type t, string propertyName, string dependsUponPropertyName, Dictionary<string, HashSet<string>> establishedDependencies)
	{
		if (dependsUponPropertyName == propertyName)
		{
			return;
		}
		PropertyInfo property = t.GetProperty(dependsUponPropertyName);
		if (property != null)
		{
			if (!establishedDependencies.ContainsKey(property.Name))
			{
				establishedDependencies[dependsUponPropertyName] = new HashSet<string> { dependsUponPropertyName };
			}
			if (!establishedDependencies[dependsUponPropertyName].Add(propertyName))
			{
				return;
			}
			{
				foreach (DependsOnAttribute customAttribute in property.GetCustomAttributes<DependsOnAttribute>(inherit: true))
				{
					AddDependency(t, propertyName, customAttribute.DependencyName, establishedDependencies);
				}
				return;
			}
		}
		throw new ArgumentException($"Improper use of DependsOnAttribute in {t.Name}: {dependsUponPropertyName} is not a valid property.", "dependsUponPropertyName");
	}

	public void AddPropertyChangeDependency(Action<string> triggeredAction, params string[] triggeringPropertyNames)
	{
		if (PropertyChangedDependencies == null)
		{
			PropertyChangedDependencies = new List<IPropertyChangedDependency>();
		}
		for (int i = 0; i < PropertyChangedDependencies.Count; i++)
		{
			IPropertyChangedDependency propertyChangedDependency = PropertyChangedDependencies[i];
			if (propertyChangedDependency.AddToPropertyChangedDependency(triggeredAction, triggeringPropertyNames, out var resultingPropertyChangedDependency))
			{
				if (propertyChangedDependency != resultingPropertyChangedDependency)
				{
					PropertyChangedDependencies[i] = resultingPropertyChangedDependency;
				}
				return;
			}
		}
		PropertyChangedDependencies.Add(PropertyChangedDependency.GeneratePropertyChangedDependency(triggeredAction, triggeringPropertyNames));
	}

	public void AddPropertyChangeDependency(BaseViewModel dependentViewModel, string dependentProperty, params string[] propertiesChanged)
	{
		AddPropertyChangeDependency(delegate
		{
			dependentViewModel.OnPropertyChanged(dependentProperty);
		}, propertiesChanged);
	}

	public void RemovePropertyChangeDependency(Action<string> triggeredAction, params string[] triggeringPropertyNames)
	{
		if (PropertyChangedDependencies == null)
		{
			return;
		}
		for (int i = 0; i < PropertyChangedDependencies.Count; i++)
		{
			IPropertyChangedDependency propertyChangedDependency = PropertyChangedDependencies[i];
			if (propertyChangedDependency.RemoveFromPropertyChangedDependency(triggeredAction, triggeringPropertyNames, out var resultingPropertyChangedDependency))
			{
				if (resultingPropertyChangedDependency == null)
				{
					PropertyChangedDependencies.RemoveAt(i);
				}
				break;
			}
		}
	}

	public override string ToString()
	{
		return (!string.IsNullOrWhiteSpace(DisplayName)) ? DisplayName : base.ToString();
	}

	public virtual void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(propertyName, null);
	}

	public void OnPropertyChanged(string propertyName, HashSet<string> alreadyNotified)
	{
		Type type = GetType();
		if (alreadyNotified == null)
		{
			alreadyNotified = new HashSet<string>();
			if (!InternalDependencies.ContainsKey(type))
			{
				EstablishTypePropertyDependencies(type);
			}
		}
		if (!InternalDependencies[type].TryGetValue(propertyName, out var value))
		{
			BugSubmitter.SilentReport(string.Format("{0} is not a valid {1} of {2} @assign matthew.kelley", propertyName, "propertyName", type.FullName));
			return;
		}
		foreach (string item in value)
		{
			if (alreadyNotified.Add(item))
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(item));
			}
		}
		if (PropertyChangedDependencies == null)
		{
			return;
		}
		for (int i = 0; i < PropertyChangedDependencies.Count; i++)
		{
			IPropertyChangedDependency propertyChangedDependency = PropertyChangedDependencies[i];
			if (propertyChangedDependency.CanTrigger(value))
			{
				if (propertyChangedDependency.Action.Execute(propertyName))
				{
					continue;
				}
			}
			else if (propertyChangedDependency.Action.IsAlive)
			{
				continue;
			}
			PropertyChangedDependencies.RemoveAt(i);
			i--;
		}
	}

	public void OnSubPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void Dispose()
	{
		PropertyChangedDependencies = null;
		InternalDispose(disposeManaged: true);
		GC.SuppressFinalize(this);
	}

	private void InternalDispose()
	{
		InternalDispose(disposeManaged: false);
	}

	private void InternalDispose(bool disposeManaged)
	{
		if (!_disposed)
		{
			Dispose(disposeManaged);
			_disposed = true;
		}
	}

	protected virtual void Dispose(bool disposeManaged)
	{
	}

	public virtual int CompareTo(IViewModel other)
	{
		if (!string.IsNullOrEmpty(DisplayName))
		{
			return string.IsNullOrEmpty(other.DisplayName) ? 1 : string.Compare(DisplayName, other.DisplayName, StringComparison.Ordinal);
		}
		if (string.IsNullOrEmpty(other.DisplayName))
		{
			return 0;
		}
		return -1;
	}
}
