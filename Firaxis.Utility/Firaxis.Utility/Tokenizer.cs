using System;
using System.Collections.Generic;
using System.Text;
using Firaxis.Collections;

namespace Firaxis.Utility;

public class Tokenizer : List<string>
{
	private int idx;

	private bool inquote;

	private string source;

	private char[] seps;

	private StringSplitOptions options;

	private bool IsEOL => idx >= source.Length;

	private bool IsQuote => source[idx] == '"';

	private bool IsSep
	{
		get
		{
			char c = source[idx];
			if (!inquote && seps.Find((char a) => a == c) != 0)
			{
				return true;
			}
			return c == '"';
		}
	}

	public Tokenizer(string value)
		: this(value, StringSplitOptions.RemoveEmptyEntries)
	{
	}

	public Tokenizer(string value, StringSplitOptions options)
	{
		this.options = options;
		char[] array = new char[3] { ',', ' ', '|' };
		Build(value, array);
	}

	public Tokenizer(string value, char[] seps, StringSplitOptions options)
	{
		this.options = options;
		Build(value, seps);
	}

	public static IList<string> Tokenize(string src)
	{
		return new Tokenizer(src);
	}

	public static IList<string> Tokenize(string src, char[] seps, StringSplitOptions options)
	{
		return new Tokenizer(src, seps, options);
	}

	private void Build(string src, char[] seps)
	{
		idx = 0;
		inquote = false;
		source = src;
		this.seps = seps;
		Clear();
		string next;
		while (!string.IsNullOrEmpty(next = GetNext()))
		{
			if ((options & StringSplitOptions.RemoveEmptyEntries) == 0 || !StringHelper.IsNullEmptyOrWhiteSpace(next))
			{
				Add(next);
			}
		}
	}

	private string GetNext()
	{
		StringBuilder stringBuilder = new StringBuilder(source.Length);
		if (!IsEOL && IsSep)
		{
			if (IsQuote)
			{
				inquote = true;
			}
			idx++;
		}
		while (!IsEOL && !IsSep)
		{
			stringBuilder.Append(source[idx++]);
		}
		while (!IsEOL && IsSep)
		{
			idx++;
		}
		if (!IsEOL && IsQuote)
		{
			inquote = !inquote;
			idx++;
		}
		return stringBuilder.ToString();
	}
}
