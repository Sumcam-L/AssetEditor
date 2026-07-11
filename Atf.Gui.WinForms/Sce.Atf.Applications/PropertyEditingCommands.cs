using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(PropertyEditingCommands))]
[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PropertyEditingCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	private enum Command
	{
		CopyProperty,
		PasteProperty,
		ResetProperty,
		CopyAll,
		PasteAll,
		ResetAll,
		ViewInTextEditor
	}

	private IPropertyEditingContext m_editingContext;

	private System.ComponentModel.PropertyDescriptor m_descriptor;

	private System.ComponentModel.PropertyDescriptor m_copyDescriptor;

	private object m_copyValue;

	private readonly Dictionary<string, object> m_descriptorToValue = new Dictionary<string, object>();

	private readonly ICommandService m_commandService;

	private readonly SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();

	[ImportingConstructor]
	public PropertyEditingCommands(ICommandService commandService)
	{
		m_commandService = commandService;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(Command.CopyProperty, null, null, "Copy Property".Localize("Copies this property's value to the local clipboard"), "Copies this property's value to the to local clipboard".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.PasteProperty, null, null, "Paste Property".Localize("Pastes the local clipboard into this property's value"), "Pastes the local clipboard into this property's value".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.ResetProperty, null, null, "Reset Property".Localize("Reset the current property to its default value"), "Reset the current property to its default value".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.CopyAll, null, null, "Copy All".Localize("Copies all properties to the local clipboard"), "Copies all properties to the local clipboard".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.PasteAll, null, null, "Paste All".Localize("Pastes the local clipboard into all properties"), "Pastes the local clipboard into all properties".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.ResetAll, null, null, "Reset All".Localize("Reset all properties to their default values"), "Reset all properties to their default values".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		m_commandService.RegisterCommand(Command.ViewInTextEditor, null, null, "View In Text Editor".Localize("Open the file in the associated text editor"), "Open the file in the associated text editor".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is Command && m_editingContext != null)
		{
			switch ((Command)commandTag)
			{
			case Command.CopyProperty:
				return m_descriptor != null && !(m_descriptor is ChildPropertyDescriptor) && !(m_descriptor is ChildAttributeCollectionPropertyDescriptor);
			case Command.PasteProperty:
			{
				object component = m_editingContext.Items.LastOrDefault();
				return m_descriptor != null && CanPaste(m_copyValue, m_copyDescriptor, m_descriptor, m_descriptor.GetValue(component));
			}
			case Command.ResetProperty:
				return CanResetValue(m_editingContext.Items, m_descriptor);
			case Command.CopyAll:
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in m_editingContext.PropertyDescriptors)
				{
					if (propertyDescriptor is ChildPropertyDescriptor || propertyDescriptor is ChildAttributeCollectionPropertyDescriptor || (propertyDescriptor is AttributePropertyDescriptor attributePropertyDescriptor && attributePropertyDescriptor.AttributeInfo.IsIdAttribute))
					{
						continue;
					}
					return true;
				}
				break;
			case Command.PasteAll:
				return m_descriptorToValue.Count > 0;
			case Command.ResetAll:
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor2 in m_editingContext.PropertyDescriptors)
				{
					if (CanResetValue(m_editingContext.Items, propertyDescriptor2))
					{
						return true;
					}
				}
				break;
			case Command.ViewInTextEditor:
				if (m_descriptor != null && m_descriptor.GetEditor(typeof(UITypeEditor)) is FileUriEditor)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		ITransactionContext context = m_editingContext.As<ITransactionContext>();
		switch ((Command)commandTag)
		{
		case Command.CopyProperty:
			if (!(m_descriptor is ChildPropertyDescriptor))
			{
				object component = m_editingContext.Items.LastOrDefault();
				m_copyDescriptor = m_descriptor;
				m_copyValue = m_descriptor.GetValue(component);
			}
			break;
		case Command.PasteProperty:
			context.DoTransaction(delegate
			{
				foreach (object item in m_editingContext.Items)
				{
					PropertyUtils.SetProperty(item, m_descriptor, m_copyValue);
				}
			}, string.Format("Paste: {0}".Localize("'Paste' is a verb and this is the name of a command"), m_descriptor.DisplayName));
			break;
		case Command.ResetProperty:
			context.DoTransaction(delegate
			{
				PropertyUtils.ResetProperty(m_editingContext.Items, m_descriptor);
			}, string.Format("Reset: {0}".Localize("'Reset' is a verb and this is the name of a command"), m_descriptor.DisplayName));
			break;
		case Command.CopyAll:
		{
			m_descriptorToValue.Clear();
			object component2 = m_editingContext.Items.LastOrDefault();
			{
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in m_editingContext.PropertyDescriptors)
				{
					if (!(propertyDescriptor is ChildPropertyDescriptor) && (!(propertyDescriptor is AttributePropertyDescriptor attributePropertyDescriptor) || !attributePropertyDescriptor.AttributeInfo.IsIdAttribute))
					{
						m_descriptorToValue.Add(propertyDescriptor.GetPropertyDescriptorKey(), propertyDescriptor.GetValue(component2));
					}
				}
				break;
			}
		}
		case Command.PasteAll:
			context.DoTransaction(delegate
			{
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor2 in m_editingContext.PropertyDescriptors)
				{
					if (!propertyDescriptor2.IsReadOnly && m_descriptorToValue.TryGetValue(propertyDescriptor2.GetPropertyDescriptorKey(), out var value))
					{
						foreach (object item2 in m_editingContext.Items)
						{
							PropertyUtils.SetProperty(item2, propertyDescriptor2, value);
						}
					}
				}
			}, "Paste All".Localize("'Paste' is a verb and this is the name of a command"));
			break;
		case Command.ResetAll:
			context.DoTransaction(delegate
			{
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor3 in m_editingContext.PropertyDescriptors)
				{
					foreach (object item3 in m_editingContext.Items)
					{
						if (propertyDescriptor3.CanResetValue(item3))
						{
							propertyDescriptor3.ResetValue(item3);
						}
					}
				}
			}, "Reset All Properties".Localize("'Reset' is a verb and this is the name of a command"));
			break;
		case Command.ViewInTextEditor:
		{
			FileUriEditor fileUriEditor = m_descriptor.GetEditor(typeof(UITypeEditor)) as FileUriEditor;
			Uri uri = m_descriptor.GetValue(m_editingContext.Items.LastOrDefault()) as Uri;
			if (uri != null && File.Exists(uri.LocalPath))
			{
				Process.Start(fileUriEditor.AssociatedTextEditor, uri.LocalPath);
			}
			break;
		}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public IEnumerable<object> GetCommands(object context, object target)
	{
		m_editingContext = null;
		m_descriptor = null;
		System.ComponentModel.PropertyDescriptor descriptor = target as System.ComponentModel.PropertyDescriptor;
		if (context != null)
		{
			m_editingContext = context.As<IPropertyEditingContext>();
			if (m_editingContext == null)
			{
				ISelectionContext selectionContext = context.As<ISelectionContext>();
				m_defaultContext.SelectionContext = selectionContext;
				if (selectionContext != null)
				{
					m_editingContext = m_defaultContext;
				}
			}
		}
		if (m_editingContext != null)
		{
			if (descriptor != null && m_editingContext.PropertyDescriptors.Contains(descriptor))
			{
				m_descriptor = descriptor;
				yield return Command.CopyProperty;
				yield return Command.PasteProperty;
				yield return Command.ResetProperty;
				yield return Command.CopyAll;
				yield return Command.PasteAll;
				yield return Command.ResetAll;
				yield return Command.ViewInTextEditor;
			}
			else if (m_editingContext.Items.LastOrDefault() != null)
			{
				yield return Command.CopyAll;
				yield return Command.PasteAll;
				yield return Command.ResetAll;
			}
		}
	}

	private bool CanPaste(object srcValue, System.ComponentModel.PropertyDescriptor srcDescriptor, System.ComponentModel.PropertyDescriptor destDescriptor, object destValue)
	{
		if (srcDescriptor == null || destDescriptor == null || destDescriptor.IsReadOnly || srcDescriptor.PropertyType != destDescriptor.PropertyType || destDescriptor is ChildAttributeCollectionPropertyDescriptor || destDescriptor is ChildPropertyDescriptor)
		{
			return false;
		}
		if (destDescriptor.PropertyType.IsArray && destValue != null && srcValue != null)
		{
			Array array = (Array)srcValue;
			Array array2 = (Array)destValue;
			if (array.Rank != array2.Rank)
			{
				return false;
			}
			for (int i = 0; i < array.Rank; i++)
			{
				if (array.GetLowerBound(i) != array2.GetLowerBound(i) || array.GetUpperBound(i) != array2.GetUpperBound(i))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool CanResetValue(IEnumerable<object> items, System.ComponentModel.PropertyDescriptor descriptor)
	{
		if (descriptor != null && !descriptor.IsReadOnly)
		{
			foreach (object item in items)
			{
				if (descriptor.CanResetValue(item))
				{
					return true;
				}
			}
		}
		return false;
	}
}
