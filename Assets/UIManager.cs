using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject keysPanel;
    public GameObject optimizationsPanel;
    public GameObject deformationParametersPanel;
    public GameObject statsPanel;

    private bool keysPanelActive = false;
    private bool optimizationsPanelActive = false;
    private bool deformationParametersPanelActive = false;
    private bool statsPanelActive = false;

    void Update()
    {
        // Touche F1 pour afficher/masquer le panel des touches disponibles
        if (Input.GetKeyDown(KeyCode.F1))
        {
            keysPanelActive = !keysPanelActive;
            keysPanel.SetActive(keysPanelActive);
        }

        // Touche F2 pour afficher/masquer le panel des optimisations en cours
        if (Input.GetKeyDown(KeyCode.F2))
        {
            optimizationsPanelActive = !optimizationsPanelActive;
            optimizationsPanel.SetActive(optimizationsPanelActive);
        }

        // Touche F3 pour afficher/masquer le panel des paramètres de déformation
        if (Input.GetKeyDown(KeyCode.F3))
        {
            deformationParametersPanelActive = !deformationParametersPanelActive;
            deformationParametersPanel.SetActive(deformationParametersPanelActive);
        }

        // Touche F10 pour afficher/masquer le panel des statistiques
        if (Input.GetKeyDown(KeyCode.F10))
        {
            statsPanelActive = !statsPanelActive;
            statsPanel.SetActive(statsPanelActive);
        }
    }
}
