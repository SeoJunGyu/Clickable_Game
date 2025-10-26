using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateProfileUI : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public Button createBtn;
    public GameObject profilePanel;

    private void Start()
    {
        createBtn.onClick.AddListener(() => OnCreateButtonClicked().Forget());
    }

    private void OnDestroy()
    {
        createBtn.onClick.RemoveAllListeners();
    }

    private async UniTaskVoid OnCreateButtonClicked()
    {
        string nickname = nicknameInput.text;

        createBtn.interactable = false;
        nicknameInput.interactable = false;

        try
        {
            var (success, error) = await ProfileManager.Instance.SaveProfileAsync(nickname);

            if (success)
            {
                Debug.Log("프로필 생성 성공!");

                profilePanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
        catch(System.Exception ex)
        {

        }
    }
}
