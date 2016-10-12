using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GUIManager : MonoBehaviour {


    private Text m_evoPoints;
    private GameObject m_floatText;
    //private Vector3 m_floatTextInitialPosition;
    public Texture2D m_mira;
    private Text m_hp;
    private Image m_healthBar;
    private Scrollbar m_evoBar;
    private GameObject m_canvasMuerte;
    private GameObject m_canvasRenacido;
    private GameObject m_pauseUI;
    private GameObject m_canvasEvolucion;

    private Text m_canvasEvolucionTotalEvoPoints;
    
    private GameObject m_eggCanvas;
    private GameObject m_eggIndicator;
    private GameObject m_abilitiesSlots;

    //private GameObject m_cooldownAbility;


    private GameObject m_teleportMap;


    private PlayerMovement m_playerMovement;
    private PlayerCombat m_playerCombat;

    private EvolutionManager m_EvolutionManager;

    private Shake m_healthBarShakeScript;
    private HealthBarBlink m_healthBarBlinkScript;

    private EvoUp m_evoUpScript;

    private Button m_OpenMapButton;

    private Color lifeBarColor;


    [Tooltip("if the egg menu is active")]
    public bool m_IsEggCanvasActive = false;

    [Tooltip("Seconds the hero arrival's warning is active")]
    public float secondsWarningIsActive = 4f;

    [Tooltip("Seconds the health bar is shaking")]
    public float hpBarShakeSec = 1f;

    [Tooltip("How much the bar shakes")]
    public float hpBarShakiness = 200f;

    [Tooltip("At what health percent the start will blink")]
    public float hpPercentToBlink = 0.25f;

    [Tooltip("How fast the health bar will blink")]
    public float secBetweenBlinks = 0.25f;

    private bool paused = false;
    private bool IsEvoCanvasOpen = false;

    private float oldHealthBarAmount;
    private float newHealthBarAmount;

    private Text m_evoCostText;

    private Image m_cinematicTopFrame;
    private Image m_cinematicBottomFrame;

    private List<Ability> m_activeCooldowns;

    void Awake()
    {


        m_cinematicTopFrame = gameObject.transform.Find("Cinematic/TopFrame").GetComponent<Image>();
        m_cinematicBottomFrame = gameObject.transform.Find("Cinematic/BottomFrame").GetComponent<Image>();

        m_evoCostText = gameObject.transform.Find("CanvasEvolucion/Evolucion/EvoCost").GetComponent<Text>();
        m_evoPoints = gameObject.transform.Find("UI/EvoPoints/EvoPointsText").GetComponent<Text>();
        m_floatText = gameObject.transform.Find("UI/EvoPoints/UIFloat").gameObject;
        //m_floatTextInitialPosition = m_floatText.transform.position;

        m_canvasMuerte = gameObject.transform.Find("CanvasMuerte").gameObject;
        m_canvasRenacido = gameObject.transform.Find("CanvasRenacido").gameObject;
        m_abilitiesSlots = gameObject.transform.Find("UI/AbilitiesSlots").gameObject;

        //m_cooldownAbility = gameObject.transform.Find("UI/AbilitiesSlots/Ability0").gameObject;
        m_healthBarShakeScript = gameObject.transform.Find("UI/BarraDeVidaVacia").GetComponent<Shake>();

        if (m_healthBarShakeScript == null)
            Debug.LogWarning("La barra de vida no tiene el componente Shake!");

        m_healthBarBlinkScript = gameObject.transform.Find("UI/BarraDeVidaVacia").GetComponent<HealthBarBlink>();

        if (m_healthBarBlinkScript == null)
            Debug.LogWarning("La barra de vida no tiene el componente HealthBarBlink!");


        m_healthBar = gameObject.transform.Find("UI/BarraDeVidaVacia/BarraDeVida").GetComponent<Image>();

        lifeBarColor = m_healthBar.GetComponent<Image>().color;

        m_eggIndicator = gameObject.transform.Find("UI/Egg").gameObject;
        m_pauseUI = gameObject.transform.Find("PauseUI").gameObject;
        m_canvasEvolucion = gameObject.transform.Find("CanvasEvolucion").gameObject;

        m_canvasEvolucionTotalEvoPoints = m_canvasEvolucion.transform.FindChild("EvoPointsText").GetComponent<Text>();
        m_eggCanvas = gameObject.transform.Find("EggCanvas").gameObject;

        m_EvolutionManager = GameObject.FindGameObjectWithTag("EvolutionManager").GetComponent<EvolutionManager>();
        m_evoUpScript = m_EvolutionManager.GetComponent<EvoUp>();

        m_activeCooldowns = new List<Ability>();


        m_teleportMap = gameObject.transform.Find("EggCanvas/TeleportMap").gameObject;

        m_OpenMapButton = gameObject.transform.Find("EggCanvas/SimboloTeletransporte").GetComponent<Button>();
        

    }

    void Start()
    {
        if (m_mira != null)
        {
            Vector2 cursorHotspot = new Vector2(m_mira.width / 2, m_mira.height / 2);
            Cursor.SetCursor(m_mira, cursorHotspot, CursorMode.Auto);
        }
            
        

        setButtonOpenMapButton();
    }

    void setButtonOpenMapButton()
    {
        m_OpenMapButton.onClick.RemoveAllListeners();
        m_OpenMapButton.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
        m_OpenMapButton.onClick.AddListener(() => openTeleportMap());
    }


    /// <summary>
    /// Update evolution points
    /// </summary>
    /// <param name="evoPoints">The amount of evo points the player has</param>
    /// <returns> </returns>
    public void updateEvoPoints(float evoPoints, bool firstTime = false)
    {
       if (!firstTime && !m_evoPoints.transform.parent.gameObject.activeInHierarchy)
       {
           m_evoPoints.transform.parent.gameObject.SetActive(true);
       }
        m_evoPoints.text = "" + evoPoints;       
    }

    public void floatEvoUIText(float evoPoints)
    {
        m_floatText.SetActive(true);
        m_floatText.GetComponent<UIFloatText>().setText(evoPoints);
    }

    /// <summary>
    /// Update hp points
    /// </summary>
    /// <param name="hp">The amount of hp the player has</param>
    /// <returns> </returns>
    public void updateHP(float hp, bool fromAStart = false)
    {
        oldHealthBarAmount = m_healthBar.fillAmount;
        newHealthBarAmount = hp / 100f;

        m_healthBar.fillAmount = newHealthBarAmount ;

        //-----Shake----
        //si antes teniamos más vida, es porque nos han hecho daño
        if (oldHealthBarAmount > newHealthBarAmount && !fromAStart)
            m_healthBarShakeScript.shakeFor(hpBarShakeSec, hpBarShakiness);


        //----Blink----
        if (m_healthBar.fillAmount < hpPercentToBlink)
            m_healthBarBlinkScript.blink(secBetweenBlinks);
        else
            m_healthBarBlinkScript.stopBlinking();

        if(m_healthBar.fillAmount <= 0)
            m_healthBarBlinkScript.stopBlinking();

    }

    /// <summary>
    /// Se utiliza cuando el jugador se muere
    /// </summary>
    /// <returns> </returns>
    public void playerHasDied(bool hasEgg)
    {
        if (hasEgg)
        {
            m_canvasRenacido.SetActive(true);
            turnEggInidicator(false);

        }
        else
        {
            m_canvasMuerte.SetActive(true);
        }
        
    }

    public void resetCanvas(){
        m_canvasRenacido.SetActive(false);
        resetEvolutionButtons();
    }

    private void resetEvolutionButtons()
    {

        //buscamos todos los botones de evolucion para volverlos interactuables
        GameObject[] evoButtons = GameObject.FindGameObjectsWithTag("EvoButton");

        foreach (GameObject button in evoButtons)
        {
            button.GetComponent<Button>().interactable = true;
        }

        m_evoUpScript.resetEvolutionButtons();

     


    }
    public void toggleCanvasEvolucion()
    {
        IsEvoCanvasOpen = !IsEvoCanvasOpen;
        m_canvasEvolucion.SetActive(IsEvoCanvasOpen);

        //Open
        if (IsEvoCanvasOpen == true)
        {
            setEvoCost(0);
            m_evoUpScript.resetHighlights();

            OutlineEffect.getInstance().hideAllRenderers();
            m_canvasEvolucionTotalEvoPoints.text = "" + m_EvolutionManager.getEvoPoints();

            
        }
        //Close
        else
        {
            OutlineEffect.getInstance().showAllRenderers();
            GameObject m_player = GameObject.FindGameObjectWithTag("Player");
            if (m_player && m_player.GetComponent<PlayerMovement>())
            {
                m_player.GetComponent<PlayerMovement>().enabled = true;
            }
            m_EvolutionManager.debugEvo = false;
        }

    }

    public void setEvoCost(int evoCost)
    {
        m_evoCostText.text = "" + evoCost;
    }

    void Update()
    {
        if (Input.GetButtonDown("Pausa") && !IsEvoCanvasOpen)
        {
            paused = !paused;
            if(paused)
                OutlineEffect.getInstance().hideAllRenderers();
            else
                OutlineEffect.getInstance().showAllRenderers();

            //print("pausa");
        }
        else if (Input.GetButtonDown("MenuEvo") && !paused)
        {
            m_EvolutionManager.debugEvo = true;
            toggleCanvasEvolucion();     
        }
 
        if (paused && !IsEvoCanvasOpen)
        {
            //print("pausaSetActive");
            m_pauseUI.SetActive(true);
            Time.timeScale = 0;
        }
        if (!paused)
        {
            //print("pausaSetActiveFalse");
            m_pauseUI.SetActive(false);
            Time.timeScale = 1;

        }

      

        if (m_activeCooldowns.Count > 0)
        {
            foreach(Ability ability in m_activeCooldowns)
            {
                ability.m_cooldownGameObject.GetComponent<Image>().fillAmount = ability.m_usableAbility.m_timeSinceLastAttack / ability.m_cooldownTime;


                //Debug.Log(ability.m_usableAbility.m_timeSinceLastAttack / ability.m_cooldownTime);
               
                if(ability.m_usableAbility.m_timeSinceLastAttack <= 0)
                {
                    m_activeCooldowns.Remove(ability);
                    return;
                }
                //m_timeSinceLastAttack -= Time.deltaTime
            }
        }
        
    }


    public void Resume()
    {
        paused = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");

    }

    public void Quit()
    {
        Application.Quit();
    }

    public GameObject getEggMenu()
    {
        return m_eggCanvas;
    }
    public void toggleEggCanvas()
    {
        //Nos interesa buscarlo de nuevo por si el jugador he evolucioda y ha cambiado la referencia al sscript
        GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
        m_playerMovement = player.GetComponent<PlayerMovement>();
        m_playerCombat = player.GetComponent<PlayerCombat>();

        //evitamos que se escurra/patine cuando se abre el canvas abierto
        m_playerMovement.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        //hacemos que no se pueda mover
        m_playerMovement.enabled = m_IsEggCanvasActive;
        m_playerCombat.enabled = m_IsEggCanvasActive;

       

        //abrir menu de huevo
        m_eggCanvas.SetActive(!m_IsEggCanvasActive);

        
        m_IsEggCanvasActive = !m_IsEggCanvasActive;

        if (m_IsEggCanvasActive)
            OutlineEffect.getInstance().hideAllRenderers();
        else
            OutlineEffect.getInstance().showAllRenderers();


    }

    public void turnEggInidicator(bool turnTo)
    {
        m_eggIndicator.SetActive(turnTo);
    }

    
    /*
     * Metodo utilizado por el script HeroArrives para activar el canvas
     * 
     */
    public void activeWarning(string nameOfTheHero)
    {
        Transform canvasAlertaTransform = gameObject.transform.Find("CanvasAvisoHeroe/Alerta" + nameOfTheHero);
        if(canvasAlertaTransform)
        {
            canvasAlertaTransform.gameObject.SetActive(true);
            StartCoroutine(deactivateGameObjectAfterDelay(canvasAlertaTransform.gameObject, secondsWarningIsActive));
            m_EvolutionManager.heroAsArrive(nameOfTheHero);
        }
        else
        {
            Debug.LogWarning("No se encontre la imagen de aviso: 'Alerta" + nameOfTheHero + "' en el canvasAvisoHeroe");
        }
        
    }

    IEnumerator deactivateGameObjectAfterDelay(GameObject canvasAvisoEspecifico,float delay)
    {
        yield return new WaitForSeconds(delay);

        canvasAvisoEspecifico.SetActive(false);

    }

    public void newAbilitySprite(Sprite abilitySprite)
    {
        if (!m_abilitiesSlots.activeInHierarchy)
            m_abilitiesSlots.SetActive(true);       

        
        int abilitySlotNumber = 0;
        foreach (Transform abilitySlot in m_abilitiesSlots.transform)
        {
            if (abilitySlot.GetComponent<Image>().sprite.name == "abilitySlot")
            {
                abilitySlot.GetComponent<Image>().sprite = abilitySprite;
                highlightSelectedAbility(abilitySlotNumber);
                
                //nos interesa buscarlo otra vez por si muere el jugador
                GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
                m_playerCombat = player.GetComponent<PlayerCombat>();
                m_playerCombat.m_selectedAbilityNumber = abilitySlotNumber;

                return;
            }
            abilitySlotNumber++;
        }
    }

    public void highlightSelectedAbility(int abilitySlot)
    {
       // Debug.Log(abilitySlot);

        //Deactivated all selections
        m_abilitiesSlots.transform.FindChild("Ability0" + "/Selected").gameObject.SetActive(false);
        m_abilitiesSlots.transform.FindChild("Ability1" + "/Selected").gameObject.SetActive(false);
        m_abilitiesSlots.transform.FindChild("Ability2" + "/Selected").gameObject.SetActive(false);
        m_abilitiesSlots.transform.FindChild("Ability3" + "/Selected").gameObject.SetActive(false);

        //activate selection
        m_abilitiesSlots.transform.FindChild("Ability" + abilitySlot + "/Selected").gameObject.SetActive(true);
    }

    public void abilityWasUsed(UsableAbility usedAbility,int selectedAbilityNumber)
    {
       GameObject cooldown = m_abilitiesSlots.transform.FindChild("Ability" + selectedAbilityNumber + "/Cooldown").gameObject;
       cooldown.GetComponent<Image>().fillAmount = 1;
       m_activeCooldowns.Add(new Ability(cooldown, usedAbility, selectedAbilityNumber, usedAbility.abilityCooldown));
    }

    //Struct de una abilidad con sus datos para poder realizar cooldowns
    private struct Ability
    {
        public GameObject m_cooldownGameObject;
        public UsableAbility m_usableAbility;
        public int m_selectedAbilityNumber;
        public float m_cooldownTime;

        public Ability(GameObject cooldownGameObject, UsableAbility usableAbility, int selectedAbilityNumber,float cooldownTime)
        {
            m_cooldownGameObject = cooldownGameObject;
            m_usableAbility = usableAbility;
            m_selectedAbilityNumber = selectedAbilityNumber;
            m_cooldownTime = cooldownTime;
        }
    }



    public void showCinematicCurtain()
    {
        StartCoroutine(showCurtains());
    }
    public void closeCinematicCurtain()
    {
        StartCoroutine(closeCurtains());

    }
    private IEnumerator showCurtains()
    {
        while (m_cinematicTopFrame.fillAmount < 1 && m_cinematicBottomFrame.fillAmount < 1)
        {
            m_cinematicTopFrame.fillAmount += 0.01f;
            m_cinematicBottomFrame.fillAmount += 0.01f;

            yield return null;
        }
    }
    private IEnumerator closeCurtains()
    {
        while (m_cinematicTopFrame.fillAmount > 0 && m_cinematicBottomFrame.fillAmount > 0 )
        {
            m_cinematicTopFrame.fillAmount -= 0.01f;
            m_cinematicBottomFrame.fillAmount -= 0.01f;

            yield return null;
        }
    }




    public void turnOnLifeGreen()
    {

        m_healthBar.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.1f,1f);
        //m_healthBar.transform.parent.GetComponent<Image>().color = new Color(0.75f, 1f, 0f, 1f);

    }
    public void turnOffLifeGreen()
    {
        m_healthBar.GetComponent<Image>().color = lifeBarColor;
        m_healthBar.transform.parent.GetComponent<Image>().color = Color.white;
    }

    public void openTeleportMap()
    {
        m_teleportMap.SetActive(true);
    }

    public void closeTeleportMap()
    {
        m_teleportMap.SetActive(false);
    }
}
