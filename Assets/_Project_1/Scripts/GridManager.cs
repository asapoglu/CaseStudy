using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Range(3, 10)]
    public int gridSize = 3; // Editörde düzenlenebilir n değeri
    
    public GameObject cellPrefab; // Grid hücresi için kullanılacak prefab
    public float cellSize = 1f; // Hücre boyutu
    public float spacing = 0.1f; // Hücreler arası boşluk
    
    private Cell[,] grid; // Grid veri yapısı
    private Transform gridContainer; // Grid hücrelerini içeren parent obje
    
    void Start()
    {
        // Grid container oluştur
        gridContainer = new GameObject("GridContainer").transform;
        gridContainer.SetParent(transform);
        gridContainer.localPosition = Vector3.zero;
        
        CreateGrid();
    }
    
    void CreateGrid()
    {
        // Grid veri yapısını oluştur
        grid = new Cell[gridSize, gridSize];
        
        // Grid'in toplam boyutunu hesapla
        float totalSize = gridSize * (cellSize + spacing) - spacing;
        
        // Grid'in başlangıç pozisyonunu hesapla (merkez noktası için)
        float startPos = -totalSize / 2 + cellSize / 2;
        
        // Grid'i oluştur
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Hücre pozisyonunu hesapla
                Vector3 position = new Vector3(
                    startPos + x * (cellSize + spacing),
                    startPos + y * (cellSize + spacing),
                    0f
                );
                
                // Hücreyi oluştur
                GameObject cellObject = Instantiate(cellPrefab, position, Quaternion.identity, gridContainer);
                cellObject.name = $"Cell_{x}_{y}";
                
                // Hücre boyutunu ayarla
                cellObject.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                
                // Hücre bileşenini al ve ayarla
                Cell cell = cellObject.GetComponent<Cell>();
                cell.Initialize(x, y, this);
                
                // Grid veri yapısına ekle
                grid[x, y] = cell;
            }
        }
        
        // Kamera pozisyonunu ayarla
        AdjustCamera();
    }
    
    void AdjustCamera()
    {
        // Grid'in toplam boyutunu hesapla
        float totalSize = gridSize * (cellSize + spacing) - spacing;
        
        // Kamerayı ayarla
        Camera.main.orthographicSize = totalSize / 1.8f;
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }
    
    // Bir hücreye X işareti konulduğunda çağrılır
    public void OnCellMarked(int x, int y)
    {
        // Yatay ve dikey kontrol yapıp, 3 veya daha fazla komşu X var mı kontrol et
        CheckAndRemoveLines(x, y);
    }
    
    // Yatay ve dikey komşu X'leri kontrol eder ve uygun durumda siler
    void CheckAndRemoveLines(int x, int y)
    {
        // Yatay kontrol
        List<Cell> horizontalMatches = CheckLine(x, y, 1, 0);
        // Dikey kontrol
        List<Cell> verticalMatches = CheckLine(x, y, 0, 1);
        
        // Eğer yatayda 3 veya daha fazla X varsa sil
        if (horizontalMatches.Count >= 3)
        {
            foreach (Cell cell in horizontalMatches)
            {
                cell.RemoveMark();
            }
        }
        
        // Eğer dikeyde 3 veya daha fazla X varsa sil
        if (verticalMatches.Count >= 3)
        {
            foreach (Cell cell in verticalMatches)
            {
                cell.RemoveMark();
            }
        }
    }
    
    // Belirli bir yönde komşu X'leri kontrol eder
    List<Cell> CheckLine(int startX, int startY, int dx, int dy)
    {
        List<Cell> matches = new List<Cell>();
        
        // Pozitif yönde kontrol
        int x = startX;
        int y = startY;
        
        while (x >= 0 && x < gridSize && y >= 0 && y < gridSize && grid[x, y].IsMarked())
        {
            matches.Add(grid[x, y]);
            x += dx;
            y += dy;
        }
        
        // Negatif yönde kontrol (başlangıç hücresini tekrar eklememek için -1 ile başla)
        x = startX - dx;
        y = startY - dy;
        
        while (x >= 0 && x < gridSize && y >= 0 && y < gridSize && grid[x, y].IsMarked())
        {
            matches.Add(grid[x, y]);
            x -= dx;
            y -= dy;
        }
        
        return matches;
    }
}
