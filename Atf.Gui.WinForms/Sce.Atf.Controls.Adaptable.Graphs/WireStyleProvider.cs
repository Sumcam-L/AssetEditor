using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class WireStyleProvider<TElement, TWire, TPin> : DomNodeAdapter, IEdgeStyleProvider where TElement : class, ICircuitElement where TWire : class, IGraphEdge<TElement, TPin> where TPin : class, ICircuitPin
{
	private class GroupPinData
	{
		public PointF Pos;

		public GroupPin GroupPin;

		public ICircuitGroupType<TElement, TWire, TPin> Group;

		public bool IsReal;
	}

	private EdgeStyle m_edgeStyle;

	private bool m_useDefaulStyle = true;

	public EdgeStyle EdgeStyle
	{
		get
		{
			return m_useDefaulStyle ? CircuitDefaultStyle.EdgeStyle : m_edgeStyle;
		}
		set
		{
			m_edgeStyle = value;
			m_useDefaulStyle = false;
		}
	}

	IEnumerable<EdgeStyleData> IEdgeStyleProvider.GetData(DiagramRenderer render, Point worldOffset, D2dGraphics g)
	{
		D2dCircuitRenderer<TElement, TWire, TPin> circuitRender = render as D2dCircuitRenderer<TElement, TWire, TPin>;
		IList<GroupPinData> groupPinChainData = GetGroupPinChainData(circuitRender, worldOffset, g);
		IEnumerable<EdgeStyleData> result = EmptyEnumerable<EdgeStyleData>.Instance;
		if (EdgeStyle == EdgeStyle.DirectCurve)
		{
			GroupPinData[] array = groupPinChainData.Where((GroupPinData pt) => pt.Group == null || !pt.Group.Expanded || pt.Group.Info.ShowExpandedGroupPins).ToArray();
			EdgeStyleData[] array2 = new EdgeStyleData[array.Length - 1];
			for (int num = 0; num < array.Length - 1; num++)
			{
				BezierCurve2F edgeData = SetupBezierCurve(array[num].Pos, array[num + 1].Pos);
				EdgeStyleData edgeStyleData = new EdgeStyleData
				{
					ShapeType = EdgeStyleData.EdgeShape.Bezier,
					EdgeData = edgeData
				};
				array2[num] = edgeStyleData;
			}
			result = array2;
		}
		else if (EdgeStyle == EdgeStyle.Default)
		{
			EdgeStyleData[] array3 = new EdgeStyleData[groupPinChainData.Count - 1];
			for (int num2 = 0; num2 < groupPinChainData.Count - 1; num2++)
			{
				BezierCurve2F edgeData2 = SetupBezierCurve(groupPinChainData[num2].Pos, groupPinChainData[num2 + 1].Pos);
				EdgeStyleData edgeStyleData2 = new EdgeStyleData
				{
					ShapeType = EdgeStyleData.EdgeShape.Bezier,
					EdgeData = edgeData2
				};
				array3[num2] = edgeStyleData2;
			}
			result = array3;
		}
		else if (EdgeStyle == EdgeStyle.Polyline)
		{
			EdgeStyleData edgeStyleData3 = new EdgeStyleData();
			PointF[] array4 = new PointF[groupPinChainData.Count];
			for (int num3 = 0; num3 < groupPinChainData.Count; num3++)
			{
				array4[num3] = groupPinChainData[num3].Pos;
			}
			edgeStyleData3.ShapeType = EdgeStyleData.EdgeShape.Polyline;
			edgeStyleData3.EdgeData = array4;
			result = new EdgeStyleData[1] { edgeStyleData3 };
		}
		return result;
	}

	private static BezierCurve2F SetupBezierCurve(PointF p0, PointF p1)
	{
		float num = Math.Abs(p1.X - p0.X) / 2f;
		return new BezierCurve2F(new Vec2F(p0.X, p0.Y), new Vec2F(p0.X + num, p0.Y), new Vec2F(p1.X - num, p1.Y), new Vec2F(p1.X, p1.Y));
	}

	private void InterpolateData(IList<Pair<PointF, Group>> dataPoints, int start, int end)
	{
		Vec2F vec2F = new Vec2F(dataPoints[start - 1].First);
		Vec2F vec2F2 = new Vec2F(dataPoints[end + 1].First);
		Vec2F vec2F3 = vec2F2 - vec2F;
		for (int i = start; i <= end; i++)
		{
			float num = (dataPoints[i].First.X - vec2F.X) / vec2F3.X;
			Vec2F vec2F4 = vec2F + num * vec2F3;
			dataPoints[i] = new Pair<PointF, Group>(new PointF(vec2F4.X, vec2F4.Y), dataPoints[i].Second);
		}
	}

	private IList<GroupPinData> GetGroupPinChainData(D2dCircuitRenderer<TElement, TWire, TPin> circuitRender, Point worldOffset, D2dGraphics g)
	{
		Wire wire = base.DomNode.Cast<Wire>();
		List<GroupPinData> list = new List<GroupPinData>();
		if (circuitRender != null)
		{
			List<ICircuitGroupType<TElement, TWire, TPin>> list2 = new List<ICircuitGroupType<TElement, TWire, TPin>>();
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = wire.OutputElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
			foreach (GroupPin item in wire.OutputPinSinkChain)
			{
				Point pinPosition = circuitRender.GetPinPosition(circuitGroupType.Cast<TElement>(), item.Index, inputSide: false, g);
				pinPosition.Offset(worldOffset);
				pinPosition.Offset(circuitRender.WorldOffset(list2.AsIEnumerable<TElement>()));
				list2.Add(circuitGroupType);
				list.Add(new GroupPinData
				{
					Pos = pinPosition,
					Group = circuitGroupType,
					GroupPin = item
				});
				if (!circuitGroupType.Expanded)
				{
					break;
				}
				circuitGroupType = item.InternalElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
			}
			list.Reverse();
			if (list2.Any())
			{
				ICircuitGroupType<TElement, TWire, TPin> circuitGroupType2 = list2.Last();
				if (circuitGroupType2.Expanded)
				{
					GroupPin groupPin = list[0].GroupPin;
					Point pinPosition2 = circuitRender.GetPinPosition(groupPin.InternalElement.Cast<TElement>(), groupPin.InternalPinIndex, inputSide: false, g);
					pinPosition2.Offset(worldOffset);
					pinPosition2.Offset(circuitRender.WorldOffset(list2.AsIEnumerable<TElement>()));
					list.Insert(0, new GroupPinData
					{
						Pos = pinPosition2
					});
				}
			}
			else
			{
				Point pinPosition3 = circuitRender.GetPinPosition(wire.OutputElement.Cast<TElement>(), wire.OutputPin.Index, inputSide: false, g);
				pinPosition3.Offset(worldOffset);
				list.Add(new GroupPinData
				{
					Pos = pinPosition3
				});
			}
			int count = list.Count;
			list[list.Count - 1].IsReal = true;
			list2.Clear();
			circuitGroupType = wire.InputElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
			foreach (GroupPin item2 in wire.InputPinSinkChain)
			{
				Point pinPosition4 = circuitRender.GetPinPosition(circuitGroupType.Cast<TElement>(), item2.Index, inputSide: true, g);
				pinPosition4.Offset(worldOffset);
				pinPosition4.Offset(circuitRender.WorldOffset(list2.AsIEnumerable<TElement>()));
				list2.Add(circuitGroupType);
				list.Add(new GroupPinData
				{
					Pos = pinPosition4,
					Group = circuitGroupType,
					GroupPin = item2
				});
				if (!circuitGroupType.Expanded)
				{
					break;
				}
				circuitGroupType = item2.InternalElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
			}
			if (list2.Any())
			{
				GroupPin groupPin2 = list[list.Count - 1].GroupPin;
				ICircuitGroupType<TElement, TWire, TPin> circuitGroupType3 = list[list.Count - 1].Group;
				if (circuitGroupType3.Expanded)
				{
					Point pinPosition5 = circuitRender.GetPinPosition(groupPin2.InternalElement.Cast<TElement>(), groupPin2.InternalPinIndex, inputSide: true, g);
					pinPosition5.Offset(worldOffset);
					pinPosition5.Offset(circuitRender.WorldOffset(list2.AsIEnumerable<TElement>()));
					list.Add(new GroupPinData
					{
						Pos = pinPosition5
					});
				}
			}
			else
			{
				Point pinPosition6 = circuitRender.GetPinPosition(wire.InputElement.Cast<TElement>(), wire.InputPin.Index, inputSide: true, g);
				pinPosition6.Offset(worldOffset);
				list.Add(new GroupPinData
				{
					Pos = pinPosition6
				});
			}
			list[count].IsReal = true;
		}
		return list;
	}
}
