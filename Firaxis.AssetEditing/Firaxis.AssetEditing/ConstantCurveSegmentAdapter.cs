using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ConstantCurveSegmentAdapter : CurveSegmentAdapter
{
	private IConstantCurveSegment m_segment;

	public float ConstantValue
	{
		get
		{
			return GetAttribute<float>(FieldSchema.ConstantCurveSegmentType.ConstantValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.ConstantCurveSegmentType.ConstantValueAttribute, value);
		}
	}

	public override void Initialize(ICurveSegment curveSegment)
	{
		base.Initialize(curveSegment);
		BugSubmitter.Assert(typeof(IConstantCurveSegment).IsAssignableFrom(curveSegment.GetType()), "ConstantCurveSegmentAdapter has been passed a curve of the wrong type.");
		m_segment = (IConstantCurveSegment)curveSegment;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		ConstantValue = m_segment.ConstantValue;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.ConstantCurveSegmentType.ConstantValueAttribute)
		{
			m_segment.ConstantValue = ConstantValue;
		}
	}
}
