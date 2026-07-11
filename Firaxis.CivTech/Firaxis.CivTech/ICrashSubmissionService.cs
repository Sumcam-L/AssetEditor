using System;

namespace Firaxis.CivTech;

public interface ICrashSubmissionService
{
	bool UseBugHelper { get; }

	ulong SessionHash { get; set; }

	void EnableBugHelper(string gameBasePath);

	void SubmitIssue(SubmissionType issueType, string message);

	void SubmitIssue(SubmissionType issueType, Exception excObj);

	void AddAttachment(Uri attachment);

	void RemoveAttachment(Uri attachment);
}
