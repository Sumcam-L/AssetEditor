using System;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.ATF;

public interface IWidgetState : IDisposable
{
	string Name { get; }

	Image Image { get; }

	string Text { get; }

	Keys HotKey { get; }

	string Description { get; }

	bool CanActivate { get; }

	void Activate();
}
