using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable;

public interface IAnnotatedDiagram
{
	IEnumerable<IAnnotation> Annotations { get; }
}
