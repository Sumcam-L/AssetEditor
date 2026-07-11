namespace Sce.Atf.Applications;

public interface IControlHostPreShowClient
{
	void BeforeControlHostShow();
}

public interface IControlHostUnregisteringClient
{
	void BeforeControlHostUnregister();
}
