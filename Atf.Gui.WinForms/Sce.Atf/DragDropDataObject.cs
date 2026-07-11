using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Sce.Atf;

[ComVisible(true)]
internal sealed class DragDropDataObject : System.Runtime.InteropServices.ComTypes.IDataObject, System.Windows.Forms.IDataObject, IDisposable
{
	private class FormatEnumerator : IEnumFORMATETC
	{
		private readonly FORMATETC[] m_formats;

		private int m_currentIndex;

		public FormatEnumerator(IEnumerable<FORMATETC> data)
		{
			m_formats = data.ToArray();
		}

		public void Clone(out IEnumFORMATETC newEnum)
		{
			newEnum = new FormatEnumerator(m_formats)
			{
				m_currentIndex = m_currentIndex
			};
		}

		public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
		{
			if (pceltFetched != null && pceltFetched.Length != 0)
			{
				pceltFetched[0] = 0;
			}
			int num = celt;
			if (celt <= 0 || rgelt == null || m_currentIndex >= m_formats.Length)
			{
				return 1;
			}
			if ((pceltFetched == null || pceltFetched.Length < 1) && celt != 1)
			{
				return 1;
			}
			if (rgelt.Length < celt)
			{
				throw new ArgumentException("The number of elements in the return array is less than the number of elements requested");
			}
			int num2 = 0;
			while (m_currentIndex < m_formats.Length && num > 0)
			{
				rgelt[num2] = m_formats[m_currentIndex];
				num2++;
				num--;
				m_currentIndex++;
			}
			if (pceltFetched != null && pceltFetched.Length != 0)
			{
				pceltFetched[0] = celt - num;
			}
			return (num != 0) ? 1 : 0;
		}

		public int Reset()
		{
			m_currentIndex = 0;
			return 0;
		}

		public int Skip(int celt)
		{
			if (m_currentIndex + celt > m_formats.Length)
			{
				return 1;
			}
			m_currentIndex += celt;
			return 0;
		}
	}

	private struct AdviseEntry
	{
		public FORMATETC Format;

		public ADVF Advf;

		public IAdviseSink Sink;
	}

	private struct OleData
	{
		public FORMATETC Format;

		public STGMEDIUM Medium;
	}

	private const int OLE_E_ADVISENOTSUPPORTED = -2147221501;

	private const int DV_E_FORMATETC = -2147221404;

	private const int DV_E_TYMED = -2147221399;

	private const int DV_E_CLIPFORMAT = -2147221398;

	private const int DV_E_DVASPECT = -2147221397;

	private readonly List<OleData> m_oleStorage = new List<OleData>();

	private readonly Dictionary<string, object> m_managedStorage = new Dictionary<string, object>();

	private readonly Dictionary<int, AdviseEntry> m_connections = new Dictionary<int, AdviseEntry>();

	private int m_nextConnectionId = 1;

	public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
	{
		if (((advf | (ADVF.ADVF_NODATA | ADVF.ADVF_PRIMEFIRST | ADVF.ADVF_ONLYONCE)) ^ (ADVF.ADVF_NODATA | ADVF.ADVF_PRIMEFIRST | ADVF.ADVF_ONLYONCE)) != 0)
		{
			connection = 0;
			return -2147221501;
		}
		AdviseEntry value = new AdviseEntry
		{
			Format = pFormatetc,
			Advf = advf,
			Sink = adviseSink
		};
		m_connections.Add(m_nextConnectionId, value);
		connection = m_nextConnectionId;
		m_nextConnectionId++;
		if ((advf & ADVF.ADVF_PRIMEFIRST) == ADVF.ADVF_PRIMEFIRST && GetDataEntry(ref pFormatetc, out var dataEntry))
		{
			RaiseDataChanged(connection, ref dataEntry);
		}
		return 0;
	}

	public void DUnadvise(int connection)
	{
		m_connections.Remove(connection);
	}

	public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
	{
		enumAdvise = null;
		return -2147221501;
	}

	public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
	{
		if (direction == DATADIR.DATADIR_GET)
		{
			FORMATETC[] data = m_oleStorage.Select((OleData oleData) => oleData.Format).ToArray();
			return new FormatEnumerator(data);
		}
		throw new NotImplementedException("OLE_S_USEREG");
	}

	public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
	{
		formatOut = formatIn;
		return -2147221404;
	}

	public void GetData(ref FORMATETC format, out STGMEDIUM medium)
	{
		medium = default(STGMEDIUM);
		GetDataHere(ref format, ref medium);
	}

	public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
	{
		if (GetDataEntry(ref format, out var dataEntry))
		{
			STGMEDIUM medium2 = dataEntry.Medium;
			medium = CopyMedium(ref medium2);
		}
		else
		{
			medium = default(STGMEDIUM);
		}
	}

