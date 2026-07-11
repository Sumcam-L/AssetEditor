using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class SerializableOperation : TransactionContext.Operation
{
	private class CompressedString
	{
		private byte[] m_compressedData;

		public string Value => DecompressData();

		public CompressedString(string value)
		{
			CompressedString compressedString = this;
			Task.Factory.StartNew(delegate
			{
				compressedString.m_compressedData = compressedString.CompressData(value);
			});
		}

		private byte[] CompressData(string value)
		{
			using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
			using MemoryStream memoryStream2 = new MemoryStream();
			using (DeflateStream destination = new DeflateStream(memoryStream2, CompressionMode.Compress, leaveOpen: true))
			{
				memoryStream.CopyTo(destination);
			}
			return memoryStream2.ToArray();
		}

		private string DecompressData()
		{
			WaitForCompression();
			using MemoryStream stream = new MemoryStream(m_compressedData);
			using MemoryStream memoryStream = new MemoryStream();
			using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true))
			{
				deflateStream.CopyTo(memoryStream);
			}
			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}

		private void WaitForCompression()
		{
			while (m_compressedData == null)
			{
				Thread.Sleep(1);
			}
		}
	}

	private AssetAdapter m_adapter;

	private CompressedString m_undoXML;

	private CompressedString m_redoXML;

	public SerializableOperation(AssetAdapter thing, Action doAction)
	{
		m_adapter = thing;
		string value = m_adapter.Asset.SerializeIntoXML();
		m_undoXML = new CompressedString(value);
		doAction();
		string value2 = m_adapter.Asset.SerializeIntoXML();
		m_redoXML = new CompressedString(value2);
	}

	public override void Do()
	{
		m_adapter.Asset.DeserializeFromXML(m_redoXML.Value);
		m_adapter.Update();
	}

	public override void Undo()
	{
		m_adapter.Asset.DeserializeFromXML(m_undoXML.Value);
		m_adapter.Update();
	}
}
