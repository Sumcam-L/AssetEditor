using System;
using System.Windows;
using System.Windows.Forms;

namespace Sce.Atf.Wpf.Behaviors;

public class DataObjectAdapter : System.Windows.Forms.IDataObject, IDisposable
{
	private System.Windows.IDataObject m_adaptee;

	public DataObjectAdapter(System.Windows.IDataObject adaptee)
	{
		m_adaptee = adaptee;
	}

	public object GetData(Type format)
	{
		return m_adaptee.GetData(format);
	}

	public object GetData(string format)
	{
		return m_adaptee.GetData(format);
	}

	public object GetData(string format, bool autoConvert)
	{
		return m_adaptee.GetData(format, autoConvert);
	}

	public bool GetDataPresent(Type format)
	{
		return m_adaptee.GetDataPresent(format);
	}

	public bool GetDataPresent(string format)
	{
		return m_adaptee.GetDataPresent(format);
	}

	public bool GetDataPresent(string format, bool autoConvert)
	{
		return m_adaptee.GetDataPresent(format, autoConvert);
	}

	public string[] GetFormats()
	{
		return m_adaptee.GetFormats();
	}

	public string[] GetFormats(bool autoConvert)
	{
		return m_adaptee.GetFormats(autoConvert);
	}

	public void SetData(object data)
	{
		m_adaptee.SetData(data);
	}

	public void SetData(Type format, object data)
	{
		m_adaptee.SetData(format, data);
	}

	public void SetData(string format, object data)
	{
		m_adaptee.SetData(format, data);
	}

	public void SetData(string format, bool autoConvert, object data)
	{
		m_adaptee.SetData(format, data, autoConvert);
	}

	public void Dispose()
	{
		if (m_adaptee is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}
