﻿using UnityEngine;
using Firebase;
using Cysharp.Threading.Tasks;

//파이어 베이스 초기화 클래스 -> 파이어 베이스 최상위 클래스가 될 것이다.
public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer instance;
    public static FirebaseInitializer Instance => instance;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private FirebaseApp firebaseApp;
    public FirebaseApp FirebaseApp => firebaseApp;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFirebaseAsync().Forget();
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    private async UniTaskVoid InitializeFirebaseAsync()
    {
        Debug.Log("[Firebase] 초기화 시작");

        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            if (status == DependencyStatus.Available)
            {
                firebaseApp = FirebaseApp.DefaultInstance;
                isInitialized = true;

                Debug.Log($"[Firebase] 초기화 성공 {firebaseApp.Name}");
            }
            else
            {
                Debug.Log($"[Firebase] 초기화 오류: {status}");
                isInitialized = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Firebase] 초기화 오류: {ex.Message}");
            isInitialized = false;
        }
    }

    public async UniTask WaitForInitializationAsync()
    {
        await UniTask.WaitUntil(() => isInitialized);
    }
}
