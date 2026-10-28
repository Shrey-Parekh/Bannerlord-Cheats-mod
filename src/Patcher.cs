using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Library;

namespace CustomCheats
{
    /// <summary>
    /// Applies Harmony patches defensively. Model patches target the implementation that the
    /// currently registered model actually runs (e.g. RBM's or Diplomacy's override), so cheats
    /// stack on top of other mods instead of replacing their models.
    /// </summary>
    internal static class Patcher
    {
        internal const string HarmonyId = "com.shreyparekh.customcheats";

        private static readonly Harmony Harmony = new Harmony(HarmonyId);
        private static readonly HashSet<string> ReportedErrors = new HashSet<string>();
        internal static readonly List<string> Failed = new List<string>();
        internal static int AppliedCount;

        /// <summary>Most-derived implementation of <paramref name="abstractMethod"/> on the model's runtime type.</summary>
        internal static MethodInfo Impl(object model, MethodInfo abstractMethod)
        {
            if (model == null || abstractMethod == null)
                return null;
            var types = abstractMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            var method = AccessTools.Method(model.GetType(), abstractMethod.Name, types);
            // Harmony only patches a method through its declaring type (inherited overrides are reflected via the subclass).
            return method == null || method.DeclaringType == method.ReflectedType
                ? method
                : AccessTools.DeclaredMethod(method.DeclaringType, method.Name, types);
        }

        internal static MethodInfo Method(Type type, string name, Type[] args = null) => AccessTools.Method(type, name, args);

        internal static MethodInfo Getter(Type type, string property)
        {
            var getter = AccessTools.PropertyGetter(type, property);
            return getter == null || getter.DeclaringType == getter.ReflectedType
                ? getter
                : AccessTools.DeclaredPropertyGetter(getter.DeclaringType, property);
        }

        internal static void Postfix(string label, MethodBase target, Type patchType, string patchMethod) =>
            Apply(label, target, null, new HarmonyMethod(AccessTools.Method(patchType, patchMethod)));

        internal static void Prefix(string label, MethodBase target, Type patchType, string patchMethod) =>
            Apply(label, target, new HarmonyMethod(AccessTools.Method(patchType, patchMethod)), null);

        private static void Apply(string label, MethodBase target, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            if (target == null)
            {
                Failed.Add($"{label}: target method not found");
                return;
            }
            if (target.IsAbstract)
            {
                Failed.Add($"{label}: {target.DeclaringType?.Name}.{target.Name} is abstract");
                return;
            }
            try
            {
                Harmony.Patch(target, prefix, postfix);
                AppliedCount++;
            }
            catch (Exception e)
            {
                Failed.Add($"{label}: {target.DeclaringType?.Name}.{target.Name} ({e.GetType().Name}: {e.Message})");
            }
        }

        internal static void UnpatchAll()
        {
            Harmony.UnpatchAll(HarmonyId);
            AppliedCount = 0;
            Failed.Clear();
        }

        /// <summary>Logs a patch exception once per label so a broken cheat never spams or crashes the game.</summary>
        internal static void Error(string label, Exception e)
        {
            if (!ReportedErrors.Add(label))
                return;
            Log.Write($"Error in {label}: {e}");
            Log.Message($"Custom Cheats: '{label}' failed and was skipped. See CustomCheats.log.", Colors.Red);
        }
    }

    internal static class Log
    {
        private static readonly string FilePath =
            Path.Combine(Path.GetDirectoryName(typeof(Log).Assembly.Location) ?? ".", "CustomCheats.log");

        internal static void Write(string text)
        {
            try
            {
                File.AppendAllText(FilePath, $"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            }
            catch
            {
                // Logging must never break the game.
            }
        }

        internal static void Message(string text, Color? color = null)
        {
            try
            {
                InformationManager.DisplayMessage(new InformationMessage(text, color ?? Colors.Green));
            }
            catch
            {
                Write(text);
            }
        }
    }
}
