using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using CSLModdingFramework.Logging;
using HarmonyLib;

namespace CSLModdingFramework.HarmonyPatch;

public sealed class HarmonyPatcher {
    public Harmony HarmonyInstance { get; private set; }
    private ILog Logger { get; }
    public bool IsApplied { get; private set; }

    public HarmonyPatcher(string harmonyID, ILog logger = null) {
        if (string.IsNullOrEmpty(harmonyID))
            throw new ArgumentException("Harmony ID cannot be null or empty.", nameof(harmonyID));
        HarmonyInstance = new Harmony(harmonyID);
        Logger = logger ?? LogManager.GetLogger();
    }

    public void ApplyAll(Action<HarmonyPatcher> patchAction = null) {
        if (IsApplied) return;

        var timer = Stopwatch.StartNew();
        try {
            HarmonyInstance.PatchAll();
            patchAction?.Invoke(this);
            timer.Stop();
            IsApplied = true;
            Logger.Debug($"Enabled patch in {timer.ElapsedMilliseconds}ms");
        }
        catch (Exception ex) {
            timer.Stop();
            Logger.Error(ex, "Failed to enable patcher");
            RemoveAll();
        }
    }

    public void RemoveAll() {
        if (HarmonyInstance is null) return;
        var timer = Stopwatch.StartNew();
        try {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            IsApplied = false;
            timer.Stop();
            Logger.Info($"Disabled patch in {timer.ElapsedMilliseconds}ms");
        }
        catch (Exception ex) {
            timer.Stop();
            Logger.Error(ex, "Failed to disable patcher");
        }
    }

    public void ApplyPrefix<TypeTarget, TypePatch>(string targetMethod, string patchMethod, Type[] parameters = null) => Patch<TypeTarget, TypePatch>(HarmonyPatchType.Prefix, targetMethod, patchMethod, parameters);

    public void ApplyPrefix(Type targetType, string targetMethod, Type patchType, string patchMethod, Type[] parameters = null) => Patch(HarmonyPatchType.Prefix, targetType, targetMethod, patchType, patchMethod, parameters);

    public void ApplyPrefix(MethodInfo targetMethodInfo, MethodInfo patchMethodInfo) => Patch(HarmonyPatchType.Prefix, targetMethodInfo, patchMethodInfo);

    public void ApplyPostfix<TypeTarget, TypePatch>(string targetMethod, string patchMethod, Type[] parameters = null) => Patch<TypeTarget, TypePatch>(HarmonyPatchType.Postfix, targetMethod, patchMethod, parameters);

    public void ApplyPostfix(Type targetType, string targetMethod, Type patchType, string patchMethod, Type[] parameters = null) => Patch(HarmonyPatchType.Postfix, targetType, targetMethod, patchType, patchMethod, parameters);

    public void ApplyPostfix(MethodInfo targetMethodInfo, MethodInfo patchMethodInfo) => Patch(HarmonyPatchType.Postfix, targetMethodInfo, patchMethodInfo);

    public void ApplyTranspiler<TypeTarget, TypePatch>(string targetMethod, string patchMethod, Type[] parameters = null) => Patch<TypeTarget, TypePatch>(HarmonyPatchType.Transpiler, targetMethod, patchMethod, parameters);

    public void ApplyTranspiler(Type targetType, string targetMethod, Type patchType, string patchMethod, Type[] parameters = null) => Patch(HarmonyPatchType.Transpiler, targetType, targetMethod, patchType, patchMethod, parameters);

    public void ApplyTranspiler(MethodInfo targetMethodInfo, MethodInfo patchMethodInfo) => Patch(HarmonyPatchType.Transpiler, targetMethodInfo, patchMethodInfo);

    public void ApplyFinalizer<TypeTarget, TypePatch>(string targetMethod, string patchMethod, Type[] parameters = null) => Patch<TypeTarget, TypePatch>(HarmonyPatchType.Finalizer, targetMethod, patchMethod, parameters);

    public void ApplyFinalizer(Type targetType, string targetMethod, Type patchType, string patchMethod, Type[] parameters = null) => Patch(HarmonyPatchType.Finalizer, targetType, targetMethod, patchType, patchMethod, parameters);

    public void ApplyFinalizer(MethodInfo targetMethodInfo, MethodInfo patchMethodInfo) => Patch(HarmonyPatchType.Finalizer, targetMethodInfo, patchMethodInfo);

