using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleInputNamespace
{
	public class TapButtonInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public SimpleInput.ButtonInput button = new SimpleInput.ButtonInput();
		public float tapTime = .2f;

		private float mouseDownTime = 0;

		private void Awake()
		{
			Graphic graphic = GetComponent<Graphic>();
			if( graphic != null )
				graphic.raycastTarget = true;
		}

		private void LateUpdate()
		{
			if (button.value == true) {
				button.value = false;
			}
		}

		private void OnEnable()
		{
			button.StartTracking();
		}

		private void OnDisable()
		{
			button.StopTracking();
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			mouseDownTime = Time.time;
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			if (Time.time - mouseDownTime < tapTime) {
				button.value = true;
			}
		}

	}
}