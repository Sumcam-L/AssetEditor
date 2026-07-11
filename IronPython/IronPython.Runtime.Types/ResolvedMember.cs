using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Types;

internal class ResolvedMember
{
	public readonly string Name;

	public readonly MemberGroup Member;

	public static readonly ResolvedMember[] Empty = new ResolvedMember[0];

	public ResolvedMember(string name, MemberGroup member)
	{
		Name = name;
		Member = member;
	}
}
