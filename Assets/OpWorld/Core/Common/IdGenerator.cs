using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class IdGenerator
{
    private static ulong value = 0;
 
    
    static public ulong GenerateId()
    {
        TimeSpan s = DateTime.Now - new DateTime(2000, 1, 1);
        ulong id = (ulong)s.TotalMilliseconds % 0xFFFFFFFFFF;
        ++value;

        if (value >= 0xFFFFFF)
            value = 0;
        
        id = (id << 40) | (value );
        return id;
    }
}