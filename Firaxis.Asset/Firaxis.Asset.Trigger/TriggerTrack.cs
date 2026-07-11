using System.Collections.Generic;
using Firaxis.Collections;

namespace Firaxis.Asset.Trigger;

public class TriggerTrack : IUniqueName
{
	public class SubTrack : IUniqueID
	{
		public int ID { get; set; }

		public string Name { get; set; }

		public object Tag { get; set; }

		public SubTrack()
		{
			Name = "";
		}

		public SubTrack(string name)
		{
			Name = name;
		}

		public SubTrack(string name, int id)
		{
			Name = name;
			ID = id;
		}
	}

	public class SubTrackCollection : ListEventID<SubTrack>
	{
		public SubTrackCollection()
		{
		}

		public SubTrackCollection(IEnumerable<SubTrack> collection)
			: base(collection)
		{
		}

		public SubTrackCollection(int capacity)
			: base(capacity)
		{
		}
	}

	public SubTrackCollection SubTracks { get; private set; }

	public string ID { get; set; }

	public string Binding { get; set; }

	public TriggerTrack()
	{
		SubTracks = new SubTrackCollection();
	}

	public TriggerTrack(string id)
	{
		SubTracks = new SubTrackCollection();
		ID = id;
	}
}
