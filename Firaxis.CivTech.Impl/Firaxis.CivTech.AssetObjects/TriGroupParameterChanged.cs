using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class TriGroupParameterChanged : EntityChangedEvent, ITriGroupParameterChanged
{
	private string m_modelName = string.Empty;

	private string m_meshName = string.Empty;

	private string m_groupName = string.Empty;

	private string m_stateName = string.Empty;

	private string m_parameterName = string.Empty;

	private IValue m_value = null;

	public virtual IValue ChangedValue
	{
		get
		{
			return m_value;
		}
		set
		{
			byte condition = ((value != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "Value has to be set in a TriGroupParameterChanged event.");
			m_value = value;
		}
	}

	public virtual string ParameterName
	{
		get
		{
			return m_parameterName;
		}
		set
		{
			m_parameterName = value;
		}
	}

	public virtual string StateName
	{
		get
		{
			return m_stateName;
		}
		set
		{
			m_stateName = value;
		}
	}

	public virtual string GroupName
	{
		get
		{
			return m_groupName;
		}
		set
		{
			m_groupName = value;
		}
	}

	public virtual string MeshName
	{
		get
		{
			return m_meshName;
		}
		set
		{
			m_meshName = value;
		}
	}

	public virtual string ModelName
	{
		get
		{
			return m_modelName;
		}
		set
		{
			m_modelName = value;
		}
	}

	public TriGroupParameterChanged()
	{
		SetChangeType(EntityChangeType.ECT_TRIGROUP_PARAMETER_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		StandardStringWrapper standardStringWrapper4 = null;
		StandardStringWrapper standardStringWrapper5 = null;
		if (m_value == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040DAABCIJO_0040Attempted_003F5to_003F5add_003F5a_003F5tri_003F5group_003F5par_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040HDLLDNNF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 29u);
			return;
		}
		Value value = (Value)ChangedValue;
		if (value == null)
		{
			BugSubmitter.SilentAssert(condition: false, $"safe_cast<Value^> failure on the value member of the change event.  Actual Value Type: {m_value.GetType().ToString()}  @assign bwhitman");
			return;
		}
		global::AssetObjects.Value* assetObject = value.GetAssetObject();
		if (assetObject == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040DIGIPNHJ_0040Adding_003F5disposed_003F5Value_003F5to_003F5entity_003F5_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040EFACGLD_0040nativeValue_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040HDLLDNNF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 45u);
			return;
		}
		if (!HasAssignedName())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D4);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HN_0040LHNGMCAG_0040Attempted_003F5to_003F5add_003F5a_003F5tri_003F5group_003F5par_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040HDLLDNNF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 51u);
			return;
		}
		global::AssetObjects.TriGroupParameterChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003ATriGroupParameterChanged_003E(changeList);
		if (ptr == null)
		{
			return;
		}
		StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(m_modelName);
		try
		{
			standardStringWrapper = standardStringWrapper6;
			StandardStringWrapper standardStringWrapper7 = new StandardStringWrapper(m_meshName);
			try
			{
				standardStringWrapper2 = standardStringWrapper7;
				StandardStringWrapper standardStringWrapper8 = new StandardStringWrapper(m_groupName);
				try
				{
					standardStringWrapper3 = standardStringWrapper8;
					StandardStringWrapper standardStringWrapper9 = new StandardStringWrapper(m_stateName);
					try
					{
						standardStringWrapper4 = standardStringWrapper9;
						StandardStringWrapper standardStringWrapper10 = new StandardStringWrapper(m_parameterName);
						try
						{
							standardStringWrapper5 = standardStringWrapper10;
							global::_003CModule_003E.AssetObjects_002ETriGroupParameterChanged_002ESetModelName(ptr, standardStringWrapper.Value);
							global::_003CModule_003E.AssetObjects_002ETriGroupParameterChanged_002ESetMeshName(ptr, standardStringWrapper2.Value);
							global::_003CModule_003E.AssetObjects_002ETriGroupParameterChanged_002ESetGroupName(ptr, standardStringWrapper3.Value);
							global::_003CModule_003E.AssetObjects_002ETriGroupParameterChanged_002ESetStateName(ptr, standardStringWrapper4.Value);
							global::_003CModule_003E.AssetObjects_002ETriGroupParameterChanged_002ESetParameterName(ptr, standardStringWrapper5.Value);
							global::_003CModule_003E.AssetObjects_002ETriGroupParameterChanged_002ESetNewValue(ptr, assetObject);
						}
						catch
						{
							//try-fault
							((IDisposable)standardStringWrapper5).Dispose();
							throw;
						}
						((IDisposable)standardStringWrapper5).Dispose();
					}
					catch
					{
						//try-fault
						((IDisposable)standardStringWrapper4).Dispose();
						throw;
					}
					((IDisposable)standardStringWrapper4).Dispose();
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper3).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper3).Dispose();
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private bool HasAssignedName()
	{
		int num = ((!string.IsNullOrEmpty(m_stateName) && !string.IsNullOrEmpty(m_groupName) && !string.IsNullOrEmpty(m_meshName) && !string.IsNullOrEmpty(m_parameterName) && !string.IsNullOrEmpty(m_modelName)) ? 1 : 0);
		return (byte)num != 0;
	}
}
