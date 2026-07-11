using System.Drawing.Printing;

namespace Sce.Atf.Applications;

public interface IPrintableDocument
{
	PrintDocument GetPrintDocument();
}
