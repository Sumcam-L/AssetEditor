using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public abstract class DependencyValidator : Validator
{
	private readonly DependencySystem<DomNode> m_dependencySystem = new DependencySystem<DomNode>();

	protected abstract IEnumerable<DomNode> GetDependencies(DomNode dependent);

	protected void Invalidate(DomNode domNode)
	{
		m_dependencySystem.Invalidate(domNode);
	}

	protected abstract void Update(DomNode dependent, IEnumerable<DomNode> dependencies);

	protected override void OnEnded(object sender, EventArgs e)
	{
		if (!m_dependencySystem.NeedsUpdate)
		{
			return;
		}
		foreach (DependencySystem<DomNode>.InvalidDependent invalidDependent in m_dependencySystem.GetInvalidDependents())
		{
			Update(invalidDependent.Dependent, invalidDependent.Dependencies);
		}
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		m_dependencySystem.Cancel();
	}

	protected override void AddNode(DomNode node)
	{
		foreach (DomNode dependency in GetDependencies(node))
		{
			m_dependencySystem.AddDependency(node, dependency);
		}
		base.AddNode(node);
	}

	protected override void RemoveNode(DomNode node)
	{
		foreach (DomNode dependency in GetDependencies(node))
		{
			m_dependencySystem.RemoveDependency(node, dependency);
		}
		base.RemoveNode(node);
	}
}
