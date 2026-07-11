namespace Sce.Atf.Controls.Adaptable;

public abstract class ControlAdapter : IControlAdapter
{
	private AdaptableControl m_control;

	public AdaptableControl AdaptedControl => m_control;

	void IControlAdapter.Bind(AdaptableControl control)
	{
		m_control = control;
		Bind(control);
	}

	void IControlAdapter.BindReverse(AdaptableControl control)
	{
		BindReverse(control);
	}

	void IControlAdapter.Unbind(AdaptableControl control)
	{
		Unbind(control);
		m_control = null;
	}

	protected virtual void Bind(AdaptableControl control)
	{
	}

	protected virtual void BindReverse(AdaptableControl control)
	{
	}

	protected virtual void Unbind(AdaptableControl control)
	{
	}
}
