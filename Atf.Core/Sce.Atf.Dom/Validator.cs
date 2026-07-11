using System;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public abstract class Validator : Observer
{
	private bool m_validating;

	public bool Validating => m_validating;

	protected virtual void OnBeginning(object sender, EventArgs e)
	{
	}

	protected virtual void OnEnding(object sender, EventArgs e)
	{
	}

	protected virtual void OnEnded(object sender, EventArgs e)
	{
	}

	protected virtual void OnCancelled(object sender, EventArgs e)
	{
	}

	protected override void AddNode(DomNode node)
	{
		foreach (IValidationContext item in node.AsAll<IValidationContext>())
		{
			item.Beginning += validationContext_Beginning;
			item.Ending += validationContext_Ending;
			item.Ended += validationContext_Ended;
			item.Cancelled += validationContext_Cancelled;
		}
	}

	protected override void RemoveNode(DomNode node)
	{
		foreach (IValidationContext item in node.AsAll<IValidationContext>())
		{
			item.Beginning -= validationContext_Beginning;
			item.Ending -= validationContext_Ending;
			item.Ended -= validationContext_Ended;
			item.Cancelled -= validationContext_Cancelled;
		}
	}

	private void validationContext_Beginning(object sender, EventArgs e)
	{
		m_validating = true;
		OnBeginning(sender, e);
	}

	private void validationContext_Ending(object sender, EventArgs e)
	{
		OnEnding(sender, e);
		m_validating = false;
	}

	private void validationContext_Ended(object sender, EventArgs e)
	{
		m_validating = false;
		OnEnded(sender, e);
	}

	private void validationContext_Cancelled(object sender, EventArgs e)
	{
		m_validating = false;
		OnCancelled(sender, e);
	}
}
