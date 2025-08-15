
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cicla imagens de fundo com crossfade e efeito Ken Burns (zoom/pan suave).
/// Use duas Images (A/B) em tela cheia sob o Canvas principal.
/// </summary>
public class BackgroundKenBurns : MonoBehaviour
{
    public Image bgA;
    public Image bgB;
    public List<Sprite> backgrounds = new List<Sprite>();

    [Header("Tempos (segundos)")]
    public float holdTime = 6f;
    public float fadeTime = 1.2f;

    [Header("Ken Burns")]
    public float zoomFrom = 1.03f;
    public float zoomTo   = 1.10f;
    public Vector2 panFrom = new Vector2(-10f, -6f);
    public Vector2 panTo   = new Vector2( 10f,  6f);

    int _index = 0;
    bool _useA = true;
    Coroutine _routine;

    void OnEnable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Run());
    }
    void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
    }

    IEnumerator Run()
    {
        if (backgrounds == null || backgrounds.Count == 0 || !bgA || !bgB)
            yield break;

        // inicial
        _index = 0;
        _useA = true;
        SetupImage(bgA, backgrounds[_index], 1f);
        SetupImage(bgB, backgrounds[(_index+1)%backgrounds.Count], 0f);

        while (true)
        {
            // anima o que está visível
            yield return StartCoroutine(KenBurnsAnim(_useA ? bgA.rectTransform : bgB.rectTransform));

            // troca
            _index = (_index + 1) % backgrounds.Count;
            Image fadeOut = _useA ? bgA : bgB;
            Image fadeIn  = _useA ? bgB : bgA;

            SetupImage(fadeIn, backgrounds[_index], 0f);
            yield return StartCoroutine(FadeCross(fadeOut, fadeIn, fadeTime));

            _useA = !_useA;
        }
    }

    void SetupImage(Image img, Sprite s, float alpha)
    {
        if (!img) return;
        img.sprite = s;
        var c = img.color;
        c.a = alpha;
        img.color = c;
        // reset transform
        img.rectTransform.localScale = Vector3.one * zoomFrom;
        img.rectTransform.anchoredPosition = panFrom;
    }

    IEnumerator FadeCross(Image from, Image to, float t)
    {
        float elapsed = 0f;
        var cFrom = from.color;
        var cTo = to.color;
        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / t);
            cFrom.a = Mathf.Lerp(1f, 0f, k);
            cTo.a   = Mathf.Lerp(0f, 1f, k);
            from.color = cFrom;
            to.color   = cTo;
            yield return null;
        }
        cFrom.a = 0f; cTo.a = 1f;
        from.color = cFrom; to.color = cTo;
    }

    IEnumerator KenBurnsAnim(RectTransform rt)
    {
        float t = 0f;
        while (t < holdTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / holdTime);
            float z = Mathf.Lerp(zoomFrom, zoomTo, k);
            Vector2 p = Vector2.Lerp(panFrom, panTo, k);
            if (rt)
            {
                rt.localScale = new Vector3(z, z, 1f);
                rt.anchoredPosition = p;
            }
            yield return null;
        }
    }
}
