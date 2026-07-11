using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(CommandLineArgsService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CommandLineArgsService : IInitializable, IPartImportsSatisfiedNotification
{
	public class ArgParserException : Exception
	{
		public ArgParserException(string message)
			: base(message)
		{
		}
	}

	private interface IOption
	{
		object Value { get; }

		int Set(string[] args, int i);
	}

	private class GenericOption : IOption
	{
		private Type m_type;

		private object m_value;

		public object Value => m_value;

		public GenericOption(Type type, object defaultValue)
		{
			m_type = type;
			m_value = defaultValue;
		}

		public int Set(string[] args, int i)
		{
			if (i >= args.Length - 1)
			{
				throw new ArgParserException($"\"{args[0]}\" argument requires a value");
			}
			string text = args[i + 1];
			TypeConverter converter = TypeDescriptor.GetConverter(m_type);
			try
			{
				m_value = converter.ConvertFromString(text);
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
		private object m_value;

		public object Value => m_value;

		public BoolOption(object defaultValue)
		{
			m_value = defaultValue;
		}

		public int Set(string[] args, int i)
		{
			m_value = true;
			return i;
		}
	}

	private readonly IDocumentRegistry m_documentRegistry;

	private readonly IDocumentService m_documentService;

	private IList<string> m_parameters;

	private Hashtable m_options = new Hashtable();

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	[ImportMany]
	private IEnumerable<Lazy<IDocumentClient>> m_documentClients;

	public IList<string> Parameters => m_parameters;

	public object this[string name] => ((IOption)m_options[name])?.Value;

	[ImportingConstructor]
	public CommandLineArgsService(IDocumentRegistry documentRegistry, IDocumentService documentService)
	{
		m_documentRegistry = documentRegistry;
		m_documentService = documentService;
	}

	public void AddOption(string name, Type type, object defaultValue)
	{
		m_options[name] = NewOption(type, defaultValue);
	}

	public void AddAlias(string aliasName, string optionName)
	{
		m_options[aliasName] = m_options[optionName];
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		if (m_mainWindow == null && m_mainForm != null)
		{
			m_mainWindow = new MainFormAdapter(m_mainForm);
		}
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
		m_mainWindow.Loading += mainWindow_Loaded;
	}

	void IInitializable.Initialize()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string[] array = new string[commandLineArgs.Length - 1];
		Array.Copy(commandLineArgs, 1, array, 0, array.Length);
		Parse(array);
	}

	private static bool IsArgOption(string arg)
	{
		return arg[0] == '-' || arg[0] == '/';
	}

	public void Parse(string[] args)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < args.Length; i++)
		{
			string text = args[i];
			if (IsArgOption(text))
			{
				if (text.Length == 1)
				{
					throw new ArgParserException($"Couldn't parse argument \"{text}\"");
				}
				string text2 = text.Substring(1);
				if (i < args.Length - 1 && !IsArgOption(args[i + 1]))
				{
					AddOption(text2, typeof(string), string.Empty);
				}
				else
				{
					AddOption(text2, typeof(bool), true);
				}
				IOption option = (IOption)m_options[text2];
				if (option == null)
				{
					throw new ArgParserException($"Couldn't parse argument \"{text}\"");
				}
				i = option.Set(args, i);
			}
			else
			{
				list.Add(text);
			}
		}
		m_parameters = list;
	}

	private IOption NewOption(Type type, object defaultValue)
	{
		if (type == typeof(bool))
		{
			return new BoolOption(defaultValue);
		}
		return new GenericOption(type, defaultValue);
	}

	private void mainWindow_Loaded(object sender, EventArgs e)
	{
		bool flag = m_documentRegistry.ActiveDocument != null;
		if (Parameters.Count <= 0 || flag)
		{
			return;
		}
		foreach (string parameter in Parameters)
		{
			if (!Uri.TryCreate(parameter, UriKind.RelativeOrAbsolute, out var result))
			{
				continue;
			}
			foreach (IDocumentClient value in m_documentClients.GetValues())
			{
				if (value.CanOpen(result))
				{
					m_documentService.OpenExistingDocument(value, result);
					break;
				}
			}
		}
	}
}
