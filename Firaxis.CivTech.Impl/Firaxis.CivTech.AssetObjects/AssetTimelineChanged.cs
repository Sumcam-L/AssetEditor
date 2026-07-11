using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AssetTimelineChanged : EntityChangedEvent, IAssetTimelineChanged
{
	private ITimeline m_timeline = null;

	public virtual ITimeline ChangedTimeline
	{
		get
		{
			return m_timeline;
		}
		set
		{
			byte condition = ((value != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "Passing in null timeline!  This is invalid.");
			m_timeline = value;
		}
	}

	public AssetTimelineChanged()
	{
		SetChangeType(EntityChangeType.ECT_ASSET_TIMELINE_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		ITimeline timeline = m_timeline;
		if (timeline != null && !string.IsNullOrEmpty(timeline.Name))
		{
			timeline = m_timeline;
			Timeline timeline2 = (Timeline)timeline;
			if (timeline2 == null)
			{
				BugSubmitter.SilentAssert(condition: false, $"safe_cast<Timeline^> failure on the timeline member of the change event.  Actual Timeline Type: {timeline.GetType().ToString()}  @assign bwhitman");
				return;
			}
			global::AssetObjects.Timeline* timelinePointer = timeline2.GetTimelinePointer();
			if (timelinePointer == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EB_0040OPAOJLAK_0040Adding_003F5disposed_003F5Timeline_003F5to_003F5enti_0040), __arglist());
				global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040BEEFMDHH_0040nativePointer_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ID_0040ILKPEMIF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 44u);
				return;
			}
			global::AssetObjects.AssetTimelineChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAssetTimelineChanged_003E(changeList);
			if (ptr != null)
			{
				global::_003CModule_003E.AssetObjects_002EAssetTimelineChanged_002ESetTimeline(ptr, timelinePointer);
			}
		}
		else
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GB_0040JPFIBCII_0040Attempted_003F5to_003F5add_003F5an_003F5unassigned_003F5t_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ID_0040ILKPEMIF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 28u);
		}
	}
}
