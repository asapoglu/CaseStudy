using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMark : MonoBehaviour
{
    void Start()
    {
        // X işareti oluşturulduğunda animasyon eklenebilir
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUp());
    }
    
    IEnumerator ScaleUp()
    {
        float duration = 0.2f;
        float elapsed = 0;
        Vector3 targetScale = new Vector3(0.8f, 0.8f, 0.8f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}