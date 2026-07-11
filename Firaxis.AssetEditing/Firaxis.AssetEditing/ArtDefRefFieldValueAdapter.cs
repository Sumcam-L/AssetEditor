using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefRefFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public string ArtDefPath
	{
		get
		{
			return GetChild<ArtDefRefValueAdapter>(FieldSchema.ArtDefRefFieldValueType.ValueChild).ArtDefPath;
		}
		set
		{
			base.DomNode.GetChild(FieldSchema.ArtDefRefFieldValueType.ValueChild).As<ArtDefRefValueAdapter>().ArtDefPath = value;
		}
	}

	public string CollectionName
	{
		get
		{
			return GetChild<ArtDefRefValueAdapter>(FieldSchema.ArtDefRefFieldValueType.ValueChild).CollectionName;
		}
		set
		{
			base.DomNode.GetChild(FieldSchema.ArtDefRefFieldValueType.ValueChild).As<ArtDefRefValueAdapter>().CollectionName = value;
		}
	}

	public override object DefaultDataAsObject => new ArtDefReferenceInfo(DefaultArtDefRefValue);

	public string ElementName
	{
		get
		{
			return GetChild<ArtDefRefValueAdapter>(FieldSchema.ArtDefRefFieldValueType.ValueChild).ElementName;
		}
		set
		{
			base.DomNode.GetChild(FieldSchema.ArtDefRefFieldValueType.ValueChild).As<ArtDefRefValueAdapter>().ElementName = value;
		}
	}

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public string TemplateName
	{
		get
		{
			return GetChild<ArtDefRefValueAdapter>(FieldSchema.ArtDefRefFieldValueType.ValueChild).TemplateName;
		}
		set
		{
			base.DomNode.GetChild(FieldSchema.ArtDefRefFieldValueType.ValueChild).As<ArtDefRefValueAdapter>().TemplateName = value;
		}
	}

	public override object ValueDataAsObject
	{
		get
		{
			return new ArtDefReferenceInfo(ArtDefRefValue);
		}
		set
		{
			ArtDefReferenceInfo fromInfo = ((!(value is string)) ? ((ArtDefReferenceInfo)value) : CivTechRegistry.ArtDefRegistry.GetArtDefInfo((string)value, ArtDefRefValue, base.Parameter));
			if (string.IsNullOrEmpty(fromInfo.artDefPath) && DefaultArtDefRefValue.ArtDefPath != fromInfo.artDefPath && !ArtDefRefParameter.IsNullAllowed)
			{
				MessageBoxes.Show("The ArtDef Path is empty.  This may lead to validation errors.  Does the entry you have selected exist in any ArtDefs?");
			}
			ArtDefRefValue.SetFromInfo(fromInfo);
			ElementName = fromInfo.elementName;
			ArtDefPath = fromInfo.artDefPath;
			CollectionName = fromInfo.collectionName;
			TemplateName = fromInfo.templateName;
		}
	}

	private IArtDefRefParameter ArtDefRefParameter => base.Parameter as IArtDefRefParameter;

	private IArtDefRefValue ArtDefRefValue => base.Value as IArtDefRefValue;

	private IArtDefRefValue DefaultArtDefRefValue => base.Parameter.DefaultValue as IArtDefRefValue;

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		UpdateNativeValue(CollectionName, ElementName, ArtDefPath, TemplateName);
	}

	public override void AssignDefaultValue()
	{
		ArtDefPath = DefaultArtDefRefValue.ArtDefPath;
		ElementName = DefaultArtDefRefValue.ElementName;
		CollectionName = DefaultArtDefRefValue.RootCollectionName;
		TemplateName = ArtDefRefValue.TemplateName;
	}

	public string[] GetElementCollection()
	{
		ArtDefElementAdapter artDefElementAdapter = base.DomNode.Parent.As<ArtDefElementAdapter>();
		if (artDefElementAdapter != null)
		{
			return GetLocallyScopedElements(artDefElementAdapter);
		}
		return CivTechRegistry.ArtDefRegistry.GetSuitableElements(base.Value, base.Parameter);
	}

	public override void Initialize(IParameter param)
	{
		base.Initialize(param);
		DomNode child = new DomNode(FieldSchema.ArtDefRefValueType.Type);
		base.DomNode.SetChild(FieldSchema.ArtDefRefFieldValueType.ValueChild, child);
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		if (base.Parameter is IArtDefRefParameter artDefRefParameter)
		{
			if (!artDefRefParameter.CollectionIsLocked)
			{
				string primaryArtDefPantry = CivTechRegistry.ArtDefRegistry.PrimaryArtDefPantry;
				IFileDialogService fileDialogService = FiraxisATFRegistry.FileDialogService;
				IDocumentService documentService = FiraxisATFRegistry.DocumentService;
				IEnumerable<IDocumentClient> documentClients = FiraxisATFRegistry.DocumentClients;
				ChildAttributeFieldPropertyDescriptor childAttributeFieldPropertyDescriptor = new ChildAttributeFieldPropertyDescriptor("Art Def".Localize(), FieldSchema.ArtDefRefValueType.ArtDefPathAttribute, FieldSchema.ArtDefRefFieldValueType.ValueChild, "Value".Localize(), "Art Def where the collections and elements are referenced.".Localize(), readOnlyFunctor, new PantryRootedFileNameEditor(fileDialogService, documentService, documentClients, primaryArtDefPantry, "ArtDef files (*.artdef)|*.artdef"));
				ChildAttributeFieldPropertyDescriptor childAttributeFieldPropertyDescriptor2 = new ChildAttributeFieldPropertyDescriptor("Collection".Localize(), FieldSchema.ArtDefRefValueType.CollectionNameAttribute, FieldSchema.ArtDefRefFieldValueType.ValueChild, "Value".Localize(), "Collection where the element values are defined".Localize(), readOnlyFunctor, new ArtDefRefCollectionUITypeEditor(), new ArtDefRefCollectionTypeConverter());
				ChildAttributeFieldPropertyDescriptor childAttributeFieldPropertyDescriptor3 = new ChildAttributeFieldPropertyDescriptor("Element".Localize(), FieldSchema.ArtDefRefValueType.ElementNameAttribute, FieldSchema.ArtDefRefFieldValueType.ValueChild, "Value".Localize(), "Art def element to reference".Localize(), readOnlyFunctor, new ArtDefRefElementUITypeEditor(), new ArtDefRefElementTypeConverter());
				childAttributeFieldPropertyDescriptor.ShowTypeInName = true;
				childAttributeFieldPropertyDescriptor2.ShowTypeInName = true;
				childAttributeFieldPropertyDescriptor3.ShowTypeInName = true;
				m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[3]
				{
					CreateProxyPropertyDescriptorIfNeeded(childAttributeFieldPropertyDescriptor, Name),
					CreateProxyPropertyDescriptorIfNeeded(childAttributeFieldPropertyDescriptor2, Name),
					CreateProxyPropertyDescriptorIfNeeded(childAttributeFieldPropertyDescriptor3, Name)
				});
			}
			else
			{
				m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new ChildAttributeFieldPropertyDescriptor("Element".Localize(), FieldSchema.ArtDefRefValueType.ElementNameAttribute, FieldSchema.ArtDefRefFieldValueType.ValueChild, "Value".Localize(), "Art def element to reference".Localize(), readOnlyFunctor, new ArtDefRefElementUITypeEditor(), new ArtDefRefElementTypeConverter()), Name) });
			}
		}
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num)
		{
			IArtDefRefValue valueOne = val as IArtDefRefValue;
			flag = MatchesData(valueOne);
		}
		if (!num)
		{
			return !flag;
		}
		return true;
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		CopyValue(val);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public override void UpdateNativeFromDom()
	{
		UpdateNativeValue(CollectionName, ElementName, ArtDefPath, TemplateName);
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		IArtDefRefValue artDefRefValue = val as IArtDefRefValue;
		CollectionName = artDefRefValue.RootCollectionName;
		ElementName = artDefRefValue.ElementName;
		ArtDefPath = artDefRefValue.ArtDefPath;
		if (!string.IsNullOrEmpty(artDefRefValue.TemplateName))
		{
			TemplateName = artDefRefValue.TemplateName;
		}
	}

	protected virtual void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.FieldValueType.NameAttribute)
		{
			return;
		}
		if (e.AttributeInfo == FieldSchema.ArtDefRefValueType.ElementNameAttribute)
		{
			if (e.NewValue is string)
			{
				ITransactionContext transactionContext = base.DomNode.GetRoot().As<ITransactionContext>();
				ArtDefRefValue.ElementName = e.NewValue as string;
				ArtDefReferenceInfo artDefInfo = CivTechRegistry.ArtDefRegistry.GetArtDefInfo(ArtDefRefValue.ElementName, ArtDefRefValue, base.Parameter);
				if (string.IsNullOrEmpty(artDefInfo.artDefPath) && DefaultArtDefRefValue.ArtDefPath != artDefInfo.artDefPath && !ArtDefRefParameter.IsNullAllowed)
				{
					HistoryContext historyContext = transactionContext.As<HistoryContext>();
					if (historyContext == null)
					{
						transactionContext = GetRootTransactionContext();
						historyContext = transactionContext.As<HistoryContext>();
					}
					if (historyContext == null || !historyContext.UndoingOrRedoing)
					{
						MessageBoxes.Show("Could not find ArtDef that provides a \"" + ArtDefRefValue.ElementName + "\" element in the current project.  This may lead to validation errors.  Does the entry you have selected exist in any ArtDefs?");
					}
				}
				ArtDefRefValue.SetFromInfo(artDefInfo);
				using (transactionContext?.SuspendTransactions())
				{
					ElementName = artDefInfo.elementName;
					ArtDefPath = artDefInfo.artDefPath;
					CollectionName = artDefInfo.collectionName;
					TemplateName = artDefInfo.templateName;
					return;
				}
			}
			if (e.NewValue is ArtDefReferenceInfo)
			{
				ArtDefReferenceInfo fromInfo = (ArtDefReferenceInfo)e.NewValue;
				ArtDefRefValue.SetFromInfo(fromInfo);
				ElementName = fromInfo.elementName;
			}
		}
		else if (e.AttributeInfo == FieldSchema.ArtDefRefValueType.CollectionNameAttribute)
		{
			if (e.NewValue is string)
			{
				ArtDefRefValue.RootCollectionName = e.NewValue as string;
			}
			else if (e.NewValue is ArtDefReferenceInfo)
			{
				ArtDefReferenceInfo fromInfo2 = (ArtDefReferenceInfo)e.NewValue;
				ArtDefRefValue.SetFromInfo(fromInfo2);
				CollectionName = fromInfo2.collectionName;
			}
		}
		else if (e.AttributeInfo == FieldSchema.ArtDefRefValueType.ArtDefPathAttribute)
		{
			if (e.NewValue is string)
			{
				ArtDefRefValue.ArtDefPath = e.NewValue as string;
			}
			else if (e.NewValue is ArtDefReferenceInfo)
			{
				ArtDefReferenceInfo fromInfo3 = (ArtDefReferenceInfo)e.NewValue;
				ArtDefRefValue.SetFromInfo(fromInfo3);
				ArtDefPath = fromInfo3.artDefPath;
			}
		}
		else if (e.AttributeInfo == FieldSchema.ArtDefRefValueType.TemplateNameAttribute)
		{
			if (e.NewValue is string)
			{
				ArtDefRefValue.TemplateName = e.NewValue as string;
			}
			else if (e.NewValue is ArtDefReferenceInfo)
			{
				ArtDefReferenceInfo fromInfo4 = (ArtDefReferenceInfo)e.NewValue;
				ArtDefRefValue.SetFromInfo(fromInfo4);
				TemplateName = fromInfo4.templateName;
			}
		}
	}

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		return PropertyDescriptors.AsIEnumerable<System.ComponentModel.PropertyDescriptor>().ToArray();
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private IArtDefCollection FindArtDefCollectionDepthFirst(ArtDefElementAdapter artDefElem, string collectionName)
	{
		if (artDefElem == null)
		{
			return null;
		}
		ArtDefCollectionAdapter artDefCollectionAdapter = artDefElem.Collections.FirstOrDefault((ArtDefCollectionAdapter col) => col.Name == collectionName);
		if (artDefCollectionAdapter != null)
		{
			return artDefCollectionAdapter.ArtDefCollection;
		}
		if (artDefElem.DomNode.Parent == null || artDefElem.DomNode.Parent.Parent == null)
		{
			return null;
		}
		return FindArtDefCollectionDepthFirst(artDefElem.DomNode.Parent.Parent.As<ArtDefElementAdapter>(), collectionName);
	}

	private IArtDefCollection FindArtDefCollectionRootFirst(IArtDef artDef, string collectionName)
	{
		if (artDef == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Attempted to find collection ({0}) in a null ArtDef.", collectionName);
			return null;
		}
		return artDef.FindArtDefCollectionRootFirst(collectionName);
	}

	private ISet<string> GetCollectionNames(IArtDef artDef)
	{
		ISet<string> collections = new HashSet<string>();
		artDef.GetAllCollections().ForEach(delegate(IArtDefCollection col)
		{
			collections.Add(col.CollectionName);
		});
		return collections;
	}

	private ISet<string> GetElementNames(IArtDef artDef, string collectionName)
	{
		ISet<string> set = new HashSet<string>();
		if (string.IsNullOrEmpty(collectionName))
		{
			foreach (IArtDefElement allElement in artDef.GetAllElements())
			{
				set.Add(allElement.Name);
			}
		}
		else
		{
			foreach (IArtDefCollection item in from c in artDef.GetAllCollections()
				where c.CollectionName == collectionName
				select c)
			{
				foreach (IArtDefElement element in item.Elements)
				{
					set.Add(element.Name);
				}
			}
		}
		return set;
	}

	private string[] GetLocallyScopedElements(ArtDefElementAdapter artDefElemAdapter)
	{
		IArtDefCollection artDefCollection = FindArtDefCollectionDepthFirst(base.DomNode.Parent.As<ArtDefElementAdapter>(), CollectionName);
		if (artDefCollection != null)
		{
			return artDefCollection.Elements.Select((IArtDefElement elem) => elem.Name).ToArray();
		}
		return CivTechRegistry.ArtDefRegistry.GetSuitableElements(base.Value, base.Parameter);
	}

	private bool MatchesData(IArtDefRefValue valueOne)
	{
		if (valueOne.ArtDefPath == ArtDefPath && valueOne.ElementName == ElementName && valueOne.RootCollectionName == CollectionName)
		{
			return valueOne.TemplateName == TemplateName;
		}
		return false;
	}

	private void UpdateNativeValue(string collectionName, string elementName, string artDefPath, string templateName)
	{
		ArtDefRefValue.RootCollectionName = collectionName;
		ArtDefRefValue.ElementName = elementName;
		ArtDefRefValue.ArtDefPath = artDefPath;
		ArtDefRefValue.TemplateName = templateName;
	}
}
