using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using kcp2k;
using Random = UnityEngine.Random;

public class H
{
    public static void klog(string ins)
    {
        Debug.Log($"<color=aliceblue> ------  {ins} -------  </color>  {DateTime.Now:T} : {DateTime.Now.Millisecond}");
    }

    public static void klog1(string ins, string name)
    {
        Debug.Log($"<color=aliceblue> ========= <b> {name} </b> ===>  {ins} =========  </color>  {DateTime.Now:T} : {DateTime.Now.Millisecond}");
    }

    public static void klog2(string ins, string name, string col = "#008080")
    {
        
        Debug.Log($"<color={col}> ###### <b> {name} </b> ===>  {ins} ###########  </color>  {DateTime.Now:T} : {DateTime.Now.Millisecond}");
    }

    public static string GenIds()
    {
        var r = new System.Random();
        string ids = "";
        for (int i = 0; i < 2; i++)
        {
            ids +=(char) r.Next(97, 122);
        }
        
        for (int i = 0; i < 2; i++)
        {
            ids += (char)r.Next(48, 58);
        }

        ids += (char) r.Next(97, 122);
        return ids;
    }

    public static Guid ToGuid(string instr)
    {
        MD5 md5 = MD5.Create();
        byte[] feed = md5.ComputeHash(Encoding.Default.GetBytes(instr));
        Guid guid = new Guid(feed);
        return guid;
    }

    public static string GenNames()
    {
        var r = new System.Random();
        string naame = "";
        for (int i = 0; i < 7; i++)
        {
            naame += r.Next(97, 122);
        }

        naame += r.Next(48, 57);
        return naame;
    }

    //** save player data object scriptable obj
    public static void SavepDat(string indpath, PlayerDaOb ddat)
    {

        Thread t = new Thread(() =>
        {
            PlayerData pd = PlayerData.StoreInfo(ddat);
            string jsobj = JsonUtility.ToJson(pd);
            File.WriteAllText(indpath, jsobj);
        });
        t.IsBackground = true;
        t.Start();
        
    }
    
    
    
    #region ====== PLAYER DATA CLASS

    public class PlayerData
    {
        public string playername;
        public string hexcol;
        public static PlayerData StoreInfo(PlayerDaOb inobj)
        {
            PlayerData pda = new PlayerData();
            pda.playername = inobj.GetName();
            pda.hexcol = inobj.GetColor();
            return pda;
        }
    }
    

    #endregion
}
