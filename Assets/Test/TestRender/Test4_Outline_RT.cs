using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test4_Outline_RT : MonoBehaviour
{
   private bool shoRT = false;

   private void OnGUI()
   {
      if(GUILayout.Button("Debug Mask",GUILayout.Width(150),GUILayout.Height(50)))
         shoRT = !shoRT;
      if (!shoRT)
         return;
      
      GUILayout.Label(Test4_Outline_Feature.outlineMaskMap,GUILayout.Width(320));
   }
}
