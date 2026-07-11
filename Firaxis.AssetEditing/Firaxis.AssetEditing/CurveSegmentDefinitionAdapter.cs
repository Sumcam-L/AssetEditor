using System;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class CurveSegmentDefinitionAdapter : DomNodeAdapter
{
	private ICurveSegmentDefinition m_segmentDefinition;

	public ICurveSegmentDefinition SegmentDefinition => m_segmentDefinition;

	public float StartingPoint
	{
		get
		{
			return GetAttribute<float>(FieldSchema.CurveSegmentDefinitionType.StartingPointAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.CurveSegmentDefinitionType.StartingPointAttribute, value);
		}
	}

	public CurveSegmentAdapter Curve
	{
		get
		{
			return GetChild<CurveSegmentAdapter>(FieldSchema.CurveSegmentDefinitionType.CurveSegmentChild);
		}
		set
		{
			SetChild(FieldSchema.CurveSegmentDefinitionType.CurveSegmentChild, value);
		}
	}

	public static CurveSegmentDefinitionAdapter Create(CurveSegmentType segmentType, params object[] args)
	{
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		ICurveSegmentDefinition curveSegmentDefinition = null;
		try
		{
			switch (segmentType)
			{
			default:
				BugSubmitter.Assert(condition: false, "Attempted to create an unsupported curve type!  Add support here.");
				break;
			case CurveSegmentType.CST_LINEAR:
				curveSegmentDefinition = civTechContext.CreateInstance<ICurveSegmentDefinition<ILinearCurveSegment>>(args);
				break;
			case CurveSegmentType.CST_CONSTANT:
				curveSegmentDefinition = civTechContext.CreateInstance<ICurveSegmentDefinition<IConstantCurveSegment>>(args);
				break;
			}
		}
		catch (Exception ex)
		{
			BugSubmitter.Assert(false, "Attempted to create a curve with invalid construction arguments.  Exception message:\n\n{0}", ex.ToString());
		}
		if (curveSegmentDefinition == null)
		{
			return null;
		}
		return Create(curveSegmentDefinition);
	}

	public static CurveSegmentDefinitionAdapter Create(ICurveSegmentDefinition segmentDefinition)
	{
		DomNode domNode = new DomNode(FieldSchema.CurveSegmentDefinitionType.Type);
		domNode.InitializeExtensions();
		CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = domNode.As<CurveSegmentDefinitionAdapter>();
		curveSegmentDefinitionAdapter.Initialize(segmentDefinition);
		return curveSegmentDefinitionAdapter;
	}

	public void Initialize(ICurveSegmentDefinition segmentDefinition)
	{
		m_segmentDefinition = segmentDefinition;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		StartingPoint = segmentDefinition.StartingPoint;
		Curve = CreateSegmentAdapter(segmentDefinition.Curve);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private CurveSegmentAdapter CreateSegmentAdapter(ICurveSegment curveSegment)
	{
		DomNodeType segmentAdapterType = GetSegmentAdapterType(curveSegment);
		if (segmentAdapterType == null)
		{
			return null;
		}
		DomNode domNode = new DomNode(segmentAdapterType);
		domNode.InitializeExtensions();
		CurveSegmentAdapter curveSegmentAdapter = domNode.As<CurveSegmentAdapter>();
		curveSegmentAdapter.Initialize(curveSegment);
		return curveSegmentAdapter;
	}

	private DomNodeType GetSegmentAdapterType(ICurveSegment curveSegment)
	{
		Type type = curveSegment.GetType();
		if (typeof(IConstantCurveSegment).IsAssignableFrom(type))
		{
			return FieldSchema.ConstantCurveSegmentType.Type;
		}
		if (typeof(ILinearCurveSegment).IsAssignableFrom(type))
		{
			return FieldSchema.LinearCurveSegmentType.Type;
		}
		BugSubmitter.Assert(condition: false, "Add the segment type to this factory function.");
		return null;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.CurveSegmentDefinitionType.StartingPointAttribute)
		{
			m_segmentDefinition.StartingPoint = StartingPoint;
		}
	}
}
