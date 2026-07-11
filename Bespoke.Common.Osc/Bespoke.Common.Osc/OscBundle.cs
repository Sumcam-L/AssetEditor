using System.Collections.Generic;
using System.Net;

namespace Bespoke.Common.Osc;

public sealed class OscBundle : OscPacket
{
	private const string BundlePrefix = "#bundle";

	private OscTimeTag mTimeStamp;

	public override bool IsBundle => true;

	public OscTimeTag TimeStamp => mTimeStamp;

	public IList<OscBundle> Bundles
	{
		get
		{
			List<OscBundle> list = new List<OscBundle>();
			foreach (object mDatum in mData)
			{
				if (mDatum is OscBundle)
				{
					list.Add((OscBundle)mDatum);
				}
			}
			return list.AsReadOnly();
		}
	}

	public IList<OscMessage> Messages
	{
		get
		{
			List<OscMessage> list = new List<OscMessage>();
			foreach (object mDatum in mData)
			{
				if (mDatum is OscMessage)
				{
					list.Add((OscMessage)mDatum);
				}
			}
			return list.AsReadOnly();
		}
	}

	public OscBundle(IPEndPoint sourceEndPoint, OscClient client = null)
		: this(sourceEndPoint, new OscTimeTag(), client)
	{
	}

	public OscBundle(IPEndPoint sourceEndPoint, OscTimeTag timeStamp, OscClient client = null)
		: base(sourceEndPoint, "#bundle", client)
	{
		mTimeStamp = timeStamp;
	}

	public override byte[] ToByteArray()
	{
		List<byte> list = new List<byte>();
		list.AddRange(OscPacket.ValueToByteArray(mAddress));
		OscPacket.PadNull(list);
		list.AddRange(OscPacket.ValueToByteArray(mTimeStamp));
		foreach (object mDatum in mData)
		{
			if (mDatum is OscPacket)
			{
				byte[] array = ((OscPacket)mDatum).ToByteArray();
				Assert.IsTrue(array.Length % 4 == 0);
				list.AddRange(OscPacket.ValueToByteArray(array.Length));
				list.AddRange(array);
			}
		}
		return list.ToArray();
	}

	public new static OscBundle FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start, int end)
	{
		string text = OscPacket.ValueFromByteArray<string>(data, ref start);
		Assert.IsTrue(text == "#bundle");
		OscTimeTag timeStamp = OscPacket.ValueFromByteArray<OscTimeTag>(data, ref start);
		OscBundle oscBundle = new OscBundle(sourceEndPoint, timeStamp);
		while (start < end)
		{
			int num = OscPacket.ValueFromByteArray<int>(data, ref start);
			int end2 = start + num;
			oscBundle.Append(OscPacket.FromByteArray(sourceEndPoint, data, ref start, end2));
		}
		return oscBundle;
	}

	public override int Append<T>(T value)
	{
		Assert.IsTrue(value is OscPacket);
		if (value is OscBundle oscBundle)
		{
			Assert.IsTrue(oscBundle.mTimeStamp >= mTimeStamp);
		}
		mData.Add(value);
		return mData.Count - 1;
	}
}
