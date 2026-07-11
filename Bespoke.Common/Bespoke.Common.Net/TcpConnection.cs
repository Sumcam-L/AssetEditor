using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Bespoke.Common.Net;

public class TcpConnection : IDisposable
{
	private enum DataReceiveState
	{
		MessageFraming,
		Message
	}

	private static readonly int ReceivedDataBufferSize = 65535;

	private Socket mClient;

	private NetworkStream mNetworkStream;

	private BinaryReader mReader;

	private BinaryWriter mWriter;

	private volatile bool mIsClosed;

	private bool mLittleEndianByteOrder;

	private DataReceiveState mReceiveState;

	private AsyncCallback mDataReceivedCallback;

	private byte[] mPartiallyReceivedData;

	private List<byte> mCompletelyReceivedData;

	private int mMessageLength;

	private byte[] mMessageLengthData;

	private int mBytesToProcessRemaining;

	public Socket Client => mClient;

	public BinaryReader Reader => mReader;

	public BinaryWriter Writer => mWriter;

	public bool LittleEndianByteOrder
	{
		get
		{
			return mLittleEndianByteOrder;
		}
		set
		{
			mLittleEndianByteOrder = value;
		}
	}

	public event EventHandler<TcpConnectionEventArgs> Disconnected;

	public event EventHandler<TcpDataReceivedEventArgs> DataReceived;

	public TcpConnection(Socket client, bool littleEndianByteOrder = true)
	{
		Assert.ParamIsNotNull(client);
		mClient = client;
		mNetworkStream = new NetworkStream(client);
		mReader = new BinaryReader(mNetworkStream);
		mWriter = new BinaryWriter(mNetworkStream);
		mLittleEndianByteOrder = littleEndianByteOrder;
		mReceiveState = DataReceiveState.MessageFraming;
		mPartiallyReceivedData = new byte[ReceivedDataBufferSize];
		mCompletelyReceivedData = new List<byte>();
		mMessageLengthData = new byte[4];
		mBytesToProcessRemaining = 0;
	}

	public void Dispose()
	{
		if (!mIsClosed)
		{
			Close();
		}
	}

	public void Close()
	{
		lock (this)
		{
			mReader.Close();
			mWriter.Close();
			mNetworkStream.Close();
			if (mClient.Connected)
			{
				mClient.Shutdown(SocketShutdown.Both);
				mClient.Close();
			}
			mDataReceivedCallback = null;
			mIsClosed = true;
			OnDisconnected(new TcpConnectionEventArgs(this));
		}
	}

	public void ReceiveDataAsynchronously()
	{
		mDataReceivedCallback = DataReceivedCallback;
		InitDataReceivedCallback();
	}

	private void InitDataReceivedCallback()
	{
		Array.Clear(mPartiallyReceivedData, 0, mPartiallyReceivedData.Length);
		mClient.BeginReceive(mPartiallyReceivedData, 0, mPartiallyReceivedData.Length, SocketFlags.None, mDataReceivedCallback, null);
	}

	private void DataReceivedCallback(IAsyncResult asyncResult)
	{
		if (mIsClosed)
		{
			return;
		}
		try
		{
			int num = mClient.EndReceive(asyncResult);
			if (num > 0)
			{
				mBytesToProcessRemaining = num;
				mCompletelyReceivedData.AddRange(new SubArray<byte>(mPartiallyReceivedData, 0, num));
				while (mBytesToProcessRemaining > 0)
				{
					switch (mReceiveState)
					{
					case DataReceiveState.MessageFraming:
						if (mCompletelyReceivedData.Count >= 4)
						{
							mCompletelyReceivedData.CopyTo(0, mMessageLengthData, 0, 4);
							if (BitConverter.IsLittleEndian != mLittleEndianByteOrder)
							{
								mMessageLengthData = Utility.SwapEndian(mMessageLengthData);
							}
							mMessageLength = BitConverter.ToInt32(mMessageLengthData, 0);
							Assert.IsTrue(mMessageLength > 0);
							mReceiveState = DataReceiveState.Message;
							mCompletelyReceivedData.RemoveRange(0, 4);
							mBytesToProcessRemaining -= 4;
						}
						break;
					case DataReceiveState.Message:
						if (mCompletelyReceivedData.Count >= mMessageLength)
						{
							byte[] array = new byte[mMessageLength];
							mCompletelyReceivedData.CopyTo(0, array, 0, mMessageLength);
							OnDataReceived(new TcpDataReceivedEventArgs(this, array));
							mReceiveState = DataReceiveState.MessageFraming;
							mCompletelyReceivedData.RemoveRange(0, mMessageLength);
						}
						mBytesToProcessRemaining = 0;
						break;
					}
				}
				InitDataReceivedCallback();
			}
			else
			{
				mCompletelyReceivedData.Clear();
				Close();
			}
		}
		catch
		{
			Close();
		}
	}

	private void OnDataReceived(TcpDataReceivedEventArgs e)
	{
		if (this.DataReceived != null)
		{
			this.DataReceived(this, e);
		}
	}

	private void OnDisconnected(TcpConnectionEventArgs e)
	{
		if (this.Disconnected != null)
		{
			this.Disconnected(this, e);
		}
	}
}
