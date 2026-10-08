using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonVisual : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("Sprites")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    [Header("Escala")]
    [SerializeField] private float selectedScale = 1.12f;
    [SerializeField] private float animationSpeed = 12f;

    private bool isHovered;
    private bool isSelected;

    private Vector3 normalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        normalScale = transform.localScale;
        ResetVisual();
    }

    private void OnEnable()
    {
        // Cada vez que el botón vuelve a aparecer,
        // empieza limpio.
        isHovered = false;
        isSelected = false;

        ResetVisual();
    }

    private void OnDisable()
    {
        // Evita que se quede guardado un hover
        // al cerrar menú o cambiar de escena.
        isHovered = false;
        isSelected = false;

        ResetVisual();
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        // El hover del mouse también pasa a ser
        // la selección oficial del EventSystem.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateVisual();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        bool active = isHovered || isSelected;

        if (targetImage != null)
        {
            targetImage.sprite = active
                ? selectedSprite
                : normalSprite;
        }

        targetScale = active
            ? normalScale * selectedScale
            : normalScale;
    }

    private void ResetVisual()
    {
        if (targetImage != null)
            targetImage.sprite = normalSprite;

        targetScale = normalScale;
        transform.localScale = normalScale;
    }
}