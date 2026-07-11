namespace Sce.Atf.Dom;

public abstract class Observer : DomNodeAdapter
{
	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += OnAttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		AddSubtree(base.DomNode);
	}

	protected virtual void OnAttributeChanged(object sender, AttributeEventArgs e)
	{
	}

	protected virtual void OnChildInserted(object sender, ChildEventArgs e)
	{
	}

	protected virtual void OnChildRemoved(object sender, ChildEventArgs e)
	{
	}

	protected virtual void AddNode(DomNode node)
	{
	}

	protected virtual void RemoveNode(DomNode node)
	{
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		AddSubtree(e.Child);
		OnChildInserted(sender, e);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		RemoveSubtree(e.Child);
		OnChildRemoved(sender, e);
	}

	private void AddSubtree(DomNode root)
	{
		foreach (DomNode item in root.Subtree)
		{
			AddNode(item);
		}
	}

	private void RemoveSubtree(DomNode root)
	{
		foreach (DomNode item in root.Subtree)
		{
			RemoveNode(item);
		}
	}
}
