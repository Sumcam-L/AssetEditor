using System;
using System.Text;

namespace Sce.Atf.Applications.NetworkTargetServices;

public class TcpCommandClient
{
	public delegate void CommandReadyHandler(object sender, TCPCommand command);

	public delegate void ExceptonHandler(object sender, Exception e);

	private readonly TargetTcpSocket m_socket;

	private int m_numBytesReceivedThisCommand;

	private int m_payloadSizeThisCommand;

	private readonly byte[] m_payloadBytes;

	private readonly byte[] m_opcodeBytes = new byte[4];

	private readonly byte[] m_payloadSizeBytes = new byte[4];

	private readonly TCPCommand m_tcpCommand = new TCPCommand();

	private readonly int m_maxPayloadSize;

	public event CommandReadyHandler CommandReady;

	public event ExceptonHandler UnHandledException;

	public TcpCommandClient(TargetTcpSocket socket, int maxPayloadSize)
	{
		m_socket = socket;
		m_socket.DataReady += DataReadyHandler;
		m_socket.UnHandledException += ExceptionHandler;
		m_maxPayloadSize = maxPayloadSize;
		m_payloadBytes = new byte[m_maxPayloadSize];
		m_numBytesReceivedThisCommand = 0;
		m_payloadSizeThisCommand = 0;
	}

	public void Reset()
	{
		m_numBytesReceivedThisCommand = 0;
		m_payloadSizeThisCommand = 0;
	}

	public void Send(TCPCommand command)
	{
		byte[] data = EncodeInt(command.m_opcode);
		byte[] data2 = EncodeInt(command.m_payloadSize);
		m_socket.Send(data);
		m_socket.Send(data2);
		m_socket.Send(command.m_payload);
	}

	private void DataReadyHandler(object sender, byte[] buf)
	{
		for (int i = 0; i < buf.Length; i++)
		{
			if (m_numBytesReceivedThisCommand < 4)
			{
				m_opcodeBytes[m_numBytesReceivedThisCommand] = buf[i];
			}
			else if (m_numBytesReceivedThisCommand < 8)
			{
				m_payloadSizeBytes[m_numBytesReceivedThisCommand - 4] = buf[i];
			}
			else
			{
				m_payloadBytes[m_numBytesReceivedThisCommand - 8] = buf[i];
				m_payloadSizeThisCommand++;
			}
			m_numBytesReceivedThisCommand++;
			if (m_numBytesReceivedThisCommand == 8)
			{
				DecodeInt(m_opcodeBytes, 0, out m_tcpCommand.m_opcode);
				DecodeInt(m_payloadSizeBytes, 0, out m_tcpCommand.m_payloadSize);
			}
			if (m_numBytesReceivedThisCommand == m_tcpCommand.m_payloadSize + 8)
			{
				CommandReadyHandler commandReadyHandler = this.CommandReady;
				if (commandReadyHandler != null)
				{
					m_tcpCommand.m_payload = m_payloadBytes;
					commandReadyHandler(this, m_tcpCommand);
				}
				m_numBytesReceivedThisCommand = 0;
				m_payloadSizeThisCommand = 0;
			}
		}
	}

	private void ExceptionHandler(object sender, Exception e)
	{
		this.UnHandledException?.Invoke(this, e);
	}

	public static int DecodeInt(byte[] data, int startIndex, out int value)
	{
		value = BitConverter.ToInt32(new byte[4]
		{
			data[startIndex + 3],
			data[startIndex + 2],
			data[startIndex + 1],
			data[startIndex]
		}, 0);
		return 4;
	}

	public static int DecodeUInt64(byte[] data, int startIndex, out ulong value)
	{
		value = BitConverter.ToUInt64(new byte[8]
		{
			data[startIndex + 7],
			data[startIndex + 6],
			data[startIndex + 5],
			data[startIndex + 4],
			data[startIndex + 3],
			data[startIndex + 2],
			data[startIndex + 1],
			data[startIndex]
		}, 0);
		return 8;
	}

	public static int DecodeFloat(byte[] data, int startIndex, out float value)
	{
		value = BitConverter.ToSingle(new byte[4]
		{
			data[startIndex + 3],
			data[startIndex + 2],
			data[startIndex + 1],
			data[startIndex]
		}, 0);
		return 4;
	}

	public static int DecodeString(byte[] data, int startIndex, out string value)
	{
		startIndex += DecodeInt(data, startIndex, out var value2);
		value = Encoding.ASCII.GetString(data, startIndex, value2);
		return value2 + 4;
	}

	public static byte[] EncodeInt(int val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		return new byte[4]
		{
			bytes[3],
			bytes[2],
			bytes[1],
			bytes[0]
		};
	}
}
