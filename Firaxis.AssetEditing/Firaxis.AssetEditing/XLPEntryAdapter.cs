using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class XLPEntryAdapter : DomNodeAdapter, IAssetBrowserTypeProvider
{
	public string EntryID
	{
		get
		{
			return GetAttribute<string>(XLPSchema.XLPEntryType.EntryIDAttribute);
		}
		set
		{
			SetAttribute(XLPSchema.XLPEntryType.EntryIDAttribute, value);
		}
	}

	public string ObjectName
	{
		get
		{
			return GetAttribute<string>(XLPSchema.XLPEntryType.ObjectNameAttribute);
		}
		set
		{
			SetAttribute(XLPSchema.XLPEntryType.ObjectNameAttribute, value);
		}
	}

	public IEnumerable<string> ValidClassNames
	{
		get
		{
			XLPDocument xLPDocument = base.DomNode.GetRoot().As<XLPDocument>();
			BugSubmitter.SilentAssert(XLP != null, "Native XLP was null when trying to find matching ValidClassNames for XLP \"{0}\" @summary Native XLP was null when trying to find matching ValidClassNames @assign bwhitman", xLPDocument.Uri.LocalPath);
			if (XLP == null)
			{
				return null;
			}
			return xLPDocument.CivTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault((IXLPClass xlpc) => xlpc.Name == XLP.ClassName)?.AllowedEntityClasses;
		}
	}

	public IEnumerable<InstanceType> ValidTypes
	{
		get
		{
			IXLPClass iXLPClass = base.DomNode.GetRoot().As<XLPDocument>().CivTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault((IXLPClass xlpc) => xlpc.Name == XLP.ClassName);
			if (iXLPClass != null)
			{
				yield return iXLPClass.InstanceType;
			}
		}
	}

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);

	public IXLPEntry XLPEntry { get; set; }

	private IXLP XLP { get; set; }

	public void Update(IXLP xlp, IXLPEntry xlpEntry)
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		XLP = xlp;
		XLPEntry = xlpEntry;
		if (xlpEntry.ID != EntryID)
		{
			EntryID = xlpEntry.ID;
		}
		if (xlpEntry.ObjectName != ObjectName)
		{
			ObjectName = xlpEntry.ObjectName;
		}
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnParentNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnParentNodeSet();
		IXLP xLP = base.DomNode.GetRoot()?.As<XLPAdapter>()?.XLP;
		XLP = xLP;
		BugSubmitter.SilentAssert(XLP != null, "XLP was during attributre change! @assign bwhitman");
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		base.DomNode.GetRoot().As<XLPContext>();
		if (e.AttributeInfo == XLPSchema.XLPEntryType.EntryIDAttribute)
		{
			foreach (IXLPEntry xLPEntry in XLP.XLPEntries)
			{
				if (xLPEntry.ID == EntryID && xLPEntry != XLPEntry)
				{
					EntryID = XLPEntry.ID;
					throw new InvalidTransactionException("ID \"" + xLPEntry.ID + "\" already in use in this XLP!");
				}
			}
			XLPEntry.ID = EntryID;
		}
		else if (e.AttributeInfo == XLPSchema.XLPEntryType.ObjectNameAttribute)
		{
			XLPEntry.ObjectName = ObjectName;
		}
	}
}
