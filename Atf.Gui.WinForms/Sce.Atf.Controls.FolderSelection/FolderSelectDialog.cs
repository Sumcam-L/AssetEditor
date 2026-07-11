using System;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf.Controls.FolderSelection;

public class FolderSelectDialog : IDisposable
{
	private class WindowWrapper : IWin32Window
	{
		public IntPtr Handle { get; private set; }

		public WindowWrapper(IntPtr handle)
		{
			Handle = handle;
		}
	}

	private class Reflector
	{
		private readonly string m_ns;

		private readonly Assembly m_asmb;

		public Reflector(string ns)
			: this(ns, ns)
		{
		}

		private Reflector(string an, string ns)
		{
			m_ns = ns;
			m_asmb = null;
			AssemblyName[] referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
			foreach (AssemblyName assemblyName in referencedAssemblies)
			{
				if (assemblyName.FullName.StartsWith(an))
				{
					m_asmb = Assembly.Load(assemblyName);
					break;
				}
			}
		}

		public Type GetType(string typeName)
		{
			Type type = null;
			string[] array = typeName.Split('.');
			if (array.Length != 0)
			{
				type = m_asmb.GetType(m_ns + "." + array[0]);
			}
			for (int i = 1; i < array.Length; i++)
			{
				type = type.GetNestedType(array[i], BindingFlags.NonPublic);
			}
			return type;
		}

		public object New(string name, params object[] parameters)
		{
			Type type = GetType(name);
			ConstructorInfo[] constructors = type.GetConstructors();
			ConstructorInfo[] array = constructors;
			foreach (ConstructorInfo constructorInfo in array)
			{
				try
				{
					return constructorInfo.Invoke(parameters);
				}
				catch
				{
				}
			}
			return null;
		}

		public object Call(object obj, string func, params object[] parameters)
		{
			return Call2(obj, func, parameters);
		}

		private object Call2(object obj, string func, object[] parameters)
		{
			return CallAs2(obj.GetType(), obj, func, parameters);
		}

		public object CallAs(Type type, object obj, string func, params object[] parameters)
		{
			return CallAs2(type, obj, func, parameters);
		}

		public object CallAs2(Type type, object obj, string func, object[] parameters)
		{
			MethodInfo method = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return method.Invoke(obj, parameters);
		}

		public object GetEnum(string typeName, string name)
		{
			Type type = GetType(typeName);
			FieldInfo field = type.GetField(name);
			return field.GetValue(null);
		}
	}

	private readonly OpenFileDialog m_ofd;

	public string InitialDirectory
	{
		get
		{
			return m_ofd.InitialDirectory;
		}
		set
		{
			m_ofd.InitialDirectory = (string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value);
		}
	}

	public string Description
	{
		get
		{
			return m_ofd.Title;
		}
		set
		{
			m_ofd.Title = value ?? "Select a directory".Localize();
		}
	}

	public string SelectedPath => m_ofd.FileName;

	private bool Disposed { get; set; }

	public FolderSelectDialog()
	{
		m_ofd = new OpenFileDialog
		{
			Filter = "Folders|\\n",
			AddExtension = false,
			CheckFileExists = false,
			DereferenceLinks = true,
			Multiselect = false
		};
		Disposed = false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public DialogResult ShowDialog()
	{
		return ShowDialog(null);
	}

	public DialogResult ShowDialog(IWin32Window owner)
	{
		DialogResult dialogResult;
		if (Environment.OSVersion.Version.Major >= 6)
		{
			Reflector reflector = new Reflector("System.Windows.Forms");
			uint num = 0u;
			Type type = reflector.GetType("FileDialogNative.IFileDialog");
			object obj = reflector.Call(m_ofd, "CreateVistaDialog");
			reflector.Call(m_ofd, "OnBeforeVistaDialog", obj);
			uint num2 = (uint)reflector.CallAs(typeof(FileDialog), m_ofd, "GetOptions");
			num2 |= (uint)reflector.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
			reflector.CallAs(type, obj, "SetOptions", num2);
			object obj2 = reflector.New("FileDialog.VistaDialogEvents", m_ofd);
			object[] array = new object[2] { obj2, num };
			reflector.CallAs2(type, obj, "Advise", array);
			num = (uint)array[1];
			try
			{
				dialogResult = (((int)reflector.CallAs(type, obj, "Show", owner?.Handle ?? IntPtr.Zero) == 0) ? DialogResult.OK : DialogResult.Cancel);
			}
			finally
			{
				reflector.CallAs(type, obj, "Unadvise", num);
				GC.KeepAlive(obj2);
			}
		}
		else
		{
			using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = Description;
			folderBrowserDialog.SelectedPath = InitialDirectory;
			folderBrowserDialog.ShowNewFolderButton = false;
			dialogResult = folderBrowserDialog.ShowDialog(owner);
			if (dialogResult == DialogResult.OK)
			{
				m_ofd.FileName = folderBrowserDialog.SelectedPath;
			}
		}
		return dialogResult;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			if (disposing && m_ofd != null)
			{
				m_ofd.Dispose();
			}
			Disposed = true;
		}
	}
}
