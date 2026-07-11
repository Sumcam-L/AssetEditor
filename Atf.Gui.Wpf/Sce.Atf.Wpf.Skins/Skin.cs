using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Sce.Atf.Wpf.Skins;

public abstract class Skin
{
	private sealed class NullSkin : Skin
	{
		protected override void LoadResources()
		{
		}
	}

	public static readonly Skin Null = new NullSkin();

	private readonly List<ResourceDictionary> m_resources = new List<ResourceDictionary>();

	public string Name { get; private set; }

	protected List<ResourceDictionary> Resources => m_resources;

	public virtual void Load()
	{
		if (Resources.Count != 0)
		{
			return;
		}
		try
		{
			LoadResources();
		}
		catch (IOException)
		{
		}
		foreach (ResourceDictionary resource in Resources)
		{
			Application.Current.Resources.MergedDictionaries.Add(resource);
		}
	}

	public virtual void Unload()
	{
		foreach (ResourceDictionary resource in Resources)
		{
			Application.Current.Resources.MergedDictionaries.Remove(resource);
		}
		Resources.Clear();
	}

	protected Skin(string name)
	{
		Name = name;
	}

	protected abstract void LoadResources();

	private Skin()
	{
	}
}
