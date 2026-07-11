using System.Drawing;
using Firaxis.Error;

namespace Firaxis.CivTech.AssetObjects;

public static class ICollectionValueExtensions
{
	public static IValue AddValue(this ICollectionValue collection, string name, IParameter param)
	{
		IValue result = null;
		switch (collection.EntryValueType)
		{
		case ValueType.VT_ARTDEF_REFERENCE:
		{
			IArtDefRefValue artDefRefValue = collection.Push<IArtDefRefValue>(name);
			artDefRefValue.RootCollectionName = ((IArtDefRefParameter)param).DefaultCollectionName;
			artDefRefValue.ElementName = ((IArtDefRefParameter)param).DefaultElementName;
			artDefRefValue.IsCollectionLocked = ((IArtDefRefParameter)param).CollectionIsLocked;
			artDefRefValue.ArtDefPath = ((IArtDefRefParameter)param).DefaultArtDefPath;
			result = artDefRefValue;
			break;
		}
		case ValueType.VT_BLP_ENTRY:
		{
			IBLPEntryValue iBLPEntryValue = collection.Push<IBLPEntryValue>(name);
			iBLPEntryValue.EntryName = ((IBLPEntryParameter)param).DefaultEntryName;
			iBLPEntryValue.BLPPackage = ((IBLPEntryParameter)param).DefaultBLPPackage;
			iBLPEntryValue.XLPPath = ((IBLPEntryParameter)param).DefaultXLPPath;
			iBLPEntryValue.XLPClass = ((IBLPEntryParameter)param).XLPClassName;
			iBLPEntryValue.LibraryName = ((IBLPEntryParameter)param).LibraryName;
			result = iBLPEntryValue;
			break;
		}
		case ValueType.VT_BOOL:
		{
			IBoolValue boolValue = collection.Push<IBoolValue>(name);
			boolValue.ParameterValue = ((IBoolParameter)param).Default;
			result = boolValue;
			break;
		}
		case ValueType.VT_COORD2D:
		{
			ICoord2DValue coord2DValue = collection.Push<ICoord2DValue>(name);
			coord2DValue.ParameterValue = new PointF(((ICoord2DParameter)param).DefaultX, ((ICoord2DParameter)param).DefaultY);
			result = coord2DValue;
			break;
		}
		case ValueType.VT_COORD3D:
		{
			ICoord3DValue coord3DValue = collection.Push<ICoord3DValue>(name);
			coord3DValue.ParameterValue = new Point3F(((ICoord3DParameter)param).DefaultX, ((ICoord3DParameter)param).DefaultY, ((ICoord3DParameter)param).DefaultZ);
			result = coord3DValue;
			break;
		}
		case ValueType.VT_FLOAT:
		{
			IFloatValue floatValue = collection.Push<IFloatValue>(name);
			floatValue.ParameterValue = ((IFloatParameter)param).Default;
			result = floatValue;
			break;
		}
		case ValueType.VT_INT:
		{
			IIntValue intValue = collection.Push<IIntValue>(name);
			intValue.ParameterValue = ((IIntParameter)param).Default;
			result = intValue;
			break;
		}
		case ValueType.VT_RGB:
		{
			IRGBValue iRGBValue = collection.Push<IRGBValue>(name);
			int red = (int)((IRGBParameter)param).DefaultR;
			int blue = (int)((IRGBParameter)param).DefaultB;
			int green = (int)((IRGBParameter)param).DefaultG;
			iRGBValue.ParameterValue = Color.FromArgb(red, green, blue);
			result = iRGBValue;
			break;
		}
		case ValueType.VT_STRING:
		{
			IStringValue stringValue = collection.Push<IStringValue>(name);
			IStringValue stringValue2 = param.DefaultValue as IStringValue;
			stringValue.ParameterValue = stringValue2.ParameterValue;
			result = stringValue;
			break;
		}
		case ValueType.VT_OBJECT:
		case ValueType.VT_COLLECTION:
		case ValueType.VT_CURVE:
		case ValueType.VT_COUNT:
			PlatformAssert.If(condition: true, "Used the wrong AddValue function - collections cannot contain other collections or count.  For objects, use Push<>.");
			break;
		default:
			PlatformAssert.If(condition: true, "Unknown value type.");
			break;
		}
		return result;
	}
}
