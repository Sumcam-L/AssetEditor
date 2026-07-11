using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface IAnimationRecorderService
{
	string AnimationVideoRoot { get; }

	IEnumerable<string> AvailableCodecs { get; }

	int CompressionLevel { get; set; }

	IEnumerable<string> FromAnimationStates { get; }

	string SelectedCodec { get; set; }

	IEnumerable<string> ToAnimationStates { get; }

	event EventHandler BoundAnimationsChanged;

	void SetActiveEntity(InstanceType insType, string entityName);

	void Record(string fromAnimationState, string toAnimationState);
}
