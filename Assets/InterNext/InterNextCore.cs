using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DetailsGroup
{
    public string name;
    public int[] details_id;
    public bool hide_others;
}

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
    public List<DetailsGroup> details_groups;
    public List<INPivotOverride> details_override_pivot;

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

public struct INInfo
{
    public string base_information;

    public string[] SplitPages()
    {
        return base_information.Split(new string[] { "$PAGE$" }, System.StringSplitOptions.RemoveEmptyEntries);
    }
}

[System.Serializable]
public struct INPivotOverride
{
    public int index;
    public float[] offset;
}

public class InterNextCore : MonoBehaviour
{
    public GameObject CrossPlane;
    public static bool PowerDevice = false;
    public string CurrentDevice = "AsyncEngine";
    public INDevice LoadedDevice;
    public INInfo LoadedInfo;
    public Dictionary<int, Vector3> OverridedOffsets;
    private bool[] DetailsLayers;
    private Vector3[] DetailsRealOffsets;
    private GameObject InstancedDevice;
    private Transform InstancedDeviceT;
    private bool disassembled = false;
    private bool playingAnimation = false;

    private void Start()
    {
        LoadDevice(CurrentDevice);
    }

    void LoadInterface()
    {

    }

    private Vector3 GetObjectCenter(GameObject obj)
    {
        var mrend = GetComponent<MeshRenderer>();
        if (mrend == null)
            return GetObjectCenter(transform.GetChild(0).gameObject);
        else
            return mrend.bounds.center;
    }

    void LoadDevice(string name)
    {
        var textPrefab = Resources.Load<GameObject>("Common/Prefabs/TMP");

        string json = Resources.Load<TextAsset>($"Devices/{name}/common").text;
        LoadedDevice = JsonUtility.FromJson<INDevice>(json);
        LoadedInfo = JsonUtility.FromJson<INInfo>(Resources.Load<TextAsset>($"Devices/{name}/text_ru").text);
        var prefab = Resources.Load<GameObject>($"Devices/{name}/prefab");
        InstancedDevice = Instantiate(prefab);
        InstancedDeviceT = InstancedDevice.transform;
        InstancedDeviceT.rotation = LoadedDevice.Rotation;
        InstancedDeviceT.position = LoadedDevice.PivotOffset;
        InstancedDeviceT.localScale = LoadedDevice.Scale;

        OverridedOffsets = new Dictionary<int, Vector3>();
        for (int i = 0; i < LoadedDevice.details_override_pivot.Count; i++)
            OverridedOffsets.Add(LoadedDevice.details_override_pivot[i].index, new Vector3(LoadedDevice.details_override_pivot[i].offset[0], LoadedDevice.details_override_pivot[i].offset[1], LoadedDevice.details_override_pivot[i].offset[2]));

        DetailsLayers = new bool[LoadedDevice.details_groups.Count];

        LoadInterface();

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
                var meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
                if (OverridedOffsets.ContainsKey(index))
                    pivot.transform.position = OverridedOffsets[index];
                else
                    pivot.transform.position = meshRenderer.bounds.center;
                pivot.transform.SetParent(InstancedDeviceT, true);
                child.SetParent(pivot.transform, true);
                var cutc = child.gameObject.AddComponent<OnePlaneCuttingController>();
                cutc.plane = CrossPlane;
                pivot.tag = "DetailPivot";

                var spin = pivot.AddComponent<InterNextSpin>();
                spin.Axis = LoadedDevice.rotation_axis[index];
                spin.Speed = 3f;

                var text = Instantiate(textPrefab);
                text.transform.SetParent(pivot.transform);
                text.transform.localPosition = new Vector3(1f, 1f, 0);
                text.GetComponent<TMPro.TMP_Text>().text = "";

                DetailsRealOffsets[index] = pivot.transform.localPosition;
                index++;
            }
        }

        

        FindObjectOfType<InterNextUI>().OnSbarUpdate();
        FindObjectOfType<InterNextUI>().Initialize();
    }

    IEnumerator LerpMove(Transform t, Vector3 target, Vector3 start, float time, float startTime)
    {
        playingAnimation = true;
        while (Time.time < startTime + time)
        {
            float tm = (Time.time - startTime) / time;
            t.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, tm));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        playingAnimation = false;
    }

    public void Disassemble()
    {
        if (playingAnimation)
            return;
        if (!disassembled)
        {
            CrossPlane.transform.position = new Vector3(0, 1000, 0);
            CrossPlane.transform.eulerAngles = new Vector3(90, 0, 0);
            int index = 0;
            for (int i = 0; i < InstancedDeviceT.childCount; i++)
            {
                var child = InstancedDeviceT.GetChild(i);
                
                if (!child.CompareTag("DeviceIgnore"))
                {
                    StartCoroutine(LerpMove(child, child.localPosition + new Vector3(LoadedDevice.details_offsets[index * 3],
                                                      LoadedDevice.details_offsets[index * 3 + 1],
                                                      LoadedDevice.details_offsets[index * 3 + 2]), child.localPosition, 2f, Time.time));
                    index++;
                }
            }
        } else
        {
            CrossPlane.transform.position = new Vector3(0, 1.5f, 0);
            CrossPlane.transform.eulerAngles = new Vector3(90, 0, 0);
            int index = 0;
            for (int i = 0; i < InstancedDeviceT.childCount; i++)
            {
                var child = InstancedDeviceT.GetChild(i);

                if (!child.CompareTag("DeviceIgnore"))
                {
                    StartCoroutine(LerpMove(child, DetailsRealOffsets[index], child.localPosition, 2f, Time.time));
                    index++;
                }
            }
        }
        disassembled = !disassembled;
    }

    private bool ArrayContains(int[] arr, int e)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == e)
                return true;
        return false;
    }

    public void ShowGroup(int[] details, bool hide_others)
    {
        int index = 0;
        for (int i = 0; i < InstancedDeviceT.childCount; i++)
        {
            var child = InstancedDeviceT.GetChild(i);

            if (!child.CompareTag("DeviceIgnore"))
            {
                if (ArrayContains(details, index))
                {
                    if (hide_others)
                        child.GetChild(0).gameObject.SetActive(true);
                    else
                        child.GetChild(0).gameObject.SetActive(!child.GetChild(0).gameObject.activeSelf);
                }
                else if (hide_others)
                    child.GetChild(0).gameObject.SetActive(false);
                
                index++;
            }
        }
    }

    public void ShowAll()
    {
        int index = 0;
        for (int i = 0; i < InstancedDeviceT.childCount; i++)
        {
            var child = InstancedDeviceT.GetChild(i);

            if (!child.CompareTag("DeviceIgnore"))
            {
                //child.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                child.GetChild(0).gameObject.SetActive(true);
                index++;
            }
        }
    }

    public void TogglePower()
    {
        PowerDevice = !PowerDevice;
    }
}
