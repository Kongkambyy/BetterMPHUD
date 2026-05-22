using BetterMPHUD.Behaviors;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace BetterMPHUD
{
    public class SubModule : MBSubModuleBase
    {
        static SubModule()
        {
            TryClearModuleShaderCache();
        }

        private static void TryClearModuleShaderCache()
        {
            try
            {
                string assemblyDir = Path.GetDirectoryName(typeof(SubModule).Assembly.Location);
                if (string.IsNullOrEmpty(assemblyDir)) return;

                // DLL is at [GameRoot]/Modules/BetterMPHUD/bin/Win64_Shipping_Client/ — 4 levels up = game root
                string gameRoot = assemblyDir;
                for (int i = 0; i < 4; i++)
                {
                    string parent = Path.GetDirectoryName(gameRoot);
                    if (string.IsNullOrEmpty(parent)) break;
                    gameRoot = parent;
                }

                string[] patterns = { "*ui_hudmenu*", "*ui_warbandkillfeed*", "*BetterMPHUD*" };
                string[] cacheDirs = {
                    Path.Combine(gameRoot, "Cache"),
                    Path.Combine(gameRoot, "cache"),
                    Path.Combine(gameRoot, "Shaders", "native_cache"),
                    Path.Combine(gameRoot, "shaders", "native_cache"),
                    Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        "Mount and Blade II Bannerlord"),
                };

                foreach (string dir in cacheDirs)
                {
                    if (!Directory.Exists(dir)) continue;
                    foreach (string pattern in patterns)
                        DeleteMatchingFiles(dir, pattern);
                }
            }
            catch { }
        }

        private static void DeleteMatchingFiles(string directory, string pattern)
        {
            try
            {
                foreach (string file in Directory.GetFiles(directory, pattern))
                {
                    try { File.Delete(file); } catch { }
                }
                foreach (string subDir in Directory.GetDirectories(directory, pattern))
                {
                    try { Directory.Delete(subDir, true); } catch { }
                }
                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    foreach (string file in Directory.GetFiles(subDir, pattern))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Loaded", Colors.Cyan));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new HudBehavior());
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Behavior Added", Colors.Cyan));
        }

        public override async void OnInitialState()
        {
            base.OnInitialState();
            try
            {
                for (int i = 0; i < 600; i++) // ~6s ceiling, not infinite
                {
                    var client = NetworkMain.GameClient;
                    if (client == null) { await Task.Delay(10); continue; }

                    var field = client.GetType().GetField("_loadedUnofficialModules",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field == null) return;

                    if (field.GetValue(client) is List<ModuleInfoModel> moduleList)
                    {
                        field.SetValue(client, moduleList.Where(m => m.Id != "BetterMPHUD").ToList());
                        return;
                    }
                    await Task.Delay(10);
                }
            }
            catch { /* never let an async void escape */ }
        }
    }
}