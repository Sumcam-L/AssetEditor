using System;
using System.ComponentModel.Composition;
using Firaxis.CivTech;

namespace Firaxis.ATF;

[Export(typeof(ICrashSubmissionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class NullCrashSubmissionService : ICrashSubmissionService
{
	public bool UseBugHelper { get; }

	public ulong SessionHash { get; set; }

	public void AddAttachment(Uri attachment)
	{
	}

	public void EnableBugHelper(string gameBasePath)
	{
	}

	public void RemoveAttachment(Uri attachment)
	{
	}

	public void SubmitIssue(SubmissionType issueType, string message)
	{
	}

	public void SubmitIssue(SubmissionType issueType, System.Exception excObj)
	{
	}
}
