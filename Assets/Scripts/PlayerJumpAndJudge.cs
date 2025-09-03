using System;
using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class PlayerJumpAndJudge : MonoBehaviour
{
    public Conductor conductor;

    [Header("判定窗口(ms)")]
    public float perfectMs = 35f;
    public float goodMs = 90f;

    [Header("跳跃表现")]
    public float jumpHeight = 0.7f;
    public float jumpDuration = 0.12f;

    public event Action<string, double> OnJudge; // 结果, 误差ms(带符号)

    private Vector3 _basePos;
    private Coroutine _running;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    // —— 新 Input System ——
    private InputAction _jump;

    private void OnEnable()
    {
        if (_jump == null)
        {
            _jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }
        _jump.performed += OnJumpPerformed;
        _jump.Enable();
    }

    private void OnDisable()
    {
        if (_jump != null)
        {
            _jump.performed -= OnJumpPerformed;
            _jump.Disable();
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        TryJudgeAndJump();
    }
#else
    // —— 旧 Input（或 Both 时走旧）——
    public KeyCode key = KeyCode.Space;

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            TryJudgeAndJump();
        }
    }
#endif

    private void Awake()
    {
        _basePos = transform.localPosition;
    }

    private void TryJudgeAndJump()
    {
        if (conductor == null) return;

        double errBeats = conductor.ErrorToNearestBeatBeats(); // +晚 -早
        double errMs = conductor.ErrorMs(errBeats);
        double abs = Math.Abs(errMs);

        string res;
        if (abs <= perfectMs) res = "Perfect";
        else if (abs <= goodMs) res = "Good";
        else res = "Miss";

        if (OnJudge != null) OnJudge(res, errMs);

        if (res != "Miss")
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Bounce());
        }
    }

    private IEnumerator Bounce()
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
        _running = null;
    }
}
