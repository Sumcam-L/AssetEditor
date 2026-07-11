using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public abstract class ReplaceToolStrip : ToolStrip, IReplaceUI, ISearchableContextUI
{
	private IQueryableReplaceContext m_replaceableContext;

	public Control Control => this;

	public abstract event EventHandler UIChanged;

	public ReplaceToolStrip()
	{
	}

	public ReplaceToolStrip(IQueryableReplaceContext replacableContext)
	{
		Bind(replacableContext);
	}

	public void Bind(IQueryableReplaceContext replaceableContext)
	{
		m_replaceableContext = replaceableContext;
		base.Enabled = replaceableContext != null;
	}

	public abstract object GetReplaceInfo();

	public virtual void DoReplace()
	{
		if (m_replaceableContext != null)
		{
			m_replaceableContext.Replace(GetReplaceInfo());
		}
	}
}
