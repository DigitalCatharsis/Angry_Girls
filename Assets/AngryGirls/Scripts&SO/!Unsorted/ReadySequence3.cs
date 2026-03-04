using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class ReadySequenceX4Full : MonoBehaviour
{
    public enum AppearMode
    {
        BigScalePop,
        SweepRevealLetters
    }

    [Header("Mode")]
    public AppearMode appearMode = AppearMode.BigScalePop;

    [Header("Timing")]
    public float popDuration = 0.25f;
    public float holdDuration = 1.2f;
    public float disappearDuration = 0.35f;

    private TextMeshProUGUI text;
    private Image flash;
    private Image sweep;
    private CanvasGroup group;
    private RectTransform rect;
    private Material textMat;

    List<Image> edgeBars = new();

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        rect = GetComponent<RectTransform>();

        CreateFlash();
        CreateText();
        CreateSweep();
        CreateEdgeBars();

        Play();
    }

    #region CREATE

    void CreateFlash()
    {
        GameObject go = new("Flash");
        go.transform.SetParent(transform, false);

        flash = go.AddComponent<Image>();
        flash.color = new Color(1, 1, 1, 0);
        flash.raycastTarget = false;

        RectTransform rt = flash.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void CreateText()
    {
        GameObject go = new("READY_Text");
        go.transform.SetParent(transform, false);

        text = go.AddComponent<TextMeshProUGUI>();
        text.text = "READY";
        text.fontSize = 180;
        text.alignment = TextAlignmentOptions.Center;
        text.alpha = 0;
        text.raycastTarget = false;

        RectTransform rt = text.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200, 400);
        rt.anchoredPosition = Vector2.zero;

        // Градиент
        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(
            new Color(0.6f, 0.9f, 1f),
            new Color(0.6f, 0.9f, 1f),
            new Color(0.2f, 0.5f, 1f),
            new Color(0.2f, 0.5f, 1f)
        );

        // Двойной контур
        text.outlineWidth = 0.35f;
        text.outlineColor = new Color(0, 0.2f, 0.5f);

        textMat = Instantiate(text.fontMaterial);
        text.fontMaterial = textMat;

        text.transform.localScale = Vector3.one * 3f;
    }

    void CreateSweep()
    {
        GameObject go = new("Sweep");
        go.transform.SetParent(text.transform, false);

        sweep = go.AddComponent<Image>();
        sweep.sprite = CreateGradientSprite();
        sweep.color = new Color(1, 1, 1, 0.8f);
        sweep.raycastTarget = false;

        RectTransform rt = sweep.rectTransform;
        rt.sizeDelta = new Vector2(500, 250);
        rt.anchoredPosition = new Vector2(-1000, 0);
    }

    void CreateEdgeBars()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject go = new("EdgeBar_" + i);
            go.transform.SetParent(transform, false);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.6f, 0.9f, 1f, 1);
            img.raycastTarget = false;

            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(200, 40);

            bool top = i < 4;
            float x = -600 + (i % 4) * 400;
            float y = top ? 400 : -400;

            rt.anchoredPosition = new Vector2(x, y);

            edgeBars.Add(img);
        }
    }

    #endregion

    void Play()
    {
        Sequence seq = DOTween.Sequence();

        // Мощная вспышка с несколькими обелениями
        seq.Append(flash.DOFade(1, 0.05f));
        seq.Append(flash.DOFade(0.6f, 0.1f));
        seq.Append(flash.DOFade(1, 0.05f));
        seq.Append(flash.DOFade(0, 0.25f));

        if (appearMode == AppearMode.BigScalePop)
        {
            seq.Join(text.DOFade(1, 0.05f));
            seq.Join(text.transform
                .DOScale(1f, popDuration)
                .SetEase(Ease.OutBack));
        }
        else
        {
            seq.Join(text.DOFade(1, 0.3f));
            seq.Join(text.transform.DOScale(1f, 0.4f));
        }

        // Сходящиеся 8 фигур
        foreach (var bar in edgeBars)
        {
            seq.Join(bar.rectTransform
                .DOAnchorPosY(0, 0.4f)
                .SetEase(Ease.OutCubic));

            seq.Join(bar.DOFade(0, 0.5f).SetDelay(0.2f));
        }

        // Sweep через текст
        sweep.rectTransform.anchoredPosition = new Vector2(-1000, 0);
        seq.Append(sweep.rectTransform.DOAnchorPosX(1000, 0.5f));

        // Наклон текста
        seq.Join(text.rectTransform
            .DORotate(new Vector3(0, 0, -6), 0.15f)
            .SetLoops(2, LoopType.Yoyo));

        // Моргание
        seq.Join(text.DOFade(0.6f, 0.05f)
            .SetLoops(4, LoopType.Yoyo));

        seq.AppendInterval(holdDuration);

        // Исчезновение
        seq.Append(text.transform.DOScale(1.3f, disappearDuration));
        seq.Join(text.DOFade(0, disappearDuration));
    }

    #region TEXTURES

    Sprite CreateGradientSprite()
    {
        int w = 256;
        int h = 4;
        Texture2D tex = new(w, h);

        for (int x = 0; x < w; x++)
        {
            float a = Mathf.Sin((x / (float)w) * Mathf.PI);
            Color c = new(1, 1, 1, a);
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, c);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }

    #endregion
}