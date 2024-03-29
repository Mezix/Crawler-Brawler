using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class REF
{
    public static Loader.Scene CurrentScene;
    public static CameraScript MainCam;
    //public static ObjectPool ObjPool;

    /// <summary>
    ///  0 = host, 1 = Server, 2 = Client
    /// </summary>
    public static int HostMode;
    /// <summary>
    /// 0 = Tech Wizard; 1 = First Aid Fiddler; 2 = Immolating Imp; 3 = Portal Priest; 4 = Potion Peddler; 5 = Woodland Wanderer
    /// </summary>
    public static int CharIndex;
    public static PlayerController2D PCon;
}
