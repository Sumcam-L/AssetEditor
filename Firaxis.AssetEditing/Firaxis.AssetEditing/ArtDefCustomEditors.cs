using System;
using System.Collections.Generic;
using Firaxis.ATF;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class ArtDefCustomEditors
{
	private interface ICustomEditorRegistry
	{
		object GetEditorInstance(object adpater);
	}

	private class CustomEditorRegistry<ET, AT> : ICustomEditorRegistry
	{
		private IDictionary<object, object> m_instances = new Dictionary<object, object>();

		public virtual object GetEditorInstance(object adapter)
		{
			if (!m_instances.ContainsKey(adapter))
			{
				m_instances[adapter] = Activator.CreateInstance(typeof(ET), (AT)adapter);
			}
			return m_instances[adapter];
		}
	}

	private static IDictionary<string, IList<KeyValuePair<Type, ICustomEditorRegistry>>> m_customEditors;

	static ArtDefCustomEditors()
	{
		m_customEditors = new Dictionary<string, IList<KeyValuePair<Type, ICustomEditorRegistry>>>();
		RegisterEditor<ArtDefCollectionKeyFrameContext, ArtDefCollectionAdapter>("KeyFrame", typeof(KeyFrameEditingContextBase));
		RegisterEditor<ArtDefElementKeyFrame, ArtDefElementAdapter>("KeyFrame", typeof(IKeyFrame));
		RegisterEditor<ArtDefCollectionListContext, ArtDefCollectionAdapter>("GridView", typeof(IGridViewEditingContext));
	}

	private static void RegisterEditor<ET, AT>(string editorName, Type editingContextType)
	{
		new List<KeyValuePair<Type, ICustomEditorRegistry>>();
		if (!m_customEditors.ContainsKey(editorName))
		{
			m_customEditors[editorName] = new List<KeyValuePair<Type, ICustomEditorRegistry>>();
		}
		foreach (KeyValuePair<Type, ICustomEditorRegistry> item in m_customEditors[editorName])
		{
			if (item.Key == editingContextType)
			{
				return;
			}
		}
		m_customEditors[editorName].Add(new KeyValuePair<Type, ICustomEditorRegistry>(editingContextType, new CustomEditorRegistry<ET, AT>()));
	}

	public static object GetCustomEditorAdapter(string editorName, Type type, DomNodeAdapter domAdapter)
	{
		if (!m_customEditors.ContainsKey(editorName))
		{
			return null;
		}
		foreach (KeyValuePair<Type, ICustomEditorRegistry> item in m_customEditors[editorName])
		{
			if (type.IsAssignableFrom(item.Key))
			{
				return item.Value.GetEditorInstance(domAdapter);
			}
		}
		return null;
	}
}
