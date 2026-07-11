using System;
using System.Collections.Generic;
using Firaxis.Asset.Trigger;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class TimelineSet : ITimelineSet, ITriggerSystem, ITagProvider, IServiceProviderProvider
{
	public TriggerCollection Triggers { get; private set; }

	public TriggerTrackCollection Tracks { get; private set; }

	public Factory<Firaxis.Asset.Trigger.ITrigger> TriggerFactory { get; private set; }

	public IDictionary<TriggerType, IEnumerable<string>> TriggerTypeToValidClassesMapping { get; private set; }

	public object Tag { get; set; }

	public IServiceProvider ServiceProvider { get; set; }

	public TimelineSet()
	{
		Triggers = new TriggerCollection();
		Tracks = new TriggerTrackCollection();
		TriggerFactory = new Factory<Firaxis.Asset.Trigger.ITrigger>();
		TriggerTypeToValidClassesMapping = new Dictionary<TriggerType, IEnumerable<string>>();
		InitializeTriggerFactory(null);
	}

	public void InitializeTriggerFactory(IDictionary<TriggerType, IEnumerable<string>> triggerTypeToValidClassesMapping)
	{
		if (triggerTypeToValidClassesMapping != null)
		{
			TriggerTypeToValidClassesMapping = triggerTypeToValidClassesMapping;
		}
		else
		{
			TriggerTypeToValidClassesMapping.Clear();
		}
		TriggerFactory.Clear();
		TriggerFactory.Add(new TriggerMaker<TriggerSound>(TriggerType.TT_SOUND, this));
		TriggerFactory.Add(new TriggerMaker<TriggerAssetVFX>(TriggerType.TT_ASSET_VFX, this));
		TriggerFactory.Add(new TriggerMaker<TriggerArtDefVFX>(TriggerType.TT_ARTDEF_VFX, this));
		TriggerFactory.Add(new TriggerMaker<TriggerTransfer>(TriggerType.TT_TRANSFER, this));
		TriggerFactory.Add(new TriggerMaker<TriggerAction>(TriggerType.TT_ACTION, this));
		TriggerFactory.Add(new TriggerMaker<TriggerLight>(TriggerType.TT_LIGHT, this));
	}

	public T GetService<T>()
	{
		if (ServiceProvider != null)
		{
			return (T)ServiceProvider.GetService(typeof(T));
		}
		return default(T);
	}
}
