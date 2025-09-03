using UnityEngine;
using SonicBloom.Koreo;

public class KoreoEventLogger : MonoBehaviour
{
    public Koreographer koreographer;   // ★务必拖 MusicPlayer 上的那个
    public string eventID = "CallJump";

    void OnEnable()
    {
        if (!koreographer) { Debug.LogError("[Logger] No Koreographer ref"); return; }
        koreographer.RegisterForEventsWithTime(eventID, OnEvt);
        Debug.Log("[Logger] Registered for " + eventID + " on " + koreographer.name);
    }

    void OnDisable()
    {
        if (koreographer) koreographer.UnregisterForEvents(eventID, OnEvt);
    }

    void OnEvt(KoreographyEvent e, int sampleTime, int _, DeltaSlice __)
    {
        Debug.Log($"[Logger] {eventID} @ sample {sampleTime}");
    }
}
