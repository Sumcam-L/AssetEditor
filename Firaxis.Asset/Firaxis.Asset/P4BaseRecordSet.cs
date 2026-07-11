using System.Collections;

namespace Firaxis.Asset;

public class P4BaseRecordSet
{
	internal ArrayList StringOutputs;

	internal ArrayList InfoOutputs;

	internal ArrayList ErrorOutputs;

	internal ArrayList WarningOutputs;

	internal ArrayList TaggedOutputs;

	private string _loginPassword;

	private string _inputData = string.Empty;

	private Hashtable FormInputDictionary;

	internal string _SpecDef;

	internal virtual string SpecDef
	{
		get
		{
			return _SpecDef;
		}
		set
		{
			_SpecDef = value;
		}
	}

	public string[] Errors => (string[])ErrorOutputs.ToArray(typeof(string));

	public string ErrorMessage
	{
		get
		{
			string text = "";
			string[] errors = Errors;
			foreach (string text2 in errors)
			{
				text = text + text2 + "\n";
			}
			return text;
		}
	}

	public string[] Warnings => (string[])WarningOutputs.ToArray(typeof(string));

	internal Hashtable FormInput
	{
		get
		{
			return FormInputDictionary;
		}
		set
		{
			FormInputDictionary = value;
		}
	}

	internal string InputData
	{
		get
		{
			return _inputData;
		}
		set
		{
			_inputData = value;
		}
	}

	internal string LoginPassword
	{
		get
		{
			return _loginPassword;
		}
		set
		{
			_loginPassword = value;
		}
	}

	internal P4BaseRecordSet()
	{
		StringOutputs = new ArrayList();
		InfoOutputs = new ArrayList();
		ErrorOutputs = new ArrayList();
		WarningOutputs = new ArrayList();
		TaggedOutputs = new ArrayList();
	}

	internal void AddInfo(string S)
	{
		StringOutputs.Add(S);
	}

	internal void AddString(string S)
	{
		InfoOutputs.Add(S);
	}

	internal void AddError(string S)
	{
		ErrorOutputs.Add(S);
	}

	internal void AddWarning(string S)
	{
		WarningOutputs.Add(S);
	}

	internal virtual void AddTag(Hashtable S)
	{
		P4Record value = new P4Record();
		TaggedOutputs.Add(value);
	}

	public bool HasErrors()
	{
		return ErrorOutputs.Count != 0;
	}

	public bool HasWarnings()
	{
		return WarningOutputs.Count != 0;
	}
}
