using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IDSGInstance : IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IEnumerable<string> GetAnimationSlots();

	void AddAnimationSlot(string animationSlotName);

	void RemoveAnimationSlot(string animationSlotName);

	void ClearAnimationSlots();

	IEnumerable<string> GetTimelineSlots();

	void AddTimelineSlot(string timelineSlotName);

	void RemoveTimelineSlot(string timelineSlotName);

	void ClearTimelineSlots();
}
