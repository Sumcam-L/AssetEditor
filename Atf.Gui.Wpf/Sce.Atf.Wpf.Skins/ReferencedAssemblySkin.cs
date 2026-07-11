using System;
using System.Windows;

namespace Sce.Atf.Wpf.Skins;

public sealed class ReferencedAssemblySkin : Skin
{
	public Uri Uri { get; private set; }

	public ReferencedAssemblySkin(string name, Uri resourceUri)
		: base(name)
	{
		Uri = resourceUri;
	}

	protected override void LoadResources()
	{
		ResourceDictionary item = (ResourceDictionary)Application.LoadComponent(Uri);
		base.Resources.Add(item);
	}
}
