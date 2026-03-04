using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ReadySequenceDOTween : MonoBehaviour
{
    [Header("Main Settings")]
    public string readyTextString = "READY";
    public float startScale = 3f;
    public float popDuration = 0.25f;
    public float holdDuration = 0.8f;
    public float disappearDuration = 0.25f;

    [Header("Camera Shake")]
    public float shakeDuration = 0.25f;
    public float shakeStrength = 0.4f;

    private Canvas canvas;
    private TextMeshProUGUI readyText;
    private Image flashImage;
    private Image sweepImage;
    private Camera cam;
    private Material tmpMaterialInstance;

    void Start()
    {
        DOTween.Init();
        cam = Camera.main;

        CreateCanvas();
        CreateFlash();
        CreateText();
        CreateSweep();

        PlaySequence();
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("READY_Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasGO.AddComponent<GraphicRaycaster>();
    }

    void CreateFlash()
    {
        GameObject flashGO = new GameObject("Flash");
        flashGO.transform.SetParent(canvas.transform);

        flashImage = flashGO.AddComponent<Image>();
        flashImage.color = new Color(1, 1, 1, 0);

        RectTransform rt = flashImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void CreateText()
    {
        GameObject textGO = new GameObject("READY_Text");
        textGO.transform.SetParent(canvas.transform);

        readyText = textGO.AddComponent<TextMeshProUGUI>();
        readyText.text = readyTextString;
        readyText.alignment = TextAlignmentOptions.Center;
        readyText.fontSize = 160;
        readyText.color = Color.white;
        readyText.alpha = 0;

        RectTransform rt = readyText.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200, 400);
        rt.anchoredPosition = Vector2.zero;

        // Outline
        readyText.fontMaterial.EnableKeyword("OUTLINE_ON");
        readyText.outlineWidth = 0.25f;
        readyText.outlineColor = Color.black;

        // Glow
        tmpMaterialInstance = Instantiate(readyText.fontMaterial);
        readyText.fontMaterial = tmpMaterialInstance;
        tmpMaterialInstance.EnableKeyword("GLOW_ON");
        tmpMaterialInstance.SetFloat(ShaderUtilities.ID_GlowPower, 0);
        tmpMaterialInstance.SetColor(ShaderUtilities.ID_GlowColor, Color.white);

        readyText.transform.localScale = Vector3.one * startScale;
    }

    void CreateSweep()
    {
        GameObject sweepGO = new GameObject("Sweep");
        sweepGO.transform.SetParent(readyText.transform);

        sweepImage = sweepGO.AddComponent<Image>();
        sweepImage.sprite = CreateGradientSprite();
        sweepImage.color = new Color(1, 1, 1, 0.8f);

        RectTransform rt = sweepImage.rectTransform;
        rt.sizeDelta = new Vector2(300, 300);
        rt.anchoredPosition = new Vector2(-800, 0);
    }

    Sprite CreateGradientSprite()
    {
        int width = 256;
        int height = 4;

        Texture2D tex = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            float alpha = Mathf.Sin((x / (float)width) * Mathf.PI);
            Color col = new Color(1, 1, 1, alpha);

            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, col);
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(tex,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f));
    }

    void PlaySequence()
    {
        Sequence seq = DOTween.Sequence();

        // Flash
        seq.Append(flashImage.DOFade(1, 0.05f));
        seq.Append(flashImage.DOFade(0, 0.2f));

        // Pop-in + glow
        seq.Join(readyText.DOFade(1, 0.05f));
        seq.Join(readyText.transform
            .DOScale(1f, popDuration)
            .SetEase(Ease.OutBack));

        seq.Join(DOTween.To(
            () => tmpMaterialInstance.GetFloat(ShaderUtilities.ID_GlowPower),
            x => tmpMaterialInstance.SetFloat(ShaderUtilities.ID_GlowPower, x),
            1f,
            0.15f
        ));

        // Camera Shake
        seq.Join(cam.transform.DOShakePosition(
            shakeDuration,
            shakeStrength,
            25,
            90,
            false,
            true
        ));

        // Sweep
        seq.Append(
            sweepImage.rectTransform.DOAnchorPosX(
                800,
                0.4f
            //).From(-800)
            ).From(true)
        );

        // Hold
        seq.AppendInterval(holdDuration);

        // Disappear
        seq.Append(
            readyText.transform
            .DOScale(1.3f, disappearDuration)
        );

        seq.Join(
            readyText.DOFade(0, disappearDuration)
        );
    }
}
