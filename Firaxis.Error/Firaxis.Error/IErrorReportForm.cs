using System.Windows.Forms;

namespace Firaxis.Error;

public class IErrorReportForm : Form
{
	private string m_sErrorReport = string.Empty;

	public string ErrorReport
	{
		get
		{
			return m_sErrorReport;
		}
		set
		{
			m_sErrorReport = value;
		}
	}

	public virtual string Comments
	{
		get
		{
			return string.Empty;
		}
		set
		{
		}
	}
}
