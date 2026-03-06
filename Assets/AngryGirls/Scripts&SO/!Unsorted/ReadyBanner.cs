using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using Angry_Girls;

[RequireComponent(typeof(CanvasGroup))]
public class ReadyBanner_VertexDeform : MonoBehaviour
{
    public enum Style
    {
        Style1,
        Style2,
        Style3
    }

    [Header("Main")]
    [Tooltip("Animation style: Style1 – basic, Style2 – radial burst + lightnings, Style3 – gradient + edge bars + slant.")]
    public Style currentStyle = Style.Style1;

    [Header("Flash & Audio")]
    [Tooltip("Color of the flash image.")]
    public Color flashColor = Color.white;

    [Tooltip("Sound to play at the start of the sequence.")]
    public AudioClipData clipSound;



    [Header("Common Timings")]
    [Tooltip("Initial text scale before appearing.")]
    public float startScale = 3f;
    [Tooltip("Duration of the pop animation.")]
    public float popDuration = 0.25f;
    [Tooltip("Hold time after all effects before disappearing.")]
    public float holdDuration = 0.8f;
    [Tooltip("Disappear animation duration.")]
    public float disappearDuration = 0.25f;

    [Header("Camera Shake")]
    [Tooltip("Enable/disable camera shake.")]
    public bool enableCameraShake = true;
    [Tooltip("Camera shake duration.")]
    public float shakeDuration = 0.25f;
    [Tooltip("Camera shake strength.")]
    public float shakeStrength = 0.4f;

    [System.Serializable]
    public class Style1Settings
    {
        [Header("Flash")]
        [Tooltip("Duration of flash fade-in (0 -> 1).")]
        public float flashFadeInDuration = 0.05f;
        [Tooltip("Duration of flash fade-out (1 -> 0).")]
        public float flashFadeOutDuration = 0.2f;


        [Tooltip("Text to display.")]
        public string readyText = "READY";
        [Tooltip("Font size.")]
        public int fontSize = 160;
        [Tooltip("Text area size (width, height).")]
        public Vector2 textSize = new Vector2(1200, 400);
        [Range(0f, 1f)]
        [Tooltip("Outline width.")]
        public float outlineWidth = 0.25f;
        [Tooltip("Glow power.")]
        public float glowPower = 1f;
        [Tooltip("Glow fade-in duration.")]
        public float glowDuration = 0.15f;

        [Header("Sweep")]
        [Tooltip("Sweep width.")]
        public float sweepWidth = 120f;
        [Tooltip("Sweep height.")]
        public float sweepHeight = 100f;
        [Tooltip("Sweep start X position.")]
        public float sweepStartX = -800f;
        [Tooltip("Sweep end X position.")]
        public float sweepEndX = 800f;
        [Tooltip("Sweep movement duration.")]
        public float sweepDuration = 0.4f;
    }
    public Style1Settings style1 = new Style1Settings();

    [System.Serializable]
    public class Style2Settings
    {
        [Header("Flash")]
        [Tooltip("Duration of flash fade-in (0 -> 1).")]
        public float flashFadeInDuration = 0.05f;
        [Tooltip("Duration of flash fade-out (1 -> 0).")]
        public float flashFadeOutDuration = 0.2f;

        [Tooltip("Text to display.")]
        public string readyText = "READY";
        [Tooltip("Font size.")]
        public int fontSize = 170;
        [Tooltip("Text area size (width, height).")]
        public Vector2 textSize = new Vector2(1200, 400);
        [Range(0f, 1f)]
        [Tooltip("Outline width.")]
        public float outlineWidth = 0.25f;

        [Header("Radial Burst")]
        [Tooltip("Radial sprite size (width & height).")]
        public float radialSize = 700f;
        [Tooltip("Radial start scale.")]
        public float radialStartScale = 0.5f;
        [Tooltip("Radial end scale.")]
        public float radialEndScale = 1.4f;
        [Tooltip("Radial fade-in duration.")]
        public float radialFadeInDuration = 0.08f;
        [Tooltip("Radial fade-out duration.")]
        public float radialFadeOutDuration = 0.3f;

