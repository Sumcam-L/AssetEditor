namespace Sce.Atf.Applications;

public interface IMessageBoxService
{
	MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image);
}
