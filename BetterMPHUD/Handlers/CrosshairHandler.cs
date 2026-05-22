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
        private GauntletLayer _nativeLayer;
        private CrosshairVM _viewModel;
        private double[] _targetGadgetOpacities = new double[4];
        private bool _isCleanedUp;

        public bool IsInitialized
        {
            get { return _customLayer != null && !_isCleanedUp; }
        }

        public void Initialize(MissionScreen screen)
        {
            try
            {
                var nativeBehavior = LayerFinder.FindBehaviorByName(Mission.Current, "MissionGauntletCrosshair");
                if (nativeBehavior != null)
                    _nativeLayer = LayerFinder.FindInBehavior(nativeBehavior);
            }
            catch (Exception)
            {
            }

            _viewModel = new CrosshairVM();
            _customLayer = new GauntletLayer("BetterCrosshairLayer", 3, false);
            _customLayer.LoadMovie("BetterCrosshair", _viewModel);
            screen.AddLayer(_customLayer);

            CombatLogManager.OnGenerateCombatLog += OnCombatLogGenerated;
        }

        public void Update(HudSettings settings, MissionScreen screen)
        {
            if (_viewModel == null || screen == null) return;

            CrosshairSettings cs = settings.CrosshairSettings;

            if (cs.CustomCrosshairEnabled)
            {
                if (_nativeLayer != null && _nativeLayer.UIContext != null)
                    _nativeLayer.UIContext.ContextAlpha = 0f;

                _viewModel.CustomEnabled = true;
                _viewModel.CrosshairColor = cs.Color;
                _viewModel.CrosshairOpacity = cs.Opacity;
                _viewModel.CrosshairSizeHor = cs.SizeHorizontal;
                _viewModel.CrosshairSizeVert = cs.SizeVertical;
                _viewModel.CrosshairOffset = cs.Offset;

                UpdateCrosshairVisibility(screen);
                UpdateCrosshairProperties(screen);
                UpdateReloadPhase();
                UpdateMeleeDirections();
            }
            else
            {
                if (_nativeLayer != null && _nativeLayer.UIContext != null)
                    _nativeLayer.UIContext.ContextAlpha = 1f;

                _viewModel.IsVisible = false;
                _viewModel.CustomEnabled = false;
                _viewModel.ClearReloadPhases();
            }

            _viewModel.IsDotEnabled = cs.DotEnabled;
            _viewModel.DotColor = cs.DotColor;
            _viewModel.DotSizeWidth = cs.DotSizeWidth;
            _viewModel.DotSizeHeight = cs.DotSizeHeight;
            _viewModel.DotIsCircular = cs.DotIsCircular;
            _viewModel.DotOffsetX = cs.DotOffsetX;
            _viewModel.DotOffsetY = cs.DotOffsetY;
            UpdateDotVisibility(screen);
        }

        private void UpdateCrosshairVisibility(MissionScreen screen)
        {
            if (Mission.Current == null || Mission.Current.MainAgent == null)
            {
                _viewModel.IsVisible = false;
                return;
            }

            Agent mainAgent = Mission.Current.MainAgent;
            MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;

            bool isRangedWeapon = !wieldedWeapon.IsEmpty &&
                                  wieldedWeapon.CurrentUsageItem != null &&
                                  wieldedWeapon.CurrentUsageItem.IsRangedWeapon;

            bool baseConditions = BannerlordConfig.DisplayTargetingReticule &&
                                  Mission.Current.Mode != MissionMode.Conversation &&
                                  Mission.Current.Mode != MissionMode.CutScene &&
                                  !screen.IsViewingCharacter() &&
                                  screen.CustomCamera == null;

            bool shouldShowCrosshair = baseConditions && isRangedWeapon;

            if (shouldShowCrosshair && wieldedWeapon.CurrentUsageItem.WeaponClass == WeaponClass.Crossbow)
            {
                try
                {
                    shouldShowCrosshair = !wieldedWeapon.IsReloading;
                }
                catch (Exception)
                {
                    shouldShowCrosshair = false;
                }
            }

            _viewModel.IsVisible = shouldShowCrosshair;
        }

        private void UpdateDotVisibility(MissionScreen screen)
        {
            if (!_viewModel.IsDotEnabled)
            {
                _viewModel.IsDotVisible = false;
                return;
            }

            if (Mission.Current == null || Mission.Current.MainAgent == null)
            {
                _viewModel.IsDotVisible = false;
                return;
            }

            bool baseConditions = BannerlordConfig.DisplayTargetingReticule &&
                                  Mission.Current.Mode != MissionMode.Conversation &&
                                  Mission.Current.Mode != MissionMode.CutScene &&
                                  !screen.IsViewingCharacter() &&
                                  screen.CustomCamera == null;

            _viewModel.IsDotVisible = baseConditions;
        }

        private void UpdateVisibility(MissionScreen screen)
        {
            if (!_viewModel.CustomEnabled)
            {
                _viewModel.IsVisible = false;
                _viewModel.IsDotVisible = false;
                return;
            }

            if (Mission.Current == null || Mission.Current.MainAgent == null)
            {
                _viewModel.IsVisible = false;
                _viewModel.IsDotVisible = false;
                return;
            }

            Agent mainAgent = Mission.Current.MainAgent;
            MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;

            bool isRangedWeapon = !wieldedWeapon.IsEmpty &&
                                  wieldedWeapon.CurrentUsageItem != null &&
                                  wieldedWeapon.CurrentUsageItem.IsRangedWeapon;

            bool baseConditions = BannerlordConfig.DisplayTargetingReticule &&
                                  Mission.Current.Mode != MissionMode.Conversation &&
                                  Mission.Current.Mode != MissionMode.CutScene &&
                                  !screen.IsViewingCharacter() &&
                                  screen.CustomCamera == null;

            bool shouldShowCrosshair = baseConditions && isRangedWeapon;

            if (shouldShowCrosshair && wieldedWeapon.CurrentUsageItem.WeaponClass == WeaponClass.Crossbow)
            {
                try
                {
                    shouldShowCrosshair = !wieldedWeapon.IsReloading;
                }
                catch (Exception)
                {
                    shouldShowCrosshair = false;
                }
            }

            _viewModel.IsVisible = shouldShowCrosshair;

            bool shouldShowDot = _viewModel.IsDotEnabled && baseConditions && !isRangedWeapon;
            _viewModel.IsDotVisible = shouldShowDot;
        }

        private void UpdateCrosshairProperties(MissionScreen screen)
        {
            if (Mission.Current == null || Mission.Current.MainAgent == null) return;

            Agent mainAgent = Mission.Current.MainAgent;
            double fovAngle = screen.CameraViewAngle * (Math.PI / 180.0);

            double accuracy = 2.0 * Math.Tan(
                (mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) *
                (0.5 / Math.Tan(fovAngle * 0.5)));

            double scale = 1.0 + (screen.CombatCamera.HorizontalFov - 1.5707963705062866) / 1.5707963705062866;

            _viewModel.CrosshairAccuracy = accuracy;
            _viewModel.CrosshairScale = scale;
            _viewModel.CrosshairType = BannerlordConfig.CrosshairType;
        }

        private void UpdateReloadPhase()
        {
            if (Mission.Current == null || Mission.Current.MainAgent == null)
            {
                _viewModel.ClearReloadPhases();
                return;
            }

            Agent mainAgent = Mission.Current.MainAgent;
            WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);

            if (!wieldedWeaponInfo.IsValid || !wieldedWeaponInfo.IsRangedWeapon ||
                !BannerlordConfig.DisplayTargetingReticule)
            {
                _viewModel.ClearReloadPhases();
                return;
            }

            MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;

            if (wieldedWeapon.IsEmpty || wieldedWeapon.CurrentUsageItem == null)
            {
                _viewModel.ClearReloadPhases();
                return;
            }

            // Read the agent's current channel-1 action BEFORE doing any reload-phase work.
            // When picking up a partially-reloaded crossbow, MissionWeapon.IsReloading is
            // persisted on the weapon itself and returns true immediately on pickup, but
            // the agent is still in the equip/pickup animation — not Reload. Entering the
            // phase computation in that state crashes on stale weapon/action-set lookups.
            // Native MissionGauntletCrosshair gates the entire block on this; we do the same.
            Agent.ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);

            // Cache all native property accesses in one guarded block to avoid
            // crashes during weapon pickup/switch transitions.
            int reloadPhaseCount;
            short reloadPhase;

            try
            {
                reloadPhaseCount = wieldedWeapon.ReloadPhaseCount;
                if (reloadPhaseCount <= 1 ||
                    !wieldedWeapon.IsReloading ||
                    currentActionType != Agent.ActionCodeType.Reload)
                {
                    _viewModel.ClearReloadPhases();
                    return;
                }

                reloadPhase = wieldedWeapon.ReloadPhase;
            }
            catch (Exception)
            {
                _viewModel.ClearReloadPhases();
                return;
            }

            // Belt-and-suspenders bounds check. The action-type gate above is the
            // primary protection, but keep this for any residual edge case where
            // ReloadPhase is at the post-final index during a phase boundary.
            if (reloadPhase < 0 || reloadPhase >= reloadPhaseCount)
            {
                _viewModel.ClearReloadPhases();
                return;
            }

            StackArray.StackArray10FloatFloatTuple reloadPhases = new StackArray.StackArray10FloatFloatTuple();
            ActionIndexCache reloadActionCode = MBItem.GetItemUsageReloadActionCode(
                wieldedWeapon.CurrentUsageItem.ItemUsage,
                9,
                mainAgent.HasMount,
                -1,
                mainAgent.GetIsLeftStance(),
                mainAgent.IsLookDirectionLow
            );

            FillReloadDurationsFromActions(ref reloadPhases, reloadPhaseCount, mainAgent, reloadActionCode);

            // Mark every phase before the current one as fully complete.
            for (int i = 0; i < reloadPhase; i++)
                reloadPhases[i] = (1f, reloadPhases[i].Item2);

            // Progress within the current phase, matching native behavior:
            //   1. Add blend-out compensation to currentActionProgress so the last
            //      sliver of each phase actually reaches 100% on screen.
            //   2. If the phase is already past its animation length (phaseComplete),
            //      DO NOT update the view this frame. Let the prior state stand until
            //      reloadPhase advances. This is what the native code does, and writing
            //      a clamped 1f here is what produces the brief jitter you'd otherwise see.
            ActionIndexCache currentActionValue = mainAgent.GetCurrentAction(1);
            float currentActionProgress = mainAgent.GetCurrentActionProgress(1);

            if (currentActionValue != ActionIndexCache.act_none)
            {
                float blendOut =
                    1f - MBActionSet.GetActionBlendOutStartProgress(mainAgent.ActionSet, currentActionValue);
                currentActionProgress += blendOut;
            }

            float animationParameter2 = MBAnimation.GetAnimationParameter2(
                mainAgent.AgentVisuals.GetSkeleton().GetAnimationAtChannel(1));

            bool phaseComplete = currentActionProgress > animationParameter2;

            if (!phaseComplete && animationParameter2 > 0.00001f)
            {
                float phaseProgress = currentActionProgress / animationParameter2;
                reloadPhases[reloadPhase] = (phaseProgress, reloadPhases[reloadPhase].Item2);
                _viewModel.SetReloadProperties(in reloadPhases, reloadPhaseCount);
            }
            // else: phase complete or animation duration was zero — intentionally do
            // not publish to the view this frame. Update() runs every tick, so the next
            // frame either advances reloadPhase or trips the action-type gate above.
        }

    private void UpdateMeleeDirections()
        {
            for (int i = 0; i < _targetGadgetOpacities.Length; i++)
                _targetGadgetOpacities[i] = 0.0;

            if (Mission.Current == null || Mission.Current.MainAgent == null)
            {
                _viewModel.SetArrowOpacities(0, 0, 0, 0);
                return;
            }

            Agent mainAgent = Mission.Current.MainAgent;
            WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand);

            bool isTargetInvalid = false;

            if (wieldedWeaponInfo.IsValid && wieldedWeaponInfo.IsRangedWeapon)
            {
                Agent.ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
                if (currentActionType == Agent.ActionCodeType.ReadyRanged)
                {
                    Vec2 rotationConstraint = mainAgent.GetBodyRotationConstraint();
                    float angle = MBMath.WrapAngle(mainAgent.LookDirection.AsVec2.RotationInRadians -
                                                   mainAgent.GetMovementDirection().RotationInRadians);

                    isTargetInvalid = Mission.Current.MainAgent.MountAgent != null &&
                                     !MBMath.IsBetween(angle, rotationConstraint.x, rotationConstraint.y) &&
                                     (rotationConstraint.x < -0.1f || rotationConstraint.y > 0.1f);
                }
            }
            else if (!wieldedWeaponInfo.IsValid || wieldedWeaponInfo.IsMeleeWeapon)
            {
                Agent.ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
                Agent.UsageDirection currentActionDirection = mainAgent.GetCurrentActionDirection(1);

                if (BannerlordConfig.DisplayAttackDirection &&
                    (currentActionType == Agent.ActionCodeType.ReadyMelee || MBMath.IsBetween((int)currentActionType, 1, 15)))
                {
                    if (currentActionType == Agent.ActionCodeType.ReadyMelee)
                    {
                        switch (mainAgent.AttackDirection)
                        {
                            case Agent.UsageDirection.AttackUp: _targetGadgetOpacities[0] = 0.7; break;
                            case Agent.UsageDirection.AttackDown: _targetGadgetOpacities[2] = 0.7; break;
                            case Agent.UsageDirection.AttackLeft: _targetGadgetOpacities[3] = 0.7; break;
                            case Agent.UsageDirection.AttackRight: _targetGadgetOpacities[1] = 0.7; break;
                        }
                    }
                    else
                    {
                        isTargetInvalid = true;
                        switch (currentActionDirection)
                        {
                            case Agent.UsageDirection.AttackEnd: _targetGadgetOpacities[0] = 0.7; break;
                            case Agent.UsageDirection.DefendDown: _targetGadgetOpacities[2] = 0.7; break;
                            case Agent.UsageDirection.DefendLeft: _targetGadgetOpacities[3] = 0.7; break;
                            case Agent.UsageDirection.DefendRight: _targetGadgetOpacities[1] = 0.7; break;
                        }
                    }
                }
                else if (BannerlordConfig.DisplayAttackDirection)
                {
                    switch (mainAgent.PlayerAttackDirection())
                    {
                        case Agent.UsageDirection.AttackUp: _targetGadgetOpacities[0] = 0.7; break;
                        case Agent.UsageDirection.AttackDown: _targetGadgetOpacities[2] = 0.7; break;
                        case Agent.UsageDirection.AttackLeft: _targetGadgetOpacities[3] = 0.7; break;
                        case Agent.UsageDirection.AttackRight: _targetGadgetOpacities[1] = 0.7; break;
                    }
                }
            }

            _viewModel.SetArrowOpacities(_targetGadgetOpacities[0], _targetGadgetOpacities[1],
                                         _targetGadgetOpacities[2], _targetGadgetOpacities[3]);
            _viewModel.IsTargetInvalid = isTargetInvalid;
        }

        private void FillReloadDurationsFromActions(
            ref StackArray.StackArray10FloatFloatTuple reloadPhases,
            int reloadPhaseCount,
            Agent mainAgent,
            ActionIndexCache reloadAction)
        {
            float maxDuration = 0f;

            for (int i = 0; i < reloadPhaseCount; i++)
            {
                if (reloadAction != ActionIndexCache.act_none)
                {
                    float duration = MBAnimation.GetAnimationParameter2(
                        MBActionSet.GetAnimationIndexOfAction(mainAgent.ActionSet, reloadAction)) *
                        MBActionSet.GetActionAnimationDuration(mainAgent.ActionSet, reloadAction);

                    reloadPhases[i] = (reloadPhases[i].Item1, duration);

                    if (duration > maxDuration)
                        maxDuration = duration;

                    reloadAction = MBActionSet.GetActionAnimationContinueToAction(mainAgent.ActionSet, reloadAction);
                }
            }

            if (maxDuration <= 0.00001f)
                return;

            for (int i = 0; i < reloadPhaseCount; i++)
                reloadPhases[i] = (reloadPhases[i].Item1, reloadPhases[i].Item2 / maxDuration);
        }

        private void OnCombatLogGenerated(CombatLogData logData)
        {
            if (_isCleanedUp || _viewModel == null || !_viewModel.CustomEnabled) return;

            bool isAttackerMine = logData.IsAttackerAgentMine;
            bool isValidHit = !logData.IsVictimAgentSameAsAttackerAgent && !logData.IsFriendlyFire;
            bool isHeadshot = logData.IsAttackerAgentHuman && logData.BodyPartHit == BoneBodyPartType.Head;

            if (isAttackerMine && isValidHit && logData.TotalDamage > 0)
                _viewModel.ShowHitMarker(logData.IsFatalDamage, isHeadshot);
        }

        public void Cleanup(MissionScreen screen)
        {
            _isCleanedUp = true;
            CombatLogManager.OnGenerateCombatLog -= OnCombatLogGenerated;

            try
            {
                if (_nativeLayer != null && _nativeLayer.UIContext != null)
                    _nativeLayer.UIContext.ContextAlpha = 1f;
            }
            catch { }

            try
            {
                if (screen != null && _customLayer != null)
                    screen.RemoveLayer(_customLayer);
            }
            catch { }

            _customLayer = null;
            _nativeLayer = null;
            _viewModel = null;
        }
    }
}