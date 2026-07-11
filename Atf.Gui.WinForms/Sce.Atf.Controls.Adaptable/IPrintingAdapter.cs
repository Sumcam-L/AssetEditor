using System.Drawing;
using System.Drawing.Printing;

namespace Sce.Atf.Controls.Adaptable;

public interface IPrintingAdapter
{
	void Print(PrintDocument printDocument, Graphics g);
}
