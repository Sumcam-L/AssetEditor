using System;

namespace Firaxis.CivTech;

public interface ISplashScreenService
{
	void ShowSplashScreen(Action action, string caption, string message);
}
