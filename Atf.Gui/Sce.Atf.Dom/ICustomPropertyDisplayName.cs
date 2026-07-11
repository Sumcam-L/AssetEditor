namespace Sce.Atf.Dom;

public interface ICustomPropertyDisplayName : INonCacheableDescriptor
{
	string GetDisplayName(object component);
}
