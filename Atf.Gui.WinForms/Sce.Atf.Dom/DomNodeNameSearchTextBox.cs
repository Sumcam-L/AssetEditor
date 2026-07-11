using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class DomNodeNameSearchTextBox : SearchTextBox
{
	private readonly DomNodeNamePredicate m_predicate;

	public override event EventHandler UIChanged;

	public DomNodeNameSearchTextBox()
	{
		m_predicate = new DomNodeNamePredicate();
		if (UIChanged != null)
		{
		}
	}

	protected override void OnTextChanged(EventArgs e)
	{
		DoSearch();
	}

	public override IQueryPredicate GetPredicate()
	{
		m_predicate.StringToMatch = Text;
		return m_predicate;
	}
}
