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
        // Mettre à jour la liste des générateurs de terrain à chaque mise à jour du cadre
        UpdateTerrainGeneratorsList();

        // Au clic gauche, élévation
        if (Input.GetMouseButton(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Terrain"); // Masque de collision pour le terrain
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                // Vérifier que le terrain touché est bien géré par un script TerrainGenerator
                TerrainGenerator terrain = hit.collider.GetComponent<TerrainGenerator>();
                if (terrain != null && terrainGenerators.Contains(terrain))
                {
                    terrain.hitPoint = hit.point;
                    terrain.ModifyTerrain(hit, terrain.deformationIntensity);
                }
            }
        }
        // Au CTRL-Click gauche, dépression
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Terrain"); // Masque de collision pour le terrain
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                // Vérifier que le terrain touché est bien géré par un script TerrainGenerator
                TerrainGenerator terrain = hit.collider.GetComponent<TerrainGenerator>();
                if (terrain != null && terrainGenerators.Contains(terrain))
                {
                    terrain.hitPoint = hit.point;
                    terrain.ModifyTerrain(hit, -terrain.deformationIntensity);
                }
            }
        }
    }

    // Mettre à jour la liste des générateurs de terrain
    private void UpdateTerrainGeneratorsList()
    {
        terrainGenerators = new List<TerrainGenerator>(FindObjectsOfType<TerrainGenerator>());
    }
}
