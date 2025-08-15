using UnityEngine;

public class PlayerVisual2D : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public SpriteRenderer sr;
    public PlayerClass defaultClass;

    [Header("Tuning")]
    public float dirDeadZone = 0.05f;

    Vector3 _lastPos;
    Vector2 _lastDir = Vector2.down;

    void Reset()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ApplyClass(defaultClass);
        _lastPos = transform.position;
    }

    public void ApplyClass(PlayerClass cls)
    {
        if (cls == null) return;
        if (animator && cls.animator) animator.runtimeAnimatorController = cls.animator;
        if (sr && cls.defaultIdleSprite) sr.sprite = cls.defaultIdleSprite;
    }

    void Update()
    {
        // Velocidade a partir do delta de posição (funciona p/ local e remoto)
        Vector3 pos = transform.position;
        Vector2 v = (pos - _lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPos = pos;

        float speed = v.magnitude;

        // ---------- Quantização com histerese ----------
        // Mantém a última direção até a diferença entre eixos passar uma margem (evita troca "nervosa")
        const float hysteresis = 0.1f; // ajuste se precisar
        Vector2 dir;

        if (speed > dirDeadZone)
        {
            float ax = Mathf.Abs(v.x);
            float ay = Mathf.Abs(v.y);

            // aplica histerese em relação à última direção
            bool preferX = ax > ay + hysteresis || (Mathf.Abs(ax - ay) <= hysteresis && Mathf.Abs(_lastDir.x) > Mathf.Abs(_lastDir.y));

            if (preferX)
                dir = new Vector2(Mathf.Sign(v.x), 0f);   // Left/Right
            else
                dir = new Vector2(0f, Mathf.Sign(v.y));   // Up/Down

            _lastDir = dir; // memoriza a direção cardinal atual
        }
        else
        {
            dir = _lastDir; // parado: mantém última direção
        }

        // Envia para o Animator
        if (animator)
        {
            animator.SetFloat("Speed", speed);
            animator.SetFloat("MoveX", dir.x); // -1, 0 ou 1
            animator.SetFloat("MoveY", dir.y); // -1, 0 ou 1
        }

        // Você tem Walk_Left e Walk_Right separados, então NÃO faça flip
        if (sr)
        {
            sr.flipX = false; // garantimos que não vamos inverter, já que há clips separados
        }
    }
}
