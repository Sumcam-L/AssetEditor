using System;
using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.AssetPreviewing;

public class SlotSelectorKnob : IValueKnob<string>, IValueKnobBase, IKnob, IEntityFilterProvider, IToolKnob
{
	private readonly IPreviewerEntityLoadingService PreviewLoaderService;

	private readonly IPreviewerSlotInfo SlotInfo;

	private readonly IXLPClass XLPClass;

	private readonly IPreviewContext PreviewContext;

	private readonly string SlotName;

	private string m_currentValue;

	public bool CanClear => SlotInfo.AllowsNull;

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
			m_currentValue = value;
			using (new Firaxis.Utility.WaitCursor())
			{
				PreviewContext.PreviewWindow.ClearSlot(SlotInfo.SlotID, delayBind: true);
				if (!string.IsNullOrEmpty(m_currentValue))
				{
					IInstanceEntity instanceEntity = PreviewLoaderService.LoadEntity(m_currentValue, XLPClass.InstanceType);
					if (instanceEntity != null)
					{
						PreviewContext.PreviewWindow.OpenAsset(instanceEntity, SlotInfo.SlotID, delayBind: true);
					}
					else
					{
						Outputs.WriteLine(OutputMessageType.Error, $"Failed to load entity {m_currentValue} for slot {SlotInfo.SlotID}");
						m_currentValue = string.Empty;
					}
				}
				PreviewContext.PreviewWindow.BindAssetsToWindow();
			}
		}
	}

	public bool IsReadOnly => SlotInfo.IsSettable;

	public string Name => SlotName;

	public string GroupName => string.Empty;

	public string SubgroupName => "Preview Assets";

	public string CategoryName => string.Empty;

	public string Label => SlotName;

	public string ToolTip => SlotName;

	public KnobType KnobType => KnobType.KT_VALUE_ENTITY;

	public event EventHandler HasUpdateEvent;

	public SlotSelectorKnob(IPreviewerEntityLoadingService loaderSvc, IPreviewerSlotInfo slotInfo, IXLPClass xlpClass, IPreviewContext previewContext)
	{
		PreviewLoaderService = loaderSvc;
		XLPClass = xlpClass;
		PreviewContext = previewContext;
		SlotInfo = slotInfo;
		SlotName = $"{SlotInfo.XLPClass} {SlotInfo.SlotID}";
		m_currentValue = SlotInfo.DefaultAsset;
	}

	public Type GetValueType()
	{
		return typeof(string);
	}

	public void SetUIValue(string value)
	{
	}
}
