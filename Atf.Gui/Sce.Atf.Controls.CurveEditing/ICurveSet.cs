using System.Collections.Generic;

namespace Sce.Atf.Controls.CurveEditing;

public interface ICurveSet
{
	IList<ICurve> Curves { get; }
}
