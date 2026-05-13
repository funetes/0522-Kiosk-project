using UnityEngine;
using UnityEngine.UI;

public static class CafeKioskUIUtility
{
    public static RectTransform Panel(string name, Transform parent, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        var image = panel.GetComponent<Image>();
        image.color = color;
        return rect;
    }

    public static Text Label(string text, Transform parent, int size, Color color, FontStyle style, TextAnchor alignment, Font font)
    {
        var label = new GameObject($"{text} Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(parent, false);
        var rect = label.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, size + 12f);

        var uiText = label.GetComponent<Text>();
        uiText.text = text;
        uiText.font = font;
        uiText.fontSize = size;
        uiText.fontStyle = style;
        uiText.color = color;
        uiText.alignment = alignment;
        uiText.supportRichText = false;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        return uiText;
    }

    public static Button Button(string text, Transform parent, int size, Color background, Color foreground, UnityEngine.Events.UnityAction action, Font font, float width = 0f, float height = 46f, float anchorX = -1f)
    {
        var buttonRect = Panel($"{text} Button", parent, background);
        buttonRect.sizeDelta = new Vector2(width, height);

        var button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonRect.GetComponent<Image>();
        button.onClick.AddListener(action);

        var label = Label(text, buttonRect, size, foreground, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        Stretch(label.rectTransform);

        if (anchorX >= 0f)
        {
            Anchor(buttonRect, anchorX, 0.22f, anchorX, 0.78f, -width * 0.5f, 0f, width * 0.5f, 0f);
        }

        return button;
    }

    public static InputField Input(string placeholder, Transform parent, Color charcoal, Font font)
    {
        var inputRect = Panel("Phone Input", parent, Color.white);
        inputRect.sizeDelta = new Vector2(0f, 48f);

        var input = inputRect.gameObject.AddComponent<InputField>();
        input.contentType = InputField.ContentType.IntegerNumber;
        input.characterLimit = 13;
        input.targetGraphic = inputRect.GetComponent<Image>();

        var text = Label("", inputRect, 21, charcoal, FontStyle.Normal, TextAnchor.MiddleLeft, font);
        Anchor(text.rectTransform, 0f, 0f, 1f, 1f, 14f, 0f, -14f, 0f);

        var placeholderText = Label(placeholder, inputRect, 19, new Color(0.62f, 0.58f, 0.52f), FontStyle.Normal, TextAnchor.MiddleLeft, font);
        Anchor(placeholderText.rectTransform, 0f, 0f, 1f, 1f, 14f, 0f, -14f, 0f);

        input.textComponent = text;
        input.placeholder = placeholderText;
        return input;
    }

    public static (RectTransform viewport, RectTransform content) ScrollArea(string name, Transform parent)
    {
        var viewportObject = new GameObject($"{name} Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportObject.transform.SetParent(parent, false);
        var viewport = viewportObject.GetComponent<RectTransform>();

        var contentObject = new GameObject($"{name} Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewport, false);
        var content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return (viewport, content);
    }

    public static void AddHorizontalLayout(RectTransform rect, float spacing, TextAnchor alignment)
    {
        var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        layout.childAlignment = alignment;
    }

    public static void AddVerticalLayout(RectTransform rect, float spacing, TextAnchor alignment)
    {
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = alignment;

        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public static void AddGrid(RectTransform rect, Vector2 cellSize, Vector2 spacing, RectOffset padding)
    {
        var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.padding = padding;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public static void Anchor(RectTransform rect, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, top);
    }

    public static void Stretch(RectTransform rect)
    {
        Anchor(rect, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
    }

    public static void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyGeneratedObject(parent.GetChild(i).gameObject);
        }
    }

    public static void DestroyGeneratedObject(Object target)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }

    public static void DestroyComponent(Component target)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }
}
