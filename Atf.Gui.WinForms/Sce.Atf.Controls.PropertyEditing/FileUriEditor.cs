using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class FileUriEditor : FileNameEditor, IAnnotatedParams
{
	private static string s_globalDefaultTextEditor = "notepad.exe";

	private string m_filter;

	private string m_initialDirectory;

	private OpenFileDialog m_dialog;

	private string m_associatedTextEditor;

	public string Filter
	{
		get
		{
			return m_filter;
		}
		set
		{
			m_filter = value;
		}
	}

	public string AssociatedTextEditor
	{
		get
		{
			return string.IsNullOrEmpty(m_associatedTextEditor) ? GlobalDefaultTextEditor : m_associatedTextEditor;
		}
		set
		{
			m_associatedTextEditor = value;
		}
	}

	public static string GlobalDefaultTextEditor
	{
		get
		{
			return s_globalDefaultTextEditor;
		}
		set
		{
			s_globalDefaultTextEditor = value;
		}
	}

	public FileUriEditor()
	{
	}

	public FileUriEditor(string filter)
	{
		m_filter = filter;
	}

	public void Initialize(string[] parameters)
	{
		if (parameters.Length == 1)
		{
			m_filter = parameters[0];
		}
		else if (parameters.Length > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < parameters.Length - 1; i++)
			{
				stringBuilder.AppendFormat("{0},", parameters[i]);
			}
			stringBuilder.Append(parameters[parameters.Length - 1]);
			m_filter = stringBuilder.ToString();
		}
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		try
		{
			m_initialDirectory = null;
			string text = value as string;
			if (text == null)
			{
				Uri uri = value as Uri;
				if (uri != null)
				{
					text = ((!uri.IsAbsoluteUri) ? uri.OriginalString : uri.LocalPath);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = text.Replace('/', '\\');
				if (text2 != text)
				{
					value = context.PropertyDescriptor.Converter.ConvertFromString(text2);
				}
				if (File.Exists(text2))
				{
					string directoryName = Path.GetDirectoryName(text2);
					if (!string.IsNullOrEmpty(directoryName))
					{
						m_initialDirectory = directoryName;
					}
				}
			}
			if (m_dialog != null && !string.IsNullOrEmpty(m_initialDirectory))
			{
				m_dialog.InitialDirectory = m_initialDirectory;
			}
			return base.EditValue(context, provider, value);
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			return value;
		}
	}

	protected override void InitializeDialog(OpenFileDialog dialog)
	{
		base.InitializeDialog(dialog);
		if (m_filter != null)
		{
			dialog.Filter = m_filter;
		}
		m_dialog = dialog;
		if (!string.IsNullOrEmpty(m_initialDirectory))
		{
			m_dialog.InitialDirectory = m_initialDirectory;
		}
	}
}
