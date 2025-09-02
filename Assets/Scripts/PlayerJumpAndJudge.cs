using System;
using System.Collections;
using UnityEngine;

public class PlayerJumpAndJudge : MonoBehaviour
{
    public Conductor conductor;
    public KeyCode key = KeyCode.Space;

    [Header("判定窗口(ms)")]
    public float perfectMs = 35f;
    public float goodMs = 90f;

    [Header("跳跃表现")]
    public float jumpHeight = 0.7f;
    public float jumpDuration = 0.12f;

    public event Action<string, double> OnJudge; // 结果, 误差ms(带符号)

    Vector3 _basePos;

    void Awake() => _basePos = transform.localPosition;

    void Update()
    {
        if (Input.GetKeyDown(key) && conductor != null)
        {
            double errBeats = conductor.ErrorToNearestBeatBeats();   // +晚 -早
            double errMs = conductor.ErrorMs(errBeats);
            double abs = Math.Abs(errMs);

            string res = abs <= perfectMs ? "Perfect" :
                         abs <= goodMs    ? "Good"    : "Miss";

            OnJudge?.Invoke(res, errMs);

            if (res != "Miss") StartCoroutine(Bounce());
        }
    }

    IEnumerator Bounce()
    {
        Vector3 up = _basePos + Vector3.up * jumpHeight;
        float t = 0f;
        while (t < jumpDuration)
        {
            t += Time.deltaTime;
            float p = t / jumpDuration;
            float ease = Mathf.Sin(p * Mathf.PI);
            transform.localPosition = Vector3.Lerp(_basePos, up, ease);
            yield return null;
        }
        transform.localPosition = _basePos;
    }
}
