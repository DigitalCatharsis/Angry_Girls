using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class ReadySequenceXStyle : MonoBehaviour
{
    [Header("Timings")]
    public float startScale = 3f;
    public float popDuration = 0.22f;
    public float holdDuration = 0.85f;
    public float disappearDuration = 0.25f;

    [Header("Electric FX")]
    public int lightningCount = 6;
    public float lightningDuration = 0.25f;

    private TextMeshProUGUI readyText;
    private Image flash;
    private Image radial;
    private Image sweep;
    private CanvasGroup group;
    private Material tmpMat;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        CreateFlash();
        CreateRadial();
        CreateText();
        CreateSweep();

        Play();
    }

    void CreateFlash()
    {
        GameObject go = new GameObject("Flash");
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

    void CreateRadial()
    {
        GameObject go = new GameObject("RadialBurst");
        go.transform.SetParent(transform, false);

        radial = go.AddComponent<Image>();
        radial.sprite = CreateRadialSprite();
        radial.color = new Color(1, 1, 1, 0);
        radial.raycastTarget = false;

        RectTransform rt = radial.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700, 700);
        rt.anchoredPosition = Vector2.zero;
        radial.transform.localScale = Vector3.one * 0.5f;
    }

    void CreateText()
    {
        GameObject go = new GameObject("READY_Text");
        go.transform.SetParent(transform, false);

        readyText = go.AddComponent<TextMeshProUGUI>();
        readyText.text = "READY";
        readyText.fontSize = 170;
        readyText.alignment = TextAlignmentOptions.Center;
        readyText.color = Color.white;
        readyText.alpha = 0;
        readyText.raycastTarget = false;

        RectTransform rt = readyText.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200, 400);
        rt.anchoredPosition = Vector2.zero;

        tmpMat = Instantiate(readyText.fontMaterial);
        readyText.fontMaterial = tmpMat;

        readyText.outlineWidth = 0.25f;
        readyText.outlineColor = Color.black;

        readyText.transform.localScale = Vector3.one * startScale;
    }

    void CreateSweep()
    {
        GameObject go = new GameObject("Sweep");
        go.transform.SetParent(readyText.transform, false);

        sweep = go.AddComponent<Image>();
        sweep.sprite = CreateSweepSprite();
        sweep.color = new Color(1, 1, 1, 0.9f);
        sweep.raycastTarget = false;

        RectTransform rt = sweep.rectTransform;
        rt.sizeDelta = new Vector2(400, 250);
        rt.anchoredPosition = new Vector2(-900, 0);
    }

    void Play()
    {
        Sequence seq = DOTween.Sequence();

        // FLASH
        seq.Append(flash.DOFade(1, 0.05f));
        seq.Append(flash.DOFade(0, 0.18f));

        // RADIAL BURST
        seq.Join(radial.DOFade(0.7f, 0.08f));
        seq.Join(radial.transform.DOScale(1.4f, 0.25f));
        seq.Join(radial.DOFade(0, 0.3f).SetDelay(0.1f));

        // POP
        seq.Join(readyText.DOFade(1, 0.05f));
        seq.Join(readyText.transform
            .DOScale(1f, popDuration)
            .SetEase(Ease.OutBack));

        // ELECTRIC SPARKS
        seq.AppendCallback(() => SpawnLightnings());

        // SWEEP (FIXED)
        sweep.rectTransform.anchoredPosition = new Vector2(-900, 0);
        seq.Append(sweep.rectTransform.DOAnchorPosX(900, 0.45f));

        seq.AppendInterval(holdDuration);

        // DISAPPEAR
        seq.Append(readyText.transform.DOScale(1.3f, disappearDuration));
        seq.Join(readyText.DOFade(0, disappearDuration));
    }

    void SpawnLightnings()
    {
        for (int i = 0; i < lightningCount; i++)
        {
            GameObject bolt = new GameObject("Lightning");
            bolt.transform.SetParent(readyText.transform, false);

            Image img = bolt.AddComponent<Image>();
            img.sprite = CreateLightningSprite();
            img.color = Color.white;
            img.raycastTarget = false;

            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(200, 30);

            float x = Random.Range(-400, 400);
            float y = Random.Range(-100, 100);
            rt.anchoredPosition = new Vector2(x, y);
            rt.rotation = Quaternion.Euler(0, 0, Random.Range(-30, 30));

            img.DOFade(0, lightningDuration)
                .OnComplete(() => Destroy(bolt));
        }
    }

    Sprite CreateSweepSprite()
    {
        int w = 256;
        int h = 4;
        Texture2D tex = new Texture2D(w, h);

        for (int x = 0; x < w; x++)
        {
            float a = Mathf.Sin((x / (float)w) * Mathf.PI);
            Color c = new Color(1, 1, 1, a);
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, c);
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateRadialSprite()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2, size / 2);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2);
                float a = Mathf.Clamp01(1 - dist);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateLightningSprite()
    {
        int w = 128;
        int h = 16;
        Texture2D tex = new Texture2D(w, h);

        for (int x = 0; x < w; x++)
        {
            float offset = Mathf.PerlinNoise(x * 0.1f, 0) * h;
            int yPos = Mathf.Clamp((int)offset, 0, h - 1);
            tex.SetPixel(x, yPos, Color.white);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }
}