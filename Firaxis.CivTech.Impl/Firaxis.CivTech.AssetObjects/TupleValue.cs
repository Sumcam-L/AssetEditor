using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class TupleValue : Value, ITupleValue
{
	private unsafe Serializer* m_pkSerializer;

	private IValueSet m_pmValueSet;

	public override ValueType ParameterType => ValueType.VT_TUPLE;

	public virtual IValueSet Elements => m_pmValueSet;

	public unsafe TupleValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
		: this(global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ATupleValue_003E(pkCollectionValue, szName))
	{
	}

	public unsafe TupleValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
		: this(global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ATupleValue_003E(pkValueSet, szName))
	{
	}

	public unsafe TupleValue(global::AssetObjects.TupleValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
		//IL_002a: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x42b5ff35_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0TupleValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07PKPJGBOM_0040pkValue_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040BPICMAID_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 14u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42b5ff35_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0TupleValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.ValueSet* pkValues = global::_003CModule_003E.AssetObjects_002ETupleValue_002EGetElements(pkValue);
		m_pmValueSet = new ValueSet(pkValues);
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_TUPLE)
		{
			ITupleValue tupleValue = (ITupleValue)otherValue;
			object[] fmtParams = new object[2]
			{
				otherValue.GetType().Name,
				ParameterName
			};
			byte condition = ((tupleValue != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.SilentAssert(condition != 0, "Attempted to CopyFrom value of type \"{0}\" to a TupleValue parameter named \"{1}\" @summary Attempted to copy mismatched value types @assign bwhitman", fmtParams);
			Elements.CopyFrom(tupleValue.Elements);
		}
	}

	private unsafe global::AssetObjects.TupleValue* GetValue()
	{
		return (global::AssetObjects.TupleValue*)m_pkValue;
	}
}
