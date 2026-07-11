using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class FieldSchema
{
	public static class ArtDefRefFieldValueType
	{
		public static DomNodeType Type;

		public static ChildInfo ValueChild;
	}

	public static class ArtDefRefValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ElementNameAttribute;

		public static AttributeInfo CollectionNameAttribute;

		public static AttributeInfo ArtDefPathAttribute;

		public static AttributeInfo TemplateNameAttribute;
	}

	public static class BLPFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo EntryNameAttribute;

		public static AttributeInfo XLPPathAttribute;

		public static AttributeInfo BLPPackageAttribute;
	}

	public static class BoolFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class CollectionFieldValueType
	{
		public static DomNodeType Type;

		public static ChildInfo ValueChild;
	}

	public static class TupleFieldValueType
	{
		public static DomNodeType Type;

		public static ChildInfo ValueChild;
	}

	public static class Coord2DFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class Coord3DFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class FieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;
	}

	public static class FloatFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class IntFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class ObjectFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ClassAttribute;

		public static AttributeInfo ObjectNameAttribute;

		public static AttributeInfo ObjectTypeAttribute;
	}

	public static class RGBFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class StringFieldValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo ValueAttribute;
	}

	public static class CurveSegmentType
	{
		public static DomNodeType Type;
	}

	public static class ConstantCurveSegmentType
	{
		public static DomNodeType Type;

		public static AttributeInfo ConstantValueAttribute;
	}

	public static class LinearCurveSegmentType
	{
		public static DomNodeType Type;

		public static AttributeInfo FirstValueAttribute;

		public static AttributeInfo LastValueAttribute;
	}

	public static class CurveSegmentDefinitionType
	{
		public static DomNodeType Type;

		public static AttributeInfo StartingPointAttribute;

		public static ChildInfo CurveSegmentChild;
	}

	public static class CurveType
	{
		public static DomNodeType Type;

		public static ChildInfo CurveSegmentsChild;
	}

	public static class CurveFieldValueType
	{
		public static DomNodeType Type;

		public static ChildInfo ValueChild;
	}

	public const string NS = "FieldTypes";

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
		FieldValueType.Type = getNodeType("FieldTypes", "FieldValueType");
		FieldValueType.NameAttribute = FieldValueType.Type.GetAttributeInfo("Name");
		FloatFieldValueType.Type = getNodeType("FieldTypes", "FloatFieldValueType");
		FloatFieldValueType.ValueAttribute = FloatFieldValueType.Type.GetAttributeInfo("Value");
		IntFieldValueType.Type = getNodeType("FieldTypes", "IntFieldValueType");
		IntFieldValueType.ValueAttribute = IntFieldValueType.Type.GetAttributeInfo("Value");
		BoolFieldValueType.Type = getNodeType("FieldTypes", "BoolFieldValueType");
		BoolFieldValueType.ValueAttribute = BoolFieldValueType.Type.GetAttributeInfo("Value");
		StringFieldValueType.Type = getNodeType("FieldTypes", "StringFieldValueType");
		StringFieldValueType.ValueAttribute = StringFieldValueType.Type.GetAttributeInfo("Value");
		RGBFieldValueType.Type = getNodeType("FieldTypes", "RGBFieldValueType");
		RGBFieldValueType.ValueAttribute = RGBFieldValueType.Type.GetAttributeInfo("Value");
		Coord2DFieldValueType.Type = getNodeType("FieldTypes", "Coord2DFieldValueType");
		Coord2DFieldValueType.ValueAttribute = Coord2DFieldValueType.Type.GetAttributeInfo("Value");
		Coord3DFieldValueType.Type = getNodeType("FieldTypes", "Coord3DFieldValueType");
		Coord3DFieldValueType.ValueAttribute = Coord3DFieldValueType.Type.GetAttributeInfo("Value");
		BLPFieldValueType.Type = getNodeType("FieldTypes", "BLPFieldValueType");
		BLPFieldValueType.EntryNameAttribute = BLPFieldValueType.Type.GetAttributeInfo("EntryName");
		BLPFieldValueType.XLPPathAttribute = BLPFieldValueType.Type.GetAttributeInfo("XLPPath");
		BLPFieldValueType.BLPPackageAttribute = BLPFieldValueType.Type.GetAttributeInfo("BLPPackage");
		ArtDefRefValueType.Type = getNodeType("FieldTypes", "ArtDefRefValueType");
		ArtDefRefValueType.ElementNameAttribute = ArtDefRefValueType.Type.GetAttributeInfo("ElementName");
		ArtDefRefValueType.CollectionNameAttribute = ArtDefRefValueType.Type.GetAttributeInfo("CollectionName");
		ArtDefRefValueType.ArtDefPathAttribute = ArtDefRefValueType.Type.GetAttributeInfo("ArtDefPath");
		ArtDefRefValueType.TemplateNameAttribute = ArtDefRefValueType.Type.GetAttributeInfo("TemplateName");
		ArtDefRefFieldValueType.Type = getNodeType("FieldTypes", "ArtDefRefFieldValueType");
		ArtDefRefFieldValueType.ValueChild = ArtDefRefFieldValueType.Type.GetChildInfo("Value");
		ObjectFieldValueType.Type = getNodeType("FieldTypes", "ObjectValueType");
		ObjectFieldValueType.ClassAttribute = ObjectFieldValueType.Type.GetAttributeInfo("Class");
		ObjectFieldValueType.ObjectNameAttribute = ObjectFieldValueType.Type.GetAttributeInfo("ObjectName");
		ObjectFieldValueType.ObjectTypeAttribute = ObjectFieldValueType.Type.GetAttributeInfo("ObjectType");
		CollectionFieldValueType.Type = getNodeType("FieldTypes", "CollectionFieldValueType");
		CollectionFieldValueType.ValueChild = CollectionFieldValueType.Type.GetChildInfo("Value");
		TupleFieldValueType.Type = getNodeType("FieldTypes", "TupleFieldValueType");
		TupleFieldValueType.ValueChild = TupleFieldValueType.Type.GetChildInfo("Value");
		CurveSegmentType.Type = getNodeType("FieldTypes", "CurveSegmentType");
		ConstantCurveSegmentType.Type = getNodeType("FieldTypes", "ConstantCurveSegmentType");
		ConstantCurveSegmentType.ConstantValueAttribute = ConstantCurveSegmentType.Type.GetAttributeInfo("ConstantValue");
		LinearCurveSegmentType.Type = getNodeType("FieldTypes", "LinearCurveSegmentType");
		LinearCurveSegmentType.FirstValueAttribute = LinearCurveSegmentType.Type.GetAttributeInfo("FirstValue");
		LinearCurveSegmentType.LastValueAttribute = LinearCurveSegmentType.Type.GetAttributeInfo("LastValue");
		CurveSegmentDefinitionType.Type = getNodeType("FieldTypes", "CurveSegmentDefinitionType");
		CurveSegmentDefinitionType.StartingPointAttribute = CurveSegmentDefinitionType.Type.GetAttributeInfo("StartingPoint");
		CurveSegmentDefinitionType.CurveSegmentChild = CurveSegmentDefinitionType.Type.GetChildInfo("CurveSegment");
		CurveType.Type = getNodeType("FieldTypes", "CurveType");
		CurveType.CurveSegmentsChild = CurveType.Type.GetChildInfo("CurveSegments");
		CurveFieldValueType.Type = getNodeType("FieldTypes", "CurveFieldValueType");
		CurveFieldValueType.ValueChild = CurveFieldValueType.Type.GetChildInfo("Value");
	}
}
