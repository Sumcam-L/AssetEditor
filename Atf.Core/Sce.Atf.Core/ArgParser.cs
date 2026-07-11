using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Core;

public class ArgParser
{
	private interface IOption
	{
		object Value { get; }

		int Set(string[] args, int i);
	}

	private class GenericOption : IOption
	{
		private Type _type;

		private object _value;

		public object Value => _value;

		public GenericOption(Type type, object defaultValue)
		{
			_type = type;
			_value = defaultValue;
		}

		public int Set(string[] args, int i)
		{
			if (i >= args.Length - 1)
			{
				throw new ArgParserException($"\"{args[0]}\" argument requires a value");
			}
			string text = args[i + 1];
			TypeConverter converter = TypeDescriptor.GetConverter(_type);
			try
			{
				_value = converter.ConvertFromString(text);
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
				Outputs.WriteLine(OutputMessageType.Info, ex.StackTrace);
				throw new ArgParserException($"Couldn't parse value \"{text}\" for argument \"{args[0]}\"");
			}
			return i + 1;
		}
	}

	private class BoolOption : IOption
	{
		private object _value;

		public object Value => _value;

		public BoolOption(object defaultValue)
		{
			_value = defaultValue;
		}

		public int Set(string[] args, int i)
		{
			_value = true;
			return i;
		}
	}

	private string[] _args;

	private IList<string> _extraArgs;

	private Hashtable _options = new Hashtable();

	public object this[string name] => ((IOption)_options[name])?.Value;

	public IList<string> ExtraArgs => _extraArgs;

	public ArgParser()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		_args = new string[commandLineArgs.Length - 1];
		Array.Copy(commandLineArgs, 1, _args, 0, _args.Length);
	}

	public ArgParser(string[] args)
	{
		_args = args;
	}

	public void AddOption(string name, Type type, object defaultValue)
	{
		_options[name] = NewOption(type, defaultValue);
	}

	public void AddAlias(string aliasName, string optionName)
	{
		_options[aliasName] = _options[optionName];
	}

	public void Parse()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < _args.Length; i++)
		{
			string text = _args[i];
			if (text[0] == '-' || text[0] == '/')
			{
				if (text.Length == 1)
				{
					throw new ArgParserException($"Couldn't parse argument \"{text}\"");
				}
				IOption option = (IOption)_options[text.Substring(1)];
				if (option == null)
				{
					throw new ArgParserException($"Couldn't parse argument \"{text}\"");
				}
				i = option.Set(_args, i);
			}
			else
			{
				list.Add(text);
			}
		}
		_extraArgs = list;
	}

	private IOption NewOption(Type type, object defaultValue)
	{
		if (type == typeof(bool))
		{
			return new BoolOption(defaultValue);
		}
		return new GenericOption(type, defaultValue);
	}
}
