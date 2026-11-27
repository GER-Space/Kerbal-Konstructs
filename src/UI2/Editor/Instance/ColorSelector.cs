using KerbalKonstructs.Core;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Color picker of the editors, laid out like paint.net's: a hue / saturation wheel, the new and the original color,
    /// and RGB, HSV and alpha bars with editable values (plus hex). Colors brighter than 1 (KK's grass colors can be
    /// multipliers) are shown as a 0..1 color times an intensity.
    /// Usage: set selectedColor and callBack, then Open(). callBack gets every change; Cancel restores the original color.
    /// </summary>
    internal static class ColorSelector
    {
        /// <summary>The color to edit (set before Open), then the current color</summary>
        internal static Color selectedColor = Color.white;

        /// <summary>Called with every change of the color</summary>
        internal static Action<Color> callBack = delegate { };

        /// <summary>The highest intensity (factor of the 0..1 color)</summary>
        internal const float maxIntensity = 8f;

        private static PickerWindow _window = null;
        private static PickerWindow window
        {
            get
            {
                if (_window == null)
                {
                    _window = new PickerWindow();
                }
                return _window;
            }
        }


        internal static void Open()
        {
            window.OpenWith(selectedColor);
        }

        internal static void Close()
        {
            if (_window != null)
            {
                _window.Close();
            }
        }

        internal static bool IsOpen()
        {
            return _window != null && _window.IsOpen();
        }

        internal static bool isOpen => IsOpen();

        internal static void Toggle()
        {
            if (IsOpen())
            {
                Close();
            }
            else
            {
                Open();
            }
        }


        private class PickerWindow : KKEditorWindow
        {
            protected override string WindowName => "ColorSelector";
            protected override string WindowTitle => "Color";
            protected override float WindowWidth => 500f;
            protected override Vector2 DefaultPosition => new Vector2(0.45f, 0.5f);

            private const int wheelSize = 180;
            private const float barWidth = 150f;
            private const float barHeight = 12f;
            private const int barResolution = 128;
            private const float rowLabelWidth = 26f;
            private const float valueWidth = 64f;

            // the color: hue, saturation, value (0..1) of the 0..1 color, its alpha and the intensity factor
            private float hue, saturation, value, alpha, intensity;
            private Color original;

            // bars: R, G, B, H, S, V, A, intensity
            private const int barR = 0, barG = 1, barB = 2, barH = 3, barS = 4, barV = 5, barA = 6, barI = 7, barCount = 8;

            private Texture2D wheelTexture;
            private float wheelTextureValue = -1f;
            private Texture2D[] barTextures = new Texture2D[barCount];
            private Texture2D newSwatch;
            private Texture2D oldSwatch;
            private static Texture2D ringTexture;
            private static Texture2D triangleTexture;

            private DialogGUIImage wheelImage;
            private DialogGUIImage[] barImages = new DialogGUIImage[barCount];
            private RectTransform wheelMarker;
            private RectTransform[] barMarkers = new RectTransform[barCount];


            #region color

            private Color Rgb => Color.HSVToRGB(hue, saturation, value);

            private Color Result
            {
                get
                {
                    Color c = Rgb;
                    return new Color(c.r * intensity, c.g * intensity, c.b * intensity, alpha);
                }
            }


            /// <summary>
            /// Splits a color into the 0..1 color (as HSV) and the intensity. keepHue: a gray keeps the current hue.
            /// </summary>
            private void SetColor(Color color, bool keepHue)
            {
                float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
                intensity = Mathf.Clamp(max, 1f, maxIntensity);
                Color c = new Color(Mathf.Clamp01(color.r / intensity), Mathf.Clamp01(color.g / intensity), Mathf.Clamp01(color.b / intensity));
                SetRgb(c, keepHue);
                alpha = Mathf.Clamp01(color.a);
            }


            private void SetRgb(Color c, bool keepHue)
            {
                float h, s, v;
                Color.RGBToHSV(c, out h, out s, out v);
                if (!keepHue || (s > 0.0001f && v > 0.0001f))
                {
                    hue = h;
                }
                if (!keepHue || v > 0.0001f)
                {
                    saturation = s;
                }
                value = v;
            }


            /// <summary>
            /// After every change: report it and update the pictures
            /// </summary>
            private void Changed()
            {
                selectedColor = Result;
                if (callBack != null)
                {
                    callBack(selectedColor);
                }
                Refresh();
            }


            private void SetChannel(int channel, float channelValue)
            {
                Color c = Rgb;
                c[channel] = Mathf.Clamp01(channelValue);
                SetRgb(c, true);
                Changed();
            }

            private void SetHue(float h) { hue = Mathf.Repeat(h, 1f); Changed(); }
            private void SetSaturation(float s) { saturation = Mathf.Clamp01(s); Changed(); }
            private void SetValue(float v) { value = Mathf.Clamp01(v); Changed(); }
            private void SetAlpha(float a) { alpha = Mathf.Clamp01(a); Changed(); }
            private void SetIntensity(float i) { intensity = Mathf.Clamp(i, 1f, maxIntensity); Changed(); }


            private void SetWheel(Vector2 uv)
            {
                float dx = uv.x * 2f - 1f;
                float dy = uv.y * 2f - 1f;
                saturation = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                if (saturation > 0.0001f)
                {
                    hue = Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI), 1f);
                }
                Changed();
            }


            private void SetHex(string text)
            {
                Color c;
                string hex = text.Trim().TrimStart('#');
                if ((hex.Length == 6 || hex.Length == 8) && ColorUtility.TryParseHtmlString("#" + hex, out c))
                {
                    SetRgb(new Color(c.r, c.g, c.b), true);
                    if (hex.Length == 8)
                    {
                        alpha = c.a;
                    }
                    Changed();
                }
            }


            /// <summary>
            /// A whole number field: value * scale shown, typed / scale set
            /// </summary>
            private DialogGUIBase NumberField(Func<float> getValue, Action<float> setValue, float scale)
            {
                return TextField(delegate { return Mathf.RoundToInt(getValue() * scale).ToString(CultureInfo.InvariantCulture); },
                    delegate (string s) { float f; if (TryParseFloat(s, out f)) { setValue(f / scale); } }, 5, valueWidth);
            }

            #endregion


            #region window

            internal void OpenWith(Color color)
            {
                DismissDialog();
                original = color;
                SetColor(color, false);
                CreateTextures();
                Build();
                AttachPickers();
                Refresh();
            }


            protected override void CreateContent()
            {
                // left: the new and the original color, the wheel
                DialogGUIImage originalImage = KKStyle.ClickableImage(oldSwatch, delegate { SetColor(original, false); Changed(); }, 44f, 30f);
                wheelImage = new DialogGUIImage(new Vector2(wheelSize, wheelSize), Vector2.zero, Color.white, wheelTexture);
                DialogGUIVerticalLayout left = new DialogGUIVerticalLayout(wheelSize + 8f, 10f, 6f, new RectOffset(2, 2, 2, 2), TextAnchor.UpperLeft,
                    new DialogGUIHorizontalLayout(false, false, 6f, new RectOffset(), TextAnchor.MiddleLeft,
                        new DialogGUIImage(new Vector2(64f, 44f), Vector2.zero, Color.white, newSwatch),
                        new DialogGUIVerticalLayout(false, false, 0f, new RectOffset(), TextAnchor.MiddleLeft,
                            Label("Original"),
                            originalImage)),
                    new DialogGUISpace(6f),
                    wheelImage);

                // right: RGB, hex, HSV, alpha, intensity
                DialogGUIVerticalLayout right = new DialogGUIVerticalLayout(barWidth + rowLabelWidth + valueWidth + 20f, 10f, 2f, new RectOffset(2, 2, 2, 2), TextAnchor.UpperLeft,
                    GoldLabel(delegate { return "RGB"; }),
                    BarRow("R:", barR, delegate (float f) { SetChannel(0, f); }, NumberField(delegate { return Rgb.r; }, delegate (float f) { SetChannel(0, f); }, 255f)),
                    BarRow("G:", barG, delegate (float f) { SetChannel(1, f); }, NumberField(delegate { return Rgb.g; }, delegate (float f) { SetChannel(1, f); }, 255f)),
                    BarRow("B:", barB, delegate (float f) { SetChannel(2, f); }, NumberField(delegate { return Rgb.b; }, delegate (float f) { SetChannel(2, f); }, 255f)),
                    new DialogGUIHorizontalLayout(false, false, 4f, new RectOffset(), TextAnchor.MiddleLeft,
                        Label("Hex:"), new DialogGUIFlexibleSpace(),
                        TextField(delegate { return ColorUtility.ToHtmlStringRGB(Rgb); }, SetHex, 9, valueWidth + 16f)),
                    new DialogGUISpace(4f),
                    GoldLabel(delegate { return "HSV"; }),
                    BarRow("H:", barH, SetHue, NumberField(delegate { return hue; }, SetHue, 360f)),
                    BarRow("S:", barS, SetSaturation, NumberField(delegate { return saturation; }, SetSaturation, 100f)),
                    BarRow("V:", barV, SetValue, NumberField(delegate { return value; }, SetValue, 100f)),
                    new DialogGUISpace(4f),
                    GoldLabel(delegate { return "Alpha"; }),
                    BarRow("A:", barA, SetAlpha, NumberField(delegate { return alpha; }, SetAlpha, 255f)),
                    new DialogGUISpace(4f),
                    GoldLabel(delegate { return "Intensity (colors brighter than 1)"; }),
                    BarRow("I:", barI, delegate (float f) { SetIntensity(1f + f * (maxIntensity - 1f)); },
                        TextField(delegate { return intensity.ToString("0.00", CultureInfo.InvariantCulture); },
                            delegate (string s) { float f; if (TryParseFloat(s, out f)) { SetIntensity(f); } }, 5, valueWidth)));

                content.Add(new DialogGUIHorizontalLayout(false, false, 10f, new RectOffset(4, 4, 4, 4), TextAnchor.UpperLeft, left, right));
                content.Add(new DialogGUIHorizontalLayout(
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton("OK", delegate { Close(); }, 90f, itemHeight, false),
                    new DialogGUIButton("Cancel", delegate { SetColor(original, false); Changed(); Close(); }, 90f, itemHeight, false)));
            }


            /// <summary>
            /// label [gradient bar] [value]: clicking or dragging on the bar sets its value (0..1 from left to right)
            /// </summary>
            private DialogGUIBase BarRow(string label, int bar, Action<float> setFromBar, DialogGUIBase valueField)
            {
                barImages[bar] = new DialogGUIImage(new Vector2(barWidth, barHeight), Vector2.zero, Color.white, barTextures[bar]);
                barSetters[bar] = setFromBar;
                return new DialogGUIHorizontalLayout(false, false, 6f, new RectOffset(0, 0, 2, 6), TextAnchor.MiddleLeft,
                    new DialogGUIHorizontalLayout(rowLabelWidth, itemHeight, 0f, new RectOffset(), TextAnchor.MiddleLeft, Label(label)),
                    barImages[bar],
                    valueField);
            }

            private Action<float>[] barSetters = new Action<float>[barCount];


            /// <summary>
            /// After the dialog was built: pointer handling and markers on the wheel and the bars (added after
            /// KKWindowDrag.Attach, so dragging on them doesn't move the window)
            /// </summary>
            private void AttachPickers()
            {
                if (wheelImage != null && wheelImage.uiItem != null)
                {
                    KKColorArea.Attach(wheelImage.uiItem, SetWheel);
                    wheelMarker = AddMarker(wheelImage.uiItem, RingTexture, new Vector2(13f, 13f), new Vector2(0.5f, 0.5f));
                }
                for (int i = 0; i < barCount; i++)
                {
                    if (barImages[i] == null || barImages[i].uiItem == null)
                    {
                        continue;
                    }
                    Action<float> setter = barSetters[i];
                    KKColorArea.Attach(barImages[i].uiItem, delegate (Vector2 uv) { setter(Mathf.Clamp01(uv.x)); });
                    barMarkers[i] = AddMarker(barImages[i].uiItem, TriangleTexture, new Vector2(10f, 6f), new Vector2(0.5f, 1f));
                }
            }


            private static RectTransform AddMarker(GameObject parent, Texture2D texture, Vector2 size, Vector2 pivot)
            {
                GameObject marker = new GameObject("KKColorMarker", typeof(RectTransform));
                marker.layer = parent.layer;
                marker.transform.SetParent(parent.transform, false);
                LayoutElement layout = marker.AddComponent<LayoutElement>();
                layout.ignoreLayout = true;
                RawImage image = marker.AddComponent<RawImage>();
                image.texture = texture;
                image.raycastTarget = false;
                RectTransform rect = marker.GetComponent<RectTransform>();
                rect.sizeDelta = size;
                rect.pivot = pivot;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                return rect;
            }

            #endregion


            #region pictures

            private void CreateTextures()
            {
                if (wheelTexture == null)
                {
                    wheelTexture = new Texture2D(wheelSize, wheelSize, TextureFormat.RGBA32, false);
                    wheelTexture.wrapMode = TextureWrapMode.Clamp;
                    wheelTextureValue = -1f;
                }
                for (int i = 0; i < barCount; i++)
                {
                    if (barTextures[i] == null)
                    {
                        barTextures[i] = new Texture2D(barResolution, (i == barA) ? 8 : 1, TextureFormat.RGBA32, false);
                        barTextures[i].wrapMode = TextureWrapMode.Clamp;
                    }
                }
                if (newSwatch == null)
                {
                    newSwatch = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    oldSwatch = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                }
                // the hue bar and the intensity bar don't change
                Color[] pixels = new Color[barResolution];
                for (int x = 0; x < barResolution; x++)
                {
                    float f = x / (barResolution - 1f);
                    pixels[x] = Color.HSVToRGB(f, 1f, 1f);
                }
                barTextures[barH].SetPixels(pixels);
                barTextures[barH].Apply(false);
                for (int x = 0; x < barResolution; x++)
                {
                    float f = Mathf.Lerp(0.25f, 1f, x / (barResolution - 1f));
                    pixels[x] = new Color(f, f, f, 1f);
                }
                barTextures[barI].SetPixels(pixels);
                barTextures[barI].Apply(false);
                Fill(oldSwatch, VisibleColor(original));
            }


            /// <summary>
            /// Updates the pictures and markers to the current color
            /// </summary>
            private void Refresh()
            {
                Color rgb = Rgb;
                if (!Mathf.Approximately(wheelTextureValue, value))
                {
                    DrawWheel();
                }
                DrawBar(barR, delegate (float f) { return new Color(f, rgb.g, rgb.b); });
                DrawBar(barG, delegate (float f) { return new Color(rgb.r, f, rgb.b); });
                DrawBar(barB, delegate (float f) { return new Color(rgb.r, rgb.g, f); });
                DrawBar(barS, delegate (float f) { return Color.HSVToRGB(hue, f, value); });
                DrawBar(barV, delegate (float f) { return Color.HSVToRGB(hue, saturation, f); });
                DrawAlphaBar(rgb);
                Fill(newSwatch, rgb);

                if (wheelMarker != null)
                {
                    float angle = hue * 2f * Mathf.PI;
                    SetAnchor(wheelMarker, new Vector2(0.5f + 0.5f * saturation * Mathf.Cos(angle), 0.5f + 0.5f * saturation * Mathf.Sin(angle)));
                }
                SetBarMarker(barR, rgb.r);
                SetBarMarker(barG, rgb.g);
                SetBarMarker(barB, rgb.b);
                SetBarMarker(barH, hue);
                SetBarMarker(barS, saturation);
                SetBarMarker(barV, value);
                SetBarMarker(barA, alpha);
                SetBarMarker(barI, (intensity - 1f) / (maxIntensity - 1f));
            }


            private void SetBarMarker(int bar, float position)
            {
                if (barMarkers[bar] != null)
                {
                    SetAnchor(barMarkers[bar], new Vector2(Mathf.Clamp01(position), 0f));
                }
            }

            private static void SetAnchor(RectTransform rect, Vector2 anchor)
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.anchoredPosition = Vector2.zero;
            }


            /// <summary>
            /// The hue (angle, red at the right, counterclockwise) / saturation (radius) wheel at the current value
            /// </summary>
            private void DrawWheel()
            {
                wheelTextureValue = value;
                Color32[] pixels = new Color32[wheelSize * wheelSize];
                float radius = wheelSize * 0.5f;
                for (int y = 0; y < wheelSize; y++)
                {
                    float dy = (y + 0.5f - radius) / radius;
                    for (int x = 0; x < wheelSize; x++)
                    {
                        float dx = (x + 0.5f - radius) / radius;
                        float r = Mathf.Sqrt(dx * dx + dy * dy);
                        // a smooth edge of about one pixel
                        float edge = Mathf.Clamp01((1f - r) * radius + 0.5f);
                        if (edge <= 0f)
                        {
                            pixels[y * wheelSize + x] = new Color32(0, 0, 0, 0);
                            continue;
                        }
                        Color c = Color.HSVToRGB(Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI), 1f), Mathf.Min(r, 1f), value);
                        c.a = edge;
                        pixels[y * wheelSize + x] = c;
                    }
                }
                wheelTexture.SetPixels32(pixels);
                wheelTexture.Apply(false);
            }


            private void DrawBar(int bar, Func<float, Color> colorAt)
            {
                Texture2D texture = barTextures[bar];
                Color[] pixels = new Color[barResolution];
                for (int x = 0; x < barResolution; x++)
                {
                    Color c = colorAt(x / (barResolution - 1f));
                    c.a = 1f;
                    pixels[x] = c;
                }
                texture.SetPixels(pixels);
                texture.Apply(false);
            }


            /// <summary>
            /// The alpha bar: from a checkerboard (transparent) to the color
            /// </summary>
            private void DrawAlphaBar(Color rgb)
            {
                Texture2D texture = barTextures[barA];
                int height = texture.height;
                Color[] pixels = new Color[barResolution * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < barResolution; x++)
                    {
                        float f = x / (barResolution - 1f);
                        float check = (((x / 4) + (y / 4)) % 2 == 0) ? 0.8f : 0.55f;
                        pixels[y * barResolution + x] = Color.Lerp(new Color(check, check, check), new Color(rgb.r, rgb.g, rgb.b), f);
                    }
                }
                texture.SetPixels(pixels);
                texture.Apply(false);
            }


            private static void Fill(Texture2D texture, Color color)
            {
                color.a = 1f;
                texture.SetPixel(0, 0, color);
                texture.Apply(false);
            }

            /// <summary>
            /// A color brighter than 1 as it can be shown
            /// </summary>
            private static Color VisibleColor(Color color)
            {
                float max = Mathf.Max(1f, Mathf.Max(color.r, Mathf.Max(color.g, color.b)));
                return new Color(color.r / max, color.g / max, color.b / max, 1f);
            }


            private static Texture2D RingTexture
            {
                get
                {
                    if (ringTexture == null)
                    {
                        const int size = 13;
                        ringTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                        float center = (size - 1) * 0.5f;
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                float r = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                                Color c = new Color(0, 0, 0, 0);
                                if (r >= 4f && r < 5f)
                                {
                                    c = Color.white;
                                }
                                else if (r >= 3f && r < 6f)
                                {
                                    c = Color.black;
                                }
                                ringTexture.SetPixel(x, y, c);
                            }
                        }
                        ringTexture.Apply(false);
                    }
                    return ringTexture;
                }
            }


            private static Texture2D TriangleTexture
            {
                get
                {
                    if (triangleTexture == null)
                    {
                        const int width = 11;
                        const int height = 6;
                        triangleTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                        for (int y = 0; y < height; y++)
                        {
                            // pointing up: the top row (y = height - 1) is the tip
                            int halfWidth = height - 1 - y;
                            for (int x = 0; x < width; x++)
                            {
                                bool inside = Mathf.Abs(x - width / 2) <= halfWidth;
                                triangleTexture.SetPixel(x, y, inside ? Color.white : new Color(0, 0, 0, 0));
                            }
                        }
                        triangleTexture.Apply(false);
                    }
                    return triangleTexture;
                }
            }

            #endregion
        }
    }


    /// <summary>
    /// Reports where a picture is clicked or dragged, as 0..1 in the picture (from the bottom left)
    /// </summary>
    internal class KKColorArea : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
    {
        private Action<Vector2> onValue;

        internal static void Attach(GameObject target, Action<Vector2> onValue)
        {
            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = true;
            }
            KKColorArea area = target.GetComponent<KKColorArea>();
            if (area == null)
            {
                area = target.AddComponent<KKColorArea>();
            }
            area.onValue = onValue;
        }

        public void OnPointerDown(PointerEventData eventData) { Report(eventData); }
        public void OnInitializePotentialDrag(PointerEventData eventData) { eventData.useDragThreshold = false; }
        public void OnBeginDrag(PointerEventData eventData) { Report(eventData); }
        public void OnDrag(PointerEventData eventData) { Report(eventData); }

        private void Report(PointerEventData eventData)
        {
            RectTransform rect = transform as RectTransform;
            Vector2 local;
            if (rect == null || onValue == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out local))
            {
                return;
            }
            Rect r = rect.rect;
            onValue(new Vector2((local.x - r.x) / r.width, (local.y - r.y) / r.height));
        }
    }
}
