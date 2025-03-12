using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject xMarkPrefab; 

    private int x, y; 
    private GridManager gridManager;
    private bool isMarked = false;
    private GameObject xMark = null;

    public void Initialize(int xPos, int yPos, GridManager manager)
    {
        x = xPos;
        y = yPos;
        gridManager = manager;
        isMarked = false;
    }

    // Tıklama işlemi için
    void OnMouseDown()
    {
        if (!isMarked)
        {
            MarkCell();

            // Grid manager'a bildir
            gridManager.OnCellMarked(x, y);
        }
    }

    // X işareti koy
    void MarkCell()
    {
        if (xMarkPrefab != null)
        {
            xMark = Instantiate(xMarkPrefab, transform.position, Quaternion.identity);
            xMark.transform.SetParent(transform);
            xMark.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); // X işareti boyutu
        }
        isMarked = true;
    }

    // X işaretini kaldır
    public void RemoveMark()
    {
        if (xMark != null)
        {
            Destroy(xMark);
        }

        isMarked = false;
    }

    // Hücrenin işaretli olup olmadığını kontrol et
    public bool IsMarked()
    {
        return isMarked;
    }
}