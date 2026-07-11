using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Rendering.Dom;

public class SceneGraphBuilder
{
	private class DomNodeTreeView : ITreeView
	{
		public object Root => null;

		public IEnumerable<object> GetChildren(object parent)
		{
			if (!(parent is DomNode node))
			{
				yield break;
			}
			foreach (DomNode child in node.Children)
			{
				yield return child;
			}
		}
	}

	private readonly ITreeView m_treeView;

	private static readonly object s_lock = new object();

	private static readonly HashSet<object> s_ancestors = new HashSet<object>();

	public SceneGraphBuilder()
		: this(typeof(IRenderObject), new DomNodeTreeView())
	{
	}

	public SceneGraphBuilder(Type type)
		: this(type, new DomNodeTreeView())
	{
	}

	public SceneGraphBuilder(Type type, ITreeView treeView)
	{
		if (!typeof(IRenderObject).IsAssignableFrom(type))
		{
			throw new InvalidOperationException("Must be an IRenderObject");
		}
		m_treeView = treeView;
	}

	public SceneNode Build(object source, SceneNode parent)
	{
		bool cancelLoad = false;
		return BuildInternal(source, parent, forceBuild: true, ref cancelLoad);
	}

	public SceneNode Build(object source, SceneNode parent, ref bool cancelLoad)
	{
		return BuildInternal(source, parent, forceBuild: true, ref cancelLoad);
	}

	public SceneNode BuildNode(object source, SceneNode parent)
	{
		return BuildNodeInternal(source, parent, forceBuild: false);
	}

	private SceneNode BuildInternal(object source, SceneNode parent, bool forceBuild, ref bool cancelLoad)
	{
		SceneNode sceneNode = BuildNodeInternal(source, parent, forceBuild);
		if (sceneNode == null)
		{
			return parent;
		}
		if (cancelLoad)
		{
			return sceneNode;
		}
		lock (s_lock)
		{
			if (s_ancestors.Contains(source))
			{
				return sceneNode;
			}
			try
			{
				s_ancestors.Add(source);
				ISceneGraphHierarchy sceneGraphHierarchy = source.As<ISceneGraphHierarchy>();
				IEnumerable<object> enumerable = ((sceneGraphHierarchy != null) ? sceneGraphHierarchy.GetChildren() : m_treeView.GetChildren(source));
				foreach (object item in enumerable)
				{
					BuildInternal(item, sceneNode, forceBuild: false, ref cancelLoad);
				}
			}
			finally
			{
				s_ancestors.Remove(source);
			}
		}
		return sceneNode;
	}

	private SceneNode BuildNodeInternal(object source, SceneNode parent, bool forceBuild)
	{
		if (parent.Source != null)
		{
			IRenderableParent renderableParent = parent.Source.As<IRenderableParent>();
			if (renderableParent != null && !renderableParent.IsRenderableChild(source))
			{
				return null;
			}
		}
		SceneNode sceneNode = new SceneNode(source);
		foreach (IBuildSceneNode item in source.AsAll<IBuildSceneNode>())
		{
			if (item.CreateByGraphBuilder)
			{
				if (item is IRenderObject renderObject && renderObject.Init(sceneNode))
				{
					sceneNode.RenderObjects.Add(renderObject);
				}
				item.OnBuildNode(sceneNode);
			}
		}
		if (sceneNode.RenderObjects.Count > 0 || sceneNode.Source.Is<IBoundable>() || forceBuild)
		{
			parent?.Children.Add(sceneNode);
		}
		else
		{
			sceneNode = parent;
		}
		return sceneNode;
	}
}
