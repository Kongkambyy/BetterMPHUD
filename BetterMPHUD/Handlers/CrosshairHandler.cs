using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using BetterMPHUD.ViewModels;
using BetterMPHUD.Services;

namespace BetterMPHUD.Handlers
{
    public class CrosshairHandler
    {
        private GauntletLayer _customLayer;
        private GauntletLayer _nativeLayer; // Store reference to native layer
        private CrosshairVM _viewModel;

        public bool IsInitialized { get { return _customLayer != null; } }

        public void Initialize(MissionScreen screen)
        {
            // 1. Find and Cache the Native Layer (Do not remove it)
            try 
            {
                var nativeBehavior = LayerFinder.FindBehaviorByName(Mission.Current, "MissionGauntletCrosshair");
                if (nativeBehavior != null)
                {
                    _nativeLayer = LayerFinder.FindInBehavior(nativeBehavior);
                }
            }
            catch (Exception) { /* Native layer not found, harmless */ }

            // 2. Initialize Custom Layer
            _viewModel = new CrosshairVM();
            _customLayer = new GauntletLayer("GauntletLayer", 3, false);
            _customLayer.LoadMovie("BetterCrosshair", _viewModel);
            screen.AddLayer(_customLayer);

            CombatLogManager.OnGenerateCombatLog += OnCombatLogGenerated;
        }

        public void Update(HudSettings settings, MissionScreen screen)
        {
            if (_viewModel == null || screen == null) return;

            CrosshairSettings cs = settings.CrosshairSettings;

            // --- TOGGLE LOGIC START ---
            if (cs.CustomCrosshairEnabled)
            {
                // MODE: Custom Crosshair Active
                
                // 1. Hide Native Layer (Set Alpha to 0)
                if (_nativeLayer != null && _nativeLayer.UIContext != null)
                {
                    _nativeLayer.UIContext.ContextAlpha = 0f;
                }

                // 2. Enable Custom Logic
                _viewModel.CustomEnabled = true;
                _viewModel.CrosshairColor = cs.Color;
                _viewModel.CrosshairOpacity = cs.Opacity;
                _viewModel.CrosshairSizeHor = cs.SizeHorizontal;
                _viewModel.CrosshairSizeVert = cs.SizeVertical;
                _viewModel.CrosshairOffset = cs.Offset;

                UpdateVisibility(screen);
                UpdateCrosshairProperties(screen);
            }
            else
            {
                // MODE: Native Crosshair Active (Custom Disabled)

                // 1. Show Native Layer (Restore Alpha to 1)
                if (_nativeLayer != null && _nativeLayer.UIContext != null)
                {
                    _nativeLayer.UIContext.ContextAlpha = 1f;
                }

                // 2. Hide Custom Layer
                _viewModel.IsVisible = false;
                _viewModel.CustomEnabled = false;
            }
            // --- TOGGLE LOGIC END ---
        }

        private void UpdateVisibility(MissionScreen screen)
        {
            // Double check CustomEnabled to ensure we don't accidentally show it
            if (!_viewModel.CustomEnabled) 
            {
                _viewModel.IsVisible = false;
                return;
            }

            if (Mission.Current == null || Mission.Current.MainAgent == null)
            {
                _viewModel.IsVisible = false;
                return;
            }

            Agent mainAgent = Mission.Current.MainAgent;
            MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;

            bool shouldShow = BannerlordConfig.DisplayTargetingReticule &&
                              Mission.Current.Mode != MissionMode.Conversation &&
                              Mission.Current.Mode != MissionMode.CutScene &&
                              !wieldedWeapon.IsEmpty &&
                              wieldedWeapon.CurrentUsageItem != null &&
                              wieldedWeapon.CurrentUsageItem.IsRangedWeapon &&
                              !screen.IsViewingCharacter() &&
                              screen.CustomCamera == null;

            // Handle Crossbow specific reload visibility logic
            if (shouldShow && wieldedWeapon.CurrentUsageItem.WeaponClass == WeaponClass.Crossbow)
                shouldShow = !wieldedWeapon.IsReloading;

            _viewModel.IsVisible = shouldShow;
        }

        private void UpdateCrosshairProperties(MissionScreen screen)
        {
            if (!_viewModel.IsVisible) return;

            Agent mainAgent = Mission.Current.MainAgent;
            double fovAngle = screen.CameraViewAngle * (Math.PI / 180.0);
            
            // Calculate accuracy using native math logic
            double accuracy = 2.0 * Math.Tan(
                (mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) *
                (0.5 / Math.Tan(fovAngle * 0.5)));
            
            double scale = 1.0 + (screen.CombatCamera.HorizontalFov - 1.5707963705062866) / 1.5707963705062866;

            _viewModel.CrosshairAccuracy = accuracy;
            _viewModel.CrosshairScale = scale;
            _viewModel.CrosshairType = BannerlordConfig.CrosshairType;
        }

        private void OnCombatLogGenerated(CombatLogData logData)
        {
            if (_viewModel == null || !_viewModel.CustomEnabled) return;

            bool isAttackerMine = logData.IsAttackerAgentMine;
            bool isValidHit = !logData.IsVictimAgentSameAsAttackerAgent && !logData.IsFriendlyFire;
            bool isHeadshot = logData.IsAttackerAgentHuman && logData.BodyPartHit == BoneBodyPartType.Head;

            if (isAttackerMine && isValidHit && logData.TotalDamage > 0)
                _viewModel.ShowHitMarker(logData.IsFatalDamage, isHeadshot);
        }

        public void Cleanup(MissionScreen screen)
        {
            CombatLogManager.OnGenerateCombatLog -= OnCombatLogGenerated;

            // Ensure native layer is visible again when our mod unloads/mission ends
            if (_nativeLayer != null && _nativeLayer.UIContext != null)
            {
                _nativeLayer.UIContext.ContextAlpha = 1f;
            }

            if (screen != null && _customLayer != null)
                screen.RemoveLayer(_customLayer);

            _customLayer = null;
            _nativeLayer = null;
            _viewModel = null;
        }
    }
}