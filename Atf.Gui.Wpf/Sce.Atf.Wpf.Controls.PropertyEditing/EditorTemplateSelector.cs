using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class EditorTemplateSelector : DataTemplateSelector
{
	private enum EditorKeyType
	{
		Style,
		Template
	}

	private static EditorTemplateSelector s_default;

	private static EditorTemplateSelector s_defaultNonEdit;

	private static List<Type> s_simpleTypes = new List<Type>(new Type[12]
	{
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(short),
		typeof(ushort),
		typeof(byte),
		typeof(sbyte),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(string)
	});

	public static EditorTemplateSelector Default => s_default ?? (s_default = new EditorTemplateSelector(new ObservableCollection<ValueEditor>()));

	public static EditorTemplateSelector DefaultNonEdit
	{
		get
		{
			object obj = s_defaultNonEdit;
			if (obj == null)
			{
				obj = new EditorTemplateSelector(new ObservableCollection<ValueEditor>())
				{
					SelectNonEditingTemplates = true
				};
				s_defaultNonEdit = (EditorTemplateSelector)obj;
			}
			return (EditorTemplateSelector)obj;
		}
	}

	public bool SelectNonEditingTemplates { get; set; }

	public IEnumerable<ValueEditor> Editors { get; private set; }

	public EditorTemplateSelector(IEnumerable<ValueEditor> editors)
	{
		Editors = editors;
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item is PropertyNode propertyNode)
		{
			ValueEditor customEditor = propertyNode.GetCustomEditor();
			if (customEditor != null)
			{
				if (customEditor.UsesCustomContext && propertyNode.EditorContext == null)
				{
					propertyNode.EditorContext = customEditor.GetCustomContext(propertyNode);
				}
				else if (!customEditor.UsesCustomContext)
				{
					propertyNode.EditorContext = null;
				}
				if (SelectNonEditingTemplates)
				{
					return customEditor.GetNonEditingTemplate(propertyNode, container);
				}
				return customEditor.GetTemplate(propertyNode, container);
			}
			if (Editors != null)
			{
				foreach (ValueEditor editor in Editors)
				{
					if (editor.CanEdit(propertyNode))
					{
						if (SelectNonEditingTemplates)
						{
							return editor.GetNonEditingTemplate(propertyNode, container);
						}
						if (editor.UsesCustomContext && propertyNode.EditorContext == null)
						{
							propertyNode.EditorContext = editor.GetCustomContext(propertyNode);
						}
						else if (!editor.UsesCustomContext)
						{
							propertyNode.EditorContext = null;
						}
						return editor.GetTemplate(propertyNode, container);
					}
				}
			}
			return GetDefaultTemplate(propertyNode, container);
		}
		return new DataTemplate();
	}

	private DataTemplate GetDefaultTemplate(PropertyNode node, DependencyObject container)
	{
		if (node != null)
		{
			bool editable = node.IsWriteable && !SelectNonEditingTemplates;
			object editorKey = GetEditorKey(node, editable, EditorKeyType.Template);
			if (editorKey != null)
			{
				if (container is FrameworkElement frameworkElement)
				{
					return frameworkElement.FindResource(editorKey) as DataTemplate;
				}
				return Application.Current.FindResource(editorKey) as DataTemplate;
			}
		}
		return null;
	}

	private static object GetEditorKey(PropertyNode node, bool editable, EditorKeyType keyType)
	{
		Type propertyType = node.PropertyType;
		if (propertyType != null)
		{
			if (propertyType == typeof(bool))
			{
				if (keyType == EditorKeyType.Style)
				{
					return PropertyGrid.BoolEditorStyleKey;
				}
				return PropertyGrid.BoolEditorTemplateKey;
			}
			if (editable)
			{
				if (node.StandardValues != null)
				{
					if (keyType == EditorKeyType.Style)
					{
						return PropertyGrid.ComboEditorStyleKey;
					}
					return PropertyGrid.ComboEditorTemplateKey;
				}
				if (s_simpleTypes.Contains(propertyType))
				{
					if (keyType == EditorKeyType.Style)
					{
						return PropertyGrid.DefaultTextEditorStyleKey;
					}
					return PropertyGrid.DefaultTextEditorTemplateKey;
				}
			}
		}
		if (keyType == EditorKeyType.Style)
		{
			return PropertyGrid.ReadOnlyStyleKey;
		}
		return PropertyGrid.ReadOnlyTemplateKey;
	}
}
