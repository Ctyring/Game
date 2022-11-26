using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace GlobalData
{
    public class GlobalSettings
    {
        public static string Theme = "Light";
        //状态列表
        public enum States
        {
            Normal, Error, Loading
        }

        public static States state = States.Normal;

        public static int CoinNum = 0;
    }
}
