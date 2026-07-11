using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Firaxis.Granny;

public interface IGrannyAnimation
{
	string Name { get; set; }

	float Duration { get; }

	float TimeStep { get; }

	float Oversampling { get; }

	List<int> EventCodes { get; }

	List<IGrannyTrackGroup> TrackGroups { get; }

    bool AddTrackGroupReference(IGrannyTrackGroup kTrackGroup);

    //unsafe bool Attach(granny_animation* pkAnimation);


    float[] SampleBone(IGrannyModel kModel, string szBoneName, float fTime);
}
