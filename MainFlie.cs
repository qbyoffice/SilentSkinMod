using System.Reflection;
using Godot;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using Sts2Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace SilentSkinMod;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "SilentSkinMod";
    public static Sts2Logger Logger { get; } = new(ModId, LogType.Generic);

    private static void InitializeTools()
    {
        var assembly = Assembly.GetExecutingAssembly();
        ScriptManagerBridge.LookupScriptsInAssembly(assembly);
    }

    public static void Initialize()
    {
        InitializeTools();
        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }
}
