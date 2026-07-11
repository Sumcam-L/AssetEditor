using System;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public interface IKnobAdapter : IDisposable
{
	object DefaultDataAsObject { get; }

	string Description { get; }

	AttributeInfo DescriptionAttribute { get; set; }

	DomNode DomNode { get; }

	string Label { get; set; }

	AttributeInfo LabelAttribute { get; set; }

	object ValueDataAsObject { get; set; }

	bool IsWrappingKnob(IKnob knob);

	void Update(IKnob knob);
}
