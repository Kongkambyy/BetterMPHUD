using System.Reflection;
using TaleWorlds.MountAndBlade.View.Screens;

namespace BetterMPHUD.Handlers
{
    public class CameraSnapbackHandler
    {
        private static readonly BindingFlags Flags = 
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static readonly string[] FieldNames = new string[]
        {
            "_cameraBearingDelta", 
            "_cameraElevationDelta",
            "_cameraSpecialTargetAddedBearing", 
            "_cameraSpecialCurrentAddedBearing",
            "_cameraSpecialTargetAddedElevation", 
            "_cameraSpecialCurrentAddedElevation"
        };

        private FieldInfo[] _fields;
        private bool _cached;

        public void OnLookAroundReleased(MissionScreen screen, bool enabled)
        {
            if (!enabled || screen == null) return;
            
            CacheFields(screen);
            ResetCameraDeltas(screen);
        }

        private void CacheFields(MissionScreen screen)
        {
            if (_cached) return;
            
            System.Type type = screen.GetType();
            _fields = new FieldInfo[FieldNames.Length];
            
            for (int i = 0; i < FieldNames.Length; i++)
                _fields[i] = type.GetField(FieldNames[i], Flags);
            
            _cached = true;
        }

        private void ResetCameraDeltas(MissionScreen screen)
        {
            try
            {
                for (int i = 0; i < _fields.Length; i++)
                {
                    if (_fields[i] != null)
                        _fields[i].SetValue(screen, 0f);
                }
            }
            catch { }
        }

        public void Reset() 
        { 
            _cached = false; 
        }
    }
}