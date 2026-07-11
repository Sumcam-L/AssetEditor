using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class CursorResourceAttribute : Attribute
{
	private readonly string m_cursorName;

	public string CursorName => m_cursorName;

	public CursorResourceAttribute(string cursorName)
	{
		m_cursorName = cursorName;
	}
}
