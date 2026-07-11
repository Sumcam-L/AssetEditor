using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetPreviewing;

public class BehaviorKnobSetWrapper : IKnobSet, IDisposable, ISlottedKnobSet
{
	private IPreviewContext TargetPreviewContext;

	private IKnobSet TargetKnobSet;

	private IList<IKnob> SlotKnobs = new List<IKnob>();

	private IList<IKnob> AttachmentKnobs = new List<IKnob>();

	private IDictionary<string, IKnobSubgroup> m_localKnobsBySubGroup = new Dictionary<string, IKnobSubgroup>();

	public string KnobSetName => TargetKnobSet.KnobSetName;

	public IEnumerable<IKnob> Knobs
	{
		get
		{
			foreach (IKnob knob in TargetKnobSet.Knobs)
			{
				yield return knob;
			}
			foreach (IKnob slotKnob in SlotKnobs)
			{
				yield return slotKnob;
			}
			foreach (IKnob attachmentKnob in AttachmentKnobs)
			{
				yield return attachmentKnob;
			}
		}
	}

	public IDictionary<string, IKnobSubgroup> KnobsBySubgroup => m_localKnobsBySubGroup;

	public event EventHandler KnobSetChanged;

	public event EventHandler KnobSetCleared;

	public BehaviorKnobSetWrapper(IProjectConfig prjCfg, IPreviewerEntityLoadingService loaderSvc, IPreviewContext previewContext, IKnobSet targetKnobSet)
	{
		TargetPreviewContext = previewContext;
		TargetKnobSet = targetKnobSet;
		TargetKnobSet.KnobsBySubgroup.ForEach(delegate(KeyValuePair<string, IKnobSubgroup> kvp)
		{
			m_localKnobsBySubGroup[kvp.Key] = kvp.Value;
		});
		foreach (IPreviewerSlotInfo slotInfo in TargetPreviewContext.PreviewWindow.GetSlotsInfo())
		{
			IXLPClass iXLPClass = prjCfg.XLPClasses.Items.FirstOrDefault((IXLPClass xc) => xc.Name == slotInfo.XLPClass);
			if (iXLPClass == null)
			{
				Outputs.WriteLine(OutputMessageType.Info, $"Unknown XLP class \"{slotInfo.XLPClass}\"! Ignoring slot {slotInfo.SlotID} in preview of {previewContext.EntityAdapter.Name}");
			}
			else if (slotInfo.SlotID == 0)
			{
				if (TargetPreviewContext.EntityAdapter.InstanceType != InstanceType.IT_BEHAVIOR && TargetPreviewContext.EntityAdapter.InstanceType != InstanceType.IT_ASSET)
				{
					continue;
				}
				IBehaviorInstance behaviorInstance = loaderSvc.InstanceSet.Push<IBehaviorInstance>(Guid.NewGuid().ToString());
				IAssetInstance assetInstance = TargetPreviewContext.EntityAdapter.InstanceEntity.As<IAssetInstance>();
				assetInstance.Flatten(loaderSvc.InstanceSet, prjCfg.Classes, behaviorInstance);
				foreach (IAttachmentPoint item in behaviorInstance.AttachmentPointSet.Items)
				{
					AttachmentKnobs.Add(new AttachmentSelectionKnob(item.Name, prjCfg.Classes, loaderSvc, slotInfo, iXLPClass, previewContext));
				}
				loaderSvc.InstanceSet.Remove(behaviorInstance);
			}
			else
			{
				SlotKnobs.Add(new SlotSelectorKnob(loaderSvc, slotInfo, iXLPClass, previewContext));
			}
		}
		if (AttachmentKnobs.Count > 0)
		{
			IKnobSubgroup knobSubgroup = Context.EnsureCreated<CivTechContext>().CreateInstance<IKnobSubgroup>(new object[2] { "Attached Assets", AttachmentKnobs });
			m_localKnobsBySubGroup[knobSubgroup.SubgroupName] = knobSubgroup;
		}
		if (SlotKnobs.Count > 0)
		{
			IKnobSubgroup knobSubgroup2 = Context.EnsureCreated<CivTechContext>().CreateInstance<IKnobSubgroup>(new object[2] { "Preview Assets", SlotKnobs });
			m_localKnobsBySubGroup[knobSubgroup2.SubgroupName] = knobSubgroup2;
		}
		TargetKnobSet.KnobSetChanged += TargetKnobSet_KnobSetChanged;
		TargetKnobSet.KnobSetCleared += TargetKnobSet_KnobSetCleared;
	}

