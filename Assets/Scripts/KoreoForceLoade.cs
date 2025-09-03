using UnityEngine;
using SonicBloom.Koreo;

public class KoreoForceLoader : MonoBehaviour
{
    [Tooltip("拖 MusicPlayer 上的 Koreographer 组件")]
    public Koreographer target;

    [Tooltip("拖你正在用的 Koreography 资产，比如 Test2")]
    public Koreography koreography;

    void Awake()
    {
        if (target == null || koreography == null)
        {
            Debug.LogError("[ForceLoader] Missing refs: target/koreography");
            return;
        }

        // 运行时再次确保把这首曲子装进同一个 Koreographer
        target.LoadKoreography(koreography);
        Debug.Log("[ForceLoader] Loaded " + koreography.name + " into " + target.name);
    }
}
