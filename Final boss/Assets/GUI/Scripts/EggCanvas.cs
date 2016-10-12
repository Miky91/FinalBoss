using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EggCanvas : MonoBehaviour
{

    private Text m_myEvoPointsText;
    private Text m_eggEvoPointsText;

    private int m_myEvoPointsInt;
    private int m_eggEvoPointsInt;

    private EvolutionManager m_evolutionManager;

    private GUIManager m_GUIManager;

    private GameObject eggIsActive;

    private bool givePoints = false;
    private bool takePoints = false;

    // Use this for initialization
    void Awake()
    {
        m_GUIManager = GameObject.FindGameObjectWithTag("GUIManager").GetComponent<GUIManager>();

        m_myEvoPointsText = gameObject.transform.Find("myEvoPoints").GetComponent<Text>();
        m_eggEvoPointsText = gameObject.transform.Find("eggEvoPoints").GetComponent<Text>();
        m_evolutionManager = GameObject.FindGameObjectWithTag("EvolutionManager").GetComponent<EvolutionManager>();
        eggIsActive = gameObject.transform.FindChild("eggIsActive").gameObject;


    }

    void Update()
    {
        if (givePoints)
            givePointToEgg();
        else if (takePoints)
            takePointFromEgg();
    }

    void OnEnable()
    {
        m_myEvoPointsInt = m_evolutionManager.getEvoPoints();
        
        m_eggEvoPointsInt = 0;

        //si ya hay huevo mostrarlo
        if (m_evolutionManager.isEggActive())
        {
            eggIsActive.SetActive(true);
            eggIsActive.GetComponent<Text>().text = "You already have an egg with " + m_evolutionManager.getEggPoints();            
        }
        else
            gameObject.transform.FindChild("eggIsActive").gameObject.SetActive(false);

        updateGUI();
    }

    private void updateGUI()
    {
        m_eggEvoPointsText.text = "" + m_eggEvoPointsInt;
        m_myEvoPointsText.text = "" + m_myEvoPointsInt;

    }
    public void givePointToEgg()
    {
        
        if (m_myEvoPointsInt > 0)
        {
            m_eggEvoPointsInt += 1;
            m_myEvoPointsInt -= 1;
            updateGUI();
        }
    }


    public void takePointFromEgg()
    {
        if (m_eggEvoPointsInt > 0)
        {
            m_eggEvoPointsInt -= 1;
            m_myEvoPointsInt += 1;
            updateGUI();
        }     
    }

    public void cancel()
    {
        m_GUIManager.toggleEggCanvas();        
    }

    public void accept()
    {

        if (m_eggEvoPointsInt != 0)
            m_evolutionManager.saveEgg(m_eggEvoPointsInt);
        
        if (!m_GUIManager)
            m_GUIManager = GameObject.FindGameObjectWithTag("GUIManager").GetComponent<GUIManager>();

        m_GUIManager.toggleEggCanvas();
    }
   
    public void activateGivePoints()
    {
        givePoints = true;
    }

    public void deactivateGivePoints()
    {
        givePoints = false;
    }


    public void activateTakePoints()
    {
        takePoints = true;
    }

    public void deactivateTakePoints()
    {
        takePoints = false;
    }
}