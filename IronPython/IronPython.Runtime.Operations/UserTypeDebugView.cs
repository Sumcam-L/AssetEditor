using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public class UserTypeDebugView
{
	private readonly IPythonObject _userObject;

	public PythonType __class__ => _userObject.PythonType;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	internal List<ObjectDebugView> Members
	{
		get
		{
			List<ObjectDebugView> list = new List<ObjectDebugView>();
			if (_userObject.Dict != null)
			{
				foreach (KeyValuePair<object, object> item in _userObject.Dict)
				{
					list.Add(new ObjectDebugView(item.Key, item.Value));
				}
			}
			object[] slots = _userObject.GetSlots();
			if (slots != null)
			{
				IList<PythonType> resolutionOrder = _userObject.PythonType.ResolutionOrder;
				List<string> list2 = new List<string>();
				for (int num = resolutionOrder.Count - 1; num >= 0; num--)
				{
					list2.AddRange(resolutionOrder[num].GetTypeSlots());
				}
				for (int i = 0; i < slots.Length - 1; i++)
				{
					if (slots[i] != Uninitialized.Instance)
					{
						list.Add(new ObjectDebugView(list2[i], slots[i]));
					}
				}
			}
			return list;
		}
	}

	public UserTypeDebugView(IPythonObject userObject)
	{
		_userObject = userObject;
	}
}
