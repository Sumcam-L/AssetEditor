using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace Sce.Atf;

public class DragDropExtender
{
	private class AdviseSink : IAdviseSink
	{
		private readonly System.Runtime.InteropServices.ComTypes.IDataObject m_data;

		public AdviseSink(System.Runtime.InteropServices.ComTypes.IDataObject data)
		{
			m_data = data;
		}

		public void OnDataChange(ref FORMATETC format, ref STGMEDIUM stgmedium)
		{
			object dropDescription = DropDescriptionHelper.GetDropDescription(m_data);
			if (dropDescription != null)
			{
				DropDescriptionHelper.SetDropDescriptionIsDefault(m_data, isDefault: false);
			}
		}

		public void OnClose()
		{
			throw new NotImplementedException();
		}

		public void OnRename(IMoniker moniker)
		{
			throw new NotImplementedException();
		}

		public void OnSave()
		{
			throw new NotImplementedException();
		}

		public void OnViewChange(int aspect, int index)
		{
			throw new NotImplementedException();
		}
	}

	private readonly Control m_owner;

	private DragDropDataObject m_dragData;

	public DragDropExtender(Control owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		m_owner = owner;
	}

	public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects, Bitmap dragImage, Point cursorOffset)
	{
		int connection = 0;
		try
		{
			DragDropHelper.SetFlags(1);
			m_dragData = new DragDropDataObject();
			if (dragImage != null)
			{
				DragDropHelper.InitializeFromBitmap(m_dragData, dragImage, cursorOffset);
			}
			FORMATETC pFormatetc = OleConverter.CreateFormat("DropDescription");
			int num = m_dragData.DAdvise(ref pFormatetc, (ADVF)0, new AdviseSink(m_dragData), out connection);
			if (num != 0)
			{
				Marshal.ThrowExceptionForHR(num);
			}
			m_dragData.SetData("DragDropExtender", this);
			m_dragData.SetData(data);
			m_owner.GiveFeedback += OnGiveFeedback;
			m_owner.QueryContinueDrag += OnQueryContinueDrag;
			return m_owner.DoDragDrop(m_dragData, allowedEffects);
		}
		finally
		{
			m_owner.GiveFeedback -= OnGiveFeedback;
			m_owner.QueryContinueDrag -= OnQueryContinueDrag;
			if (m_dragData != null)
			{
				m_dragData.DUnadvise(connection);
				m_dragData.Dispose();
				m_dragData = null;
			}
		}
	}

	private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
	{
		if (m_dragData != null)
		{
			DropDescriptionHelper.DefaultGiveFeedback(m_dragData, e);
		}
	}

	private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		if (e.EscapePressed)
		{
			e.Action = DragAction.Cancel;
		}
	}
}
