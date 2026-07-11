using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;

namespace Firaxis.CivTech.Packages;

public class FlattenedXLP : IFlattenedXLP, IXLP, IAssemblyInstance, IDisposable, ISerializable, IVersionedData
{
	private readonly ICollection<IXLP> _xlps = new List<IXLP>();

	private readonly IDictionary<string, IXLPEntry> _entryMap = new Dictionary<string, IXLPEntry>();

	private readonly Action<string> _errorAction;

	private string _className;

	private string _package;

	public IEnumerable<Platforms> AllowedPlatforms => Enum.GetValues(typeof(Platforms)).Cast<Platforms>();

	public string ClassName
	{
		get
		{
			return _className;
		}
		set
		{
			BugSubmitter.SilentReport("Unable to set the class name on flattened XLPs!  @assign dgurley");
		}
	}

	public string Package
	{
		get
		{
			return _package;
		}
		set
		{
			BugSubmitter.SilentReport("Unable to set the package on flattened XLPs!  @assign dgurley");
		}
	}

	public Version Version => new Version(1, 0, 0, 0);

	public IList<IXLPEntry> XLPEntries => _entryMap.Values.ToList();

	public FlattenedXLP(Action<string> errorAction)
	{
		BugSubmitter.Assert(errorAction != null, "Error action cannot be null! @assign dgurley");
		_errorAction = errorAction;
	}

	public IXLPEntry FindEntry(string ID)
	{
		_entryMap.TryGetValue(ID, out var value);
		return value;
	}

	public bool IsPlatformAllowed(Platforms ePlatform)
	{
		return true;
	}

	public void AddXLP(IXLP xlp)
	{
		AddXLPs(new IXLP[1] { xlp });
	}

	public void AddXLPs(IEnumerable<IXLP> xlps)
	{
		foreach (IXLP xlp in xlps)
		{
			if (IsValidXLP(xlp))
			{
				_className = xlp.ClassName;
				_package = xlp.Package;
				foreach (IXLPEntry xLPEntry in xlp.XLPEntries)
				{
					_entryMap[xLPEntry.ID] = xLPEntry;
				}
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (_className != xlp.ClassName)
			{
				stringBuilder.AppendFormat("There's an XLP in your pantry that has the same File Name as another XLP but has a different XLP Class which is not allowed.  Expected class: {0}, Invalid class: {1}", _className, xlp.ClassName);
				stringBuilder.AppendLine();
			}
			if (_package != xlp.Package)
			{
				stringBuilder.AppendFormat("There's an XLP in your pantry that has the same File Name as another XLP but has a different Package Name which is not allowed.  Expected package: {0}, Invalid package: {1}", _package, xlp.Package);
				stringBuilder.AppendLine();
			}
			_errorAction(stringBuilder.ToString());
		}
	}

	private bool IsValidXLP(IXLP xlp)
	{
		if (string.IsNullOrEmpty(_className) && string.IsNullOrEmpty(_package))
		{
			return true;
		}
		return _className == xlp.ClassName && _package == xlp.Package;
	}

	public void Reset()
	{
		_className = string.Empty;
		_package = string.Empty;
		_entryMap.Clear();
		foreach (IXLP xlp in _xlps)
		{
			xlp.Dispose();
		}
		_xlps.Clear();
	}

	public void Dispose()
	{
		Reset();
	}

	public IXLPEntry AddEntry(string ID, string objectName)
	{
		BugSubmitter.SilentReport("Unable to add Entries to flattened XLPs!  @assign dgurley");
		return null;
	}

	public void AllowPlatform(Platforms ePlatform)
	{
		BugSubmitter.SilentReport("Unable to change allowed platforms on flattened XLPs!  @assign dgurley");
	}

	public void ClearAllowedPlatforms()
	{
		BugSubmitter.SilentReport("Unable to change allowed platforms on flattened XLPs!  @assign dgurley");
	}

	public ResultCode DeserializeFromFile(string filename)
	{
		BugSubmitter.SilentReport("Unable to deserialize flattened XLPs!  @assign dgurley");
		return ResultCode.Success;
	}

	public bool DeserializeFromXML(string xmlText)
	{
		BugSubmitter.SilentReport("Unable to deserialize flattened XLPs!  @assign dgurley");
		return false;
	}

	public void RemoveEntry(string ID)
	{
		BugSubmitter.SilentReport("Unable to remove Entries from flattened XLPs!  @assign dgurley");
	}

	public bool SerializeIntoFile(string filename)
	{
		BugSubmitter.SilentReport("Unable to serialize flattened XLPs!  @assign dgurley");
		return false;
	}

	public string SerializeIntoXML()
	{
		BugSubmitter.SilentReport("Unable to serialize flattened XLPs!  @assign dgurley");
		return string.Empty;
	}

	public void SetVersion(string versionString)
	{
		BugSubmitter.SilentReport("Unable to set the version on flattened XLPs!  @assign dgurley");
	}

	public void SetVersion(int major, int minor, int build, int revision)
	{
		BugSubmitter.SilentReport("Unable to set the version on flattened XLPs!  @assign dgurley");
	}
}
