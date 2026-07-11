using System;

namespace Sce.Atf;

public interface IValidationContext
{
	event EventHandler Beginning;

	event EventHandler Cancelled;

	event EventHandler Ending;

	event EventHandler Ended;
}
