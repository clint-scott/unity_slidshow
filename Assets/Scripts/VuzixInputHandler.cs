using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class VuzixInputHandler : MonoBehaviour {

	public GameObject DebugTextInput;
	private UnityEngine.UI.Text inputText;

    // Start is called before the first frame update
    void Start() {
		inputText = DebugTextInput.GetComponent<UnityEngine.UI.Text>();

        VInput.onVInputEvent += _onVInputEvent;
    }

    // Update is called once per frame
    void Update() {
        VInput.Update(Time.unscaledDeltaTime);
    }

	protected virtual void _onVInputEvent( VINPUT_EVENT pEvent ) {

		Debug.Log("Vuzix pEvent: "+pEvent);

		inputText.text = "Vuzix pEvent: "+pEvent;

        switch( pEvent) {
			case VINPUT_EVENT.TAP_1FINGER:
				// basic input "click"
				break;
			case VINPUT_EVENT.TAP_2FINGER:
				// back or cancel. 
				break;
			case VINPUT_EVENT.SWIPE_FORWARD_1FINGER:
				// scroll right through menu. 
				break;
			case VINPUT_EVENT.SWIPE_BACKWARD_1FINGER:
				// scroll left through menu. 
				break;
			case VINPUT_EVENT.SWIPE_UP_1FINGER:
				break;
			case VINPUT_EVENT.SWIPE_DOWN_1FINGER:
				break;
			case VINPUT_EVENT.SWIPE_FORWARD_2FINGER:
				break;
			case VINPUT_EVENT.SWIPE_BACKWARD_2FINGER:
				break;
			case VINPUT_EVENT.HOLD_1FINGER:
				// open a menu! 
				break;
		}
    }
}
