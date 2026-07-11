using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ModelInstanceChanged : EntityChangedEvent, IModelInstanceChanged
{
	private string m_modelName = string.Empty;

	private string m_geoName = string.Empty;

	private IModelInstance m_model = null;

	public virtual IModelInstance Model
	{
		get
		{
			return m_model;
		}
		set
		{
			byte condition = ((value != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "It is not valid to pass in a null model to the model instance changed event.  @assign bwhitman");
			m_model = value;
		}
	}

	public virtual string GeoName
	{
		get
		{
			return m_geoName;
		}
		set
		{
			m_geoName = value;
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

	public ModelInstanceChanged()
	{
		SetChangeType(EntityChangeType.ECT_MODEL_INSTANCE_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		if (m_model != null && !string.IsNullOrWhiteSpace(m_modelName) && !string.IsNullOrWhiteSpace(m_geoName))
		{
			ModelInstance modelInstance = (ModelInstance)m_model;
			if (modelInstance == null)
			{
				BugSubmitter.SilentAssert(condition: false, $"safe_cast<ModelInstance^> failure on the model member of the change event.  Actual Model Type: {modelInstance.GetType().ToString()}  @assign bwhitman");
				return;
			}
			global::AssetObjects.ModelInstance* assetObject = modelInstance.GetAssetObject();
			if (assetObject == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040IKDKICFB_0040Adding_003F5disposed_003F5Model_003F5to_003F5entity_003F5_0040), __arglist());
				global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040KIKHKHHM_0040nativeModel_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ID_0040BPMFEKBJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 44u);
				return;
			}
			global::AssetObjects.ModelInstanceChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AModelInstanceChanged_003E(changeList);
			if (ptr == null)
			{
				return;
			}
			StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(m_modelName);
			try
			{
				standardStringWrapper = standardStringWrapper3;
				StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(m_geoName);
				try
				{
					standardStringWrapper2 = standardStringWrapper4;
					global::_003CModule_003E.AssetObjects_002EModelInstanceChanged_002ESetModelName(ptr, standardStringWrapper.Value);
					global::_003CModule_003E.AssetObjects_002EModelInstanceChanged_002ESetGeoName(ptr, standardStringWrapper2.Value);
					global::_003CModule_003E.AssetObjects_002EModelInstanceChanged_002ESetNewModel(ptr, assetObject);
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
		else
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FD_0040DOCBEMGA_0040Attempted_003F5to_003F5add_003F5a_003F5model_003F5changed_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ID_0040BPMFEKBJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 28u);
		}
	}
}
