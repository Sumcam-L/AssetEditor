using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

internal static class DocBuilder
{
	private const string _frameworkReferencePath = "Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.0";

	private static readonly object _CachedDocLockObject = new object();

	private static readonly List<Assembly> _AssembliesWithoutXmlDoc = new List<Assembly>();

	private static XPathDocument _CachedDoc;

	private static string _CachedDocName;

	internal static string GetDefaultDocumentation(string methodName)
	{
		switch (methodName)
		{
		case "__abs__":
			return "x.__abs__() <==> abs(x)";
		case "__add__":
			return "x.__add__(y) <==> x+y";
		case "__call__":
			return "x.__call__(...) <==> x(...)";
		case "__cmp__":
			return "x.__cmp__(y) <==> cmp(x,y)";
		case "__delitem__":
			return "x.__delitem__(y) <==> del x[y]";
		case "__div__":
			return "x.__div__(y) <==> x/y";
		case "__eq__":
			return "x.__eq__(y) <==> x==y";
		case "__floordiv__":
			return "x.__floordiv__(y) <==> x//y";
		case "__getitem__":
			return "x.__getitem__(y) <==> x[y]";
		case "__gt__":
			return "x.__gt__(y) <==> x>y";
		case "__hash__":
			return "x.__hash__() <==> hash(x)";
		case "__init__":
			return "x.__init__(...) initializes x; see x.__class__.__doc__ for signature";
		case "__len__":
			return "x.__len__() <==> len(x)";
		case "__lshift__":
			return "x.__rshift__(y) <==> x<<y";
		case "__lt__":
			return "x.__lt__(y) <==> x<y";
		case "__mod__":
			return "x.__mod__(y) <==> x%y";
		case "__mul__":
			return "x.__mul__(y) <==> x*y";
		case "__neg__":
			return "x.__neg__() <==> -x";
		case "__pow__":
			return "x.__pow__(y[, z]) <==> pow(x, y[, z])";
		case "__reduce__":
		case "__reduce_ex__":
			return "helper for pickle";
		case "__rshift__":
			return "x.__rshift__(y) <==> x>>y";
		case "__setitem__":
			return "x.__setitem__(i, y) <==> x[i]=";
		case "__str__":
			return "x.__str__() <==> str(x)";
		case "__sub__":
			return "x.__sub__(y) <==> x-y";
		case "__truediv__":
			return "x.__truediv__(y) <==> x/y";
		default:
			return null;
		}
	}

	private static string DocOneInfoForProperty(Type declaringType, string propertyName, MethodInfo getter, MethodInfo setter, object[] attrs)
	{
		if (attrs == null || attrs.Length == 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string summary = null;
			string returns = null;
			GetXmlDocForProperty(declaringType, propertyName, out summary, out returns);
			if (summary != null)
			{
				stringBuilder.AppendLine(summary);
				stringBuilder.AppendLine();
			}
			if (getter != null)
			{
				stringBuilder.Append("Get: ");
				stringBuilder.AppendLine(CreateAutoDoc(getter, propertyName, 0));
			}
			if (setter != null)
			{
				stringBuilder.Append("Set: ");
				stringBuilder.Append(CreateAutoDoc(setter, propertyName, 1));
				stringBuilder.AppendLine(" = value");
			}
			return stringBuilder.ToString();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int i = 0; i < attrs.Length; i++)
		{
			stringBuilder2.Append(((DocumentationAttribute)attrs[i]).Documentation);
			stringBuilder2.Append(Environment.NewLine);
		}
		return stringBuilder2.ToString();
	}

	public static string DocOneInfo(ExtensionPropertyInfo info)
	{
		return DocOneInfoForProperty(info.DeclaringType, info.Name, info.Getter, info.Setter, null);
	}

	public static string DocOneInfo(PropertyInfo info)
	{
		object[] customAttributes = info.GetCustomAttributes(typeof(DocumentationAttribute), inherit: false);
		return DocOneInfoForProperty(info.DeclaringType, info.Name, info.GetGetMethod(), info.GetSetMethod(), customAttributes);
	}

	public static string DocOneInfo(FieldInfo info)
	{
		object[] customAttributes = info.GetCustomAttributes(typeof(DocumentationAttribute), inherit: false);
		return DocOneInfoForProperty(info.DeclaringType, info.Name, null, null, customAttributes);
	}

	public static string DocOneInfo(MethodBase info, string name)
	{
		return DocOneInfo(info, name, includeSelf: true);
	}

