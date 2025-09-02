using System.Collections;
using UnityEngine;

public class BeatBouncer : MonoBehaviour
{
    public Conductor conductor;
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.10f;

    Vector3 _basePos;

    void Awake() => _basePos = transform.localPosition;
    void OnEnable() { if (conductor) conductor.OnBeat += HandleBeat; }
    void OnDisable() { if (conductor) conductor.OnBeat -= HandleBeat; }

    void HandleBeat(int beat) => StartCoroutine(Bounce());

    IEnumerator Bounce()
    {
        Vector3 up = _basePos + Vector3.up * jumpHeight;
        float t = 0f;
        while (t < jumpDuration)
        {
            t += Time.deltaTime;
            float p = t / jumpDuration;               // 0→1
            float ease = Mathf.Sin(p * Mathf.PI);     // 上下一个弧线
            transform.localPosition = Vector3.Lerp(_basePos, up, ease);
            yield return null;
        }
        transform.localPosition = _basePos;
    }
}
