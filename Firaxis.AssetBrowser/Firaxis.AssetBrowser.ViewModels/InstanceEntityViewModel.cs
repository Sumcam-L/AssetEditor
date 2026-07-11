using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

public class InstanceEntityViewModel : SelectableItemViewModel
{
	private static readonly Dictionary<InstanceType, Type> ViewModelDictionary;

	private string _name;

	private string _type;

	private string _p4status;

	private DateTime _perforceModTime;

	private string _project;

	public string Name
	{
		get
		{
			return _name;
		}
		private set
		{
			if (_name != value)
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}
	}

	public string Type
	{
		get
		{
			return _type;
		}
		private set
		{
			if (_type != value)
			{
				_type = value;
				OnPropertyChanged("Type");
			}
		}
	}

	public string PerforceStatus
	{
		get
		{
			return _p4status;
		}
		set
		{
			_p4status = value;
			OnPropertyChanged("PerforceStatus");
		}
	}

	public DateTime PerforceModTime
	{
		get
		{
			return _perforceModTime;
		}
		set
		{
			if (_perforceModTime != value)
			{
				_perforceModTime = value;
				OnPropertyChanged("PerforceModTime");
			}
		}
	}

	public string Project
	{
		get
		{
			return _project;
		}
		private set
		{
			if (_project != value)
			{
				_project = value;
				OnPropertyChanged("Project");
			}
		}
	}

	public string ToolTip => $"Name: {Name}\nType: {Type}\nProject: {Project}\nPerforce Status: {PerforceStatus}\nLast Modified: {PerforceModTime.ToString()}";

	public InstanceType InstanceType { get; private set; }

	public IInstanceEntity Entity => LoadFunction?.Invoke(Name, InstanceType);

	private Func<string, InstanceType, IInstanceEntity> LoadFunction { get; set; } = null;

	protected ICivTechService CivTechService { get; set; }

	static InstanceEntityViewModel()
	{
		ViewModelDictionary = new Dictionary<InstanceType, Type>();
		ViewModelDictionary.Add(InstanceType.IT_ASSET, typeof(InstanceEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_MATERIAL, typeof(InstanceEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_GEOMETRY, typeof(GeometryInstanceViewModel));
		ViewModelDictionary.Add(InstanceType.IT_TEXTURE, typeof(TextureInstanceViewModel));
		ViewModelDictionary.Add(InstanceType.IT_ANIMATION, typeof(AnimationInstanceViewModel));
		ViewModelDictionary.Add(InstanceType.IT_DSG, typeof(InstanceEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_ANALYTIC_LIGHT, typeof(ImportedEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_ENVIRONMENT_LIGHT, typeof(ImportedEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_LIGHT_RIG, typeof(InstanceEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_PARTICLE_EFFECT, typeof(ImportedEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_BEHAVIOR, typeof(InstanceEntityViewModel));
		ViewModelDictionary.Add(InstanceType.IT_FIREFX, typeof(InstanceEntityViewModel));
	}

	public static InstanceEntityViewModel Create(InstanceType type, ICivTechService civTechService, string name, Func<string, InstanceType, IInstanceEntity> loadFunction)
	{
		object[] args = new object[4] { civTechService, name, type, loadFunction };
		Type type2 = ViewModelDictionary[type];
		return (InstanceEntityViewModel)Activator.CreateInstance(type2, args);
	}

	public InstanceEntityViewModel(ICivTechService civTechSvc, string name, InstanceType type, Func<string, InstanceType, IInstanceEntity> loadFunction)
	{
		Name = name;
		Type = EnumToStringConverter.GetNameFromType(type);
		InstanceType = type;
		CivTechService = civTechSvc;
		LoadFunction = loadFunction;
		string pantryPath = CivTechService.ProjectMapService.LayeredPantry.GetPantryPath(Name, InstanceType);
		if (!string.IsNullOrEmpty(pantryPath))
		{
			Uri uri = new Uri(pantryPath);
			string localPath = uri.LocalPath;
			if (File.Exists(localPath))
			{
				PerforceModTime = File.GetLastWriteTime(localPath);
			}
			Project = CivTechService.GetProjectName(uri);
		}
		else
		{
			Project = "N/A";
		}
	}

	public override string ToString()
	{
		return Name;
	}

	protected override void Dispose(bool disposeManaged)
	{
		LoadFunction = null;
	}
}
