using UnityEngine;

public class VibrateManager : MonoBehaviour
{
    public static VibrateManager Instance { get; private set; }
    public bool isVibrationEnabled = true;

    private const string VibrationPrefsKey = "VibrationEnabled";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Load vibration setting; default enabled if no preference exists.
            isVibrationEnabled = PlayerPrefs.GetInt(VibrationPrefsKey, 1) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleVibration()
    {
        isVibrationEnabled = !isVibrationEnabled;
        PlayerPrefs.SetInt(VibrationPrefsKey, isVibrationEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void TriggerWeakVibration()
    {
        if (!isVibrationEnabled)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int sdkInt = versionClass.GetStatic<int>("SDK_INT");
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        if (vibrator.Call<bool>("hasVibrator"))
        {
            if (sdkInt >= 26)
            {
                AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                // Pass duration as a long and amplitude as int
                AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", 50L, 50);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                // Fallback for devices with API level lower than 26
                vibrator.Call("vibrate", 50);
            }
        }
#else
        Handheld.Vibrate();
#endif
    }

    public void TriggerStrongVibration()
    {
        if (!isVibrationEnabled)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int sdkInt = versionClass.GetStatic<int>("SDK_INT");
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        if (vibrator.Call<bool>("hasVibrator"))
        {
            if (sdkInt >= 26)
            {
                AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                // Pass duration as a long and amplitude as int (200 ms strong vibration)
                AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", 200L, 255);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                // Fallback for devices lower than API 26
                vibrator.Call("vibrate", 200);
            }
        }
#else
        Handheld.Vibrate();
#endif
    }
}