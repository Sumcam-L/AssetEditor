using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class ArtDefSchema
{
	public static class ArtDefType
	{
		public static DomNodeType Type;

		public static AttributeInfo TemplateAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static ChildInfo RootCollectionChild;
	}

	public static class ArtDefCollectionType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ReplaceMergedCollectionElementsAttribute;

		public static ChildInfo ElementsChild;
	}

	public static class ArtDefElementType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo AppendMergedParameterCollectionsAttribute;

		public static ChildInfo FieldsChild;

		public static ChildInfo CollectionsChild;
	}

	public static ChildInfo ArtDefRootElement;

	public const string NS = "ArtDef";

	public static void Initialize(XmlSchemaTypeCollection typeCollection)
	{
		Initialize((string ns, string name) => typeCollection.GetNodeType(ns, name), (string ns, string name) => typeCollection.GetRootElement(ns, name));
	}

	public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
	{
		Initialize((string ns, string name) => typeCollections[ns].GetNodeType(name), (string ns, string name) => typeCollections[ns].GetRootElement(name));
	}

	private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
	{
		ArtDefType.Type = getNodeType("ArtDef", "ArtDefType");
		ArtDefType.TemplateAttribute = ArtDefType.Type.GetAttributeInfo("Template");
		ArtDefType.DescriptionAttribute = ArtDefType.Type.GetAttributeInfo("Description");
		ArtDefType.RootCollectionChild = ArtDefType.Type.GetChildInfo("RootCollection");
		ArtDefCollectionType.Type = getNodeType("ArtDef", "ArtDefCollectionType");
		ArtDefCollectionType.NameAttribute = ArtDefCollectionType.Type.GetAttributeInfo("Name");
		ArtDefCollectionType.DescriptionAttribute = ArtDefCollectionType.Type.GetAttributeInfo("Description");
		ArtDefCollectionType.ReplaceMergedCollectionElementsAttribute = ArtDefCollectionType.Type.GetAttributeInfo("ReplaceMergedCollectionElements");
		ArtDefCollectionType.ElementsChild = ArtDefCollectionType.Type.GetChildInfo("Elements");
		ArtDefElementType.Type = getNodeType("ArtDef", "ArtDefElementType");
		ArtDefElementType.NameAttribute = ArtDefElementType.Type.GetAttributeInfo("Name");
		ArtDefElementType.AppendMergedParameterCollectionsAttribute = ArtDefElementType.Type.GetAttributeInfo("AppendMergedParameterCollections");
		ArtDefElementType.FieldsChild = ArtDefElementType.Type.GetChildInfo("Fields");
		ArtDefElementType.CollectionsChild = ArtDefElementType.Type.GetChildInfo("Collections");
		ArtDefRootElement = getRootElement("ArtDef", "ArtDef");
	}
}
