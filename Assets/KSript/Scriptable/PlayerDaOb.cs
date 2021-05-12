using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


[CreateAssetMenu(menuName = "Player Data Object", fileName = "playerData")]
public class PlayerDaOb : Kcripton<PlayerDaOb>
{
    [SerializeField] private string pname = "";
    [SerializeField] private string state = "Not Ready";
    [SerializeField] private int ready = 0;
    [SerializeField] private Color pickedcolor;
    [SerializeField] private string colorhexstr;
    
    
    public string GetName()
    {
        return pname;
    }

    public void SetName(string iname)
    {
        this.pname = iname;
    }
    public void PrintYrself()
    {
        Thread trh = new Thread(new ThreadStart( ()=> {H.klog($"This is fuking thread {pname}");}));
        trh.IsBackground = true;
        trh.Start();
    }

    public void SetColor(Color inc)
    {
        pickedcolor = inc;
        colorhexstr = ColorUtility.ToHtmlStringRGBA(inc);
    }

    public Color GetColorObj()
    {
        Color c;
        if (ColorUtility.TryParseHtmlString(colorhexstr, out c))
            return c;
        else
            return Color.black;
    }

    public string GetColor()
    {
        return colorhexstr;
    }

    public void RestoreData(H.PlayerData inpd)
    {
        this.pname = inpd.playername;
        this.colorhexstr = inpd.hexcol;
        ColorUtility.TryParseHtmlString(inpd.hexcol, out this.pickedcolor);
    }
    
}
