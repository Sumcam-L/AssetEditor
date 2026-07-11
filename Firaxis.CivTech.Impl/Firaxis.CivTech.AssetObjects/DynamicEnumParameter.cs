using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.Utility;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class DynamicEnumParameter : Parameter, IDynamicEnumParameter
{
	private class EnumeratorInstanceReceipt : Receipt, IDisposable
	{
		private DynamicEnumParameter m_owner;

		public unsafe EnumeratorInstanceReceipt(DynamicEnumParameter owner)
		{
			//IL_003e: Expected I, but got I8
			m_owner = owner;
			base._002Ector();
			try
			{
				if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003F0EnumeratorInstanceReceipt_0040DynamicEnumParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAV2345_0040_0040Z_00404_NA && m_owner.m_instanceEnums == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040CJPLJCBP_0040m_owner_003F9_003F_0024DOm_instanceEnums_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040LHAHBGJB_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 36u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003F0EnumeratorInstanceReceipt_0040DynamicEnumParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAV2345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		private void _007EEnumeratorInstanceReceipt()
		{
			_0021EnumeratorInstanceReceipt();
		}

		private unsafe void _0021EnumeratorInstanceReceipt()
		{
			//IL_002d: Expected I, but got I8
			if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003CFinalizer_003E_0040EnumeratorInstanceReceipt_0040DynamicEnumParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_owner.m_instanceEnums == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040CJPLJCBP_0040m_owner_003F9_003F_0024DOm_instanceEnums_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040LHAHBGJB_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 42u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003CFinalizer_003E_0040EnumeratorInstanceReceipt_0040DynamicEnumParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			m_owner.m_instanceEnums = Enumerable.Empty<string>();
		}

		[HandleProcessCorruptedStateExceptions]
		protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
		{
			if (A_0)
			{
				try
				{
					_007EEnumeratorInstanceReceipt();
					return;
				}
				finally
				{
					base.Dispose();
				}
			}
			try
			{
				_0021EnumeratorInstanceReceipt();
			}
			finally
			{
				base.Finalize();
			}
		}

		public sealed override void Dispose()
		{
			Dispose(A_0: true);
			GC.SuppressFinalize(this);
		}

		~EnumeratorInstanceReceipt()
		{
			Dispose(A_0: false);
		}
	}

	private IEnumerable<string> m_instanceEnums = Enumerable.Empty<string>();

	public unsafe virtual string EnumerationProperty
	{
		get
		{
			return new string(global::_003CModule_003E.AssetObjects_002EDynamicEnumParameter_002EGetSourceProperty((global::AssetObjects.DynamicEnumParameter*)m_pkParameter));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EDynamicEnumParameter_002ESetSourceProperty((global::AssetObjects.DynamicEnumParameter*)m_pkParameter, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe DynamicEnumParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ADynamicEnumParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe DynamicEnumParameter(global::AssetObjects.DynamicEnumParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	public unsafe virtual Receipt SetEnumeratorInstance(object inst)
	{
		//IL_003b: Expected I, but got I8
		string enumerationProperty = EnumerationProperty;
		if (string.IsNullOrEmpty(enumerationProperty))
		{
			return null;
		}
		Type type = inst.GetType();
		if (!global::_003CModule_003E._003FA0x8b38bdd9_002E_003FbIgnoreAlways_0040_003F5_003F_003FSetEnumeratorInstance_0040DynamicEnumParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVReceipt_0040Utility_00405_0040PE_0024AAVObject_0040System_0040_0040_0040Z_00404_NA && (object)type == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08PGLLJKOJ_0040instType_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HM_0040NAAMLAJO_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 33u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x8b38bdd9_002E_003FbIgnoreAlways_0040_003F5_003F_003FSetEnumeratorInstance_0040DynamicEnumParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVReceipt_0040Utility_00405_0040PE_0024AAVObject_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		PropertyInfo property = type.GetProperty(enumerationProperty);
		if ((object)property == null)
		{
			return null;
		}
		object value = property.GetValue(inst);
		if (value == null)
		{
			return null;
		}
		Type type2 = value.GetType();
		if (!typeof(IEnumerable<string>).IsAssignableFrom(type2))
		{
			return null;
		}
		m_instanceEnums = (IEnumerable<string>)value;
		return new EnumeratorInstanceReceipt(this);
	}

	public virtual IList<string> GetEnumerations()
	{
		return new List<string>(m_instanceEnums);
	}

	private unsafe global::AssetObjects.DynamicEnumParameter* GetParameter()
	{
		return (global::AssetObjects.DynamicEnumParameter*)m_pkParameter;
	}
}
