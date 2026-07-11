namespace Sce.Atf.Adaptation;

public interface IAdapter : IAdaptable, IDecoratable
{
	object Adaptee { get; set; }
}
