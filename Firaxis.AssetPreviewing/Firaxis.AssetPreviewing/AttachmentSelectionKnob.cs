using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetPreviewing;

public class AttachmentSelectionKnob : IValueKnob<string>, IValueKnobBase, IKnob, IEntityFilterProvider, IToolKnob
{
	private readonly IPreviewerEntityLoadingService PreviewLoaderService;

	private readonly IPreviewerSlotInfo SlotInfo;

	private readonly IXLPClass XLPClass;

	private readonly IPreviewContext PreviewContext;

	private readonly IClassSet EntityClasses;

	private readonly string AttachmentName;

	private static readonly string kSplitCharacter = ";";

	private static readonly char[] kSplitCharacterArray = new char[1] { ';' };

	private string m_currentValue = string.Empty;

	public bool CanClear => true;

	public IEnumerable<InstanceType> AllowedTypes
	{
		get
		{
			yield return XLPClass.InstanceType;
		}
	}

	public IEnumerable<string> AllowedClasses => XLPClass.AllowedEntityClasses;

	public string Value
	{
		get
		{
			return m_currentValue;
		}
		set
		{
			if (m_currentValue == value)
			{
				return;
			}
			using (new Firaxis.Utility.WaitCursor())
			{
				IBehaviorInstance behaviorInstance = PreviewLoaderService.InstanceSet.Push<IBehaviorInstance>(Guid.NewGuid().ToString());
				IAssetInstance assetInstance = PreviewContext.EntityAdapter.InstanceEntity.As<IAssetInstance>();
				assetInstance.Flatten(PreviewLoaderService.InstanceSet, EntityClasses, behaviorInstance);
				IAttachmentPoint attachmentPoint = behaviorInstance.AttachmentPointSet.FindByName(AttachmentName);
				if (attachmentPoint == null)
				{
					Outputs.WriteLine(OutputMessageType.Error, $"Failed to find attachment {AttachmentName} for slot {SlotInfo.SlotID}");
					PreviewLoaderService.InstanceSet.Remove(behaviorInstance);
					return;
				}
				IEnumerable<string> attachmentOverrides = PreviewContext.PreviewWindow.GetAttachmentOverrides(AttachmentName, InstanceType.IT_ASSET, SlotInfo.SlotID);
				if (string.IsNullOrEmpty(value))
				{
					m_currentValue = string.Empty;
				}
				IList<string> list = new List<string>(m_currentValue.Split(kSplitCharacterArray, StringSplitOptions.RemoveEmptyEntries));
				if (!string.IsNullOrEmpty(value) && !list.Contains(value, StringComparer.CurrentCultureIgnoreCase))
				{
					list.Add(value);
				}
				foreach (string item in attachmentOverrides.Except(list))
				{
					PreviewContext.PreviewWindow.RemoveAttachment(AttachmentName, item, InstanceType.IT_ASSET, SlotInfo.SlotID, delayBind: true);
				}
				foreach (string item2 in list.Except(attachmentOverrides))
				{
					IInstanceEntity instanceEntity = PreviewLoaderService.LoadEntity(item2, InstanceType.IT_ASSET);
					if (instanceEntity != null)
					{
						AttachmentPointData attachmentData = new AttachmentPointData(item2, InstanceType.IT_ASSET, attachmentPoint);
						PreviewContext.PreviewWindow.AddAttachment(instanceEntity, attachmentData, SlotInfo.SlotID, delayBind: true);
					}
					else
					{
						Outputs.WriteLine(OutputMessageType.Error, $"Failed to load entity {item2} as attachment for slot {SlotInfo.SlotID} and attachment {AttachmentName}");
					}
				}
				m_currentValue = string.Join(kSplitCharacter, list);
				PreviewContext.PreviewWindow.BindAssetsToWindow();
				PreviewLoaderService.InstanceSet.Remove(behaviorInstance);
			}
		}
	}

	public bool IsReadOnly => SlotInfo.IsSettable;

	public string Name => $"AttachmentKnob:{AttachmentName}";

	public string GroupName => string.Empty;

	public string SubgroupName => "Attached Assets";

	public string CategoryName => string.Empty;

	public string Label => AttachmentName;

	public string ToolTip => $"Specify additional entities to preview in attachment slot \"{AttachmentName}\"";

	public KnobType KnobType => KnobType.KT_VALUE_ENTITY;

	public event EventHandler HasUpdateEvent;

	public AttachmentSelectionKnob(string attName, IClassSet classes, IPreviewerEntityLoadingService loaderSvc, IPreviewerSlotInfo slotInfo, IXLPClass xlpClass, IPreviewContext previewContext)
	{
		PreviewLoaderService = loaderSvc;
		PreviewContext = previewContext;
		SlotInfo = slotInfo;
		XLPClass = xlpClass;
		EntityClasses = classes;
		AttachmentName = attName;
	}

	public Type GetValueType()
	{
		return typeof(string);
	}

	public void SetUIValue(string value)
	{
	}
}
