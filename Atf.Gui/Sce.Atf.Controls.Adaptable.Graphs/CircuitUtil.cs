using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class CircuitUtil
{
	private class EmptyGraph : IGraph<Element, Wire, ICircuitPin>
	{
		public IEnumerable<Element> Nodes => EmptyEnumerable<Element>.Instance;

		public IEnumerable<Wire> Edges => EmptyEnumerable<Wire>.Instance;
	}

	private static EmptyGraph s_emptyGraph = new EmptyGraph();

	public static IGraph<Element, Wire, ICircuitPin> EmptyCircuit => s_emptyGraph;

	public static void GetSubGraph(ICircuitContainer graphContainer, IEnumerable<object> objects, HashSet<Element> modules, ICollection<Wire> internalConnections, ICollection<Wire> incomingConnections, ICollection<Wire> outgoingConnections)
	{
		foreach (Element item in objects.AsIEnumerable<Element>())
		{
			modules.Add(item);
		}
		foreach (Wire wire in graphContainer.Wires)
		{
			bool flag = modules.Contains(wire.OutputElement);
			bool flag2 = modules.Contains(wire.InputElement);
			if (flag && flag2)
			{
				internalConnections.Add(wire);
			}
			else if (flag)
			{
				outgoingConnections.Add(wire);
			}
			else if (flag2)
			{
				incomingConnections.Add(wire);
			}
		}
	}

	public static string GetDomNodeName(DomNode domNode)
	{
		string text = string.Empty;
		if (domNode.Is<Element>())
		{
			text = domNode.Cast<Element>().Name;
		}
		else if (domNode.Is<GroupPin>())
		{
			text = "Group Pin : " + domNode.Cast<GroupPin>().Name;
		}
		else if (domNode.Is<Wire>())
		{
			Wire wire = domNode.Cast<Wire>();
			text = ((!wire.IsValid(out var inputPinIndex, out var outputPinIndex)) ? ("Edge from " + wire.OutputElement.Name + "[" + outputPinIndex + "] to " + wire.InputElement.Name + "[" + inputPinIndex + "]") : ("Edge from " + wire.OutputElement.Name + "[" + wire.OutputPin.Name + "] to " + wire.InputElement.Name + "[" + wire.InputPin.Name + "]"));
		}
		else if (domNode.Is<Circuit>())
		{
			IDocument document = domNode.As<IDocument>();
			text = ((document == null || !(document.Uri != null)) ? ("Circuit " + domNode.GetId()) : ("Circuit " + Path.GetFileNameWithoutExtension(document.Uri.LocalPath)));
		}
		if (text == string.Empty)
		{
			text = domNode.GetId() ?? domNode.ToString();
		}
		return text;
	}

	public static bool IsGroupTemplateInstance(object node)
	{
		return node.Is<IReference<Group>>();
	}

	public static bool IsTemplateTargetMissing(object node)
	{
		IReference<DomNode> reference = node.As<IReference<DomNode>>();
		if (reference == null)
		{
			return false;
		}
		Element element = reference.Target.As<Element>();
		if (element == null)
		{
			return false;
		}
		return element.Type is MissingElementType;
	}

	public static string GetGroupPath(Group group)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (DomNode item in group.DomNode.Lineage.Reverse())
		{
			if (item.Is<IDocument>())
			{
				stringBuilder.Append(Path.GetFileNameWithoutExtension(item.Cast<IDocument>().Uri.LocalPath));
			}
			else if (item.Is<Element>())
			{
				stringBuilder.Append(item.Cast<Element>().Name);
			}
			stringBuilder.Append(":");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Length--;
		}
		return stringBuilder.ToString();
	}

	public static Group GetGroupTemplate(object templateInstance)
	{
		IReference<Group> reference = templateInstance.Cast<IReference<Group>>();
		return reference.Target;
	}

	public static TEdgeRoute EdgeRouteTraverser<TEdgeRoute>(AdaptablePath<object> hitPath, object destNode, TEdgeRoute hitRoute) where TEdgeRoute : class, ICircuitPin
	{
		if (!hitPath.Last.Is<Element>())
		{
			return null;
		}
		int num = hitPath.Count - 1;
		int num2 = hitPath.IndexOf(destNode);
		if (num2 < 0 || num2 > num)
		{
			return null;
		}
		TEdgeRoute val = hitRoute;
		object obj = hitPath.Last;
		for (int num3 = num - 1; num3 >= num2; num3--)
		{
			ICircuitPin circuitPin = null;
			object obj2 = hitPath[num3];
			if (obj2.Is<Group>())
			{
				Group obj3 = obj2.Cast<Group>();
				foreach (GroupPin inputGroupPin in obj3.InputGroupPins)
				{
					if (inputGroupPin.InternalElement.Equals(obj) && inputGroupPin.InternalElement.InputPin(inputGroupPin.InternalPinIndex) == val)
					{
						circuitPin = inputGroupPin;
						break;
					}
				}
				if (circuitPin == null)
				{
					foreach (GroupPin outputGroupPin in obj3.OutputGroupPins)
					{
						if (outputGroupPin.InternalElement.Equals(obj) && outputGroupPin.InternalElement.OutputPin(outputGroupPin.InternalPinIndex) == val)
						{
							circuitPin = outputGroupPin;
							break;
						}
					}
				}
			}
			if (circuitPin == null)
			{
				return null;
			}
			val = circuitPin.Cast<TEdgeRoute>();
			obj = obj2;
		}
		return val;
	}
}
