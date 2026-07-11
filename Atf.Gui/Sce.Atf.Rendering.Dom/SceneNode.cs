using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Rendering.Dom;

public class SceneNode
{
	private readonly object m_sourceObject;

	private readonly RenderObjectList m_renderObjects = new RenderObjectList();

	private readonly List<SceneNode> m_children = new List<SceneNode>();

	private readonly RenderStateStack m_stateStack = new RenderStateStack();

	private bool m_visible = true;

	public DomNode DomNode => m_sourceObject.As<DomNode>();

	public object Source => m_sourceObject;

	public ICollection<IRenderObject> RenderObjects => m_renderObjects;

	public IList<SceneNode> Children => m_children;

	public RenderStateStack StateStack => m_stateStack;

	public bool IsVisibile
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_visible = value;
		}
	}

	public SceneNode(object source)
	{
		m_sourceObject = source;
	}

	public SceneNode(SceneNode original, bool copyChildren)
	{
		m_sourceObject = original.m_sourceObject;
		for (LinkedListNode<IRenderObject> linkedListNode = original.m_renderObjects.InternalList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			m_renderObjects.InternalList.AddLast(linkedListNode.Value);
		}
		if (copyChildren)
		{
			throw new NotImplementedException("'copyChildren' must be false currently");
		}
		m_visible = original.m_visible;
	}

	public SceneNode FindNode(object source)
	{
		if (source == null)
		{
			return null;
		}
		Queue<SceneNode> queue = new Queue<SceneNode>();
		queue.Enqueue(this);
		return FindInternal(source, queue);
	}

	public SceneNode FindNode(Path<object> path)
	{
		SceneNode sceneNode = this;
		foreach (object item in path)
		{
			SceneNode sceneNode2 = sceneNode.FindNode(item);
			if (sceneNode2 != null)
			{
				sceneNode = sceneNode2;
			}
		}
		return (sceneNode.Source == path.Last) ? sceneNode : null;
	}

	public SceneNode[] FindNodePath(object obj)
	{
		List<SceneNode> list = new List<SceneNode>();
		FindNodePathInternal(obj, list);
		return list.ToArray();
	}

	public bool AddRenderObject<T>(bool addOnSubTree) where T : class, IRenderObject
	{
		bool result = false;
		if (m_sourceObject != null)
		{
			IRenderObject renderObject = m_sourceObject.As<T>();
			if (renderObject != null)
			{
				m_renderObjects.Add(renderObject);
				result = true;
			}
		}
		if (addOnSubTree)
		{
			foreach (SceneNode child in m_children)
			{
				child.AddRenderObject<T>(addOnSubTree);
			}
		}
		return result;
	}

	public void RemoveRenderObject(Type interfaceType, bool removeOnSubTree)
	{
		LinkedListNode<IRenderObject> linkedListNode = m_renderObjects.InternalList.First;
		while (linkedListNode != null)
		{
			LinkedListNode<IRenderObject> next = linkedListNode.Next;
			if (interfaceType.IsAssignableFrom(linkedListNode.Value.GetType()))
			{
				m_renderObjects.InternalList.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
		if (!removeOnSubTree)
		{
			return;
		}
		foreach (SceneNode child in m_children)
		{
			child.RemoveRenderObject(interfaceType, removeOnSubTree);
		}
	}

	public void Clear()
	{
		foreach (IRenderObject renderObject in m_renderObjects)
		{
			renderObject.Release();
		}
		m_renderObjects.Clear();
	}

	public void ClearSubGraph()
	{
		Clear();
		foreach (SceneNode child in m_children)
		{
			child.ClearSubGraph();
		}
		m_children.Clear();
	}

	private SceneNode FindInternal(object source, Queue<SceneNode> queue)
	{
		SceneNode sceneNode = null;
		while (queue.Count > 0 && sceneNode == null)
		{
			SceneNode sceneNode2 = queue.Dequeue();
			if (sceneNode2.m_sourceObject == source)
			{
				return sceneNode2;
			}
			foreach (SceneNode child in sceneNode2.m_children)
			{
				queue.Enqueue(child);
			}
		}
		return null;
	}

	private void FindNodePathInternal(object source, List<SceneNode> path)
	{
		if (m_sourceObject == source)
		{
			path.Add(this);
			return;
		}
		bool flag = false;
		foreach (SceneNode child in Children)
		{
			child.FindNodePathInternal(source, path);
			if (path.Count > 0)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			path.Add(this);
		}
	}
}
