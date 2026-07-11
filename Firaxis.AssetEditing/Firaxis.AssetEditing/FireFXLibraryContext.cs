using System;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class FireFXLibraryContext : EditingContext, IObservableContext, IInstancingContext
{
	private FireFXLibraryDocument m_document;

	private ControlInfo m_controlInfo;

	private FireFXScriptControl m_control;

	public ControlInfo ControlInfo
	{
		get
		{
			return m_controlInfo;
		}
		set
		{
			m_controlInfo = value;
		}
	}

	public FireFXLibraryDocument Doc
	{
		get
		{
			return m_document;
		}
		set
		{
			if (m_document != value)
			{
				m_document = value;
				m_control = new FireFXScriptControl(m_document);
			}
		}
	}

	public Control Control => m_control;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler Reloaded;

	public FireFXLibraryContext()
	{
		if (this.ItemChanged != null && this.ItemInserted != null && this.ItemRemoved != null)
		{
			_ = this.Reloaded;
		}
	}

	public bool CanCopy()
	{
		return false;
	}

	public bool CanDelete()
	{
		return false;
	}

	public bool CanInsert(object dataObject)
	{
		return false;
	}

	public object Copy()
	{
		return null;
	}

	public void Delete()
	{
	}

	public void Insert(object dataObject)
	{
	}
}
