using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
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

    public static void klog2(string ins, string name, string col = "teal")
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
            ids += (char)r.Next(48, 64);
        }

        ids += (char) r.Next(97, 122);
        return ids;
    }

    public static string ToGuid(string instr)
    {
        MD5 md5 = MD5.Create();
        byte[] feed = md5.ComputeHash(Encoding.Default.GetBytes(instr));
        Guid guid = new Guid(feed);
        return guid.ToString();
    }
}
