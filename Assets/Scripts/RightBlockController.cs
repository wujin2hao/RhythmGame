using UnityEngine;
using System.Collections;

public class RightBlockController : MonoBehaviour
{
    [Header("Jump")]
    public float jumpHeight = 2.0f;     // 和左边方块一致的 public
    public float jumpDuration = 0.25f;  // 跳起到落回总时长
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Miss Feedback")]
    public float shakeDuration = 0.18f;
    public float shakeAmplitude = 0.25f;     // 左右摆动幅度
    public float shakeFrequency = 30f;       // 次数/秒
    public Color missFlashColor = Color.red;
    public float flashDuration = 0.08f;

    Vector3 _idlePos;
    Color _baseColor;
    bool _busy;

    SpriteRenderer _sr;
    Renderer _mr;

    void Awake()
    {
        _idlePos = transform.position;
        _sr = GetComponent<SpriteRenderer>();
        _mr = GetComponent<Renderer>();

        if (_sr != null) _baseColor = _sr.color;
        else if (_mr != null && _mr.material.HasProperty("_Color")) _baseColor = _mr.material.color;
        else _baseColor = Color.white;
    }

    public void PlayJump()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(JumpCo());
    }

    public void PlayMissFeedback()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(ShakeAndFlashCo());
    }

    IEnumerator JumpCo()
    {
        _busy = true;
        Vector3 start = _idlePos;
        Vector3 apex  = _idlePos + Vector3.up * jumpHeight;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
            float y01 = jumpCurve.Evaluate(t);
            // 上升到顶再回落（对称）
            float arc = (t <= 0.5f)
                ? jumpCurve.Evaluate(t * 2f)
                : jumpCurve.Evaluate((1f - t) * 2f);
            transform.position = Vector3.Lerp(start, apex, arc);
            yield return null;
        }
        transform.position = _idlePos;
        _busy = false;
    }

    IEnumerator ShakeAndFlashCo()
    {
        // 颜色闪
        StartCoroutine(FlashOnce());

        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float phase = t * shakeFrequency * Mathf.PI * 2f;
            float x = Mathf.Sin(phase) * shakeAmplitude * (1f - t / shakeDuration); // 逐渐收敛
            transform.position = _idlePos + new Vector3(x, 0f, 0f);
            yield return null;
        }
        transform.position = _idlePos;
    }

    IEnumerator FlashOnce()
    {
        // 改 SpriteRenderer 或 Material 颜色
        if (_sr != null)
        {
            _sr.color = missFlashColor;
            yield return new WaitForSeconds(flashDuration);
            _sr.color = _baseColor;
        }
        else if (_mr != null && _mr.material.HasProperty("_Color"))
        {
            _mr.material.color = missFlashColor;
            yield return new WaitForSeconds(flashDuration);
            _mr.material.color = _baseColor;
        }
        else
        {
            yield return null;
        }
    }
}
