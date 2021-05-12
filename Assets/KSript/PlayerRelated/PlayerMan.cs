using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMan : MonoBehaviour
{
    public static PlayerMan _ins;
    
    //TODO: DELETE SERIALIZEFIELD LATER
    [SerializeField] public PlayerDaOb playerdat;
    
    private void Awake()
    {
        if (playerdat == null)
        {
            playerdat = PlayerDaOb.GetIns;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _ins = this;
        //H.klog($"{playerdat.GetName()}");
        playerdat.PrintYrself();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region  ========== CHARACTER CUSTOMIZATION

    public void PickColor(GameObject ingo)
    {
        Color c = ingo.GetComponent<Image>().color;
        H.klog(ColorUtility.ToHtmlStringRGBA(c));
        playerdat.SetColor(c);
        H.SavepDat(System.IO.Path.Combine(Application.dataPath, "pdat.dat"), playerdat);
    }


    #endregion

}
