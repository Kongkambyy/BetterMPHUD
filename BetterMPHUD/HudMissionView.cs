using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data; // Needed for IGauntletMovie interface
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using BetterMPHUD.ViewModels;

namespace BetterMPHUD.Views
{
    [DefaultView]
    public class HudMissionView : MissionView
    {
        private GauntletLayer _configLayer;
        private HudMenuVM _dataSource;
        private IGauntletMovie _configMovie;
        
        private float _lastOpenTime = -1;
        private float _openDebounceTime = 0.5f;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

            _dataSource = new HudMenuVM();
            _dataSource.OnCloseConfigMenu = CloseConfigMenu;

            // FIX 1: Constructor order matches your decompiled code: (string name, int localOrder)
            _configLayer = new GauntletLayer("GauntletLayer", 50, false); 
            
            // FIX 2: LoadMovie returns GauntletMovieIdentifier, so we must access the .Movie property
            // to assign it to our IGauntletMovie variable.
            _configMovie = _configLayer.LoadMovie("HudConfig", _dataSource).Movie;

            MissionScreen.AddLayer(_configLayer);
            
            // Disable input initially
            _configLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Invalid);
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (Input.IsKeyPressed(InputKey.BackSpace) && (_lastOpenTime + _openDebounceTime < Mission.CurrentTime))
            {
                if (_dataSource.IsConfigMenuOpen)
                    CloseConfigMenu();
                else
                    OpenConfigMenu();
            }
        }

        public void OpenConfigMenu()
        {
            _dataSource.IsConfigMenuOpen = true;
            _configLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _lastOpenTime = Mission.CurrentTime;
        }

        public void CloseConfigMenu()
        {
            _dataSource.IsConfigMenuOpen = false;
            _configLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Invalid);
            _lastOpenTime = Mission.CurrentTime;
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            if (_configLayer != null)
            {
                MissionScreen.RemoveLayer(_configLayer);
                _configLayer = null;
            }
            _dataSource = null;
            _configMovie = null;
        }
    }
}