// Caminho: Assets/Lan/Code/Client/UI/LoginScreenManager.cs
using UnityEngine;

public class LoginScreenManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject panelConnect;
    public GameObject panelCreate;

    void Awake()
    {
        ShowConnect();
    }

    public void ShowConnect()
    {
        if (panelConnect) panelConnect.SetActive(true);
        if (panelCreate)  panelCreate.SetActive(false);
    }

    public void ShowCreate()
    {
        if (panelConnect) panelConnect.SetActive(false);
        if (panelCreate)  panelCreate.SetActive(true);
    }
}