	public int QueryGetData(ref FORMATETC format)
	{
		if ((DVASPECT.DVASPECT_CONTENT & format.dwAspect) == 0)
		{
			return -2147221397;
		}
		int result = -2147221399;
		for (int i = 0; i < m_oleStorage.Count; i++)
		{
			OleData oleData = m_oleStorage[i];
			if ((oleData.Format.tymed & format.tymed) > TYMED.TYMED_NULL)
			{
				if (oleData.Format.cfFormat == format.cfFormat)
				{
					return 0;
				}
				result = -2147221398;
			}
			else
			{
				result = -2147221399;
			}
		}
		return result;
	}

	public void SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
	{
		for (int i = 0; i < m_oleStorage.Count; i++)
		{
			OleData item = m_oleStorage[i];
			FORMATETC format = item.Format;
			if (IsFormatCompatible(ref formatIn, ref format))
			{
				STGMEDIUM pmedium = item.Medium;
				ReleaseStgMedium(ref pmedium);
				m_oleStorage.Remove(item);
				break;
			}
		}
		STGMEDIUM medium2 = medium;
		if (!release)
		{
			medium2 = CopyMedium(ref medium);
		}
		OleData dataEntry = new OleData
		{
			Format = formatIn,
			Medium = medium2
		};
		m_oleStorage.Add(dataEntry);
		RaiseDataChanged(ref dataEntry);
	}

	private static bool IsFormatEqual(string formatA, string formatB)
	{
		return string.CompareOrdinal(formatA, formatB) == 0;
	}

	public object GetData(Type format)
	{
		if (GetCompatibleFormat(format.FullName, format) != TYMED.TYMED_NULL)
		{
			return GetData(format.FullName);
		}
		object value;
		return m_managedStorage.TryGetValue(format.FullName, out value) ? value : null;
	}

	public object GetData(string format)
	{
		return GetData(format, autoConvert: true);
	}

	public object GetData(string format, bool autoConvert)
	{
		if (m_managedStorage.TryGetValue(format, out var value))
		{
			return value;
		}
		FORMATETC format2 = OleConverter.CreateFormat(format);
		if (QueryGetData(ref format2) == 0)
		{
			GetData(ref format2, out var medium);
			return OleConverter.Convert(format, ref medium);
		}
		return null;
	}

	public bool GetDataPresent(Type format)
	{
		if (GetCompatibleFormat(format.FullName, format) != TYMED.TYMED_NULL)
		{
			return GetDataPresent(format.FullName);
		}
		return m_managedStorage.ContainsKey(format.FullName);
	}

	public bool GetDataPresent(string format)
	{
		return GetDataPresent(format, autoConvert: true);
	}

	public bool GetDataPresent(string format, bool autoConvert)
	{
		if (m_managedStorage.ContainsKey(format))
		{
			return true;
		}
		FORMATETC format2 = OleConverter.CreateFormat(format);
		if (QueryGetData(ref format2) == 0)
		{
			return true;
		}
		return false;
	}

	public string[] GetFormats()
	{
		return GetFormats(autoConvert: true);
	}

	public string[] GetFormats(bool autoConvert)
	{
		HashSet<string> hashSet = new HashSet<string>(m_managedStorage.Keys);
		for (int i = 0; i < m_oleStorage.Count; i++)
		{
			string name = DataFormats.GetFormat(m_oleStorage[i].Format.cfFormat).Name;
			hashSet.Add(name);
		}
		return hashSet.ToArray();
	}

	public void SetData(object data)
	{
		if (data is ISerializable)
		{
			SetData(DataFormats.Serializable, data);
		}
		else
		{
			SetData(data.GetType(), data);
		}
	}

	public void SetData(Type format, object data)
	{
		SetData(format.FullName, format, autoConvert: true, data);
	}

	public void SetData(string format, object data)
	{
		SetData(format, autoConvert: true, data);
	}

	public void SetData(string format, bool autoConvert, object data)
	{
		Type type = ((data != null) ? data.GetType() : typeof(object));
		SetData(format, type, autoConvert, data);
	}

