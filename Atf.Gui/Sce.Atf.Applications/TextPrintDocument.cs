using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace Sce.Atf.Applications;

public class TextPrintDocument : PrintDocument
{
	private TextReader m_streamReader;

	private readonly string m_fileName;

	private Stream m_stream;

	public TextPrintDocument(string fileName)
	{
		m_fileName = fileName;
	}

	public TextPrintDocument(Stream stream)
	{
		m_stream = stream;
	}

	protected override void OnBeginPrint(PrintEventArgs e)
	{
		base.OnBeginPrint(e);
		if (m_fileName != null)
		{
			try
			{
				m_streamReader = new StreamReader(m_fileName);
				return;
			}
			catch (FileNotFoundException)
			{
				e.Cancel = true;
				return;
			}
		}
		if (m_stream != null)
		{
			m_streamReader = new StreamReader(m_stream);
		}
		else
		{
			e.Cancel = true;
		}
	}

	protected override void OnEndPrint(PrintEventArgs e)
	{
		base.OnEndPrint(e);
		m_streamReader.Close();
		m_streamReader = null;
		if (m_stream != null)
		{
			m_stream.Close();
			m_stream = null;
		}
	}

	protected override void OnPrintPage(PrintPageEventArgs e)
	{
		base.OnPrintPage(e);
		using Font font = new Font("Courier", 10f);
		float x = e.MarginBounds.Left;
		float num = e.MarginBounds.Top;
		float height = font.GetHeight(e.Graphics);
		float num2 = (float)e.MarginBounds.Height / height;
		int num3 = 0;
		string text = null;
		while ((float)num3 < num2 && (text = m_streamReader.ReadLine()) != null)
		{
			e.Graphics.DrawString(text, font, Brushes.Black, x, num + (float)num3++ * height);
		}
		if (text != null)
		{
			e.HasMorePages = true;
		}
		else
		{
			e.HasMorePages = false;
		}
	}
}
