using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IAsyncToken
{
	bool IsDone { get; }

	event EventHandler Completed;

	void Start();

	void Wait();

	void Wait(TimeSpan timeSpan);
}
public interface IAsyncToken<T> : IAsyncToken where T : class
{
	T Result { get; }

	event EventHandler<AsyncCompletionEventArgs<T>> ResultReady;

	void Wait(out T result);

	void Wait(TimeSpan timeSpan, out T result);
}
