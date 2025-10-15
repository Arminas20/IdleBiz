using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonPulse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float pressedScale = 0.97f;
    [SerializeField] private float speed = 18f;
    private Vector3 target = Vector3.one;

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * speed);
    }

    public void OnPointerDown(PointerEventData e) => target = Vector3.one * pressedScale;
    public void OnPointerUp(PointerEventData e) => target = Vector3.one;
}
