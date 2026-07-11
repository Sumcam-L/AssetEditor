using System;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using Firaxis.Reflection;

namespace Firaxis.AssetBrowser.ViewModels;

public class AssetTypeViewModel : SelectableItemViewModel, IEquatable<AssetTypeViewModel>
{
	private string _name;

	private string _imageUri;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (!(_name == value))
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}
	}

	public string ImageUri
	{
		get
		{
			return _imageUri;
		}
		set
		{
			if (!(_imageUri == value))
			{
				_imageUri = value;
				OnPropertyChanged("ImageUri");
			}
		}
	}

	public InstanceType InstanceType { get; set; }

	public AssetTypeViewModel(InstanceType type, string imageUri)
		: this(type, imageUri, isSel: false)
	{
	}

	public AssetTypeViewModel(InstanceType type, string imageUri, bool isSel)
		: base(isSel)
	{
		Name = GetNameFromType(type);
		ImageUri = imageUri;
		InstanceType = type;
	}

	private string GetNameFromType(InstanceType type)
	{
		return ReflectionHelper.GetDisplayName(type);
	}

	public bool Equals(AssetTypeViewModel other)
	{
		return other != null && Name == other.Name;
	}
}