	public static string DocOneInfo(MethodBase info, string name, bool includeSelf)
	{
		object[] customAttributes = info.GetCustomAttributes(typeof(DocumentationAttribute), inherit: false);
		if (customAttributes.Length > 0)
		{
			DocumentationAttribute documentationAttribute = customAttributes[0] as DocumentationAttribute;
			return documentationAttribute.Documentation;
		}
		string defaultDocumentation = GetDefaultDocumentation(name);
		if (defaultDocumentation != null)
		{
			return defaultDocumentation;
		}
		return CreateAutoDoc(info, name, 0, includeSelf);
	}

	public static string CreateAutoDoc(MethodBase info)
	{
		return CreateAutoDoc(info, null, 0);
	}

	public static string CreateAutoDoc(EventInfo info)
	{
		string summary = null;
		GetXmlDoc(info, out summary, out var _);
		return summary;
	}

	public static string CreateAutoDoc(Type t)
	{
		string summary = null;
		GetXmlDoc(t, out summary);
		if (t.IsEnum)
		{
			string[] names = Enum.GetNames(t);
			Array values = Enum.GetValues(t);
			for (int i = 0; i < names.Length; i++)
			{
				names[i] = names[i] + " (" + Convert.ChangeType(values.GetValue(i), Enum.GetUnderlyingType(t), null).ToString() + ")";
			}
			Array.Sort(names);
			summary = summary + Environment.NewLine + Environment.NewLine + "enum " + (t.IsDefined(typeof(FlagsAttribute), inherit: false) ? "(flags) " : "") + GetPythonTypeName(t) + ", values: " + string.Join(", ", names);
		}
		return summary;
	}

	public static OverloadDoc GetOverloadDoc(MethodBase info, string name, int endParamSkip)
	{
		return GetOverloadDoc(info, name, endParamSkip, includeSelf: true);
	}

