using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Kcripton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _ins;

    public static T GetIns
    {
        get
        {

            if (_ins == null)
            {
                H.klog($"==KRIPTON= Find the one inresource");
                _ins = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                H.klog($"==KRIPTON= Resources find ---- {_ins.name}");
                //_ins = ScriptableObject.CreateInstance<T>();
            }
            return _ins;
        }
    }
}