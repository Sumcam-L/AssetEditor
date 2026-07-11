using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using Firaxis.MathEx;

namespace Firaxis.CivTech;

[Export(typeof(BugSubmitter))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BugSubmitter : IPartImportsSatisfiedNotification
{
	private static ISet<ulong> seenOnce;

	private static ICrashSubmissionService s_crashSubmissionService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<ICrashSubmissionService> m_crashSubmissionService = null;

	static BugSubmitter()
	{
		seenOnce = new HashSet<ulong>();
		s_crashSubmissionService = null;
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		s_crashSubmissionService = m_crashSubmissionService?.Value;
	}

	public static void SilentException(Exception exObj)
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
		s_crashSubmissionService?.SubmitIssue(SubmissionType.kSilentAssert, exObj);
	}

	public static void SilentExceptionOnce(Exception exObj)
	{
		ulong item = FNV1a.HashString64(exObj.ToString(), bIgnoreCase: false);
		if (seenOnce.Add(item))
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
			s_crashSubmissionService?.SubmitIssue(SubmissionType.kSilentAssert, exObj);
		}
	}

	public static void SilentAssert(bool condition, string message)
	{
		if (!condition)
		{
			DoSubmission(SubmissionType.kSilentAssert, message, isFatal: false);
		}
	}

	public static void SilentAssertOnce(bool condition, string fmtText, params object[] fmtParams)
	{
		if (!condition)
		{
			string text = string.Format(fmtText, fmtParams);
			ulong item = FNV1a.HashString64(text, bIgnoreCase: false);
			if (seenOnce.Add(item))
			{
				DoSubmission(SubmissionType.kSilentAssert, text, isFatal: false);
			}
		}
	}

	public static void SilentAssert(bool condition, string fmtText, params object[] fmtParams)
	{
		if (!condition)
		{
			string message = string.Format(fmtText, fmtParams);
			DoSubmission(SubmissionType.kSilentAssert, message, isFatal: false);
		}
	}

	public static void SilentReport(string message)
	{
		DoSubmission(SubmissionType.kSilentAssert, message, isFatal: false);
	}

	public static void SilentReportOnce(string message)
	{
		ulong item = FNV1a.HashString64(message, bIgnoreCase: false);
		if (seenOnce.Add(item))
		{
			DoSubmission(SubmissionType.kSilentAssert, message, isFatal: false);
		}
	}

	public static void Exception(Exception exObj)
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
		s_crashSubmissionService?.SubmitIssue(SubmissionType.kAssert, exObj);
	}

	public static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			DoSubmission(SubmissionType.kAssert, message, isFatal: true);
		}
	}

	public static void Assert(bool condition, string fmtText, params object[] fmtParams)
	{
		if (!condition)
		{
			string message = string.Format(fmtText, fmtParams);
			DoSubmission(SubmissionType.kAssert, message, isFatal: true);
		}
	}

	private static void DoSubmission(SubmissionType subType, string message, bool isFatal)
	{
		if (Debugger.IsAttached)
		{
			Debugger.Log(99, Debugger.DefaultCategory, $"{subType.ToString()}: {message}\n");
		}
		s_crashSubmissionService?.SubmitIssue(subType, message);
		if (isFatal)
		{
			MessageBox.Show($"{message}\n\nApplication will exit", "Unrecoverable application error!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Application.Exit();
		}
	}
}
