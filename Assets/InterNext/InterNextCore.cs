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
    public static bool PowerDevice = false;
    public string CurrentDevice = "AsyncEngine";
    private INDevice LoadedDevice;
    private Vector3[] DetailsRealOffsets;
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
        var prefab = Resources.Load<GameObject>($"Devices/{name}/prefab");
        InstancedDevice = Instantiate(prefab);
        InstancedDeviceT = InstancedDevice.transform;
        InstancedDeviceT.rotation = LoadedDevice.Rotation;
        InstancedDeviceT.position = LoadedDevice.PivotOffset;
        InstancedDeviceT.localScale = LoadedDevice.Scale;

        Transform[] children = new Transform[InstancedDeviceT.childCount];
        for (int i = 0; i < InstancedDeviceT.childCount; i++)
            children[i] = InstancedDeviceT.GetChild(i);

        DetailsRealOffsets = new Vector3[children.Length];

        int index = 0;
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];

            if (!child.CompareTag("DeviceIgnore") && !child.CompareTag("DetailPivot"))
            {
                var pivot = new GameObject($"Detail {index}");
                pivot.transform.position = child.gameObject.GetComponent<MeshRenderer>().bounds.center + InstancedDeviceT.position;
                pivot.transform.SetParent(InstancedDeviceT, true);
                child.SetParent(pivot.transform, true);
                pivot.tag = "DetailPivot";
                var spin = pivot.AddComponent<InterNextSpin>();
                spin.Axis = LoadedDevice.rotation_axis[index];
                spin.Speed = 3f;
                DetailsRealOffsets[index] = pivot.transform.localPosition;
                index++;
            }
        }
    }

    IEnumerator LerpMove(Transform t, Vector3 target, Vector3 start, float time, float startTime)
    {
        while (Time.time < startTime + time)
        {
            float tm = (Time.time - startTime) / time;
            t.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, tm));
            yield return new WaitForSeconds(Time.deltaTime);
        }

    }

    public void Disassemble()
    {
        StopAllCoroutines();
        if (!disassembled)
        {
            int index = 0;
            for (int i = 0; i < InstancedDeviceT.childCount; i++)
            {
                var child = InstancedDeviceT.GetChild(i);
                
                if (!child.CompareTag("DeviceIgnore"))
                {
                    //child.localPosition += new Vector3(LoadedDevice.details_offsets[index * 3],
                    //                                  LoadedDevice.details_offsets[index * 3 + 1],
                    //                                  LoadedDevice.details_offsets[index * 3 + 2]);
                    StartCoroutine(LerpMove(child, child.localPosition + new Vector3(LoadedDevice.details_offsets[index * 3],
                                                      LoadedDevice.details_offsets[index * 3 + 1],
                                                      LoadedDevice.details_offsets[index * 3 + 2]), child.localPosition, 2f, Time.time));
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
                    StartCoroutine(LerpMove(child, DetailsRealOffsets[index], child.localPosition, 2f, Time.time));
                    //child.localPosition = DetailsRealOffsets[index];
                    index++;
                }
            }
        }
        disassembled = !disassembled;
    }

    public void TogglePower()
    {
        PowerDevice = !PowerDevice;
    }
}
