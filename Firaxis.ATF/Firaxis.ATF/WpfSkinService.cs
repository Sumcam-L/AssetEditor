using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Firaxis.MVVMBase;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(WpfSkinService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WpfSkinService : IWpfSkinService
{
	private ISkinService SkinService { get; set; }

	private HashSet<WeakKey<FrameworkElement>> SkinnedObjects { get; set; } = new HashSet<WeakKey<FrameworkElement>>();

	private ResourceDictionary SkinnedResources { get; } = new ResourceDictionary();

	[ImportingConstructor]
	public WpfSkinService(ISkinService skinService)
	{
		SkinService = skinService;
		Context.Add(this);
		SkinService.SkinChangedOrApplied += UpdateActiveSkin;
		SkinToWpfSkinConverter.ConvertFrom(SkinService.ActiveSkin, SkinnedResources);
	}

	public void ApplySkin(FrameworkElement target)
	{
		if (target != null)
		{
			WeakKey<FrameworkElement> item = new WeakKey<FrameworkElement>(target);
			if (SkinnedObjects.Add(item))
			{
				target.Resources.MergedDictionaries.Add(SkinnedResources);
			}
		}
	}

	public void RemoveSkin(FrameworkElement target)
	{
		if (target != null)
		{
			WeakKey<FrameworkElement> item = new WeakKey<FrameworkElement>(target);
			if (SkinnedObjects.Remove(item))
			{
				target.Resources.MergedDictionaries.Remove(SkinnedResources);
			}
		}
	}

	private void UpdateActiveSkin(object sender, EventArgs e)
	{
		SkinnedObjects.RemoveWhere((WeakKey<FrameworkElement> weakKey) => !weakKey.IsAlive);
		SkinnedResources.Clear();
		SkinToWpfSkinConverter.ConvertFrom(SkinService.ActiveSkin, SkinnedResources);
	}
}
