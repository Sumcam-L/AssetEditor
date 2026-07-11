using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LinearCurveSegmentAdapter : CurveSegmentAdapter
{
	private ILinearCurveSegment m_segment;

	public float FirstValue
	{
		get
		{
			return GetAttribute<float>(FieldSchema.LinearCurveSegmentType.FirstValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.LinearCurveSegmentType.FirstValueAttribute, value);
		}
	}

	public float LastValue
	{
		get
		{
			return GetAttribute<float>(FieldSchema.LinearCurveSegmentType.LastValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.LinearCurveSegmentType.LastValueAttribute, value);
		}
	}

	public override void Initialize(ICurveSegment curveSegment)
	{
		base.Initialize(curveSegment);
		BugSubmitter.Assert(typeof(ILinearCurveSegment).IsAssignableFrom(curveSegment.GetType()), "LinearCurveSegmentAdapter has been passed a curve of the wrong type.");
		m_segment = (ILinearCurveSegment)curveSegment;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		FirstValue = m_segment.FirstValue;
		LastValue = m_segment.LastValue;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.LinearCurveSegmentType.FirstValueAttribute)
		{
			m_segment.FirstValue = FirstValue;
		}
		else if (e.AttributeInfo == FieldSchema.LinearCurveSegmentType.LastValueAttribute)
		{
			m_segment.LastValue = LastValue;
		}
	}
}