	public void SetData(string format, Type type, bool autoConvert, object data)
	{
		TYMED compatibleFormat = GetCompatibleFormat(format, type);
		if (compatibleFormat != TYMED.TYMED_NULL)
		{
			FORMATETC format2 = OleConverter.CreateFormat(format);
			format2.tymed = compatibleFormat;
			DataObject dataObject = new DataObject();
			dataObject.SetData(format, autoConvert: true, data);
			((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).GetData(ref format2, out STGMEDIUM medium);
			try
			{
				SetData(ref format2, ref medium, release: true);
				return;
			}
			catch
			{
				ReleaseStgMedium(ref medium);
				throw;
			}
		}
		m_managedStorage[format] = data;
	}

	private static TYMED GetCompatibleFormat(string format, Type type)
	{
		if (IsFormatEqual(format, DataFormats.Bitmap) && typeof(Bitmap).IsAssignableFrom(type))
		{
			return TYMED.TYMED_GDI;
		}
		if (IsFormatEqual(format, DataFormats.EnhancedMetafile))
		{
			return TYMED.TYMED_ENHMF;
		}
		if (typeof(Stream).IsAssignableFrom(type) || IsFormatEqual(format, DataFormats.Html) || IsFormatEqual(format, DataFormats.Text) || IsFormatEqual(format, DataFormats.Rtf) || IsFormatEqual(format, DataFormats.OemText) || IsFormatEqual(format, DataFormats.UnicodeText) || IsFormatEqual(format, "ApplicationTrust") || IsFormatEqual(format, DataFormats.FileDrop) || IsFormatEqual(format, "FileName") || IsFormatEqual(format, "FileNameW"))
		{
			return TYMED.TYMED_HGLOBAL;
		}
		if (IsFormatEqual(format, DataFormats.Dib) && typeof(Image).IsAssignableFrom(type))
		{
			return TYMED.TYMED_NULL;
		}
		if (IsFormatEqual(format, typeof(Bitmap).FullName))
		{
			return TYMED.TYMED_HGLOBAL;
		}
		if (IsFormatEqual(format, DataFormats.Serializable) || typeof(ISerializable).IsAssignableFrom(type) || type.IsSerializable)
		{
			return TYMED.TYMED_HGLOBAL;
		}
		return TYMED.TYMED_NULL;
	}

	public void Dispose()
	{
		ClearStorage();
		GC.SuppressFinalize(this);
	}

	~DragDropDataObject()
	{
		ClearStorage();
	}

	[DllImport("urlmon.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern int CopyStgMedium(ref STGMEDIUM pcstgmedSrc, ref STGMEDIUM pstgmedDest);

	[DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

	private void ClearStorage()
	{
		for (int i = 0; i < m_oleStorage.Count; i++)
		{
			STGMEDIUM pmedium = m_oleStorage[i].Medium;
			ReleaseStgMedium(ref pmedium);
		}
		m_oleStorage.Clear();
		m_managedStorage.Clear();
	}

	private static STGMEDIUM CopyMedium(ref STGMEDIUM medium)
	{
		STGMEDIUM pstgmedDest = default(STGMEDIUM);
		int num = CopyStgMedium(ref medium, ref pstgmedDest);
		if (num != 0)
		{
			throw Marshal.GetExceptionForHR(num);
		}
		return pstgmedDest;
	}

	private static bool IsFormatCompatible(FORMATETC format1, FORMATETC format2)
	{
		return IsFormatCompatible(ref format1, ref format2);
	}

	private static bool IsFormatCompatible(ref FORMATETC format1, ref FORMATETC format2)
	{
		return (format1.tymed & format2.tymed) > TYMED.TYMED_NULL && format1.dwAspect == format2.dwAspect && format1.cfFormat == format2.cfFormat;
	}

	private bool GetDataEntry(ref FORMATETC pFormatetc, out OleData dataEntry)
	{
		for (int i = 0; i < m_oleStorage.Count; i++)
		{
			OleData oleData = m_oleStorage[i];
			FORMATETC format = oleData.Format;
			if (IsFormatCompatible(ref pFormatetc, ref format))
			{
				dataEntry = oleData;
				return true;
			}
		}
		dataEntry = default(OleData);
		return false;
	}

	private void RaiseDataChanged(int connection, ref OleData dataEntry)
	{
		AdviseEntry adviseEntry = m_connections[connection];
		FORMATETC format = dataEntry.Format;
		STGMEDIUM stgmedium = (((adviseEntry.Advf & ADVF.ADVF_NODATA) != ADVF.ADVF_NODATA) ? dataEntry.Medium : default(STGMEDIUM));
		adviseEntry.Sink.OnDataChange(ref format, ref stgmedium);
		if ((adviseEntry.Advf & ADVF.ADVF_ONLYONCE) == ADVF.ADVF_ONLYONCE)
		{
			m_connections.Remove(connection);
		}
	}

	private void RaiseDataChanged(ref OleData dataEntry)
	{
		foreach (KeyValuePair<int, AdviseEntry> connection in m_connections)
		{
			if (IsFormatCompatible(connection.Value.Format, dataEntry.Format))
			{
				RaiseDataChanged(connection.Key, ref dataEntry);
			}
		}
	}
}
