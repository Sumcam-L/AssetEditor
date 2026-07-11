using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class ReplaceTextBox : TextBox, IReplaceUI, ISearchableContextUI
{
	public Control Control => this;

	protected IQueryableReplaceContext ReplaceableContext { get; private set; }

	public event EventHandler UIChanged;

	public ReplaceTextBox()
	{
		if (this.UIChanged != null)
		{
		}
		base.KeyDown += textBox_KeyDown;
	}

	public void Bind(IQueryableReplaceContext replaceableContext)
	{
		ReplaceableContext = replaceableContext;
		base.Enabled = replaceableContext != null;
	}

	protected virtual void textBox_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Return)
		{
			DoReplace();
		}
	}

	public virtual object GetReplaceInfo()
	{
		return Text;
	}

	public void DoReplace()
	{
		if (ReplaceableContext != null)
		{
			ReplaceableContext.Replace(GetReplaceInfo());
		}
	}
}
