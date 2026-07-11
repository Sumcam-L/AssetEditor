using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class LightTagDirectionChanged : EntityChangedEvent, ILightTagDirectionChanged
{
	private IEnvironmentLightDirectionTag m_lightTag = null;

	public virtual IEnvironmentLightDirectionTag LightTag
	{
		get
		{
			return m_lightTag;
		}
		set
		{
			byte condition = ((value != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "It is invalid to set light tag to null.  @assign bwhitman");
			m_lightTag = value;
		}
	}

	public LightTagDirectionChanged()
	{
		SetChangeType(EntityChangeType.ECT_LIGHT_DIRECTION_TAG_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		IEnvironmentLightDirectionTag lightTag = m_lightTag;
		if (lightTag == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GI_0040GPNCILBJ_0040Attempted_003F5to_003F5add_003F5a_003F5light_003F5tag_003F5cha_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040JOAEEBAF_0040m_lightTag_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040KEFGPAIM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 30u);
			return;
		}
		EnvironmentLightDirectionTag environmentLightDirectionTag = (EnvironmentLightDirectionTag)lightTag;
		if (environmentLightDirectionTag == null)
		{
			BugSubmitter.SilentAssert(condition: false, $"safe_cast<EnvironmentLightDirectionTag^> failure on the tag member of the change event.  Actual LightTagType Type: {lightTag.GetType().ToString()}  @assign bwhitman");
			return;
		}
		global::AssetObjects.EnvironmentLightDirectionTag* unmanaged = environmentLightDirectionTag.GetUnmanaged();
		if (unmanaged == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FF_0040CMDADGFD_0040Adding_003F5disposed_003F5EnvironmentLight_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BF_0040BBOLJAPF_0040nativeTag_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040KEFGPAIM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 46u);
			return;
		}
		LightDirectionTagChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003ALightDirectionTagChanged_003E(changeList);
		if (ptr != null)
		{
			global::_003CModule_003E.AssetObjects_002ELightDirectionTagChanged_002ESetNewLightTag(ptr, unmanaged);
		}
	}
}
