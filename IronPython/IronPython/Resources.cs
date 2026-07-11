using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace IronPython;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("IronPython.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string CantFindMember => ResourceManager.GetString("CantFindMember", resourceCulture);

	internal static string DefaultRequired => ResourceManager.GetString("DefaultRequired", resourceCulture);

	internal static string DuplicateArgumentInFuncDef => ResourceManager.GetString("DuplicateArgumentInFuncDef", resourceCulture);

	internal static string DuplicateKeywordArg => ResourceManager.GetString("DuplicateKeywordArg", resourceCulture);

	internal static string EofInString => ResourceManager.GetString("EofInString", resourceCulture);

	internal static string EofInTripleQuotedString => ResourceManager.GetString("EofInTripleQuotedString", resourceCulture);

	internal static string EolInSingleQuotedString => ResourceManager.GetString("EolInSingleQuotedString", resourceCulture);

	internal static string ExpectedIndentation => ResourceManager.GetString("ExpectedIndentation", resourceCulture);

	internal static string ExpectedName => ResourceManager.GetString("ExpectedName", resourceCulture);

	internal static string ExpectingIdentifier => ResourceManager.GetString("ExpectingIdentifier", resourceCulture);

	internal static string InconsistentWhitespace => ResourceManager.GetString("InconsistentWhitespace", resourceCulture);

	internal static string IndentationMismatch => ResourceManager.GetString("IndentationMismatch", resourceCulture);

	internal static string InvalidArgumentValue => ResourceManager.GetString("InvalidArgumentValue", resourceCulture);

	internal static string InvalidOperation_MakeGenericOnNonGeneric => ResourceManager.GetString("InvalidOperation_MakeGenericOnNonGeneric", resourceCulture);

	internal static string InvalidParameters => ResourceManager.GetString("InvalidParameters", resourceCulture);

	internal static string InvalidSyntax => ResourceManager.GetString("InvalidSyntax", resourceCulture);

	internal static string KeywordCreateUnavailable => ResourceManager.GetString("KeywordCreateUnavailable", resourceCulture);

	internal static string KeywordOutOfSequence => ResourceManager.GetString("KeywordOutOfSequence", resourceCulture);

	internal static string MemberDoesNotExist => ResourceManager.GetString("MemberDoesNotExist", resourceCulture);

	internal static string MisplacedFuture => ResourceManager.GetString("MisplacedFuture", resourceCulture);

	internal static string MisplacedReturn => ResourceManager.GetString("MisplacedReturn", resourceCulture);

	internal static string MisplacedYield => ResourceManager.GetString("MisplacedYield", resourceCulture);

	internal static string NewLineInDoubleQuotedString => ResourceManager.GetString("NewLineInDoubleQuotedString", resourceCulture);

	internal static string NewLineInSingleQuotedString => ResourceManager.GetString("NewLineInSingleQuotedString", resourceCulture);

	internal static string NoFutureStar => ResourceManager.GetString("NoFutureStar", resourceCulture);

	internal static string NonKeywordAfterKeywordArg => ResourceManager.GetString("NonKeywordAfterKeywordArg", resourceCulture);

	internal static string NotAChance => ResourceManager.GetString("NotAChance", resourceCulture);

	internal static string NotImplemented => ResourceManager.GetString("NotImplemented", resourceCulture);

	internal static string OneKeywordArgOnly => ResourceManager.GetString("OneKeywordArgOnly", resourceCulture);

	internal static string OneListArgOnly => ResourceManager.GetString("OneListArgOnly", resourceCulture);

	internal static string PythonContextRequired => ResourceManager.GetString("PythonContextRequired", resourceCulture);

	internal static string Slot_CantDelete => ResourceManager.GetString("Slot_CantDelete", resourceCulture);

	internal static string Slot_CantGet => ResourceManager.GetString("Slot_CantGet", resourceCulture);

	internal static string Slot_CantSet => ResourceManager.GetString("Slot_CantSet", resourceCulture);

	internal static string StaticAccessFromInstanceError => ResourceManager.GetString("StaticAccessFromInstanceError", resourceCulture);

	internal static string StaticAssignmentFromInstanceError => ResourceManager.GetString("StaticAssignmentFromInstanceError", resourceCulture);

	internal static string TokenHasNoValue => ResourceManager.GetString("TokenHasNoValue", resourceCulture);

	internal static string TooManyVersions => ResourceManager.GetString("TooManyVersions", resourceCulture);

	internal static string UnexpectedToken => ResourceManager.GetString("UnexpectedToken", resourceCulture);

	internal static string UnknownFutureFeature => ResourceManager.GetString("UnknownFutureFeature", resourceCulture);

	internal Resources()
	{
	}
}
