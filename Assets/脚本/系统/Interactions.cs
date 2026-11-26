using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    public bool PlayerCanInteract() 
    {
        if (!动作系统.Instance.IsPerforming)return true;
        else return false;
    }

    public bool PlayerCanHover()
    {
        if (PlayerIsDragging) return false;
        return true;
    }
}
