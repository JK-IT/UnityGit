using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class IntroMan : MonoBehaviour
{
    public static IntroMan _ins;

    [SerializeField] private GameObject ninputpanel = null;
    [SerializeField] private GameObject ninputfield = null;

    [SerializeField] private GameObject popuploading = null;
    //TODO: DELETE SERIALIZEFIELD
    [FormerlySerializedAs("pdat")] [SerializeField] private PlayerDaOb pdatscript;

    private string datfile = "pdat.dat";
    private string datpath;


    //--------------

    public void GooLog()
    {
    }

    public void GuestLog()
    {
        if(pdatscript.GetName() == "")
            ninputfield.GetComponent<TMP_InputField>().text = H.GenNames();
        ninputpanel.SetActive(true);
    }

    public void UIStartLogin()
    {
        H.PlayerData pdo = new H.PlayerData();
        pdo.playername = ninputfield.GetComponent<TMP_InputField>().text;
        pdatscript.SetName(pdo.playername);
        H.SavepDat(datpath, pdatscript);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
    }
    private void Awake()
    {
        if (!pdatscript)
            pdatscript = PlayerDaOb.GetIns;
        popuploading.SetActive(false);
    }

    //---------------
    // Start is called before the first frame update
    void Start()
    {
        _ins = this;
        // open the saved data or create a saved data
        datpath = System.IO.Path.Combine(Application.dataPath, datfile);
        popuploading.SetActive(true);
        
        string retobs = "";
        Thread t = new Thread(() =>
        {
            if (File.Exists(datpath))
            {
                retobs = File.ReadAllText(datpath, Encoding.UTF8);
                H.klog($"This is what i read from dat file --- {retobs}");
            }
        });
        t.Start();
        // DOING OTHER THINGS IF NECESSaRY, then wait here
        t.Join();
        if(retobs != "")
        {
            H.klog(retobs);
            H.PlayerData pdobject = JsonUtility.FromJson<H.PlayerData>(retobs);
            H.klog($"player name after convert from json {pdobject.playername}");
            pdatscript.RestoreData(pdobject);
            ninputfield.GetComponent<TMP_InputField>().text = pdatscript.GetName();
        }
        
        
        popuploading.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
