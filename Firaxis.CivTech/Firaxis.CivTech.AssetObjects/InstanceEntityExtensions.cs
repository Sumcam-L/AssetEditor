using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.Packages;
using Firaxis.Collections;

namespace Firaxis.CivTech.AssetObjects;

public static class InstanceEntityExtensions
{
	public static void CrawlEntityTimelineDependencies(this IInstanceEntity entity, TriggerType triggerType, InstanceType instanceType, Action<InstanceType, string> entityCrawler)
	{
		IEnumerable<ITimeline> source = (entity as IBehaviorDataProvider)?.Timelines?.Timelines ?? Enumerable.Empty<ITimeline>();
		(from assetTrigger in source.SelectMany((ITimeline timeline) => timeline.Triggers)
			where assetTrigger.Type == triggerType
			select assetTrigger.FXName).ForEach(delegate(string name)
		{
			entityCrawler(instanceType, name);
		});
	}

	public static void CrawlAttachmentDependencies(this IInstanceEntity entity, IProjectConfig projCfg, Action<InstanceType, string> entityCrawler)
	{
		IEnumerable<IAttachmentPoint> enumerable = (entity as IBehaviorDataProvider)?.AttachmentPointSet?.Items ?? Enumerable.Empty<IAttachmentPoint>();
		BugSubmitter.SilentAssert(entityCrawler != null, "Invalid entity crawler functor! @assign bwhitman");
		foreach (IAttachmentPoint item in enumerable)
		{
			foreach (IBLPEntryValue cookParam in item.CookParameters.Items.OfType<IBLPEntryValue>())
			{
				if (!string.IsNullOrEmpty(cookParam.EntryName))
				{
					IXLPClass iXLPClass = projCfg.XLPClasses.Items.FirstOrDefault((IXLPClass xlpCls) => xlpCls.Name == cookParam.XLPClass);
					if (iXLPClass != null)
					{
						entityCrawler?.Invoke(iXLPClass.InstanceType, cookParam.EntryName);
					}
				}
			}
		}
	}

	public static IEnumerable<Uri> GetDataFileURIs(this IInstanceEntity entity)
	{
		ICollection<Uri> collection = new List<Uri>();
		IEnumerable<string> enumerable = from df in entity.DataFiles
			where !string.IsNullOrEmpty(df.RelativePath)
			select entity.GetDataFilePath(df.RelativePath);
		foreach (string item in enumerable)
		{
			collection.Add(new Uri(item));
		}
		return collection;
	}
}
