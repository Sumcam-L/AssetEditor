using System;
using System.Collections.Generic;

namespace Sce.Atf.Collections;

public static class LinkedListExtensions
{
	public static LinkedListNode<T> FindNext<T>(this LinkedList<T> list, LinkedListNode<T> node, T value)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (node == null)
		{
			return list.Find(value);
		}
		if (list != node.List)
		{
			throw new ArgumentException("The list does not contain the given node.");
		}
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (node = node.Next; node != null; node = node.Next)
		{
			if (value != null)
			{
				if (equalityComparer.Equals(node.Value, value))
				{
					return node;
				}
			}
			else if (node.Value == null)
			{
				return node;
			}
		}
		return null;
	}

	public static LinkedListNode<T> FindPrevious<T>(this LinkedList<T> list, LinkedListNode<T> node, T value)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (node == null)
		{
			return list.FindLast(value);
		}
		if (list != node.List)
		{
			throw new ArgumentException("The list does not contain the given node.");
		}
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (node = node.Previous; node != null; node = node.Previous)
		{
			if (value != null)
			{
				if (equalityComparer.Equals(node.Value, value))
				{
					return node;
				}
			}
			else if (node.Value == null)
			{
				return node;
			}
		}
		return null;
	}
}
