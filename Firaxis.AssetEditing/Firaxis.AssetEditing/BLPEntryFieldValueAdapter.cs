using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BLPEntryFieldValueAdapter : FieldValueAdapter, IXLPBrowserTypeProvider
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => new BLPData(DefaultBLPValue);

	public string EntryName
	{
		get
		{
			return GetAttribute<string>(FieldSchema.BLPFieldValueType.EntryNameAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.BLPFieldValueType.EntryNameAttribute, value);
		}
	}

	public string PackagePath
	{
		get
		{
			return GetAttribute<string>(FieldSchema.BLPFieldValueType.BLPPackageAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.BLPFieldValueType.BLPPackageAttribute, value);
		}
	}

	public IEnumerable<string> ValidTypes
	{
		get
		{
			IBLPEntryParameter iBLPEntryParameter = base.Parameter as IBLPEntryParameter;
			yield return iBLPEntryParameter.LibraryName;
		}
	}

	public override object ValueDataAsObject
	{
		get
		{
			return new BLPData(BLPValue);
		}
		set
		{
			if (value is BLPData bLPData)
			{
				SetAttribute(FieldSchema.BLPFieldValueType.EntryNameAttribute, bLPData.Name);
				SetAttribute(FieldSchema.BLPFieldValueType.BLPPackageAttribute, bLPData.BLPPath);
				SetAttribute(FieldSchema.BLPFieldValueType.XLPPathAttribute, bLPData.XLPPath);
			}
			else
			{
				SetAttribute(FieldSchema.BLPFieldValueType.EntryNameAttribute, value as string);
			}
		}
	}

	public string XLPPath
	{
		get
		{
			return GetAttribute<string>(FieldSchema.BLPFieldValueType.XLPPathAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.BLPFieldValueType.XLPPathAttribute, value);
		}
	}

	private IBLPEntryValue BLPValue => base.Value as IBLPEntryValue;

	private IBLPEntryValue DefaultBLPValue => base.Parameter.DefaultValue as IBLPEntryValue;

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		new ColorPickerEditor().EnableAlpha = true;
		IBLPEntryParameter iBLPEntryParameter = base.Parameter as IBLPEntryParameter;
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(iBLPEntryParameter.Name, "BLP Entry".Localize()), FieldSchema.BLPFieldValueType.EntryNameAttribute, BindDynamicValueOrDefault(iBLPEntryParameter.Category, "Value".Localize()), BindDynamicValueOrDefault(iBLPEntryParameter.Description, "BLP entry for this value".Localize()), readOnlyFunctor, new BLPEntryBrowserNameEditor(CivTechRegistry.XLPRegistry, CivTechRegistry.CivTechService, FiraxisATFRegistry.AssetBrowserFileCommands), new BLPEntryNameConverter()), Name) });
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		BLPValue.EntryName = EntryName;
		BLPValue.XLPPath = XLPPath;
		BLPValue.BLPPackage = PackagePath;
	}

	public override void AssignDefaultValue()
	{
		EntryName = DefaultBLPValue.EntryName;
		PackagePath = DefaultBLPValue.BLPPackage;
		XLPPath = DefaultBLPValue.XLPPath;
	}

	public IInstanceEntity LoadReferencedEntity(ArtDefDocument artDefDoc)
	{
		EntityID entityID = CivTechRegistry.XLPRegistry.GetEntityID(XLPPath, EntryName);
		if (entityID.Type != InstanceType.IT_INVALID)
		{
			return artDefDoc.InstanceSet.LoadEntityIfUnique(entityID.Name, entityID.Type);
		}
		return null;
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num)
		{
			IBLPEntryValue otherValue = val as IBLPEntryValue;
			flag = MatchesData(otherValue);
		}
		if (!num)
		{
			return !flag;
		}
		return true;
	}

	public void SetBLPData(BLPData blpData)
	{
		SetAttribute(FieldSchema.BLPFieldValueType.EntryNameAttribute, blpData);
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.UpdateDomFromNative(val);
		CopyValue(val);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public override void UpdateNativeFromDom()
	{
		UpdateNativeValue(EntryName, XLPPath, PackagePath);
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		IBLPEntryValue iBLPEntryValue = val as IBLPEntryValue;
		EntryName = iBLPEntryValue.EntryName;
		PackagePath = iBLPEntryValue.BLPPackage;
		XLPPath = iBLPEntryValue.XLPPath;
	}

	protected virtual void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.BLPFieldValueType.EntryNameAttribute)
		{
			if (e.NewValue is string)
			{
				HandleEntryNameChanged((string)e.NewValue);
			}
			else if (e.NewValue == null)
			{
				HandleEntryNameChanged(string.Empty);
			}
			else
			{
				BugSubmitter.SilentReport($"Invalid value type on BLPFieldValue EntryName change with type {e.NewValue.GetType()} @summary Invalid value type on BLPFieldValue EntryName change @assign bwhitman");
			}
		}
		else if (e.AttributeInfo == FieldSchema.BLPFieldValueType.XLPPathAttribute)
		{
			BLPValue.XLPPath = (string)e.NewValue;
		}
		else if (e.AttributeInfo == FieldSchema.BLPFieldValueType.BLPPackageAttribute)
		{
			BLPValue.BLPPackage = (string)e.NewValue;
		}
		OnItemChanged(e.DomNode);
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private void FindXLPandBLP(string blpEntry, ref string xlpPath, ref string blpPath)
	{
		xlpPath = "";
		blpPath = "";
		IXLPCacheData iXLPCacheData = CivTechRegistry.XLPRegistry.FindXLPData(blpEntry);
		if (iXLPCacheData != null)
		{
			xlpPath = iXLPCacheData.XLPPath;
			blpPath = iXLPCacheData.BLPPackage;
		}
	}

	private void HandleBLPDataChanged(BLPData data)
	{
		UpdateNativeValue(data.Name, data.XLPPath, data.BLPPath);
	}

	private void HandleEntryNameChanged(string entryName)
	{
		string blpPath = string.Empty;
		string xlpPath = string.Empty;
		FindXLPandBLP(entryName, ref xlpPath, ref blpPath);
		if ((string.IsNullOrEmpty(xlpPath) || string.IsNullOrEmpty(blpPath)) && !string.IsNullOrEmpty(entryName))
		{
			string message = "Unable to find a matching XLP for the entry named: (" + EntryName + ").";
			TransactionContext transactionContext = base.DomNode.GetRoot().As<TransactionContext>();
			if (transactionContext != null && transactionContext.InTransaction)
			{
				using (transactionContext.SuspendTransactions())
				{
					EntryName = BLPValue.EntryName;
				}
				OnItemChanged(base.DomNode);
				throw new InvalidTransactionException(message);
			}
			EntryName = BLPValue.EntryName;
			OnItemChanged(base.DomNode);
		}
		else
		{
			UpdateNativeValue(entryName, xlpPath, blpPath);
		}
	}

	private bool MatchesData(IBLPEntryValue otherValue)
	{
		if (otherValue.BLPPackage == PackagePath && otherValue.EntryName == EntryName)
		{
			return otherValue.XLPPath == XLPPath;
		}
		return false;
	}

	private void UpdateNativeValue(string entryName, string xlpPath, string blpPackage)
	{
		if (xlpPath != BLPValue.XLPPath || blpPackage != BLPValue.BLPPackage)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Updating XLP and BLP paths.  Entry ({0}) exists in ({1}).  Previous entry ({2}) existed in ({3})", entryName, xlpPath, BLPValue.EntryName, BLPValue.XLPPath);
		}
		BLPValue.EntryName = entryName;
		BLPValue.XLPPath = xlpPath;
		BLPValue.BLPPackage = blpPackage;
	}
}
