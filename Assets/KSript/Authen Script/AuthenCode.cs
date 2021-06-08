using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEditor;

public class AuthenCode : MonoBehaviour
{
    [SerializeField] private GameObject email;
    [SerializeField] private GameObject pw;
    
    public static AuthenCode _ins;
    private static string sessionTicket;
    
    // Start is called before the first frame update
    void Start()
    {
        AutoLogin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AutoLogin()
    {
        if (PlayerPrefs.HasKey("EMAIL"))
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
            CustomId = cusid, CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(req, res =>{H.klog("Success logging in with custom id");}, FailedLoginGeneral);
    }

    public void GuestLogin()
    {
        //LoginCustomId($"Guest");
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
    }

    private void MobileLoginFailed(PlayFabError err)
    {
        H.klog2($"The device failed to login {err.GenerateErrorReport()}", "Authen code", "#dc143c");
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
        sessionTicket = res.SessionTicket;
        PlayerPrefs.SetString("EMAIL", email.GetComponent<TMP_InputField>().text);
        PlayerPrefs.SetString("PWD", pw.GetComponent<TMP_InputField>().text);
    }

    private void FailedLoginGeneral(PlayFabError err)
    {
        H.klog2($"Failed to login -- {err.ErrorMessage}", "AthenCode SCript", "#dc143c");
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
            PlayerPrefs.SetString("EMAIL", email.GetComponent<TMP_InputField>().text);
            PlayerPrefs.SetString("PWD", pw.GetComponent<TMP_InputField>().text);
        }, error =>
        {
            H.klog2($"Failed to Create User-- {error.GenerateErrorReport()} ", "Authen Code", "#dc143c");
        });
    }
    
}
