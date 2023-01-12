using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public GameManager gameManager;
    private string _gameId = "5115689";
    public string _adUnitId = "Rewarded_Android";

    void Start()
    {
        Advertisement.Initialize(_gameId);
        LoadAd();
    }
    public void LoadAd() {
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }
    public void OnUnityAdsAdLoaded(string adUnitId){
        Debug.Log("Ad Loaded: " + adUnitId);
    }
    public void ShowAd() {
        Advertisement.Show(_adUnitId, this);
    }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED)) {
            Debug.Log("성공");
            Advertisement.Load(_adUnitId, this);
            gameManager.Restart(true);
        }
    }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) {
        Debug.Log($"로딩실패 {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
    }
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) {
        Debug.Log($"보기실패 {adUnitId}: {error.ToString()} - {message}");
        ShowAd();
    }
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
