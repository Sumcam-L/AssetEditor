using System;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface ITransformAdapter
{
	Matrix Transform { get; }

	Point Translation { get; set; }

	Point MinTranslation { get; set; }

	Point MaxTranslation { get; set; }

	Point Scale { get; set; }

	Point MinScale { get; set; }

	Point MaxScale { get; set; }

	bool UniformScale { get; }

	event EventHandler TransformChanged;

	void SetTransform(double xScale, double yScale, double xTranslation, double yTranslation);
}
