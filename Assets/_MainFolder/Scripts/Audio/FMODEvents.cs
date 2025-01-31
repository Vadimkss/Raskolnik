using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    // Singleton instance
    public static FMODEvents Instance { get; private set; }

    // Player Sounds
    [Header("Player SFX")]
    [SerializeField] public EventReference Falled;
    [SerializeField] public EventReference Slamed;
    [SerializeField] public EventReference SlamFall;
    [SerializeField] public EventReference FootSteps;
    [SerializeField] public EventReference Sliding;
    [SerializeField] public EventReference Jump;
    [SerializeField] public EventReference jump2;
    [SerializeField] public EventReference Zipline;
    [SerializeField] public EventReference DashSFX;

    [Header("EnemySFX")]

    [SerializeField] public EventReference RS_Step1;
    [SerializeField] public EventReference RS_Step2;
    [SerializeField] public EventReference RS_Moving;


    // Weapon Sounds
    [Header("Weapon SFX")]
    [SerializeField] public EventReference JakylShooting;
   
    [SerializeField] public EventReference Focusing;

    // Main Menu Sounds
    [Header("Main Menu SFX")]
    [SerializeField] public EventReference CaseOpen;
    [SerializeField] public EventReference CaseClose;
    [SerializeField] public EventReference DOPath;

    // Music
    [Header("Music")]
    [SerializeField] public EventReference MenuAmbient;
    [SerializeField] public EventReference MAinTheme;
    [SerializeField] public EventReference HiHat;



    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Ensure object persists between scenes
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        DontDestroyOnLoad(gameObject);
    }
}