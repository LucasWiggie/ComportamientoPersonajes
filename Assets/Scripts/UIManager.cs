using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI castorCountText;
    public TextMeshProUGUI patoCountText;
    public TextMeshProUGUI cocodriloCountText;
    public TextMeshProUGUI salamandraCountText;
    public TextMeshProUGUI palosCountText;
    public TextMeshProUGUI moscasCountText;

    private int castorCount = 0;
    private int patoCount = 0;
    private int cocodriloCount = 0;
    private int salamandraCount = 0;
    private int palosCount = 0;
    private int moscasCount = 0;

    public GeneracionAleatoria generacionAleatoriaScript;

    void Start()
    {
        UpdateUI();
    }

    public void IncrementCastor()
    {
        castorCount++;
        UpdateUI();
    }

    public void DecrementCastor()
    {
        if (castorCount > 0) castorCount--;
        UpdateUI();
    }

    public void IncrementPato()
    {
        patoCount++;
        UpdateUI();
    }

    public void DecrementPato()
    {
        if (patoCount > 0) patoCount--;
        UpdateUI();
    }

    public void IncrementCocodrilo()
    {
        cocodriloCount++;
        UpdateUI();
    }

    public void DecrementCocodrilo()
    {
        if (cocodriloCount > 0) cocodriloCount--;
        UpdateUI();
    }

    public void IncrementSalamandra()
    {
        salamandraCount++;
        UpdateUI();
    }

    public void DecrementSalamandra()
    {
        if (salamandraCount > 0) salamandraCount--;
        UpdateUI();
    }
    public void IncrementPalos()
    {
        palosCount++;
        UpdateUI();
    }

    public void DecrementPalos()
    {
        if (palosCount > 0) palosCount--;
        UpdateUI();
    }

    public void IncrementMoscas()
    {
        moscasCount++;
        UpdateUI();
    }

    public void DecrementMoscas()
    {
        if (moscasCount > 0) moscasCount--;
        UpdateUI();
    }


    public void StartGeneration()
    {
        generacionAleatoriaScript.nCastores = castorCount;
        generacionAleatoriaScript.nPatos = patoCount;
        generacionAleatoriaScript.nCocodrilos = cocodriloCount;
        generacionAleatoriaScript.nSalamandras = salamandraCount;
        generacionAleatoriaScript.nPalos = palosCount;
        generacionAleatoriaScript.nMoscas = moscasCount;
        generacionAleatoriaScript.GenerateAnimals();
    }

    public void ResetCounters()
    {
        castorCount = 0;
        patoCount = 0;
        cocodriloCount = 0;
        salamandraCount = 0;
        palosCount = 0;
        moscasCount = 0;
        UpdateUI();
        generacionAleatoriaScript.ClearAnimals();
    }

    private void UpdateUI()
    {
        castorCountText.text = castorCount.ToString();
        patoCountText.text = patoCount.ToString();
        cocodriloCountText.text = cocodriloCount.ToString();
        salamandraCountText.text = salamandraCount.ToString();
        palosCountText.text = palosCount.ToString();
        moscasCountText.text = moscasCount.ToString();
    }
}
