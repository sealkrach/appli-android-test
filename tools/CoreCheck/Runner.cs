using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

static class Runner
{
    static int Main()
    {
        int passed = 0, failed = 0;
        var fixtures = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "PolitiRush.Core.Tests" && t.IsClass && !t.IsNested);
        foreach (var f in fixtures)
        {
            var methods = f.GetMethods().Where(m => m.GetCustomAttribute<TestAttribute>() != null);
            foreach (var m in methods)
            {
                try { m.Invoke(Activator.CreateInstance(f), null); passed++; Console.WriteLine($"  ok   {f.Name}.{m.Name}"); }
                catch (TargetInvocationException ex) { failed++; Console.WriteLine($"  FAIL {f.Name}.{m.Name}: {ex.InnerException?.Message}"); }
            }
        }
        Console.WriteLine($"\n{passed} réussis, {failed} échoués");
        return failed == 0 ? 0 : 1;
    }
}
