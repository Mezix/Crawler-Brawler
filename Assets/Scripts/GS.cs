using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GS // Stands for "Get Strings"
{
    public static string Prefabs(string filename = "")
    {
        return "Prefabs/" + filename;
    }

    //  Graphics

    public static string Props(string filename = "")
    {
        return Graphics() + "Props/" + filename;
    }
    public static string Graphics(string filename = "")
    {
        return "Graphics/" + filename;
    }

    //  Dialogue
    public static string Dialogue(string filename = "")
    {
        return "Dialogue/" + filename;
    }
    public static string Conversations(string filename = "")
    {
        return Dialogue() + "Conversations/" + filename;
    }
}
