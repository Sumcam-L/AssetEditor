using System.Collections.Generic;

namespace Firaxis.Controls.Scrollables;

public interface IScrollableItemKeys
{
	IEnumerable<IKey> FindKeys(int x, int y, TimeLineHitArgs e);

	void MoveKey(object sender, IKey key, int delta);

	IKey AddKey(object sender, int x, int y);

	void RemoveKey(object sender, IKey key);
}
