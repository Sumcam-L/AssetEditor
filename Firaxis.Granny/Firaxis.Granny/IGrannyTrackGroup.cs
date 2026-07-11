using System.Collections.Generic;

namespace Firaxis.Granny;

public interface IGrannyTrackGroup
{
	string Name { get; set; }

    IGrannyTransformTrack AddTransformTrack();
    List<IGrannyTransformTrack> TransformTracks { get; }
}
