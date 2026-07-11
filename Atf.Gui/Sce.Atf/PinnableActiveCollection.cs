using System;
using System.Reflection;

namespace Sce.Atf;

public class PinnableActiveCollection<T> : ActiveCollection<T> where T : class
{
	public override T ActiveItem
	{
		get
		{
			return base.ActiveItem;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			T activeItem = ActiveItem;
			if (value == activeItem)
			{
				return;
			}
			OnActiveItemChanging(EventArgs.Empty);
			if (Contains(value))
			{
				T val = base.RawList[base.RawList.IndexOf(value)];
				if (val is IPinnable && value is IPinnable)
				{
					IPinnable pinnable = val as IPinnable;
					IPinnable pinnable2 = value as IPinnable;
					if (pinnable.Pinned != pinnable2.Pinned)
					{
						pinnable2.Pinned = pinnable.Pinned;
					}
				}
				base.RawList.Remove(value);
				base.RawList.Add(value);
			}
			else
			{
				if (base.Count == base.MaximumCount)
				{
					int index = 0;
					T val2 = null;
					foreach (T raw in base.RawList)
					{
						if (!(raw is IPinnable) || !((IPinnable)raw).Pinned)
						{
							val2 = raw;
							break;
						}
					}
					if (val2 != null)
					{
						index = base.RawList.IndexOf(val2);
						base.RawList.Remove(val2);
					}
					else
					{
						val2 = base.RawList[0];
						base.RawList.RemoveAt(0);
					}
					OnItemRemoved(new ItemRemovedEventArgs<T>(index, val2));
				}
				base.RawList.Add(value);
				OnItemAdded(new ItemInsertedEventArgs<T>(0, value));
			}
			OnActiveItemChanged(EventArgs.Empty);
		}
	}

	public PinnableActiveCollection()
	{
	}

	public PinnableActiveCollection(int maximumCount)
		: base(maximumCount)
	{
	}

	public bool? GetPinnedState(Uri uri)
	{
		foreach (T raw in base.RawList)
		{
			if (!(raw is IPinnable))
			{
				continue;
			}
			IPinnable pinnable = raw as IPinnable;
			MemberInfo[] member = pinnable.GetType().GetMember("Uri");
			MemberInfo[] array = member;
			foreach (MemberInfo memberInfo in array)
			{
				if (!(memberInfo is PropertyInfo))
				{
					continue;
				}
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				MethodInfo getMethod = propertyInfo.GetGetMethod();
				if (getMethod != null)
				{
					Uri uri2 = getMethod.Invoke(pinnable, null) as Uri;
					if (uri2 != null && object.Equals(uri2, uri))
					{
						return pinnable.Pinned;
					}
				}
			}
		}
		return null;
	}
}
