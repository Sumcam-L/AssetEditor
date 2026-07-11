using Reflection;

namespace Firaxis.CivTech.ReflectionHelperEx;

public interface INativeReflection
{
	unsafe TypeInfo* GetInstanceTypeInfo();
}
