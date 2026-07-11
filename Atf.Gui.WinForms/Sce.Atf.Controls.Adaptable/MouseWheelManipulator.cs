using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class MouseWheelManipulator : ControlAdapter
{
	private readonly ITransformAdapter m_transformAdapter;

	public MouseWheelManipulator(ITransformAdapter transformAdapter)
	{
		m_transformAdapter = transformAdapter;
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseWheel += control_MouseWheel;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.MouseWheel -= control_MouseWheel;
	}

	private void control_MouseWheel(object sender, MouseEventArgs e)
	{
		PointF translation = m_transformAdapter.Translation;
		PointF scale = m_transformAdapter.Scale;
		PointF pointF = new PointF(((float)e.X - translation.X) / scale.X, ((float)e.Y - translation.Y) / scale.Y);
		float num = 1f + (float)e.Delta / 1200f;
		scale = TransformAdapters.ConstrainScale(scale: new PointF(scale.X * num, scale.Y * num), adapter: m_transformAdapter);
		translation = new PointF((float)e.X - pointF.X * scale.X, (float)e.Y - pointF.Y * scale.Y);
		m_transformAdapter.SetTransform(scale.X, scale.Y, translation.X, translation.Y);
	}
}