    private void Patch<TypeTarget, TypePatch>(HarmonyPatchType patchType, string targetMethod, string patchMethod, Type[] parameters = null) => Patch(patchType, typeof(TypeTarget), targetMethod, typeof(TypePatch), patchMethod, parameters);

    private void Patch(HarmonyPatchType harmonyPatchType, Type targetType, string targetMethod, Type patchType, string patchMethod, Type[] parameters = null) {
        try {
            var target = AccessTools.Method(targetType, targetMethod, parameters);
            var patch = AccessTools.Method(patchType, patchMethod);
            if (target is null) {
                Logger.Error($"Target method [{targetType.FullName}.{targetMethod}] was not found during patch process");
                return;
            }

            if (patch is null) {
                Logger.Error($"Patch method [{patchType.FullName}.{patchMethod}] was not found during patch process");
                return;
            }

            ApplyPatch(harmonyPatchType, target, patch);
            Logger.Info($"[Harmony {harmonyPatchType}] target method: {target.DeclaringType?.FullName}.{targetMethod}, patch method: {patch.DeclaringType?.FullName}.{patchMethod}");
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Patch failed target:{targetType?.FullName}.{targetMethod} patch:{patchType?.FullName}.{patchMethod}");
        }
    }

    private void Patch(HarmonyPatchType harmonyPatchType, MethodInfo targetMethodInfo, MethodInfo patchMethodInfo) {
        try {
            if (targetMethodInfo is null) {
                var stackTrace = new StackTrace();
                var stackTraceString = stackTrace.ToString();
                Logger.Error($"Target method was not found during patch process:\n{stackTraceString}");
                return;
            }

            if (patchMethodInfo is null) {
                var stackTrace = new StackTrace();
                var stackTraceString = stackTrace.ToString();
                Logger.Error($"Patch method was not found during patch process:\n{stackTraceString}");
                return;
            }

            ApplyPatch(harmonyPatchType, targetMethodInfo, patchMethodInfo);
            Logger.Info($"[Harmony {harmonyPatchType}] target method: {targetMethodInfo.DeclaringType?.FullName}, patch method: {patchMethodInfo.DeclaringType?.FullName}");
        }
        catch (Exception ex) {
            Logger.Error(ex, $"Patch failed target:{targetMethodInfo?.DeclaringType?.FullName},  patch:{patchMethodInfo?.DeclaringType?.FullName}");
        }
    }

    private void ApplyPatch(HarmonyPatchType type, MethodInfo target, MethodInfo patch) {
        switch (type) {
            case HarmonyPatchType.Prefix:
                HarmonyInstance.Patch(target, new HarmonyMethod(patch));
                break;
            case HarmonyPatchType.Postfix:
                HarmonyInstance.Patch(target, postfix: new HarmonyMethod(patch));
                break;
            case HarmonyPatchType.Transpiler:
                HarmonyInstance.Patch(target, transpiler: new HarmonyMethod(patch));
                break;
            case HarmonyPatchType.Finalizer:
                HarmonyInstance.Patch(target, finalizer: new HarmonyMethod(patch));
                break;
            default:
                Logger.Error($"Unknown Harmony patch type: {type}");
                break;
        }
    }

    public void LogPatchedMethods() => Logger.Info(GetPatchedMethods());

    public string GetPatchedMethods() {
        var builder = new StringBuilder();
        builder.AppendLine("Harmony patched methods:");

        foreach (var method in HarmonyInstance.GetPatchedMethods()) {
            var info = Harmony.GetPatchInfo(method);

            builder.AppendLine();
            builder.AppendLine($"{method.DeclaringType?.FullName}.{method.Name}");

            if (info == null) continue;

            AppendPatchInfo(builder, "Prefix", info.Prefixes);
            AppendPatchInfo(builder, "Postfix", info.Postfixes);
            AppendPatchInfo(builder, "Transpiler", info.Transpilers);
            AppendPatchInfo(builder, "Finalizer", info.Finalizers);
        }

        return builder.ToString();
    }

    private void AppendPatchInfo(StringBuilder builder, string name, ReadOnlyCollection<Patch> patches) {
        if (patches == null || patches.Count == 0) return;

        builder.AppendLine($"  {name}:");

        foreach (var patch in patches) {
            builder.AppendLine(
                $"    {patch.owner} -> {patch.PatchMethod.DeclaringType?.FullName}.{patch.PatchMethod.Name}"
            );
        }
    }
}