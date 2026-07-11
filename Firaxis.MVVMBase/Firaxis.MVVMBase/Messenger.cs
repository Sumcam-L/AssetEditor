using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Firaxis.MVVMBase;

public static class Messenger
{
	private interface IMessageRegistration
	{
		IWeakAction Handler { get; }
	}

	private class MessageRegistration : IMessageRegistration
	{
		public WeakAction Handler { get; }

		IWeakAction IMessageRegistration.Handler => Handler;

		public MessageRegistration(WeakAction handler)
		{
			Handler = handler;
		}
	}

	private class MessageRegistration<T> : IMessageRegistration
	{
		public WeakAction<T> Handler { get; }

		IWeakAction IMessageRegistration.Handler => Handler;

		public MessageRegistration(WeakAction<T> handler)
		{
			Handler = handler;
		}
	}

	private static readonly object _lock = new object();

	private static readonly ConcurrentDictionary<Type, LinkedList<IMessageRegistration>> _typedRegistrations = new ConcurrentDictionary<Type, LinkedList<IMessageRegistration>>();

	private static readonly ConcurrentDictionary<object, LinkedList<MessageRegistration>> _hashRegistrations = new ConcurrentDictionary<object, LinkedList<MessageRegistration>>();

	public static void RegisterByType<T>(Action<T> handler)
	{
		LinkedList<IMessageRegistration> orAdd = _typedRegistrations.GetOrAdd(typeof(T), (Type type) => new LinkedList<IMessageRegistration>());
		orAdd.AddLast(new MessageRegistration<T>(new WeakAction<T>(handler)));
	}

	public static void UnRegisterByType<T>(Action<T> handler)
	{
		if (!_typedRegistrations.TryGetValue(typeof(T), out var value) || value.Count <= 0)
		{
			return;
		}
		lock (_lock)
		{
			LinkedListNode<IMessageRegistration> linkedListNode = value.First;
			do
			{
				LinkedListNode<IMessageRegistration> next = linkedListNode.Next;
				if (linkedListNode.Value.Handler.ActionEquals(handler))
				{
					value.Remove(linkedListNode);
				}
				linkedListNode = next;
			}
			while (linkedListNode != null);
		}
	}

	public static void SendByType<T>(T message)
	{
		if (!_typedRegistrations.TryGetValue(typeof(T), out var value) || value.Count <= 0)
		{
			return;
		}
		lock (_lock)
		{
			LinkedListNode<IMessageRegistration> linkedListNode = value.First;
			do
			{
				LinkedListNode<IMessageRegistration> next = linkedListNode.Next;
				if (!linkedListNode.Value.Handler.Execute(message))
				{
					value.Remove(linkedListNode);
				}
				linkedListNode = next;
			}
			while (linkedListNode != null);
		}
	}

	public static void RegisterByToken(object token, Action handler)
	{
		_hashRegistrations.AddOrUpdate(token, (object type) => new LinkedList<MessageRegistration>(), delegate(object type, LinkedList<MessageRegistration> list)
		{
			list.AddLast(new MessageRegistration(new WeakAction(handler)));
			return list;
		});
	}

	public static void UnRegisterByToken(object token, Action handler)
	{
		if (!_hashRegistrations.TryGetValue(token, out var value) || value.Count <= 0)
		{
			return;
		}
		lock (_lock)
		{
			LinkedListNode<MessageRegistration> linkedListNode = value.First;
			do
			{
				LinkedListNode<MessageRegistration> next = linkedListNode.Next;
				if (linkedListNode.Value.Handler.ActionEquals(handler))
				{
					value.Remove(linkedListNode);
				}
				linkedListNode = next;
			}
			while (linkedListNode != null);
		}
	}

	public static void SendByToken(object token)
	{
		if (!_hashRegistrations.TryGetValue(token, out var value) || value.Count <= 0)
		{
			return;
		}
		lock (_lock)
		{
			LinkedListNode<MessageRegistration> linkedListNode = value.First;
			do
			{
				LinkedListNode<MessageRegistration> next = linkedListNode.Next;
				if (!linkedListNode.Value.Handler.Execute())
				{
					value.Remove(linkedListNode);
				}
				linkedListNode = next;
			}
			while (linkedListNode != null);
		}
	}
}