	public static OverloadDoc GetOverloadDoc(MethodBase info, string name, int endParamSkip, bool includeSelf)
	{
		string summary = null;
		string returns = null;
		List<KeyValuePair<string, string>> parameters = null;
		GetXmlDoc(info, out summary, out returns, out parameters);
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		MethodInfo methodInfo = info as MethodInfo;
		if (methodInfo != null)
		{
			if (methodInfo.ReturnType != typeof(void))
			{
				stringBuilder.Append(GetPythonTypeName(methodInfo.ReturnType));
				num++;
				object[] customAttributes = methodInfo.ReturnParameter.GetCustomAttributes(typeof(SequenceTypeInfoAttribute), inherit: true);
				if (customAttributes.Length > 0)
				{
					stringBuilder.Append(" (of ");
					SequenceTypeInfoAttribute sequenceTypeInfoAttribute = (SequenceTypeInfoAttribute)customAttributes[0];
					for (int i = 0; i < sequenceTypeInfoAttribute.Types.Count; i++)
					{
						if (i != 0)
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.Append(GetPythonTypeName(sequenceTypeInfoAttribute.Types[i]));
					}
					stringBuilder.Append(")");
				}
				object[] customAttributes2 = methodInfo.ReturnParameter.GetCustomAttributes(typeof(DictionaryTypeInfoAttribute), inherit: true);
				if (customAttributes2.Length > 0)
				{
					DictionaryTypeInfoAttribute dictionaryTypeInfoAttribute = (DictionaryTypeInfoAttribute)customAttributes2[0];
					stringBuilder.Append($" (of {GetPythonTypeName(dictionaryTypeInfoAttribute.KeyType)} to {GetPythonTypeName(dictionaryTypeInfoAttribute.ValueType)})");
				}
			}
			if (name == null)
			{
				int num2 = methodInfo.Name.IndexOf('#');
				name = ((num2 != -1) ? methodInfo.Name.Substring(0, num2) : methodInfo.Name);
			}
		}
		else if (name == null)
		{
			name = "__new__";
		}
		if (methodInfo != null && methodInfo.IsGenericMethod)
		{
			Type[] genericArguments = methodInfo.GetGenericArguments();
			bool containsGenericParameters = methodInfo.ContainsGenericParameters;
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(name);
			stringBuilder2.Append("[");
			if (genericArguments.Length > 1)
			{
				stringBuilder2.Append("(");
			}
			bool flag = false;
			Type[] array = genericArguments;
			foreach (Type type in array)
			{
				if (flag)
				{
					stringBuilder2.Append(", ");
				}
				if (containsGenericParameters)
				{
					stringBuilder2.Append(type.Name);
				}
				else
				{
					stringBuilder2.Append(GetPythonTypeName(type));
				}
				flag = true;
			}
			if (genericArguments.Length > 1)
			{
				stringBuilder2.Append(")");
			}
			stringBuilder2.Append("]");
			name = stringBuilder2.ToString();
		}
		List<ParameterDoc> list = new List<ParameterDoc>();
		if (methodInfo == null)
		{
			if (name == "__new__")
			{
				list.Add(new ParameterDoc("cls", "type"));
			}
		}
		else if (!methodInfo.IsStatic && includeSelf)
		{
			list.Add(new ParameterDoc("self", GetPythonTypeName(methodInfo.DeclaringType)));
		}
		ParameterInfo[] parameters2 = info.GetParameters();
		for (int k = 0; k < parameters2.Length - endParamSkip; k++)
		{
			ParameterInfo parameterInfo = parameters2[k];
			if (k == 0 && parameterInfo.ParameterType == typeof(CodeContext))
			{
				continue;
			}
			if ((parameterInfo.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out || parameterInfo.ParameterType.IsByRef)
			{
				if (num == 1)
				{
					stringBuilder.Insert(0, "(");
				}
				if (num != 0)
				{
					stringBuilder.Append(", ");
				}
				num++;
				stringBuilder.Append(GetPythonTypeName(parameterInfo.ParameterType));
				if ((parameterInfo.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out)
				{
					continue;
				}
			}
			ParameterFlags parameterFlags = ParameterFlags.None;
			if (parameterInfo.IsDefined(typeof(ParamArrayAttribute), inherit: false))
			{
				parameterFlags |= ParameterFlags.ParamsArray;
			}
			else if (parameterInfo.IsDefined(typeof(ParamDictionaryAttribute), inherit: false))
			{
				parameterFlags |= ParameterFlags.ParamsDict;
			}
			string documentation = null;
			if (parameters != null)
			{
				foreach (KeyValuePair<string, string> item in parameters)
				{
					if (item.Key == parameterInfo.Name)
					{
						documentation = item.Value;
						break;
					}
				}
			}
			list.Add(new ParameterDoc(parameterInfo.Name ?? "", GetPythonTypeName(parameterInfo.ParameterType), documentation, parameterFlags));
		}
		if (num > 1)
		{
			stringBuilder.Append(')');
		}
		ParameterDoc returnParameter = new ParameterDoc(string.Empty, stringBuilder.ToString(), returns);
		return new OverloadDoc(name, summary, list, returnParameter);
	}

	internal static string CreateAutoDoc(MethodBase info, string name, int endParamSkip)
	{
		return CreateAutoDoc(info, name, endParamSkip, includeSelf: true);
	}

	internal static string CreateAutoDoc(MethodBase info, string name, int endParamSkip, bool includeSelf)
	{
		int lineWidth;
		try
		{
			lineWidth = Console.WindowWidth - 30;
		}
		catch
		{
			lineWidth = 80;
		}
		OverloadDoc overloadDoc = GetOverloadDoc(info, name, endParamSkip, includeSelf);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(overloadDoc.Name);
		stringBuilder.Append("(");
		string value = "";
		foreach (ParameterDoc parameter in overloadDoc.Parameters)
		{
			stringBuilder.Append(value);
			if ((parameter.Flags & ParameterFlags.ParamsArray) != ParameterFlags.None)
			{
				stringBuilder.Append('*');
			}
			else if ((parameter.Flags & ParameterFlags.ParamsDict) != ParameterFlags.None)
			{
				stringBuilder.Append("**");
			}
			stringBuilder.Append(parameter.Name);
			if (!string.IsNullOrEmpty(parameter.TypeName))
			{
				stringBuilder.Append(": ");
				stringBuilder.Append(parameter.TypeName);
			}
			value = ", ";
		}
		stringBuilder.Append(")");
		if (!string.IsNullOrEmpty(overloadDoc.ReturnParameter.TypeName))
		{
			stringBuilder.Append(" -> ");
			stringBuilder.AppendLine(overloadDoc.ReturnParameter.TypeName);
		}
		if (!string.IsNullOrEmpty(overloadDoc.Documentation))
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(StringUtils.SplitWords(overloadDoc.Documentation, indentFirst: true, lineWidth));
		}
		bool flag = false;
		foreach (ParameterDoc parameter2 in overloadDoc.Parameters)
		{
			if (!string.IsNullOrEmpty(parameter2.Documentation))
			{
				if (!flag)
				{
					stringBuilder.AppendLine();
					flag = true;
				}
				stringBuilder.Append("    ");
				stringBuilder.Append(parameter2.Name);
				stringBuilder.Append(": ");
				stringBuilder.AppendLine(StringUtils.SplitWords(parameter2.Documentation, indentFirst: false, lineWidth));
			}
		}
		if (!string.IsNullOrEmpty(overloadDoc.ReturnParameter.Documentation))
		{
			stringBuilder.Append("    Returns: ");
			stringBuilder.AppendLine(StringUtils.SplitWords(overloadDoc.ReturnParameter.Documentation, indentFirst: false, lineWidth));
		}
		return stringBuilder.ToString();
	}

	private static string GetPythonTypeName(Type type)
	{
		if (type.IsByRef)
		{
			type = type.GetElementType();
		}
		return DynamicHelpers.GetPythonTypeFromType(type).Name;
	}

	private static string GetXmlName(Type type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("T:");
		AppendTypeFormat(type, stringBuilder);
		return stringBuilder.ToString();
	}

	private static string GetXmlName(EventInfo field)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("E:");
		AppendTypeFormat(field.DeclaringType, stringBuilder);
		stringBuilder.Append('.');
		stringBuilder.Append(field.Name);
		return stringBuilder.ToString();
	}

