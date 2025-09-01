using System;
using UnityEngine;

public class Conductor : MonoBehaviour
{
    [Header("Audio & Tempo")]
    public AudioSource musicSource;
    [Range(40, 220)] public double bpm = 128;
    public int beatsPerBar = 4;
    [Tooltip("音乐第1拍相对音频起点的偏移(秒)，正值表示前面有空白。")]
    public double firstBeatOffsetSec = 0.0;

    [Header("Latency Calibration")]
    [Tooltip("整体校准(毫秒)。正值=把判定时间线向后推(更容易早按)。")]
    public double latencyMs = 0.0;

    [Header("Run")]
    public bool playOnStart = true;
    public int subdivision = 2; // 每拍再细分(2=八分)

    public event Action<int> OnBeat;        // 0,1,2...
    public event Action<int> OnBar;         // 0,1,2...
    public event Action<int> OnSubdivision; // 0,1,2... (拍*subdivision)

    double _dspStart;
    bool _started;
    int _lastBeat = -1, _lastBar = -1, _lastSub = -1;

    public double SecPerBeat => 60.0 / bpm;
    public double SongPosSec => !_started ? 0 :
        (AudioSettings.dspTime - _dspStart) - firstBeatOffsetSec + (latencyMs / 1000.0);
    public double SongPosBeats => SongPosSec / SecPerBeat;

    void Start() { if (playOnStart) StartSong(); }

    public void StartSong(double delay = 0.1)
    {
        double start = AudioSettings.dspTime + delay;
        _dspStart = start;
        musicSource.PlayScheduled(start);
        _started = true;
        _lastBeat = _lastBar = _lastSub = -1;
    }

    void Update()
    {
        if (!_started) return;

        double beats = SongPosBeats;

        // 整拍事件
        int beatIndex = Mathf.FloorToInt((float)beats);
        while (beatIndex > _lastBeat)
        {
            _lastBeat++;
            OnBeat?.Invoke(_lastBeat);

            if (beatsPerBar > 0 && _lastBeat % beatsPerBar == 0)
            {
                int barIndex = _lastBeat / beatsPerBar;
                if (barIndex > _lastBar)
                {
                    _lastBar = barIndex;
                    OnBar?.Invoke(_lastBar);
                }
            }
        }

        // 细分拍事件
        double subPos = beats * subdivision;
        int subIndex = Mathf.FloorToInt((float)subPos);
        while (subIndex > _lastSub)
        {
            _lastSub++;
            OnSubdivision?.Invoke(_lastSub);
        }
    }

    // 误差：当前离最近整拍的偏差(单位: 拍，+晚 -早)
    public double ErrorToNearestBeatBeats()
    {
        double b = SongPosBeats;
        return b - Math.Round(b);
    }

    public double ErrorMs(double errorBeats) => errorBeats * SecPerBeat * 1000.0;

    public void SetLatencyMs(float ms) => latencyMs = ms;
}
