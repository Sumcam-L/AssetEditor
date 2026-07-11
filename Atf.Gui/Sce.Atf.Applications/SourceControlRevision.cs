using System;

namespace Sce.Atf.Applications;

public class SourceControlRevision
{
	private SourceControlRevisionKind m_kind;

	private int m_revisionNumber;

	private DateTime m_dateTime;

	public SourceControlRevisionKind Kind
	{
		get
		{
			return m_kind;
		}
		set
		{
			m_kind = value;
		}
	}

	public int Number
	{
		get
		{
			if (m_kind != SourceControlRevisionKind.Number)
			{
				throw new InvalidOperationException("This revision is not a Number");
			}
			return m_revisionNumber;
		}
		set
		{
			m_kind = SourceControlRevisionKind.Number;
			m_revisionNumber = value;
		}
	}

	public DateTime Date
	{
		get
		{
			if (m_kind != SourceControlRevisionKind.Date)
			{
				throw new InvalidOperationException("This revision is not a Date");
			}
			return m_dateTime;
		}
		set
		{
			m_kind = SourceControlRevisionKind.Date;
			m_dateTime = value;
		}
	}

	public int ChangeListNumber
	{
		get
		{
			if (m_kind != SourceControlRevisionKind.ChangeList)
			{
				throw new InvalidOperationException("This revision is not a changelist number");
			}
			return m_revisionNumber;
		}
		set
		{
			m_kind = SourceControlRevisionKind.ChangeList;
			m_revisionNumber = value;
		}
	}

	public static SourceControlRevision Head => new SourceControlRevision(SourceControlRevisionKind.Head);

	public static SourceControlRevision Unspecified => new SourceControlRevision(SourceControlRevisionKind.Unspecified);

	public static SourceControlRevision Working => new SourceControlRevision(SourceControlRevisionKind.Working);

	public static SourceControlRevision Base => new SourceControlRevision(SourceControlRevisionKind.Base);

	public SourceControlRevision(int revisionNumber)
	{
		m_kind = SourceControlRevisionKind.Number;
		m_revisionNumber = revisionNumber;
	}

	public SourceControlRevision()
	{
		m_kind = SourceControlRevisionKind.Unspecified;
	}

	public SourceControlRevision(DateTime referenceDate)
	{
		m_kind = SourceControlRevisionKind.Date;
		m_dateTime = referenceDate;
	}

	public SourceControlRevision(SourceControlRevisionKind kind)
	{
		m_kind = kind;
	}
}
