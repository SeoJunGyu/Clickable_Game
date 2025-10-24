using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEditor.Analytics;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    private static ProfileManager instance;
    public static ProfileManager Instance => instance;

    private DatabaseReference databaseRef;
    private DatabaseReference usersRef;

    private UserProfile cachedProfile; //메모리에 올라갈 프로파일

    public UserProfile CachedProfile => cachedProfile;

    private void Awake()
    {
        {
            if(instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private async UniTaskVoid Start()
    {
        await FirebaseInitializer.Instance.WaitForInitializationAsync();

        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        usersRef = databaseRef.Child("users"); //하위에 입력되어있는 데이터가 리턴되는 함수다. / root : json 통째로, users : users라고 적혀있는 내부 데이터만

        Debug.Log("[Profile] ProfileManager 초기화 완료");
    }

    public async UniTask<(bool success, string error)> SaveProfileAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "[pROFILE] 로그인 x");
        }

        string userId = AuthManager.Instance.UserId;
        string email = AuthManager.Instance.CurrentUser.Email ?? "익명";

        try
        {
            Debug.Log($"[Profile] 프로필 저장 시도 {nickname}");

            UserProfile profile = new UserProfile(nickname, email);
            string json = profile.ToJson();
            await usersRef.Child(userId).SetRawJsonValueAsync(json).AsUniTask();

            cachedProfile = profile;

            Debug.Log($"[Profile] 프로필 성공 {nickname}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 프로필 실패{ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<(UserProfile profile, string error)> LoadProfileAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (null, "[pROFILE] 로그인 x");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[Profile] 프로필 로드 시도 {nickname}");

            DataSnapshot snapshot = await usersRef.Child(userId).GetValueAsync().AsUniTask(); //json 포맷으로 읽어오기
            if (!snapshot.Exists)
            {
                return (null, "[Profile] 프로필 없음");
            }

            string json = snapshot.GetRawJsonValue();
            cachedProfile = UserProfile.FromJson(json); //json 데이터를 원래대로 변환

            Debug.Log($"[Profile] 프로필 성공 {nickname}");
            return (cachedProfile, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 프로필 실패{ex.Message}");
            return (null, ex.Message);
        }
    }

    public async UniTask<(bool success, string error)> UpdateNicknameAsync(string newNickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인 x");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"[Profile] 닉네임 변경 시도 {newNickname}");

            //usersRef.Child(userId) : 프로필 루트
            await usersRef.Child(userId).Child("nickname").SetValueAsync(newNickname).AsUniTask();

            cachedProfile.nickname = newNickname;

            Debug.Log($"[Profile] 닉네임 변경 성공 {cachedProfile.nickname}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Profile] 닉네임 변경 실패{ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<bool> ProfileExistAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return false;
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot snapshot = await usersRef.Child(userId).GetValueAsync().AsUniTask();
            return snapshot.Exists; //스넵샷이 있다면 true 없으면 false
        }
        catch(System.Exception ex)
        {
            Debug.LogError($"[Profile] 프로필 확인 실패: {ex.Message}");
            return false;
        }
    }
}
