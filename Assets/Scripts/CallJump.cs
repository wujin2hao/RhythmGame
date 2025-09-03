using System.Collections;
using UnityEngine;
using SonicBloom.Koreo;

public class CallJump : MonoBehaviour
{
    [Header("Koreographer Ref (拖 MusicPlayer 上的 Koreographer 过来)")]
    public Koreographer koreographer;        // ★ 新增

    [Tooltip("要监听的 Koreography Track 的 Event ID")]
    public string eventID = "CallJump";

    [Header("跳跃表现")]
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.10f;
    public bool usePayloadAsHeight = true;

    Vector3 basePos;
    Coroutine running;

    void Awake(){ basePos = transform.localPosition; }

    void OnEnable()
    {
        var k = koreographer != null ? koreographer : Koreographer.Instance;
        if (k != null) k.RegisterForEvents(eventID, OnKoreoEvent);
        else Debug.LogError("[CallJump] No Koreographer available.");
    }

    void OnDisable()
    {
        var k = koreographer != null ? koreographer : Koreographer.Instance;
        if (k != null) k.UnregisterForEvents(eventID, OnKoreoEvent);
    }

    void OnKoreoEvent(KoreographyEvent evt)
    {
        float h = jumpHeight;
        if (usePayloadAsHeight)
        {
            if (evt.HasFloatPayload()) { var f = evt.GetFloatValue(); if (f > 0f) h *= f; }
            else if (evt.HasIntPayload()) { var i = evt.GetIntValue(); if (i > 0) h *= i; }
        }
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Bounce(h));
    }

    System.Collections.IEnumerator Bounce(float height)
    {
        Vector3 up = basePos + Vector3.up * height;
        float t = 0f;
        while (t < jumpDuration)
        {
            t += Time.deltaTime;
            float p = t / jumpDuration;
            float ease = Mathf.Sin(p * Mathf.PI);
            transform.localPosition = Vector3.Lerp(basePos, up, ease);
            yield return null;
        }
        transform.localPosition = basePos;
        running = null;
    }
}
