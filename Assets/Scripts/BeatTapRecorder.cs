using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// 枚举放到类外，避免把 Header 标到 enum 上
public enum SaveRoot { ProjectAssets, PersistentData }

public class BeatTapRecorder : MonoBehaviour
{
    public Conductor conductor;

    [Header("Old Input (used if Active Input Handling = Both or Old)")]
    public KeyCode tapKeyLegacy = KeyCode.J;
    public KeyCode saveKeyLegacy = KeyCode.K;

    [Header("Save Location")]
    public SaveRoot saveRoot = SaveRoot.ProjectAssets; // 存到 项目/Assets
    public string subFolder = "Charts";                // 子目录（留空=直接放到 Assets 根）
    public string fileName = "chart.json";

    [Serializable] class Chart { public List<double> cues = new(); }
    Chart _chart = new Chart();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    // —— New Input System ——
    InputAction _tapAction;
    InputAction _saveAction;

    void OnEnable()
    {
        if (_tapAction == null)
        {
            _tapAction  = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/j");
            _saveAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/k");
            _tapAction.performed  += OnTap;
            _saveAction.performed += OnSave;
        }
        _tapAction.Enable();
        _saveAction.Enable();
    }

    void OnDisable() { _tapAction?.Disable(); _saveAction?.Disable(); }

    void OnDestroy()
    {
        if (_tapAction != null)  { _tapAction.performed  -= OnTap;  _tapAction.Dispose(); }
        if (_saveAction != null) { _saveAction.performed -= OnSave; _saveAction.Dispose(); }
    }

    void OnTap(InputAction.CallbackContext ctx)
    {
        if (conductor == null) return;
        _chart.cues.Add(conductor.SongPosBeats);
        Debug.Log($"Tap at beat: {conductor.SongPosBeats:F3}");
    }

    void OnSave(InputAction.CallbackContext ctx) => Save();

#else
    // —— Old Input (或 Both) ——
    void Update()
    {
        if (conductor == null) return;

        if (Input.GetKeyDown(tapKeyLegacy))
        {
            _chart.cues.Add(conductor.SongPosBeats);
            Debug.Log($"Tap at beat: {conductor.SongPosBeats:F3}");
        }
        if (Input.GetKeyDown(saveKeyLegacy)) Save();
    }
#endif

    void Save()
    {
        string root = (saveRoot == SaveRoot.ProjectAssets)
            ? Application.dataPath
            : Application.persistentDataPath;

        string dir = string.IsNullOrEmpty(subFolder) ? root : Path.Combine(root, subFolder);
        Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, fileName);
        string json = JsonUtility.ToJson(_chart, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved chart: " + path);

#if UNITY_EDITOR
        if (saveRoot == SaveRoot.ProjectAssets)
        {
            UnityEditor.AssetDatabase.Refresh();
            // UnityEditor.EditorUtility.RevealInFinder(path); // 需要自动打开时取消注释
        }
#endif
    }
}
