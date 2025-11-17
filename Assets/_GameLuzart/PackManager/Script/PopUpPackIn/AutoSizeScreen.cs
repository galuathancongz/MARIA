using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSizeScreen : MonoBehaviour
{
    [SerializeField]
    private Canvas _canvas; // Reference to the Canvas component
    private Canvas canvas
    {
        get
        {
            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }
            return _canvas;
        }
    }
    private void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float screen = Screen.width / canvas.scaleFactor;
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(screen, rectTransform.sizeDelta.y);
        }
    }
}
