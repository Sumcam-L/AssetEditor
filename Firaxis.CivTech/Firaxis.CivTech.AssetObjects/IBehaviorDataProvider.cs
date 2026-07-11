using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IBehaviorDataProvider : IAnimatable
{
	ITimelineBindingSet TimelineBindings { get; }

	ITimelineSet Timelines { get; }

	IAttachmentPointSet AttachmentPointSet { get; }

	IEnumerable<string> ReferencedBehaviors { get; }

	void ClearBehaviorOverrides();

	void Flatten(IInstanceSet instances, IClassSet classes, IBehaviorInstance targetBehavior);

	void Export(IBehaviorInstance targetBehavior);

	void AddBehavior(IBehaviorInstance behavior);

	void RemoveBehavior(IBehaviorInstance behavior);

	void RemoveBehavior(string behaviorName);

	void MoveChildBehaviors(int childIndex1, int childIndex2);

	void MoveChildBehaviors(string childName, int desiredPosition);

	void MoveChildBehaviors(string childNameOne, string childNameTwo);
}
