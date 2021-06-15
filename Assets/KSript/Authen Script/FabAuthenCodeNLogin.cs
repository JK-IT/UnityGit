using System;
using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using TMPro;
using UnityEditor;

public class FabAuthenCodeNLogin : MonoBehaviour
{
    [SerializeField] private GameObject email;
    [SerializeField] private GameObject pw;
    
    public static FabAuthenCodeNLogin _ins;
    public static bool authenNverified = false; // bool to keep track if this cli is authenticated and login 
    public KnetMan knet;
    
    [Header("PlayFab Info")]
    [SerializeField]
    public string buildid;
    public string fabip;
    public ushort fabport;

    [Header("Client Identity from Playfab")]
    public string playerfabid;
    private string sessionTicket;
    
    // store successful login information
    private MyIden _iden = new MyIden();


    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isBatchMode)
        {
            //AutoLogin();
            //H.klog($"playfab id {PlayFabSettings.BuildIdentifier}");
            _ins = this;
            DontDestroyOnLoad(this);
            buildid = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AutoLogin()
    {
        if (PlayerPrefs.HasKey("EMAIL") && PlayerPrefs.GetString("EMAIL") != "")
        {
            string mail = PlayerPrefs.GetString("EMAIL");
            H.klog($"Email save on this machine {mail}");
            string pwd = PlayerPrefs.GetString("PWD");
            H.klog($"Password save o this machinee {pwd}");
            var req = new LoginWithEmailAddressRequest
            {
                Email = mail, Password = pwd
            };
            PlayFabClientAPI.LoginWithEmailAddress(req, SuccessLoginEmail, FailedPlayfabUserNotExist);
        }
    }
    private void LoginCustomId(string cusid)
    {
        var req = new LoginWithCustomIDRequest
        {
            CustomId = Guid.NewGuid().ToString(), CreateAccount = true,
            //TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.LoginWithCustomID(req, res =>
        {
            H.klog("Success logging in with custom id");
            _iden.fabid = res.PlayFabId;
            _iden.sessTicket = res.SessionTicket;
            playerfabid = res.PlayFabId;
            sessionTicket = res.SessionTicket;
            authenNverified = true;
            RequestFabServerInfo();
        }, FailedLoginGeneral);
    }

    private void RequestFabServerInfo()
    {
        H.klog1($"Requesting Fab server info", this.name);
        RequestMultiplayerServerRequest req = new RequestMultiplayerServerRequest();
        req.BuildId = buildid;
        req.SessionId = System.Guid.NewGuid().ToString();
        req.PreferredRegions = new List<string>() { "EastUs" };
        PlayFabMultiplayerAPI.RequestMultiplayerServer(req, ResRequestMultiplayer, ErrRequestMultiplayer);
    }

    private void ResRequestMultiplayer(RequestMultiplayerServerResponse res)
    {
        H.klog($" response from fab server {res.IPV4Address} : {(ushort)res.Ports[0].Num}");
        fabip = res.IPV4Address;
        fabport = (ushort) res.Ports[0].Num;
        knet.ConnectToFab(fabip, fabport); // connect to server on fab server
    }

    private void ErrRequestMultiplayer(PlayFabError err)
    {
        H.klog($"Request Multiplayer Error {err.ErrorMessage}");
    }
    public void GuestLogin()
    {
        //todo: auto try to connect to local containers --> Delete this later
        TestQuickLocalLogin();
        //LoginCustomId($"Guest {System.Guid.NewGuid().ToString()}");
        // we will use device id on android and IOS to log in instead
#if UNITY_ANDROID
        var reqAnd = new LoginWithAndroidDeviceIDRequest{AndroidDeviceId = GetMobileDeviceID(), CreateAccount = true};
        PlayFabClientAPI.LoginWithAndroidDeviceID(reqAnd, MobileLoginSuccess, MobileLoginFailed);
#endif

#if UNITY_IOS
        var reqIos = new LoginWithIOSDeviceIDRequest{DeviceId = GetMobileDeviceID(), CreateAccount = true};
        PlayFabClientAPI.LoginWithIOSDeviceID(reqIos, MobileLoginSuccess, MobileLoginFailed);
#endif

    }

    private string GetMobileDeviceID()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }

    private void MobileLoginSuccess(LoginResult res)
    {
        H.klog($"Mobile log in Successes");
        _iden.fabid = res.PlayFabId;
        _iden.sessTicket = res.SessionTicket;
        playerfabid = res.PlayFabId;
        sessionTicket = res.SessionTicket;
        authenNverified = true;
    }

    private void MobileLoginFailed(PlayFabError err)
    {
        H.klog2($"The device failed to login {err.GenerateErrorReport()}", "Authen code", "#dc143c");
        authenNverified = false;
    }
    public void EmailLogin()
    {
        var req = new LoginWithEmailAddressRequest();
        req.Email = email.GetComponent<TMP_InputField>().text; 
        req.Password = pw.GetComponent<TMP_InputField>().text;
        PlayFabClientAPI.LoginWithEmailAddress(req, SuccessLoginEmail, FailedPlayfabUserNotExist);
    }

    private void SuccessLoginEmail(LoginResult res)
    {
        H.klog($"Successful login");
        _iden.fabid = res.PlayFabId;
        _iden.sessTicket = res.SessionTicket;
        playerfabid = res.PlayFabId;
        sessionTicket = res.SessionTicket;
        authenNverified = true;
        if (  email.GetComponent<TMP_InputField>().text != "")
        {
            PlayerPrefs.SetString("EMAIL", email.GetComponent<TMP_InputField>().text); 
            PlayerPrefs.SetString("PWD", pw.GetComponent<TMP_InputField>().text);
        }
    }

    private void FailedLoginGeneral(PlayFabError err)
    {
        H.klog2($"Failed to login -- {err.ErrorMessage}", "AthenCode SCript", "#dc143c");
        authenNverified = false;
    }

    private void FailedPlayfabUserNotExist(PlayFabError err)
    {
        var registerReq = new RegisterPlayFabUserRequest
        {
            Email = email.GetComponent<TMP_InputField>().text, Password = pw.GetComponent<TMP_InputField>().text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerReq, result =>
        {
            H.klog($"Successful register Player");
            authenNverified = true;
            PlayerPrefs.SetString("EMAIL", email.GetComponent<TMP_InputField>().text);
            PlayerPrefs.SetString("PWD", pw.GetComponent<TMP_InputField>().text);
        }, error =>
        {
            H.klog2($"Failed to Create User-- {error.GenerateErrorReport()} ", "Authen Code", "#dc143c");
            authenNverified = false;
        });
    }

    private void TestQuickLocalLogin()
    {
        NetworkManager kn = GameObject.Find("KnetMan").GetComponent<KnetMan>();
        kn.networkAddress = "127.0.0.1";
        kn.GetComponent<TelepathyTransport>().port = 56100;
        kn.GetComponent<KcpTransport>().Port = 56100;
        H.klog($"Testing local docker container ");
        kn.StartClient();
    }

    public void SetCliConnidToIden(int inid)
    {
        _iden.myConnId = inid;
    }

    public string GetCliFabid()
    {
        if (_iden.fabid != null)
        {
            return _iden.fabid;
        }
        else return null;
    }
    
    public class MyIden
    {
        public string fabid;
        public int myConnId;
        public string sessTicket;

        public MyIden() { }

        public void Clear()
        {
            fabid = "";
            myConnId = -1;
            sessTicket = "";
        }
    }
    
    
}
