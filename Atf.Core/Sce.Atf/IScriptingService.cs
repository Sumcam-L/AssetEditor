using System.Reflection;

namespace Sce.Atf;

public interface IScriptingService
{
	string DisplayName { get; }

	void LoadAssembly(Assembly assembly);

	void ImportAllTypes(string nmspace);

	void ImportType(string nmspace, string typename);

	bool TryGetVariable<T>(string name, out T var);

	void SetVariable(string name, object var);

	void RemoveVariable(string name);

	string ExecuteStatement(string statement);

	string ExecuteStatements(string statements);

	string ExecuteFile(string fileName);
}
