using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class CurveAdapter : DomNodeAdapter
{
	private ICurve m_curve;

	private IList<CurveSegmentDefinitionAdapter> m_curveSegments;

	public ICurve Curve => m_curve;

	public IList<CurveSegmentDefinitionAdapter> CurveSegments => m_curveSegments;

	public void Initialize(ICurve adaptedCurve)
	{
		m_curve = adaptedCurve;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		PopulateChildren(m_curve);
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	public void UpdateDomFromNative(ICurve curve)
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		foreach (CurveSegmentDefinitionAdapter curveSegment in m_curveSegments)
		{
			curveSegment.DomNode.AttributeChanged -= ChildDomNode_AttributeChanged;
		}
		m_curveSegments.Clear();
		m_curve = curve;
		PopulateChildren(m_curve);
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void PopulateChildren(ICurve curve)
	{
		foreach (ICurveSegmentDefinition curveSegment in curve.CurveSegments)
		{
			CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = CurveSegmentDefinitionAdapter.Create(curveSegment);
			curveSegmentDefinitionAdapter.DomNode.AttributeChanged += ChildDomNode_AttributeChanged;
			m_curveSegments.Add(curveSegmentDefinitionAdapter);
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		m_curveSegments = new DomNodeListAdapter<CurveSegmentDefinitionAdapter>(base.DomNode, FieldSchema.CurveType.CurveSegmentsChild);
	}

	private void ChildDomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		DomNode child = e.Child;
		if (child.Type == FieldSchema.CurveSegmentDefinitionType.Type)
		{
			CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = child.As<CurveSegmentDefinitionAdapter>();
			m_curve.RemoveCurveSegment(curveSegmentDefinitionAdapter.SegmentDefinition);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		DomNode child = e.Child;
		if (child.Type == FieldSchema.CurveSegmentDefinitionType.Type)
		{
			CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = child.As<CurveSegmentDefinitionAdapter>();
			m_curve.AddCurveSegment(curveSegmentDefinitionAdapter.SegmentDefinition);
		}
	}
}
