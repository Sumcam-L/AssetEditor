using System.Collections.Generic;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.Asset;

public interface IStateTransitionNameProvider
{
	IDictionary<string, IList<StateTransitionInfo>> TimelineStateTransitions { get; }
}
