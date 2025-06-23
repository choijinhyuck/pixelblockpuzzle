using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class BannerAdExample : MonoBehaviour
{
    [SerializeField] Button _loadBannerButton;
    [SerializeField] Button _showBannerButton;
    [SerializeField] Button _hideBannerButton;

    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    [SerializeField] string _androidAdUnitId = "Banner_Android";
    [SerializeField] string _iOSAdUnitId = "Banner_iOS";
    string _adUnitId = null; // Remains null on unsupported platforms
    private bool isBannerLoaded = false;

    void Start()
    {
        // Set ad unit ID based on the platform
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif
        // Set banner position
        Advertisement.Banner.SetPosition(_bannerPosition);

        // Subscribe to the Ads initialization complete event
        AdsInitializer.OnAdsInitializationComplete += OnAdsInitialized;
    }

    // Called when Ads initialization is complete
    void OnAdsInitialized()
    {
        Debug.Log("AdsInitializer complete - now loading banner.");
        LoadBanner();
    }

    // Called when the load banner button is pressed (for testing)
    public void LoadBanner()
    {
        Debug.Log("Starting banner load: " + _adUnitId);
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);
    }

    // Called when the banner loads successfully
    void OnBannerLoaded()
    {
        Debug.Log("Banner loaded successfully");
        isBannerLoaded = true;
        ShowBannerAd();
    }

    // Called when the banner load fails
    void OnBannerError(string message)
    {
        Debug.Log("Banner Error: " + message);
        // Additional error handling can be added here.
    }

    // Display the banner ad
    void ShowBannerAd()
    {
        Debug.Log("Banner Ad show request");
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    // Hide the banner ad
    void HideBannerAd()
    {
        Debug.Log("Banner Ad hide request");
        Advertisement.Banner.Hide();
    }

    void OnBannerClicked()
    {
        Debug.Log("Banner clicked");
    }

    void OnBannerShown()
    {
        Debug.Log("Banner shown");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner hidden");
    }

    void OnDestroy()
    {
        // Unsubscribe from the initialization complete event
        AdsInitializer.OnAdsInitializationComplete -= OnAdsInitialized;
        
        // Remove button listeners if assigned
        _loadBannerButton.onClick.RemoveAllListeners();
        _showBannerButton.onClick.RemoveAllListeners();
        _hideBannerButton.onClick.RemoveAllListeners();
    }
}