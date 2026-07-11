using System.Collections.Generic;
using Firaxis.Asset;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface IBehaviorProviderAdapter : IAnimatableEntityAdapter, IInstanceEntityAdapter, INamedAdapter, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, IAttachmentPointNameProvider
{
	TimelineSetAdapter TimelineSet { get; }

	TimelineBindingSetAdapter TimelineBindingSet { get; }

	AttachmentPointSetAdapter AttachmentPointSet { get; }

	IBehaviorDataProvider BehaviorData { get; }

	IEnumerable<string> AllowedBehaviorClasses { get; }

	IEnumerable<string> AllowedTriggerLightClasses { get; }

	IEnumerable<string> AllowedTriggerVFXClasses { get; }

	IEnumerable<string> ReferenceGeometryNames { get; }

	IEnumerable<IAssetArtDefReference> AllowedBehaviorArtDefs { get; }

	string GetAnimationName(string slotName);
}
