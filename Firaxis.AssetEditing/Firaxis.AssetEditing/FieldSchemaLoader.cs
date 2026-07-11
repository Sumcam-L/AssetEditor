using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(FieldSchemaLoader))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FieldSchemaLoader : XmlSchemaTypeLoader, IInitializable
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public FieldSchemaLoader()
	{
	}

	void IInitializable.Initialize()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/Fields");
		Load("FieldTypes.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			FieldSchema.Initialize(current);
			FieldSchema.FloatFieldValueType.Type.Define(new ExtensionInfo<FloatFieldValueAdapter>());
			FieldSchema.RGBFieldValueType.Type.Define(new ExtensionInfo<RGBFieldValueAdapter>());
			FieldSchema.Coord2DFieldValueType.Type.Define(new ExtensionInfo<Coord2DFieldValueAdapter>());
			FieldSchema.Coord3DFieldValueType.Type.Define(new ExtensionInfo<Coord3DFieldValueAdapter>());
			FieldSchema.IntFieldValueType.Type.Define(new ExtensionInfo<IntFieldValueAdapter>());
			FieldSchema.BoolFieldValueType.Type.Define(new ExtensionInfo<BoolFieldValueAdapter>());
			FieldSchema.StringFieldValueType.Type.Define(new ExtensionInfo<StringFieldValueAdapter>());
			FieldSchema.ObjectFieldValueType.Type.Define(new ExtensionInfo<ObjectFieldValueAdapter>());
			FieldSchema.BLPFieldValueType.Type.Define(new ExtensionInfo<BLPEntryFieldValueAdapter>());
			FieldSchema.ArtDefRefValueType.Type.Define(new ExtensionInfo<ArtDefRefValueAdapter>());
			FieldSchema.ArtDefRefFieldValueType.Type.Define(new ExtensionInfo<ArtDefRefFieldValueAdapter>());
			FieldSchema.CurveFieldValueType.Type.Define(new ExtensionInfo<CurveFieldValueAdapter>());
			FieldSchema.CurveType.Type.Define(new ExtensionInfo<CurveAdapter>());
			FieldSchema.CurveType.Type.Define(new ExtensionInfo<CurveEditingContext>());
			FieldSchema.CurveSegmentDefinitionType.Type.Define(new ExtensionInfo<CurveSegmentDefinitionAdapter>());
			FieldSchema.CurveSegmentType.Type.Define(new ExtensionInfo<CurveSegmentAdapter>());
			FieldSchema.ConstantCurveSegmentType.Type.Define(new ExtensionInfo<ConstantCurveSegmentAdapter>());
			FieldSchema.LinearCurveSegmentType.Type.Define(new ExtensionInfo<LinearCurveSegmentAdapter>());
			FieldSchema.CollectionFieldValueType.Type.Define(new ExtensionInfo<CollectionFieldValueAdapter>());
			FieldSchema.TupleFieldValueType.Type.Define(new ExtensionInfo<TupleFieldValueAdapter>());
		}
	}
}
