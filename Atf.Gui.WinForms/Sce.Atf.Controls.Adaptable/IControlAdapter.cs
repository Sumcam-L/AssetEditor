namespace Sce.Atf.Controls.Adaptable;

public interface IControlAdapter
{
	AdaptableControl AdaptedControl { get; }

	void Bind(AdaptableControl control);

	void BindReverse(AdaptableControl control);

	void Unbind(AdaptableControl control);
}
