using System;

namespace Sce.Atf.Applications;

public interface ISkinService
{
	ISkin ActiveSkin { get; set; }

	event EventHandler SkinChangedOrApplied;
}
