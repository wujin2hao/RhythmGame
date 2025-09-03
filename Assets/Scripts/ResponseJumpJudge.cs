using UnityEngine;
using System.Collections.Generic;
using SonicBloom.Koreo;   // Koreographer API

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新输入系统
#endif

public class ResponseJumpJudge : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource musicSource;                 // 播放这首歌的 AudioSource（与该 Koreography 的 M Source Clip 一致）
    public KoreographyTrackBase responseJumpTrack;  // 直接拖 “ResponseJump” 这条 Track
    public RightBlockController rightBlock;         // 右边玩家方块（含跳跃/摇摆/闪红）

    [Header("Timing (ms)")]
    [Range(20f, 200f)] public float hitWindowMs   = 100f;   // 命中窗口（±）
    [Range(-150f,150f)] public float inputOffsetMs = 0f;    // 输入整体校准：正=把按键当作更晚

    [Header("Debug")]
    public bool debugLog = false;

    // 内部
    private List<KoreographyEvent> _events;
    private int _cursor;
    private int _sr;
    private int _winSamples;
    private int _offsetSamples;

    void Start()
    {
        if (!CheckRefs()) { enabled = false; return; }

        _sr = musicSource.clip.frequency;
        RecalcSamples();

        // 直接拿这条 Track 的全部事件（无需 EventID 过滤）
        _events = new List<KoreographyEvent>(responseJumpTrack.GetAllEvents());
        _events.Sort((a,b) => a.StartSample.CompareTo(b.StartSample));
        _cursor = 0;

        if (_events.Count == 0)
            Debug.LogWarning("[ResponseJumpJudge] 该 Track 没有事件（确认是否已打点）。");
    }

    void OnValidate()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            _sr = musicSource.clip.frequency;
            RecalcSamples();
        }
    }

    void RecalcSamples()
    {
        _winSamples    = Mathf.RoundToInt(Mathf.Abs(hitWindowMs) * 0.001f * _sr);
        _offsetSamples = Mathf.RoundToInt(inputOffsetMs * 0.001f * _sr);
    }

    void Update()
    {
        if (_events == null || _cursor >= _events.Count) return;

        // 用 Koreographer 的全局音乐时钟（无参），再加偏移
        int now = Koreographer.Instance.GetMusicSampleTime() + _offsetSamples;

        // 自动跳过“彻底错过窗口”的旧事件（不触发摇摆/闪红）
        while (_cursor < _events.Count && now - _events[_cursor].StartSample > _winSamples)
            _cursor++;

        if (debugLog && _cursor < _events.Count)
        {
            int delta = now - _events[_cursor].StartSample;
            Debug.Log($"[RJ Debug] now:{now} next:{_events[_cursor].StartSample} Δ:{delta} samples ({delta * 1000f / (float)_sr:F1} ms)");
        }

        // —— 统一的按键检测（新/旧输入系统均可用）——
        bool spacePressed =
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
            Input.GetKeyDown(KeyCode.Space);
#endif

        if (spacePressed)
            TryJudge(now);
    }

    void TryJudge(int nowSamples)
    {
        if (_cursor >= _events.Count) return;

        // 与“下一个目标点”比对
        int delta = nowSamples - _events[_cursor].StartSample; // 正=偏晚，负=偏早
        int ad = Mathf.Abs(delta);

        if (ad <= _winSamples)
        {
            // 命中：消费事件并让右方块起跳
            _cursor++;
            rightBlock.PlayJump();
        }
        else
        {
            // 未命中：不消费事件，给摇摆+闪红反馈（玩家仍可在窗口内再尝试命中）
            rightBlock.PlayMissFeedback();
        }
    }

    bool CheckRefs()
    {
        if (musicSource == null || musicSource.clip == null)
        { Debug.LogError("ResponseJumpJudge: 缺少 AudioSource 或 Clip。"); return false; }

        if (responseJumpTrack == null)
        { Debug.LogError("ResponseJumpJudge: 缺少 ResponseJump Track 引用。"); return false; }

        if (rightBlock == null)
        { Debug.LogError("ResponseJumpJudge: 缺少 RightBlockController 引用。"); return false; }

        return true;
    }
}