        [Header("Lightnings")]
        [Tooltip("Number of lightning bolts.")]
        public int lightningCount = 6;
        [Tooltip("Lightning lifetime.")]
        public float lightningDuration = 0.25f;
        [Tooltip("Lightning size (width, height).")]
        public Vector2 lightningSize = new Vector2(200, 30);
        [Tooltip("Random X range for lightning placement.")]
        public float lightningXRange = 400f;
        [Tooltip("Random Y range for lightning placement.")]
        public float lightningYRange = 100f;
        [Tooltip("Max rotation angle for lightning.")]
        public float lightningAngleRange = 30f;

        [Header("Sweep")]
        [Tooltip("Sweep width.")]
        public float sweepWidth = 400f;
        [Tooltip("Sweep height.")]
        public float sweepHeight = 250f;
        [Tooltip("Sweep start X position.")]
        public float sweepStartX = -900f;
        [Tooltip("Sweep end X position.")]
        public float sweepEndX = 900f;
        [Tooltip("Sweep movement duration.")]
        public float sweepDuration = 0.45f;
    }
    public Style2Settings style2 = new Style2Settings();

    [System.Serializable]
    public class Style3Settings
    {
        [Header("Flash")]
        [Tooltip("Duration of first flash (0 -> 1).")]
        public float flashPhase1Duration = 0.05f;
        [Tooltip("Duration of second flash (1 -> 0.6).")]
        public float flashPhase2Duration = 0.1f;
        [Tooltip("Duration of third flash (0.6 -> 1).")]
        public float flashPhase3Duration = 0.05f;
        [Tooltip("Duration of final fade-out (1 -> 0).")]
        public float flashPhase4Duration = 0.25f;

        [Tooltip("Text to display.")]
        public string readyText = "READY";
        [Tooltip("Font size.")]
        public int fontSize = 180;
        [Tooltip("Text area size (width, height).")]
        public Vector2 textSize = new Vector2(1200, 400);
        [Tooltip("Outline width.")]
        public float outlineWidth = 0.35f;
        [Tooltip("Outline color.")]
        public Color outlineColor = new Color(0, 0.2f, 0.5f);

        [Header("Gradient")]
        [Tooltip("Top-left gradient color.")]
        public Color gradientTopLeft = new Color(0.6f, 0.9f, 1f);
        [Tooltip("Top-right gradient color.")]
        public Color gradientTopRight = new Color(0.6f, 0.9f, 1f);
        [Tooltip("Bottom-left gradient color.")]
        public Color gradientBottomLeft = new Color(0.2f, 0.5f, 1f);
        [Tooltip("Bottom-right gradient color.")]
        public Color gradientBottomRight = new Color(0.2f, 0.5f, 1f);

        [Header("Edge Bars")]
        [Tooltip("Number of edge bars (must be even).")]
        public int edgeBarCount = 8;
        [Tooltip("Edge bar size (width, height).")]
        public Vector2 edgeBarSize = new Vector2(200, 40);
        [Tooltip("Start X position for the first bar.")]
        public float edgeBarStartX = -600f;
        [Tooltip("X step between bars.")]
        public float edgeBarXStep = 400f;
        [Tooltip("Y position for top bars.")]
        public float edgeBarYTop = 400f;
        [Tooltip("Y position for bottom bars.")]
        public float edgeBarYBottom = -400f;
        [Tooltip("Edge bar color.")]
        public Color edgeBarColor = new Color(0.6f, 0.9f, 1f);
        [Tooltip("Duration of bars moving to center.")]
        public float edgeBarMoveDuration = 0.4f;
        [Tooltip("Delay before bars start fading.")]
        public float edgeBarFadeDelay = 0.2f;
        [Tooltip("Bars fade duration.")]
        public float edgeBarFadeDuration = 0.5f;

        [Header("Sweep")]
        [Tooltip("Sweep width.")]
        public float sweepWidth = 500f;
        [Tooltip("Sweep height.")]
        public float sweepHeight = 250f;
        [Tooltip("Sweep start X position.")]
        public float sweepStartX = -1000f;
        [Tooltip("Sweep end X position.")]
        public float sweepEndX = 1000f;
        [Tooltip("Sweep movement duration.")]
        public float sweepDuration = 0.5f;

        [Header("Italic Animation (Vertex Deformation)")]
        [Tooltip("Slant strength (0.2–0.5 recommended).")]
        public float italicSlant = 0.3f;
        [Tooltip("Duration of returning from slanted to normal.")]
        public float italicReturnDuration = 0.2f;

