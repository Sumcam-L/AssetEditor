using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls.Adaptable;

public interface ITransformAdapter : IControlAdapter
{
	Matrix Transform { get; }

	PointF Translation { get; set; }

	PointF MinTranslation { get; set; }

	PointF MaxTranslation { get; set; }

	PointF Scale { get; set; }

	PointF MinScale { get; set; }

	PointF MaxScale { get; set; }

	bool UniformScale { get; }

	event EventHandler TransformChanged;

	void SetTransform(float xScale, float yScale, float xTranslation, float yTranslation);
}
