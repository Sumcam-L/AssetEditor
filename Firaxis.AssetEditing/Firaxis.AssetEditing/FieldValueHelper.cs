using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class FieldValueHelper
{
	private static IDictionary<ValueType, DomNodeType> m_valueDomTypeMap;

	static FieldValueHelper()
	{
		m_valueDomTypeMap = new Dictionary<ValueType, DomNodeType>();
		m_valueDomTypeMap.Add(ValueType.VT_FLOAT, FieldSchema.FloatFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_INT, FieldSchema.IntFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_BOOL, FieldSchema.BoolFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_STRING, FieldSchema.StringFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_RGB, FieldSchema.RGBFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_COORD2D, FieldSchema.Coord2DFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_COORD3D, FieldSchema.Coord3DFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_OBJECT, FieldSchema.ObjectFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_COLLECTION, FieldSchema.CollectionFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_BLP_ENTRY, FieldSchema.BLPFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_ARTDEF_REFERENCE, FieldSchema.ArtDefRefFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_CURVE, FieldSchema.CurveFieldValueType.Type);
		m_valueDomTypeMap.Add(ValueType.VT_TUPLE, FieldSchema.TupleFieldValueType.Type);
	}

	public static FieldValueAdapter CreateField(IParameter param)
	{
		if (param == null)
		{
			return null;
		}
		DomNodeType fieldDomNodeType = GetFieldDomNodeType(param.ParameterValueType);
		if (fieldDomNodeType == null)
		{
			return null;
		}
		DomNode domNode = new DomNode(fieldDomNodeType);
		domNode.InitializeExtensions();
		FieldValueAdapter fieldValueAdapter = domNode.As<FieldValueAdapter>();
		fieldValueAdapter.Initialize(param);
		return fieldValueAdapter;
	}

	public static FieldValueAdapter CreateField(string paramName, IParameterSet parameters)
	{
		return CreateField(parameters.Items.FirstOrDefault((IParameter paramItem) => paramItem.Name == paramName));
	}

	public static DomNodeType GetFieldDomNodeType(ValueType valueType)
	{
		DomNodeType value = null;
		m_valueDomTypeMap.TryGetValue(valueType, out value);
		return value;
	}

	public static void UpdateObjectValues(IFieldValueAdapter fldValAdapter, IParameterSet paramSet)
	{
		if (fldValAdapter is ObjectFieldValueAdapter fldValAdapter2)
		{
			UpdateObjectValueByName(fldValAdapter2, paramSet);
		}
		else
		{
			if (!(fldValAdapter is CollectionFieldValueAdapter collectionFieldValueAdapter))
			{
				return;
			}
			foreach (IFieldValueAdapter value in collectionFieldValueAdapter.Values)
			{
				UpdateObjectValueFromBase(value);
			}
		}
	}

	public static IValue NativeAddCollectionValue(ICollectionValue collectionValue, ICollectionParameter collectionParam, string newValueName)
	{
		IValue result = null;
		IParameter entryParameter = collectionParam.EntryParameter;
		switch (entryParameter.ParameterValueType)
		{
		case ValueType.VT_FLOAT:
		{
			IFloatValue floatValue = collectionValue.Push<IFloatValue>(newValueName);
			floatValue.ParameterValue = ((IFloatParameter)entryParameter).Default;
			result = floatValue;
			break;
		}
		case ValueType.VT_INT:
		{
			IIntValue intValue = collectionValue.Push<IIntValue>(newValueName);
			intValue.ParameterValue = ((IIntParameter)entryParameter).Default;
			result = intValue;
			break;
		}
		case ValueType.VT_BOOL:
		{
			IBoolValue boolValue = collectionValue.Push<IBoolValue>(newValueName);
			boolValue.ParameterValue = ((IBoolParameter)entryParameter).Default;
			result = boolValue;
			break;
		}
		case ValueType.VT_RGB:
		{
			IRGBValue iRGBValue = collectionValue.Push<IRGBValue>(newValueName);
			int red = (int)((IRGBParameter)entryParameter).DefaultR;
			int blue = (int)((IRGBParameter)entryParameter).DefaultB;
			int green = (int)((IRGBParameter)entryParameter).DefaultG;
			iRGBValue.ParameterValue = Color.FromArgb(red, green, blue);
			result = iRGBValue;
			break;
		}
		case ValueType.VT_STRING:
		{
			IStringValue stringValue = collectionValue.Push<IStringValue>(newValueName);
			IStringValue stringValue2 = entryParameter.DefaultValue as IStringValue;
			stringValue.ParameterValue = stringValue2.ParameterValue;
			result = stringValue;
			break;
		}
		case ValueType.VT_OBJECT:
			result = collectionValue.Push<IObjectValue>(newValueName, ((IObjectParameter)entryParameter).ObjectType, "");
			break;
		case ValueType.VT_COORD2D:
		{
			ICoord2DValue coord2DValue = collectionValue.Push<ICoord2DValue>(newValueName);
			coord2DValue.ParameterValue = new PointF(((ICoord2DParameter)entryParameter).DefaultX, ((ICoord2DParameter)entryParameter).DefaultY);
			result = coord2DValue;
			break;
		}
		case ValueType.VT_COORD3D:
		{
			ICoord3DValue coord3DValue = collectionValue.Push<ICoord3DValue>(newValueName);
			coord3DValue.ParameterValue = new Point3F(((ICoord3DParameter)entryParameter).DefaultX, ((ICoord3DParameter)entryParameter).DefaultY, ((ICoord3DParameter)entryParameter).DefaultZ);
			result = coord3DValue;
			break;
		}
		case ValueType.VT_BLP_ENTRY:
		{
			IBLPEntryValue iBLPEntryValue = collectionValue.Push<IBLPEntryValue>(newValueName);
			iBLPEntryValue.EntryName = ((IBLPEntryParameter)entryParameter).DefaultEntryName;
			iBLPEntryValue.BLPPackage = ((IBLPEntryParameter)entryParameter).DefaultBLPPackage;
			iBLPEntryValue.XLPPath = ((IBLPEntryParameter)entryParameter).DefaultXLPPath;
			result = iBLPEntryValue;
			break;
		}
		case ValueType.VT_ARTDEF_REFERENCE:
		{
			IArtDefRefValue artDefRefValue = collectionValue.Push<IArtDefRefValue>(newValueName);
			artDefRefValue.RootCollectionName = ((IArtDefRefParameter)entryParameter).DefaultCollectionName;
			artDefRefValue.ElementName = ((IArtDefRefParameter)entryParameter).DefaultElementName;
			artDefRefValue.IsCollectionLocked = ((IArtDefRefParameter)entryParameter).CollectionIsLocked;
			artDefRefValue.ArtDefPath = ((IArtDefRefParameter)entryParameter).DefaultArtDefPath;
			result = artDefRefValue;
			break;
		}
		case ValueType.VT_TUPLE:
		{
			ITupleValue tupleValue = collectionValue.Push<ITupleValue>(newValueName);
			tupleValue.Elements.CopyFrom(((ITupleValue)((ITupleParameter)entryParameter).DefaultValue).Elements);
			result = tupleValue;
			break;
		}
		}
		return result;
	}

	public static IValue NativeAddElementField(IValueSet set, IParameter param)
	{
		IValue result = null;
		switch (param.ParameterValueType)
		{
		case ValueType.VT_FLOAT:
		{
			IFloatValue floatValue = set.Push<IFloatValue>(param.Name);
			floatValue.ParameterValue = ((IFloatParameter)param).Default;
			result = floatValue;
			break;
		}
		case ValueType.VT_INT:
		{
			IIntValue intValue = set.Push<IIntValue>(param.Name);
			intValue.ParameterValue = ((IIntParameter)param).Default;
			result = intValue;
			break;
		}
		case ValueType.VT_BOOL:
		{
			IBoolValue boolValue = set.Push<IBoolValue>(param.Name);
			boolValue.ParameterValue = ((IBoolParameter)param).Default;
			result = boolValue;
			break;
		}
		case ValueType.VT_RGB:
		{
			IRGBValue iRGBValue = set.Push<IRGBValue>(param.Name);
			int red = (int)((IRGBParameter)param).DefaultR;
			int blue = (int)((IRGBParameter)param).DefaultB;
			int green = (int)((IRGBParameter)param).DefaultG;
			iRGBValue.ParameterValue = Color.FromArgb(red, green, blue);
			result = iRGBValue;
			break;
		}
		case ValueType.VT_STRING:
		{
			IStringValue stringValue = set.Push<IStringValue>(param.Name);
			IStringValue stringValue2 = param.DefaultValue as IStringValue;
			stringValue.ParameterValue = stringValue2.ParameterValue;
			result = stringValue;
			break;
		}
		case ValueType.VT_OBJECT:
			result = set.Push<IObjectValue>(param.Name, ((IObjectParameter)param).ObjectType, "");
			break;
		case ValueType.VT_COORD2D:
		{
			ICoord2DValue coord2DValue = set.Push<ICoord2DValue>(param.Name);
			coord2DValue.ParameterValue = new PointF(((ICoord2DParameter)param).DefaultX, ((ICoord2DParameter)param).DefaultY);
			result = coord2DValue;
			break;
		}
		case ValueType.VT_COORD3D:
		{
			ICoord3DValue coord3DValue = set.Push<ICoord3DValue>(param.Name);
			coord3DValue.ParameterValue = new Point3F(((ICoord3DParameter)param).DefaultX, ((ICoord3DParameter)param).DefaultY, ((ICoord3DParameter)param).DefaultZ);
			result = coord3DValue;
			break;
		}
		case ValueType.VT_BLP_ENTRY:
		{
			IBLPEntryValue iBLPEntryValue = set.Push<IBLPEntryValue>(param.Name);
			iBLPEntryValue.EntryName = ((IBLPEntryParameter)param).DefaultEntryName;
			iBLPEntryValue.BLPPackage = ((IBLPEntryParameter)param).DefaultBLPPackage;
			iBLPEntryValue.XLPPath = ((IBLPEntryParameter)param).DefaultXLPPath;
			iBLPEntryValue.XLPClass = ((IBLPEntryParameter)param).XLPClassName;
			iBLPEntryValue.LibraryName = ((IBLPEntryParameter)param).LibraryName;
			result = iBLPEntryValue;
			break;
		}
		case ValueType.VT_ARTDEF_REFERENCE:
		{
			IArtDefRefValue artDefRefValue = set.Push<IArtDefRefValue>(param.Name);
			artDefRefValue.RootCollectionName = ((IArtDefRefParameter)param).DefaultCollectionName;
			artDefRefValue.ElementName = ((IArtDefRefParameter)param).DefaultElementName;
			artDefRefValue.IsCollectionLocked = ((IArtDefRefParameter)param).CollectionIsLocked;
			artDefRefValue.ArtDefPath = ((IArtDefRefParameter)param).DefaultArtDefPath;
			result = artDefRefValue;
			break;
		}
		case ValueType.VT_CURVE:
			result = set.Push<ICurveValue>(param.Name);
			break;
		case ValueType.VT_TUPLE:
		{
			ITupleValue tupleValue = set.Push<ITupleValue>(param.Name);
			tupleValue.Elements.CopyFrom(((ITupleValue)((ITupleParameter)param).DefaultValue).Elements);
			result = tupleValue;
			break;
		}
		}
		return result;
	}

	public static IValue NativeAddElementCollectionField(IValueSet set, ICollectionParameter param)
	{
		IValue result = null;
		switch (param.EntryValueType)
		{
		case ValueType.VT_FLOAT:
			result = set.Push<IFloatCollectionValue>(param.Name);
			break;
		case ValueType.VT_INT:
			result = set.Push<IIntCollectionValue>(param.Name);
			break;
		case ValueType.VT_BOOL:
			result = set.Push<IBoolCollectionValue>(param.Name);
			break;
		case ValueType.VT_RGB:
			result = set.Push<IRGBCollectionValue>(param.Name);
			break;
		case ValueType.VT_STRING:
			result = set.Push<IStringCollectionValue>(param.Name);
			break;
		case ValueType.VT_OBJECT:
			result = set.PushCollection<IObjectCollectionValue>(param.Name, param.EntryObjectType);
			break;
		case ValueType.VT_COORD2D:
			result = set.Push<ICoord2DCollectionValue>(param.Name);
			break;
		case ValueType.VT_COORD3D:
			result = set.Push<ICoord3DCollectionValue>(param.Name);
			break;
		case ValueType.VT_BLP_ENTRY:
			result = set.Push<IBLPEntryCollectionValue>(param.Name);
			break;
		case ValueType.VT_ARTDEF_REFERENCE:
			result = set.Push<IArtDefRefCollectionValue>(param.Name);
			break;
		case ValueType.VT_TUPLE:
			result = set.Push<ITupleCollectionValue>(param.Name);
			break;
		}
		return result;
	}

	private static void UpdateObjectValue(ObjectFieldValueAdapter objAdp, IObjectParameter objParam)
	{
		if (objAdp != null && objParam != null)
		{
			objAdp.UpdateParameterInformation(objParam);
		}
	}

	private static void UpdateObjectValueByName(ObjectFieldValueAdapter fldValAdapter, IParameterSet paramSet)
	{
		IObjectParameter objParam = paramSet.FindByName(fldValAdapter.Name) as IObjectParameter;
		UpdateObjectValue(fldValAdapter, objParam);
	}

	private static void UpdateObjectValueFromBase(IFieldValueAdapter fldValAdapter)
	{
		ObjectFieldValueAdapter objAdp = fldValAdapter as ObjectFieldValueAdapter;
		IObjectParameter objParam = fldValAdapter.Parameter as IObjectParameter;
		UpdateObjectValue(objAdp, objParam);
	}
}
