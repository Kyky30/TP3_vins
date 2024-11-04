using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainRaycaster : MonoBehaviour
{
    public LayerMask terrainLayer; // Layer du terrain
    private Camera camera;
    private List<TerrainGenerator> terrainGenerators;

    private void Start()
    {
        camera = GetComponent<Camera>();
        UpdateTerrainGeneratorsList();
    }

    void Update()
    {
        // Mettre � jour la liste des g�n�rateurs de terrain � chaque mise � jour du cadre
        UpdateTerrainGeneratorsList();

        // Au clic gauche, �l�vation
        if (Input.GetMouseButton(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Terrain"); // Masque de collision pour le terrain
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                // V�rifier que le terrain touch� est bien g�r� par un script TerrainGenerator
                TerrainGenerator terrain = hit.collider.GetComponent<TerrainGenerator>();
                if (terrain != null && terrainGenerators.Contains(terrain))
                {
                    terrain.hitPoint = hit.point;
                    terrain.ModifyTerrain(hit, terrain.deformationIntensity);
                }
            }
        }
        // Au CTRL-Click gauche, d�pression
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Terrain"); // Masque de collision pour le terrain
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                // V�rifier que le terrain touch� est bien g�r� par un script TerrainGenerator
                TerrainGenerator terrain = hit.collider.GetComponent<TerrainGenerator>();
                if (terrain != null && terrainGenerators.Contains(terrain))
                {
                    terrain.hitPoint = hit.point;
                    terrain.ModifyTerrain(hit, -terrain.deformationIntensity);
                }
            }
        }
    }

    // Mettre � jour la liste des g�n�rateurs de terrain
    private void UpdateTerrainGeneratorsList()
    {
        terrainGenerators = new List<TerrainGenerator>(FindObjectsOfType<TerrainGenerator>());
    }
}
