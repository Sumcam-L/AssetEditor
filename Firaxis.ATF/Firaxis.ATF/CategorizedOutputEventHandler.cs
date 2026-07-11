using Sce.Atf;

namespace Firaxis.ATF;

public delegate void CategorizedOutputEventHandler(string context, OutputMessageType msgType, OutputMessageVerbosity msgVerb, string text);
