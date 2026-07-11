using System;
using System.Collections.Generic;
using System.IO;

namespace Sce.Atf.Dom;

public class DomNodeSerializer
{
	private class Reference
	{
		public readonly DomNode Node;

		public readonly AttributeInfo Info;

		public readonly int RefId;

		public Reference(DomNode node, AttributeInfo info, int refId)
		{
			Node = node;
			Info = info;
			RefId = refId;
		}
	}

	public byte[] Serialize(IEnumerable<DomNode> nodes)
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		byte[] result = EmptyArray<byte>.Instance;
		List<DomNode> list = new List<DomNode>(nodes);
		Dictionary<DomNode, int> nodeIds = new Dictionary<DomNode, int>();
		AssignNodeIds(list, nodeIds);
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(list.Count);
			foreach (DomNode item in list)
			{
				Serialize(item, nodeIds, binaryWriter);
			}
			result = memoryStream.ToArray();
		}
		return result;
	}

	public IEnumerable<DomNode> Deserialize(byte[] data, Func<string, DomNodeType> getNodeType)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (getNodeType == null)
		{
			throw new ArgumentNullException("getNodeType");
		}
		List<DomNode> list = new List<DomNode>();
		List<Reference> references = new List<Reference>();
		using (MemoryStream input = new MemoryStream(data))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = (list.Capacity = binaryReader.ReadInt32());
			for (int i = 0; i < num; i++)
			{
				DomNode item = Deserialize(binaryReader, getNodeType, references);
				list.Add(item);
			}
		}
		FixReferences(list, references);
		return list;
	}

	private static void AssignNodeIds(IEnumerable<DomNode> nodes, Dictionary<DomNode, int> nodeIds)
	{
		int num = 0;
		foreach (DomNode node in nodes)
		{
			foreach (DomNode item in node.Subtree)
			{
				if (nodeIds.ContainsKey(item))
				{
					throw new InvalidOperationException("duplicate nodes in stream");
				}
				nodeIds.Add(item, num++);
			}
		}
	}

	private static void Serialize(DomNode node, Dictionary<DomNode, int> nodeIds, BinaryWriter writer)
	{
		writer.Write(node.Type.Name);
		foreach (AttributeInfo attribute in node.Type.Attributes)
		{
			object localAttribute = node.GetLocalAttribute(attribute);
			if (attribute.Type.Type == AttributeTypes.Reference)
			{
				if (!(localAttribute is DomNode key) || !nodeIds.TryGetValue(key, out var value))
				{
					writer.Write(value: false);
					continue;
				}
				writer.Write(value: true);
				writer.Write(value);
			}
			else if (localAttribute == null)
			{
				writer.Write(value: false);
			}
			else
			{
				writer.Write(value: true);
				writer.Write(attribute.Type.Convert(localAttribute));
			}
		}
		foreach (ChildInfo child2 in node.Type.Children)
		{
			if (child2.IsList)
			{
				IList<DomNode> childList = node.GetChildList(child2);
				writer.Write(childList.Count);
				foreach (DomNode item in childList)
				{
					Serialize(item, nodeIds, writer);
				}
			}
			else
			{
				DomNode child = node.GetChild(child2);
				if (child == null)
				{
					writer.Write(value: false);
					continue;
				}
				writer.Write(value: true);
				Serialize(child, nodeIds, writer);
			}
		}
	}

	private static DomNode Deserialize(BinaryReader reader, Func<string, DomNodeType> getNodeType, List<Reference> references)
	{
		string arg = reader.ReadString();
		DomNodeType domNodeType = getNodeType(arg);
		if (domNodeType == null)
		{
			throw new InvalidOperationException("unknown node type");
		}
		DomNode domNode = new DomNode(domNodeType);
		foreach (AttributeInfo attribute in domNodeType.Attributes)
		{
			if (reader.ReadBoolean())
			{
				if (attribute.Type.Type == AttributeTypes.Reference)
				{
					int refId = reader.ReadInt32();
					references.Add(new Reference(domNode, attribute, refId));
				}
				else
				{
					string s = reader.ReadString();
					object value = attribute.Type.Convert(s);
					domNode.SetAttribute(attribute, value);
				}
			}
		}
		foreach (ChildInfo child2 in domNodeType.Children)
		{
			if (child2.IsList)
			{
				int num = reader.ReadInt32();
				IList<DomNode> childList = domNode.GetChildList(child2);
				for (int i = 0; i < num; i++)
				{
					DomNode item = Deserialize(reader, getNodeType, references);
					childList.Add(item);
				}
			}
			else if (reader.ReadBoolean())
			{
				DomNode child = Deserialize(reader, getNodeType, references);
				domNode.SetChild(child2, child);
			}
		}
		return domNode;
	}

	private static void FixReferences(List<DomNode> nodeList, List<Reference> references)
	{
		Dictionary<int, DomNode> dictionary = new Dictionary<int, DomNode>();
		int num = 0;
		foreach (DomNode node in nodeList)
		{
			foreach (DomNode item in node.Subtree)
			{
				dictionary.Add(num++, item);
			}
		}
		foreach (Reference reference in references)
		{
			DomNode value = dictionary[reference.RefId];
			reference.Node.SetAttribute(reference.Info, value);
		}
	}
}
