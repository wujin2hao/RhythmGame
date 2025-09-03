using UnityEngine;
using System.Collections.Generic;
using SonicBloom.Koreo;   // Koreographer

public class ResponseJumpJudge : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource musicSource;
    public KoreographyTrackBase responseJumpTrack;
    public RightBlockController rightBlock;

    [Header("Timing (ms)")]
    [Range(10f, 200f)] public float hitWindowMs  = 90f;   // 命中容差窗口（单侧 ±hitWindowMs）
    [Range(-120f, 120f)] public float inputOffsetMs = 0f; // 输入整体偏移校准（正=更晚）

    // 内部
    List<KoreographyEvent> _events;
    int _cursor;
    int _sr;
    int _winSamples;
    int _offsetSamples;

    void Start()
    {
        if (musicSource == null || musicSource.clip == null ||
            responseJumpTrack == null || rightBlock == null)
        {
            Debug.LogError("ResponseJumpJudge: 引用未设置完整。");
            enabled = false; return;
        }

        _sr = musicSource.clip.frequency;
        RecalcSamples();

        _events = new List<KoreographyEvent>(responseJumpTrack.GetAllEvents());
        _events.Sort((a, b) => a.StartSample.CompareTo(b.StartSample));
        _cursor = 0;
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

        // 让已经“过窗很久”的事件自动过账（玩家没按，不震动不提示）
        int now = Koreographer.Instance.GetMusicSampleTime() + _offsetSamples;

        // TryJudge(int nowSamples) 的调用点不变；如果你在 TryJudge 里也有取 now 的地方，同样改成无参：
int nowSamples = Koreographer.Instance.GetMusicSampleTime() + _offsetSamples;
        while (_cursor < _events.Count && now - _events[_cursor].StartSample > _winSamples)
        {
            _cursor++; // 这次点算错过
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJudge(now);
        }
    }

    void TryJudge(int nowSamples)
    {
        if (_cursor >= _events.Count) return;

        int delta = nowSamples - _events[_cursor].StartSample;  // 正=偏晚，负=偏早
        int ad = Mathf.Abs(delta);

        if (ad <= _winSamples)
        {
            // 命中：消费事件 + 右方块跳起
            _cursor++;
            rightBlock.PlayJump();
        }
        else
        {
            // 不在窗口：不消费事件（你仍可在窗口内再次按到），给 Miss 反馈
            rightBlock.PlayMissFeedback();
        }
    }
}
