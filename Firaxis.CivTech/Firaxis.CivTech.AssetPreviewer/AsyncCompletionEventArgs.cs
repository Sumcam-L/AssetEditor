using System;

namespace Firaxis.CivTech.AssetPreviewer;

public class AsyncCompletionEventArgs<T> : EventArgs where T : class
{
	public readonly T Result;

	public AsyncCompletionEventArgs(T result)
	{
		Result = result;
	}
}
