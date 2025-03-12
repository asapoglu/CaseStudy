using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMark : MonoBehaviour
{
    public float targetScale = 0.8f;
    
    void Start()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUp());
    }
    
    IEnumerator ScaleUp()
    {
        float duration = 0.2f;
        float elapsed = 0;
        Vector3 targetScaleVector = new Vector3(targetScale, targetScale, 1f);
        
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color startColor = renderer.color;
            startColor.a = 0f;
            renderer.color = startColor;
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScaleVector, t);
            
            if (renderer != null)
            {
                Color currentColor = renderer.color;
                currentColor.a = t;
                renderer.color = currentColor;
            }
            
            yield return null;
        }
        
        transform.localScale = targetScaleVector;
        
        if (renderer != null)
        {
            Color finalColor = renderer.color;
            finalColor.a = 1f;
            renderer.color = finalColor;
        }
    }
}