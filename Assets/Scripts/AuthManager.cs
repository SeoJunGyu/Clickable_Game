using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Unity.VisualScripting;
using UnityEngine;

//로그인 
public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;
    public static AuthManager Instance => instance;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private bool isInitialized = false;

    public FirebaseUser CurrentUser => currentUser;

    public bool IsLoggedIn => currentUser != null; // 널이 아니면 로그인 되어있다고 판단

    public string UserId => currentUser?.UserId ?? string.Empty;
    public bool IsInitialized => isInitialized;

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
        }
    }

    private async UniTaskVoid Start()
    {
        await FirebaseInitializer.Instance.WaitForInitializationAsync(); //파이어 베이스 초기화

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;

        currentUser = auth.CurrentUser; //로그인 이력 확인용 현재 유저 가져오기

        if(currentUser != null)
        {
            Debug.Log($"[Auth] 이미 로그인됨: {UserId}"); //테스트 용도로 찍는것이기에 절대 유출되면 안된다.
        }
        else
        {
            Debug.Log($"[Auth] 로그인 필요");

        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if(auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }

    //System.EventArgs : C# 표준 이벤트 클래스
    private void OnAuthStateChanged(object sender, System.EventArgs eventArgrs)
    {
        if(auth.CurrentUser != currentUser)
        {
            bool signedIn = auth.CurrentUser != currentUser && auth.CurrentUser != null;
            if (!signedIn && currentUser != null)
            {
                Debug.Log("[Auth] 로그 아웃 됨");
            }

            currentUser = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("[Auth] 로그 인 됨");
            }
        }
    }

    //익명 로그인 실행 함수
    //성공 여부, 오류 내용
    public async UniTask<(bool success, string error)> SingInAnonymouslyAsync()
    {
        try
        {
            Debug.Log("[Auth] 익명 로그인 시도...");

            AuthResult result = await auth.SignInAnonymouslyAsync().AsUniTask();
            currentUser = result.User;

            Debug.Log($"[Auth] 익명 로그인 성공: {UserId}");

            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Auth] 익명 로그인 실패: {ex.Message}");
            return (false, ex.Message);
        }

        return (true, "");
    }

    public async UniTask<(bool success, string error)> CreateUserWithEmailAsync(string email, string passwd)
    {
        try
        {
            Debug.Log("[Auth] 회원 가입 시도...");

            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, passwd).AsUniTask();
            currentUser = result.User;

            Debug.Log($"[Auth] 회원 가입 성공: {UserId}");

            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Auth] 회원 가입 실패: {ex.Message}");
            return (false, ex.Message);
        }

        return (true, "");
    }

    public async UniTask<(bool success, string error)> SighInWithEmailAsync(string email, string passwd)
    {
        try
        {
            Debug.Log("[Auth] 로그인 시도...");

            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, passwd).AsUniTask();
            currentUser = result.User;

            Debug.Log($"[Auth] 로그인 성공: {UserId}");

            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Auth] 로그인 실패: {ex.Message}");
            return (false, ex.Message);
        }

        return (true, "");
    }

    //로그아웃 실행
    public void SignOut()
    {
        if(auth != null && currentUser != null)
        {
            Debug.Log("[Auth] 로그아웃 시도");
            auth.SignOut();
            currentUser = null;
            Debug.Log("[Auth] 로그아웃");
        }
    }

    //컴퓨터 언어 오류는 인간이 읽기 힘들기에 사용자가 읽기 편하도록 만들 함수
    private string ParseFirebaseError(string error)
    {
        return error;
    }
}