        [Header("Blink Overlay")]
        [Tooltip("Fade value during blinking (0 = transparent, 1 = opaque).")]
        public float blinkFade = 0.6f;
        [Tooltip("Duration of one blink cycle (there and back).")]
        public float blinkDuration = 0.05f;
        [Tooltip("Number of blink loops.")]
        public int blinkLoops = 4;
    }
    public Style3Settings style3 = new Style3Settings();

    private Canvas _canvas;
    private CanvasGroup _group;
    private Camera _cam;

    private Image _flashImage;
    private TextMeshProUGUI _readyText;
    private Image _sweepImage;
    private Material _tmpMaterial;

    private Image _radialImage;

    private List<Image> _edgeBars = new List<Image>();

    // Vertex deformation fields
    private Vector3[] _originalVertices;
    private bool _originalVerticesSaved = false;
    private float _currentSlant;

    // Blink overlay for Style3
    private Image _blinkOverlay;

    private AudioManager _audioManager;

    void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _group.blocksRaycasts = false;
        _group.interactable = false;
    }

    void Start()
    {
        if (CoreManager.Instance != null)
        {
            _audioManager = CoreManager.Instance.AudioManager;
        }

        DOTween.Init();
        _cam = Camera.main;

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("ReadyBanner_VertexDeform: object must be inside a Canvas!");
            return;
        }

        CreateFlash();

        switch (currentStyle)
        {
            case Style.Style1:
                CreateStyle1();
                break;
            case Style.Style2:
                CreateStyle2();
                break;
            case Style.Style3:
                CreateStyle3();
                break;
        }

        _flashImage.transform.SetAsLastSibling();

        PlaySequence();
    }

    private void PlaySound()
    {
        //play sound

        if (_audioManager == null)
        {
            Debug.LogError("ReadyBanner: Audiomanager is null!");
            return;
        }

        _audioManager.PlayClipData(clipSound, clipSound.fallbackCategory, false);
    }

    #region Create Elements

    void CreateFlash()
    {
        GameObject flashGO = new GameObject("Flash");
        flashGO.transform.SetParent(_canvas.transform, false);

        _flashImage = flashGO.AddComponent<Image>();
        _flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        _flashImage.raycastTarget = false;

        RectTransform rt = _flashImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void CreateStyle1()
    {
        GameObject textGO = new GameObject("READY_Text");
        textGO.transform.SetParent(transform, false);

        _readyText = textGO.AddComponent<TextMeshProUGUI>();
        _readyText.text = style1.readyText;
        _readyText.alignment = TextAlignmentOptions.Center;
        _readyText.fontSize = style1.fontSize;
        _readyText.color = Color.white;
        _readyText.alpha = 0;
        _readyText.raycastTarget = false;

        RectTransform rt = _readyText.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = style1.textSize;
        rt.anchoredPosition = Vector2.zero;

        _readyText.fontMaterial.EnableKeyword("OUTLINE_ON");
        _readyText.outlineWidth = style1.outlineWidth;
        _readyText.outlineColor = Color.black;

        _tmpMaterial = Instantiate(_readyText.fontMaterial);
        _readyText.fontMaterial = _tmpMaterial;
        _tmpMaterial.EnableKeyword("GLOW_ON");
        _tmpMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
        _tmpMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.white);

        _readyText.transform.localScale = Vector3.one * startScale;

        CreateSweep(style1.sweepWidth, style1.sweepHeight, style1.sweepStartX);
    }

    void CreateStyle2()
    {
        GameObject radialGO = new GameObject("RadialBurst");
        radialGO.transform.SetParent(transform, false);

        _radialImage = radialGO.AddComponent<Image>();
        _radialImage.sprite = CreateRadialSprite();
        _radialImage.color = new Color(1, 1, 1, 0);
        _radialImage.raycastTarget = false;

        RectTransform rtRadial = _radialImage.rectTransform;
        rtRadial.anchorMin = rtRadial.anchorMax = new Vector2(0.5f, 0.5f);
        rtRadial.sizeDelta = new Vector2(style2.radialSize, style2.radialSize);
        rtRadial.anchoredPosition = Vector2.zero;
        _radialImage.transform.localScale = Vector3.one * style2.radialStartScale;

        GameObject textGO = new GameObject("READY_Text");
        textGO.transform.SetParent(transform, false);

        _readyText = textGO.AddComponent<TextMeshProUGUI>();
        _readyText.text = style2.readyText;
        _readyText.alignment = TextAlignmentOptions.Center;
        _readyText.fontSize = style2.fontSize;
        _readyText.color = Color.white;
        _readyText.alpha = 0;
        _readyText.raycastTarget = false;

        RectTransform rtText = _readyText.rectTransform;
        rtText.anchorMin = rtText.anchorMax = new Vector2(0.5f, 0.5f);
        rtText.sizeDelta = style2.textSize;
        rtText.anchoredPosition = Vector2.zero;

        _tmpMaterial = Instantiate(_readyText.fontMaterial);
        _readyText.fontMaterial = _tmpMaterial;

        _readyText.outlineWidth = style2.outlineWidth;
        _readyText.outlineColor = Color.black;

        _readyText.transform.localScale = Vector3.one * startScale;

        CreateSweep(style2.sweepWidth, style2.sweepHeight, style2.sweepStartX);
    }

    void CreateStyle3()
    {
        GameObject textGO = new GameObject("READY_Text");
        textGO.transform.SetParent(transform, false);

        _readyText = textGO.AddComponent<TextMeshProUGUI>();
        _readyText.text = style3.readyText;
        _readyText.alignment = TextAlignmentOptions.Center;
        _readyText.fontSize = style3.fontSize;
        _readyText.alpha = 0;
        _readyText.raycastTarget = false;

        _readyText.enableVertexGradient = true;
        _readyText.colorGradient = new VertexGradient(
            style3.gradientTopLeft,
            style3.gradientTopRight,
            style3.gradientBottomLeft,
            style3.gradientBottomRight
        );

        _readyText.outlineWidth = style3.outlineWidth;
        _readyText.outlineColor = style3.outlineColor;

        RectTransform rt = _readyText.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = style3.textSize;
        rt.anchoredPosition = Vector2.zero;

        _tmpMaterial = Instantiate(_readyText.fontMaterial);
        _readyText.fontMaterial = _tmpMaterial;

        _readyText.transform.localScale = Vector3.one * startScale;

        // Force mesh update so vertices are generated
        _readyText.ForceMeshUpdate();

        CreateSweep(style3.sweepWidth, style3.sweepHeight, style3.sweepStartX);

        for (int i = 0; i < style3.edgeBarCount; i++)
        {
            GameObject barGO = new GameObject("EdgeBar_" + i);
            barGO.transform.SetParent(transform, false);

            Image bar = barGO.AddComponent<Image>();
            bar.color = style3.edgeBarColor;
            bar.raycastTarget = false;

            RectTransform rtBar = bar.rectTransform;
            rtBar.sizeDelta = style3.edgeBarSize;

            bool top = i < style3.edgeBarCount / 2;
            float x = style3.edgeBarStartX + (i % (style3.edgeBarCount / 2)) * style3.edgeBarXStep;
            float y = top ? style3.edgeBarYTop : style3.edgeBarYBottom;

            rtBar.anchoredPosition = new Vector2(x, y);

            _edgeBars.Add(bar);
        }

        // Blink overlay
        GameObject blinkGO = new GameObject("BlinkOverlay");
        blinkGO.transform.SetParent(_readyText.transform, false);
        _blinkOverlay = blinkGO.AddComponent<Image>();
        _blinkOverlay.color = new Color(1, 1, 1, 0);
        _blinkOverlay.raycastTarget = false;

        RectTransform rtBlink = _blinkOverlay.rectTransform;
        rtBlink.anchorMin = Vector2.zero;
        rtBlink.anchorMax = Vector2.one;
        rtBlink.offsetMin = Vector2.zero;
        rtBlink.offsetMax = Vector2.zero;

        // Ensure overlay is above text
        _blinkOverlay.transform.SetAsLastSibling();

        _currentSlant = 0f;
    }

    void CreateSweep(float width, float height, float startX)
    {
        if (_readyText == null) return;

        GameObject sweepGO = new GameObject("Sweep");
        sweepGO.transform.SetParent(_readyText.transform, false);

        _sweepImage = sweepGO.AddComponent<Image>();
        _sweepImage.sprite = CreateGradientSprite();
        _sweepImage.color = new Color(1, 1, 1, 0.8f);
        _sweepImage.raycastTarget = false;

        RectTransform rt = _sweepImage.rectTransform;
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(startX, 0);
    }

    #endregion

    #region Texture Generators

    Sprite CreateGradientSprite()
    {
        int w = 256;
        int h = 4;
        Texture2D tex = new Texture2D(w, h);

        for (int x = 0; x < w; x++)
        {
            float alpha = Mathf.Sin((x / (float)w) * Mathf.PI);
            Color col = new Color(1, 1, 1, alpha);
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, col);
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

    #endregion

    #region Vertex Deformation

    void ApplySlant(float slant)
    {
        if (_readyText == null) return;
        _currentSlant = slant;

        TMP_TextInfo textInfo = _readyText.textInfo;
        if (textInfo == null || textInfo.meshInfo == null) return;

        if (!_originalVerticesSaved)
        {
            SaveOriginalVertices();
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            Vector3[] vertices = meshInfo.vertices;

            for (int j = 0; j < vertices.Length; j++)
            {
                Vector3 orig = _originalVertices[GetGlobalVertexIndex(i, j)];
                vertices[j].x = orig.x + slant * orig.y;
                vertices[j].y = orig.y;
                vertices[j].z = orig.z;
            }
        }

        _readyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    void SaveOriginalVertices()
    {
        TMP_TextInfo textInfo = _readyText.textInfo;
        int totalVertexCount = 0;
        foreach (var meshInfo in textInfo.meshInfo)
            totalVertexCount += meshInfo.vertices.Length;

        _originalVertices = new Vector3[totalVertexCount];

        int idx = 0;
        foreach (var meshInfo in textInfo.meshInfo)
        {
            foreach (var vert in meshInfo.vertices)
            {
                _originalVertices[idx++] = vert;
            }
        }
        _originalVerticesSaved = true;
    }

    int GetGlobalVertexIndex(int meshIndex, int vertexIndex)
    {
        int offset = 0;
        for (int i = 0; i < meshIndex; i++)
            offset += _readyText.textInfo.meshInfo[i].vertices.Length;
        return offset + vertexIndex;
    }

    #endregion

    #region Sequence

    void PlaySequence()
    {
        Sequence seq = DOTween.Sequence();

        // Flash animation based on current style
        if (currentStyle == Style.Style3)
        {
            seq.Append(_flashImage.DOFade(1, style3.flashPhase1Duration));
            seq.Append(_flashImage.DOFade(0.6f, style3.flashPhase2Duration));
            seq.Append(_flashImage.DOFade(1, style3.flashPhase3Duration));
            seq.Append(_flashImage.DOFade(0, style3.flashPhase4Duration));
        }
        else
        {
            float fadeIn = (currentStyle == Style.Style1) ? style1.flashFadeInDuration : style2.flashFadeInDuration;
            float fadeOut = (currentStyle == Style.Style1) ? style1.flashFadeOutDuration : style2.flashFadeOutDuration;

            seq.Append(_flashImage.DOFade(1, fadeIn));
            seq.Append(_flashImage.DOFade(0, fadeOut));
        }

        seq.AppendCallback(PlaySound);

        switch (currentStyle)
        {
            case Style.Style1:
                BuildStyle1Sequence(seq);
                break;
            case Style.Style2:
                BuildStyle2Sequence(seq);
                break;
            case Style.Style3:
                BuildStyle3Sequence(seq);
                break;
        }

        seq.Play();
    }

    void BuildStyle1Sequence(Sequence seq)
    {
        seq.Join(_readyText.DOFade(1, 0.05f));
        seq.Join(_readyText.transform
            .DOScale(1f, popDuration)
            .SetEase(Ease.OutBack));

        seq.Join(DOTween.To(
            () => _tmpMaterial.GetFloat(ShaderUtilities.ID_GlowPower),
            x => _tmpMaterial.SetFloat(ShaderUtilities.ID_GlowPower, x),
            style1.glowPower,
            style1.glowDuration
        ));

        if (enableCameraShake)
        {
            seq.Join(_cam.transform.DOShakePosition(shakeDuration, shakeStrength, 25, 90, false, true));
        }

        seq.Append(_sweepImage.rectTransform
            .DOAnchorPosX(style1.sweepEndX, style1.sweepDuration)
            .From(true));

        seq.AppendInterval(holdDuration);

        seq.Append(_readyText.transform.DOScale(1.3f, disappearDuration));
        seq.Join(_readyText.DOFade(0, disappearDuration));
    }

    void BuildStyle2Sequence(Sequence seq)
    {
        seq.Join(_radialImage.DOFade(0.7f, style2.radialFadeInDuration));
        seq.Join(_radialImage.transform.DOScale(style2.radialEndScale, style2.radialFadeOutDuration + 0.1f));
        seq.Join(_radialImage.DOFade(0, style2.radialFadeOutDuration).SetDelay(0.1f));

        seq.Join(_readyText.DOFade(1, 0.05f));
        seq.Join(_readyText.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack));

        seq.AppendCallback(() => SpawnLightnings());

        _sweepImage.rectTransform.anchoredPosition = new Vector2(style2.sweepStartX, 0);
        seq.Append(_sweepImage.rectTransform.DOAnchorPosX(style2.sweepEndX, style2.sweepDuration));

        if (enableCameraShake)
        {
            seq.Join(_cam.transform.DOShakePosition(shakeDuration, shakeStrength, 25, 90, false, true));
        }

        seq.AppendInterval(holdDuration);

        seq.Append(_readyText.transform.DOScale(1.3f, disappearDuration));
        seq.Join(_readyText.DOFade(0, disappearDuration));
    }

    void BuildStyle3Sequence(Sequence seq)
    {
        // Pop
        seq.Join(_readyText.DOFade(1, 0.05f));
        seq.Join(_readyText.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack));

        // Edge bars movement
        foreach (var bar in _edgeBars)
        {
            seq.Join(bar.rectTransform.DOAnchorPosY(0, style3.edgeBarMoveDuration).SetEase(Ease.OutCubic));
            seq.Join(bar.DOFade(0, style3.edgeBarFadeDuration).SetDelay(style3.edgeBarFadeDelay));
        }

        // Blink (parallel to edge bars, but before sweep)
        seq.Join(_readyText.DOFade(style3.blinkFade, style3.blinkDuration)
            .SetLoops(style3.blinkLoops, LoopType.Yoyo));

        // Sweep
        _sweepImage.rectTransform.anchoredPosition = new Vector2(style3.sweepStartX, 0);
        seq.Append(_sweepImage.rectTransform.DOAnchorPosX(style3.sweepEndX, style3.sweepDuration));

        // Italic forward (parallel to sweep)
        seq.Join(DOTween.To(() => _currentSlant, x => ApplySlant(x), style3.italicSlant, style3.sweepDuration)
                 .SetEase(Ease.OutSine));

        // After sweep, return italic to normal
        seq.Append(DOTween.To(() => _currentSlant, x => ApplySlant(x), 0f, style3.italicReturnDuration)
                  .SetEase(Ease.InSine));

        // Camera Shake (optional, parallel to italic return)
        if (enableCameraShake)
        {
            seq.Join(_cam.transform.DOShakePosition(shakeDuration, shakeStrength, 25, 90, false, true));
        }

        // Hold
        seq.AppendInterval(holdDuration);

        // Disappear
        seq.Append(_readyText.transform.DOScale(1.3f, disappearDuration));
        seq.Join(_readyText.DOFade(0, disappearDuration));
    }

    void SpawnLightnings()
    {
        for (int i = 0; i < style2.lightningCount; i++)
        {
            GameObject bolt = new GameObject("Lightning");
            bolt.transform.SetParent(_readyText.transform, false);

            Image img = bolt.AddComponent<Image>();
            img.sprite = CreateLightningSprite();
            img.color = Color.white;
            img.raycastTarget = false;

            RectTransform rt = img.rectTransform;
            rt.sizeDelta = style2.lightningSize;

            float x = Random.Range(-style2.lightningXRange, style2.lightningXRange);
            float y = Random.Range(-style2.lightningYRange, style2.lightningYRange);
            rt.anchoredPosition = new Vector2(x, y);
            rt.rotation = Quaternion.Euler(0, 0, Random.Range(-style2.lightningAngleRange, style2.lightningAngleRange));

            img.DOFade(0, style2.lightningDuration)
                .OnComplete(() => Destroy(bolt));
        }
    }

    #endregion
}