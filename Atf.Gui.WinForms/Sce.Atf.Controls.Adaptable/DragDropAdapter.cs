using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class DragDropAdapter : ControlAdapter
{
	private readonly IStatusService m_statusService;

	protected Point m_mousePosition;

	private bool m_isDropping;

	public bool IsDropping => m_isDropping;

	public Point MousePosition => m_mousePosition;

	public DragDropAdapter(IStatusService statusService)
	{
		m_statusService = statusService;
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.DragOver += control_DragOver;
		control.DragDrop += control_DragDrop;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.DragOver -= control_DragOver;
		control.DragDrop -= control_DragDrop;
	}

	private void control_DragOver(object sender, DragEventArgs e)
	{
		SetMousePosition(e);
		e.Effect = DragDropEffects.None;
		IInstancingContext instancingContext = base.AdaptedControl.ContextAs<IInstancingContext>();
		if (instancingContext != null && instancingContext.CanInsert(e.Data))
		{
			OnDragOver(e);
		}
	}

	protected virtual void OnDragOver(DragEventArgs e)
	{
		e.Effect = DragDropEffects.Copy;
	}

	private void control_DragDrop(object sender, DragEventArgs e)
	{
		SetMousePosition(e);
		IInstancingContext instancingContext = base.AdaptedControl.ContextAs<IInstancingContext>();
		if (instancingContext == null || !instancingContext.CanInsert(e.Data))
		{
			return;
		}
		try
		{
			m_isDropping = true;
			string name = "Drag and Drop".Localize();
			ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
			context.DoTransaction(delegate
			{
				instancingContext.Insert(e.Data);
				if (m_statusService != null)
				{
					m_statusService.ShowStatus(name);
				}
			}, name);
			base.AdaptedControl.Focus();
		}
		finally
		{
			m_isDropping = false;
		}
	}

	protected void SetMousePosition(DragEventArgs e)
	{
		m_mousePosition = base.AdaptedControl.PointToClient(new Point(e.X, e.Y));
	}
}
