using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BaseStatsForZolaBoss
{
    public static int zolaMaxHP = 1500;

    public static int spatialEruptionDamage = 40;
    public static int voidRift = 4;
    public static int aethricStirkeDamage = 30;
    public static int dimensionalWaveDamage = 35;
    public static int temporalSurgeDamage = 20;

    public static int[] chronoDamages = { spatialEruptionDamage, voidRift, aethricStirkeDamage, dimensionalWaveDamage, temporalSurgeDamage };
    public static void BuffDamages(int buff)
    {
        for (int i = 0; i < chronoDamages.Length; i++)
        {
            chronoDamages[i] += buff;
        }
    }
}
