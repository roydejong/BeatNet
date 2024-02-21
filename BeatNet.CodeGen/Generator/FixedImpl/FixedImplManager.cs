using System.Reflection;

namespace BeatNet.CodeGen.Generator.FixedImpl;

public static class FixedImplManager
{
    private static readonly List<FixedImpl> FixedImpls;
    
    static FixedImplManager()
    {
        FixedImpls = LoadFixedImpls();
    }
    
    private static List<FixedImpl> LoadFixedImpls()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(FixedImpl)))
            .Select(fixedImplType =>(FixedImpl)Activator.CreateInstance(fixedImplType)!)
            .ToList();
    }
    
    public static FixedImpl? TryFindFixedImpl(string typeName) =>
        FixedImpls.FirstOrDefault(fixedImpl => fixedImpl.AppliesToType(typeName));
}