using System.Collections.Generic;

namespace Firaxis.MVVMBase.Helpers;

public class TaskStack : TaskFactory
{
	private readonly Stack<ITaskGenerator> _generators;

	public override int Count => _generators.Count;

	public TaskStack(int maxConcurrent = 10)
		: base(maxConcurrent)
	{
		_generators = new Stack<ITaskGenerator>(maxConcurrent);
	}

	public override ITaskGenerator GetNext()
	{
		return _generators.Pop();
	}

	protected override void AddInternal(ITaskGenerator taskGenerator)
	{
		_generators.Push(taskGenerator);
	}
}
