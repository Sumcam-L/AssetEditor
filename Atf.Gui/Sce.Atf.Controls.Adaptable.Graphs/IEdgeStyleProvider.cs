using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IEdgeStyleProvider
{
	EdgeStyle EdgeStyle { get; set; }

	IEnumerable<EdgeStyleData> GetData(DiagramRenderer render, Point worldOffset, D2dGraphics g);
}
