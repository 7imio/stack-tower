using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class CoreInput : MonoBehaviour
{
    public static CoreInput Instance { get; private set; }

    [Header("Options")]
    [SerializeField] private bool listenMouseClick = true;

    public System.Action OnDropPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CoreInput] Duplicate instance found, destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // --- Espace (Drop) ---
        bool spacePressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        spacePressed = Input.GetKeyDown(KeyCode.Space);
#endif

        if (spacePressed)
        {
            Debug.Log("[CoreInput] DROP (Space) pressed");
            OnDropPressed?.Invoke();
        }

        // --- Clic gauche (optionnel) ---
        if (listenMouseClick)
        {
            bool clickPressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            clickPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            clickPressed = Input.GetMouseButtonDown(0);
#endif

            if (clickPressed)
            {
                Debug.Log("[CoreInput] DROP (Mouse Left) pressed");
                OnDropPressed?.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
