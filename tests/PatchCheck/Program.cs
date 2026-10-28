using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

// Applies every CustomCheats patch to model classes built without running the game:
// pass 1 uses vanilla/SandBox models, pass 2 prefers RBM's overrides where RBM has them.
// Any target that can not be found or any postfix Harmony rejects is reported and the exit code is 1.
internal static class Program
{
    private static string[] _probe;

    private static int Main()
    {
        _probe = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "probe.txt"))
            .SelectMany(l => l.Split(';')).Where(Directory.Exists).ToArray();
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
        {
            var name = new AssemblyName(e.Name).Name + ".dll";
            var path = _probe.Select(d => Path.Combine(d, name)).FirstOrDefault(File.Exists);
            return path == null ? null : Assembly.LoadFrom(path);
        };
        return Run();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Run()
    {
        var cheats = typeof(CustomCheats.SubModule).Assembly;
        var patcher = cheats.GetType("CustomCheats.Patcher");
        var failed = (List<string>)patcher.GetField("Failed", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        var unpatch = patcher.GetMethod("UnpatchAll", BindingFlags.Static | BindingFlags.NonPublic);
        var campaignApply = cheats.GetType("CustomCheats.CampaignPatches").GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic);
        var combat = cheats.GetType("CustomCheats.CombatPatches");

        var vanilla = new[] { "TaleWorlds.CampaignSystem", "TaleWorlds.MountAndBlade", "SandBox", "StoryMode" }.Select(Load).ToArray();
        var rbm = new[] { "RBM", "RBMAI", "RBMCampaign", "RBMCombat", "RBMTournament" }.Select(Load).Where(a => a != null).ToArray();

        int exit = 0;
        foreach (var (label, assemblies) in new[] { ("vanilla", vanilla), ("RBM", rbm.Concat(vanilla).ToArray()) })
        {
            unpatch.Invoke(null, null);
            var used = new List<string>();
            var gameModels = Fake(typeof(TaleWorlds.CampaignSystem.GameModels), assemblies, used);
            var missionModels = Fake(typeof(TaleWorlds.MountAndBlade.MissionGameModels), assemblies, used);

            combat.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            combat.GetMethod("ApplyModels", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { missionModels });
            campaignApply.Invoke(null, new[] { gameModels });

            int applied = (int)patcher.GetField("AppliedCount", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Console.WriteLine($"[{label}] applied {applied} patches, {failed.Count} failed. Non-vanilla models: {string.Join(", ", used.Where(u => !u.StartsWith("TaleWorlds") && !u.StartsWith("SandBox")))}");
            foreach (var f in failed)
                Console.WriteLine("  FAIL " + f);
            if (failed.Count > 0)
                exit = 1;
        }
        unpatch.Invoke(null, null);
        return exit;
    }

    private static Assembly Load(string name)
    {
        try { return Assembly.Load(name); } catch { return null; }
    }

    private static IEnumerable<Type> Types(Assembly a)
    {
        try { return a.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
    }

    /// <summary>Uninitialized model registry whose model properties hold uninitialized concrete models (first assembly wins).</summary>
    private static object Fake(Type registry, Assembly[] assemblies, List<string> used)
    {
        var instance = FormatterServices.GetUninitializedObject(registry);
        var all = assemblies.SelectMany(Types).Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters).ToList();
        foreach (var p in registry.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var setter = p.GetSetMethod(true);
            if (setter == null || !typeof(TaleWorlds.Core.GameModel).IsAssignableFrom(p.PropertyType))
                continue;
            var impl = all.FirstOrDefault(t => p.PropertyType.IsAssignableFrom(t));
            if (impl == null)
                continue;
            setter.Invoke(instance, new[] { FormatterServices.GetUninitializedObject(impl) });
            used.Add(impl.FullName);
        }
        return instance;
    }
}
