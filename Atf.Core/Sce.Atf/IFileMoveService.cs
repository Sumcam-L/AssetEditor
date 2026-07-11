using System.Collections.Generic;

namespace Sce.Atf;

public interface IFileMoveService
{
	void AtomicMove(IEnumerable<FileMoveInfo> moves);
}
