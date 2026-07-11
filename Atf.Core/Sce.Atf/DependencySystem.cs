using System.Collections.Generic;

namespace Sce.Atf;

public class DependencySystem<T>
{
	public struct InvalidDependent
	{
		public readonly T Dependent;

		public readonly IEnumerable<T> Dependencies;

		public InvalidDependent(T dependent, IEnumerable<T> dependencies)
		{
			Dependent = dependent;
			Dependencies = dependencies;
		}
	}

	private readonly Dictionary<T, List<T>> m_dependenciesToDependents = new Dictionary<T, List<T>>();

	private readonly HashSet<T> m_invalidDependencies = new HashSet<T>();

	public bool NeedsUpdate => m_invalidDependencies.Count > 0;

	public void AddDependency(T dependent, T dependency)
	{
		if (!m_dependenciesToDependents.TryGetValue(dependency, out var value))
		{
			value = new List<T>();
			m_dependenciesToDependents.Add(dependency, value);
		}
		value.Add(dependent);
	}

	public void RemoveDependency(T dependent, T dependency)
	{
		if (m_dependenciesToDependents.TryGetValue(dependency, out var value))
		{
			value.Remove(dependent);
			if (value.Count == 0)
			{
				m_dependenciesToDependents.Remove(dependency);
			}
		}
	}

	public void Invalidate(T dependency)
	{
		if (m_dependenciesToDependents.ContainsKey(dependency))
		{
			m_invalidDependencies.Add(dependency);
		}
	}

	public void Cancel()
	{
		m_invalidDependencies.Clear();
	}

	public IEnumerable<InvalidDependent> GetInvalidDependents()
	{
		Dictionary<T, List<T>> dependentsToDependencies = new Dictionary<T, List<T>>();
		Queue<T> queue = new Queue<T>(m_invalidDependencies);
		HashSet<T> visited = new HashSet<T>(m_invalidDependencies);
		m_invalidDependencies.Clear();
		while (queue.Count > 0)
		{
			T dependency = queue.Dequeue();
			if (m_dependenciesToDependents.TryGetValue(dependency, out var dependents))
			{
				foreach (T dependent in dependents)
				{
					if (!dependentsToDependencies.TryGetValue(dependent, out var dependencies))
					{
						dependencies = new List<T>();
						dependentsToDependencies.Add(dependent, dependencies);
					}
					dependencies.Add(dependency);
					if (!visited.Contains(dependent))
					{
						queue.Enqueue(dependent);
						visited.Add(dependent);
					}
					dependencies = null;
				}
			}
			dependents = null;
		}
		Stack<T> stack = new Stack<T>();
		visited.Clear();
		foreach (KeyValuePair<T, List<T>> item in dependentsToDependencies)
		{
			T dependent2 = item.Key;
			if (visited.Contains(dependent2))
			{
				continue;
			}
			stack.Push(dependent2);
			visited.Add(dependent2);
			while (stack.Count > 0)
			{
				dependent2 = stack.Peek();
				bool canUpdate = true;
				dependentsToDependencies.TryGetValue(dependent2, out var dependencies2);
				foreach (T dependency2 in dependencies2)
				{
					if (!visited.Contains(dependency2) && dependentsToDependencies.ContainsKey(dependency2))
					{
						canUpdate = false;
						stack.Push(dependency2);
						visited.Add(dependency2);
					}
				}
				if (canUpdate)
				{
					stack.Pop();
					yield return new InvalidDependent(dependent2, dependencies2);
				}
				dependencies2 = null;
			}
		}
	}
}