	private string GenerateAttachmentKnobName(string attName)
	{
		return $"AttachmentKnob:{attName}";
	}

	private void TargetKnobSet_KnobSetCleared(object sender, EventArgs e)
	{
		this.KnobSetCleared.Raise(sender, e);
	}

	private void TargetKnobSet_KnobSetChanged(object sender, EventArgs e)
	{
		this.KnobSetChanged.Raise(sender, e);
	}

	public void Dispose()
	{
		TargetKnobSet = null;
		SlotKnobs.Clear();
		SlotKnobs = null;
		AttachmentKnobs.Clear();
		AttachmentKnobs = null;
		m_localKnobsBySubGroup.Clear();
		m_localKnobsBySubGroup = null;
	}

	public IKnob FindKnobByName(string name)
	{
		return TargetKnobSet.FindKnobByName(name) ?? SlotKnobs.FirstOrDefault((IKnob kn) => kn.Name == name) ?? AttachmentKnobs.FirstOrDefault((IKnob kn) => kn.Name == name);
	}

	public void ApplySlotData(AssetData slotData)
	{
		if (slotData.SlotID != 0)
		{
			return;
		}
		if (slotData.AssetName != TargetPreviewContext.EntityAdapter.Name)
		{
			Outputs.WriteLine(OutputMessageType.Info, $"Ignoring preview data saved for asset {slotData.AssetName} because the active asset is {TargetPreviewContext.EntityAdapter.Name}");
			return;
		}
		if (slotData.AssetType != TargetPreviewContext.EntityAdapter.InstanceType)
		{
			Outputs.WriteLine(OutputMessageType.Info, $"Ignoring preview data saved for entity of type {slotData.AssetType} because the active entity is of type {TargetPreviewContext.EntityAdapter.InstanceType}");
			return;
		}
		if (slotData.AssetKnobSetData.KnobSetName != TargetKnobSet.KnobSetName)
		{
			Outputs.WriteLine(OutputMessageType.Info, $"Ignoring preview data saved for knob set {slotData.AssetKnobSetData.KnobSetName} because the active knob set is {TargetKnobSet.KnobSetName}");
		}
		foreach (KnobData knobDatum in slotData.AssetKnobSetData.KnobData)
		{
			IKnob knob = FindKnobByName(knobDatum.KnobName);
			if (knob == null)
			{
				Outputs.WriteLine(OutputMessageType.Info, $"Ignoring preview data saved for missing knob {knobDatum.KnobName} of knob set {slotData.AssetKnobSetData.KnobSetName}");
				return;
			}
			KnobData.ApplyKnobData(knob, knobDatum);
		}
		foreach (SerializableAttachmentPointData attachedAssetDatum in slotData.AttachedAssetData)
		{
			string name = GenerateAttachmentKnobName(attachedAssetDatum.AttachmentPointName);
			IKnob knob2 = FindKnobByName(name);
			if (knob2 == null)
			{
				Outputs.WriteLine(OutputMessageType.Info, $"Ignoring preview data saved for missing knob {attachedAssetDatum.AttachmentPointName} of knob set {slotData.AssetKnobSetData.KnobSetName}");
				break;
			}
			IValueKnob<string> valueKnob = knob2.As<IValueKnob<string>>();
			if (valueKnob == null)
			{
				Outputs.WriteLine(OutputMessageType.Info, $"Ignoring preview data saved for incorrectly typed knob for attachment {attachedAssetDatum.AttachmentPointName} of knob set {slotData.AssetKnobSetData.KnobSetName}");
				break;
			}
			Outputs.WriteLine(OutputMessageType.Info, $"Attaching asset {attachedAssetDatum.AssetName} to attachment {attachedAssetDatum.AttachmentPointName}");
			valueKnob.Value = attachedAssetDatum.AssetName;
		}
	}
}
