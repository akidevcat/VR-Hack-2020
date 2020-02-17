using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct INDevice
{
    public string name;
    public float pivotoffset_x;
    public float pivotoffset_y;
    public float pivotoffset_z;
    public float rotation_x;
    public float rotation_y;
    public float rotation_z;
    public float rotation_w;
    public float scale_x;
    public float scale_y;
    public float scale_z;
    public List<float> details_offsets;
    public List<int> rotation_axis;

    public Vector3 PivotOffset
    {
        get => new Vector3(pivotoffset_x, pivotoffset_y, pivotoffset_z);
    }

    public Quaternion Rotation
    {
        get => new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);
    }

    public Vector3 Scale
    {
        get => new Vector3(scale_x, scale_y, scale_z);
    }
}



public class InterNextCore : MonoBehaviour
{
    public string CurrentDevice = "AsyncEngine";
    private INDevice LoadedDevice;
    private GameObject InstancedDevice;
    private Transform InstancedDeviceT;
    private bool disassembled = false;

    private void Start()
    {
        LoadDevice(CurrentDevice);
    }

    void LoadDevice(string name)
    {
        string json = Resources.Load<TextAsset>($"Devices/{name}/common").text;
        LoadedDevice = JsonUtility.FromJson<INDevice>(json);
        Debug.Log(LoadedDevice.name);
        var prefab = Resources.Load<GameObject>($"Devices/{name}/prefab");
        InstancedDevice = Instantiate(prefab);
        InstancedDeviceT = InstancedDevice.transform;
        InstancedDeviceT.rotation = LoadedDevice.Rotation;
        InstancedDeviceT.position = LoadedDevice.PivotOffset;
        InstancedDeviceT.localScale = LoadedDevice.Scale;
    }

    public void Disassemble()
    {
        if (!disassembled)
        {
            int index = 0;
            for (int i = 0; i < InstancedDeviceT.childCount; i++)
            {
                var child = InstancedDeviceT.GetChild(i);
                
                if (!child.CompareTag("DeviceIgnore"))
                {
                    child.localPosition = new Vector3(LoadedDevice.details_offsets[index * 3],
                                                      LoadedDevice.details_offsets[index * 3 + 1],
                                                      LoadedDevice.details_offsets[index * 3 + 2]);
                    index++;
                }
            }
        } else
        {
            int index = 0;
            for (int i = 0; i < InstancedDeviceT.childCount; i++)
            {
                var child = InstancedDeviceT.GetChild(i);

                if (!child.CompareTag("DeviceIgnore"))
                {
                    child.localPosition = new Vector3();
                    index++;
                }
            }
        }
        disassembled = !disassembled;
    }
}
