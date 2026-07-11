using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class CurveSegmentAdapter : DomNodeAdapter
{
	private ICurveSegment m_segment;

	public ICurveSegment CurveSegment => m_segment;

	public float GetCurveValue(float X)
	{
		return m_segment.GetValue(X);
	}

	public virtual void Initialize(ICurveSegment segment)
	{
		m_segment = segment;
	}
}