	private static string GetXmlNameForProperty(Type declaringType, string propertyName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("P:");
		stringBuilder.Append(declaringType.Namespace);
		stringBuilder.Append('.');
		stringBuilder.Append(declaringType.Name);
		stringBuilder.Append('.');
		stringBuilder.Append(propertyName);
		return stringBuilder.ToString();
	}

	private static string GetXmlName(MethodBase info)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("M:");
		stringBuilder.Append(info.DeclaringType.Namespace);
		stringBuilder.Append('.');
		stringBuilder.Append(info.DeclaringType.Name);
		stringBuilder.Append('.');
		stringBuilder.Append(info.Name);
		ParameterInfo[] parameters = info.GetParameters();
		if (parameters.Length > 0)
		{
			stringBuilder.Append('(');
			for (int i = 0; i < parameters.Length; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				if (i != 0)
				{
					stringBuilder.Append(',');
				}
				AppendTypeFormat(parameterType, stringBuilder);
			}
			stringBuilder.Append(')');
		}
		return stringBuilder.ToString();
	}

	private static void AppendTypeFormat(Type curType, StringBuilder res)
	{
		if (curType.IsGenericType)
		{
			curType = curType.GetGenericTypeDefinition();
		}
		if (curType.IsGenericParameter)
		{
			res.Append('`');
			res.Append(curType.GenericParameterPosition);
		}
		else if (curType.ContainsGenericParameters)
		{
			res.Append(curType.Namespace);
			res.Append('.');
			res.Append(curType.Name.Substring(0, curType.Name.Length - 2));
			res.Append('{');
			Type[] genericArguments = curType.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (i != 0)
				{
					res.Append(',');
				}
				if (genericArguments[i].IsGenericParameter)
				{
					res.Append('`');
					res.Append(genericArguments[i].GenericParameterPosition);
				}
				else
				{
					AppendTypeFormat(genericArguments[i], res);
				}
			}
			res.Append('}');
		}
		else
		{
			res.Append(curType.FullName);
		}
	}

	private static string GetXmlDocLocation(Assembly assem)
	{
		if (_AssembliesWithoutXmlDoc.Contains(assem))
		{
			return null;
		}
		string text = null;
		try
		{
			text = assem.Location;
		}
		catch (SecurityException)
		{
		}
		catch (NotSupportedException)
		{
		}
		if (text == null)
		{
			_AssembliesWithoutXmlDoc.Add(assem);
		}
		return text;
	}

	private static XPathDocument GetXPathDocument(Assembly asm)
	{
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		string xmlDocLocation = GetXmlDocLocation(asm);
		if (xmlDocLocation == null)
		{
			return null;
		}
		string directoryName = Path.GetDirectoryName(xmlDocLocation);
		string path = Path.GetFileNameWithoutExtension(xmlDocLocation) + ".xml";
		string text = Path.Combine(Path.Combine(directoryName, currentCulture.Name), path);
		if (!File.Exists(text))
		{
			int num = currentCulture.Name.IndexOf('-');
			if (num != -1)
			{
				text = Path.Combine(Path.Combine(directoryName, currentCulture.Name.Substring(0, num)), path);
			}
			if (!File.Exists(text))
			{
				text = Path.Combine(directoryName, path);
				if (!File.Exists(text))
				{
					text = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.0"), path);
					if (!File.Exists(text))
					{
						_AssembliesWithoutXmlDoc.Add(asm);
						return null;
					}
				}
			}
		}
		lock (_CachedDocLockObject)
		{
			XPathDocument result = (_CachedDoc = ((!(_CachedDocName == text)) ? new XPathDocument(text) : _CachedDoc));
			_CachedDocName = text;
			return result;
		}
	}

	private static void GetXmlDoc(MethodBase info, out string summary, out string returns, out List<KeyValuePair<string, string>> parameters)
	{
		summary = null;
		returns = null;
		parameters = null;
		XPathDocument xPathDocument = GetXPathDocument(info.DeclaringType.Assembly);
		if (xPathDocument == null)
		{
			return;
		}
		XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
		string xpath = "/doc/members/member[@name='" + GetXmlName(info) + "']/*";
		XPathNodeIterator xPathNodeIterator = xPathNavigator.Select(xpath);
		while (xPathNodeIterator.MoveNext())
		{
			switch (xPathNodeIterator.Current.Name)
			{
			case "summary":
				summary = XmlToString(xPathNodeIterator);
				break;
			case "returns":
				returns = XmlToString(xPathNodeIterator);
				break;
			case "param":
			{
				string text = null;
				string text2 = XmlToString(xPathNodeIterator);
				if (xPathNodeIterator.Current.MoveToFirstAttribute())
				{
					text = xPathNodeIterator.Current.Value;
				}
				if (text != null)
				{
					if (parameters == null)
					{
						parameters = new List<KeyValuePair<string, string>>();
					}
					parameters.Add(new KeyValuePair<string, string>(text, text2.Trim()));
				}
				break;
			}
			}
		}
	}

	private static void GetXmlDoc(Type type, out string summary)
	{
		summary = null;
		XPathDocument xPathDocument = GetXPathDocument(type.Assembly);
		if (xPathDocument == null)
		{
			return;
		}
		XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
		string xpath = "/doc/members/member[@name='" + GetXmlName(type) + "']/*";
		XPathNodeIterator xPathNodeIterator = xPathNavigator.Select(xpath);
		while (xPathNodeIterator.MoveNext())
		{
			string name;
			if ((name = xPathNodeIterator.Current.Name) != null && name == "summary")
			{
				summary = XmlToString(xPathNodeIterator);
			}
		}
	}

	private static void GetXmlDocForProperty(Type declaringType, string propertyName, out string summary, out string returns)
	{
		summary = null;
		returns = null;
		XPathDocument xPathDocument = GetXPathDocument(declaringType.Assembly);
		if (xPathDocument == null)
		{
			return;
		}
		XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
		string xpath = "/doc/members/member[@name='" + GetXmlNameForProperty(declaringType, propertyName) + "']/*";
		XPathNodeIterator xPathNodeIterator = xPathNavigator.Select(xpath);
		while (xPathNodeIterator.MoveNext())
		{
			switch (xPathNodeIterator.Current.Name)
			{
			case "summary":
				summary = XmlToString(xPathNodeIterator);
				break;
			case "returns":
				returns = XmlToString(xPathNodeIterator);
				break;
			}
		}
	}

	private static void GetXmlDoc(EventInfo info, out string summary, out string returns)
	{
		summary = null;
		returns = null;
		XPathDocument xPathDocument = GetXPathDocument(info.DeclaringType.Assembly);
		if (xPathDocument == null)
		{
			return;
		}
		XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
		string xpath = "/doc/members/member[@name='" + GetXmlName(info) + "']/*";
		XPathNodeIterator xPathNodeIterator = xPathNavigator.Select(xpath);
		while (xPathNodeIterator.MoveNext())
		{
			string name;
			if ((name = xPathNodeIterator.Current.Name) != null && name == "summary")
			{
				summary = XmlToString(xPathNodeIterator) + Environment.NewLine;
			}
		}
	}

	private static string XmlToString(XPathNodeIterator iter)
	{
		XmlReader xmlReader = iter.Current.ReadSubtree();
		StringBuilder stringBuilder = new StringBuilder();
		if (xmlReader.Read())
		{
			do
			{
				IL_001d:
				switch (xmlReader.NodeType)
				{
				case XmlNodeType.Text:
					stringBuilder.Append(xmlReader.ReadString());
					goto IL_001d;
				case XmlNodeType.Element:
					switch (xmlReader.Name)
					{
					case "see":
						if (xmlReader.MoveToFirstAttribute() && xmlReader.ReadAttributeValue())
						{
							int num = xmlReader.Value.IndexOf('`');
							if (num != -1)
							{
								stringBuilder.Append(xmlReader.Value, 2, num - 2);
							}
							else
							{
								stringBuilder.Append(xmlReader.Value, 2, xmlReader.Value.Length - 2);
							}
						}
						break;
					case "paramref":
						if (xmlReader.MoveToAttribute("name"))
						{
							stringBuilder.Append(xmlReader.Value);
						}
						break;
					}
					break;
				}
			}
			while (xmlReader.Read());
		}
		return stringBuilder.ToString().Trim();
	}
}
