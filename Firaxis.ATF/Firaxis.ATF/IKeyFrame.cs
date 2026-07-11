using System;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public interface IKeyFrame
{
	float Time { get; set; }

	event EventHandler TimeChanged;

	event EventHandler<DomNode> ValueChanged;
}
