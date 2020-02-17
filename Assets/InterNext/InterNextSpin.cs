using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterNextSpin : MonoBehaviour
{
    public int Axis;
    public float Speed;

    void Update()
    {
        if (!InterNextCore.PowerDevice)
            return;
        if (Axis == 0)
            transform.Rotate(Speed, 0, 0, Space.World);
        if (Axis == 1)
            transform.Rotate(0, Speed, 0, Space.World);
        if (Axis == 2)
            transform.Rotate(0, 0, Speed, Space.World);
    }
}
