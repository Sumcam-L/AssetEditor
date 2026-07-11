using System.Collections.Generic;
using System.Linq;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class ChannelList : List<Channel>
{
	public Channel[] ToIdArray()
	{
		short num = this.Max((Channel x) => x.ID);
		Channel[] array = new Channel[num + 1];
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Channel current = enumerator.Current;
				if (current.ID >= 0)
				{
					array[current.ID] = current;
				}
			}
		}
		return array;
	}

	public Channel GetId(int id)
	{
		return this.Single((Channel x) => x.ID == id);
	}

	public bool ContainsId(int id)
	{
		return Exists((Channel x) => x.ID == id);
	}
}
