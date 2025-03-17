using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abdurrahman.Project_1.Core
{
    public class GridManager : MonoBehaviour
    {

        public GameObject cellPrefab; // Grid hücresi için kullanılacak prefab
        public float cellSize = 1f; // Hücre boyutu
        public float spacing = 0.1f; // Hücreler arası boşluk
        [Range(3, 15)] public int gridSize = 3; // Editörde düzenlenebilir n değeri
        public Slider gridSizeSlider; // Editördeki slider

        private Cell[,] grid; // Grid veri yapısı
        private Transform gridContainer; // Grid hücrelerini içeren parent obje

        void Start()
        {
            // Grid container oluştur
            gridContainer = new GameObject("GridContainer").transform;
            gridContainer.SetParent(transform);
            gridContainer.localPosition = Vector3.zero;
            gridSizeSlider.onValueChanged.AddListener(SliderValueChanged);
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
                    cellObject.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                    // Hücre bileşenini al ve ayarla
                    Cell cell = cellObject.GetComponent<Cell>();
                    cell.Initialize(x, y, this);

                    // Grid veri yapısına ekle
                    grid[x, y] = cell;
                }
            }
            gridSizeSlider.SetValueWithoutNotify(gridSize);
            // Kamera pozisyonunu ayarla
            AdjustCamera();
        }

        void AdjustCamera()
        {
            // Grid'in toplam boyutunu hesapla
            float totalSize = gridSize * (cellSize + spacing) - spacing;

            // Kamerayı ayarla
            Camera.main.orthographicSize = totalSize * 1.2f;
        }

        // Bir hücreye X işareti konulduğunda çağrılır
        public void OnCellMarked(int x, int y)
        {
            // Sadece işaretlenen hücreden başlayarak bağlantılı X'leri kontrol et
            // Bu şekilde birbirine bağlı olmayan grupları ayrı ayrı ele alırız
            CheckAndRemoveConnectedXs(x, y);
        }

        void SliderValueChanged(float value)
        {
            gridSize = (int)value;
            CancelInvoke("RebuildGrid");
            Invoke("RebuildGrid", 0.01f);
        }

        // Verilen koordinattan başlayarak bağlantılı X gruplarını bulur ve kontrol eder
        void CheckAndRemoveConnectedXs(int startX, int startY)
        {
            // İşaretli hücreleri takip etmek için bir ziyaret edildi matrisi oluştur
            bool[,] visited = new bool[gridSize, gridSize];

            // Başlangıç hücresinden bağlantılı grup bul
            List<Cell> connectedGroup = new List<Cell>();
            FindConnectedXs(startX, startY, connectedGroup, visited);

            // Eğer grup 3 veya daha fazla X içeriyorsa
            if (connectedGroup.Count >= 3)
            {
                // Gruptaki tüm X'leri sil
                foreach (Cell cell in connectedGroup)
                {
                    cell.RemoveMark();
                }
            }
        }

        // Depth-First Search kullanarak bağlantılı X'leri bulur
        void FindConnectedXs(int x, int y, List<Cell> group, bool[,] visited)
        {
            // Sınırları kontrol et
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
                return;

            // Ziyaret edilmiş veya işaretlenmemiş hücreyi atla
            if (visited[x, y] || !grid[x, y].IsMarked())
                return;

            // Hücreyi işaretle ve gruba ekle
            visited[x, y] = true;
            group.Add(grid[x, y]);

            // Dört yöne doğru bağlantıları kontrol et (yukarı, sağ, aşağı, sol)
            FindConnectedXs(x, y + 1, group, visited); // Yukarı
            FindConnectedXs(x + 1, y, group, visited); // Sağ
            FindConnectedXs(x, y - 1, group, visited); // Aşağı
            FindConnectedXs(x - 1, y, group, visited); // Sol
        }

        // Editor'da değişiklik yapıldığında gridi yeniden oluştur
        void OnValidate()
        {
            if (Application.isPlaying && gridContainer != null)
            {
                CancelInvoke("RebuildGrid");
                Invoke("RebuildGrid", 0.01f);
            }
        }

        void RebuildGrid()
        {
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            // Yeni grid oluştur
            CreateGrid();
        }
    }
}