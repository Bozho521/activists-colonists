using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    [DefaultExecutionOrder(90)]
    public class CardRaycaster : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] LayerMask cardMask = ~0;
        [SerializeField] float rayDistance = 1000f;
        [SerializeField] bool ignoreWhenPointerOverUI = true;
        [SerializeField] CardInfoPanel infoPanel;

        private CardInteractable _hovered;

        void Awake()
        {
            if (!cam) cam = Camera.main;
        }

        void Update()
        {
            if (!cam) return;

            if (ignoreWhenPointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                ClearHover();
                return;
            }

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, rayDistance, cardMask, QueryTriggerInteraction.Ignore))
            {
                var card = hit.collider.GetComponentInParent<CardInteractable>();
                if (card != _hovered)
                {
                    if (_hovered) _hovered.Hover(false);
                    _hovered = card;
                    if (!_hovered) return;

                    _hovered.Hover(true);
                    if (infoPanel)
                    {
                        infoPanel.Show(_hovered);
                    }
                }
            }
            else
            {
                ClearHover();
            }
        }

        void ClearHover()
        {
            if (_hovered) _hovered.Hover(false);
            _hovered = null;
            if (infoPanel) infoPanel.Hide();
        }
    }
}