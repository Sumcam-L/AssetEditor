using System.ComponentModel;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface IMainForm : IMainWindow
{
	ISynchronizeInvoke Invoker { get; }
}
