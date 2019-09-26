using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DentedPixel;

public class UIManager : MonoBehaviour
{
	public GameObject Canvas, HUD, VideoWrapper, MenuButton, DebugText;
	public float TweenDuration = 0.5f;
	public VideoClip[] VideoClips;



	private GameObject activeScreen, btnWrapper;
	private VideoPlayer videoPlayer;
	private AudioSource audioSource;
	private UnityEngine.UI.Text inputText;
	private Vector3[] screenVectors;
	private bool pauseVideo = true, allowTouch = false;
	private DeviceOrientation orientation;
	private Coroutine coroutine;
	public List<GameObject> menuButtons = new List<GameObject>(), activeButtons = new List<GameObject>();
	private int openScreenIndex = 0, currentIndex = 0, targetIndex = 0;
	private float btnSize = 0.6f,// % of the width of the stage
				btnWidth,
				btnOffset,
				canvasWidth,
				canvasHeight;


	public void Start () {
		//Used for debugging traces
		inputText = DebugText.GetComponent<UnityEngine.UI.Text>();

		//Set up event handler for Vuzix touch gestures 
		VuzixInput.onVuzixInputEvent += onVuzixInputEvent;

		videoPlayer =  gameObject.AddComponent<VideoPlayer>();
		audioSource =  gameObject.AddComponent<AudioSource>();
		
		orientation = Input.deviceOrientation;
		updateOrientation();

		btnWrapper = new GameObject("btnWrapper");
		btnWrapper.AddComponent<RectTransform>();
		btnWrapper.transform.SetParent (HUD.transform, true);
		LeanTween.move(btnWrapper.GetComponent<RectTransform>(),new Vector3(canvasWidth, 0, 0),0);
		
		int index = 0;
		while(index < VideoClips.Length) {
			float xPos = btnOffset*index;
			
			GameObject button = Instantiate(MenuButton, Vector3.zero, Quaternion.identity, btnWrapper.transform);
				button.transform.localPosition = new Vector3(xPos,0,0);
				button.name = "btn_video_"+index;
			
			RectTransform btnRect = button.GetComponent<RectTransform>();
				btnRect.sizeDelta = new Vector2(btnWidth, btnWidth);

			LeanTween.scale(btnRect,new Vector3(0.7f,0.7f,0.7f),0);

			GameObject label = button.transform.GetChild(0).gameObject;
			label.GetComponent<UnityEngine.UI.Text>().text = "Step "+(index+1);
			
			menuButtons.Add(button);
			index++;
		}
		
		Invoke("animateBtnWrapper",1f);
	}

	public void Update() {
		VuzixInput.Update(Time.unscaledDeltaTime);

		if(orientation != Input.deviceOrientation) {
			updateOrientation();
		}
	}

	public void screenCreate() {

		activeScreen = Instantiate(VideoWrapper, Vector3.zero, Quaternion.identity, Canvas.transform);
		activeScreen.transform.localPosition = screenVectors[currentIndex];
		activeScreen.name = "vidScreen";
		
		openScreenIndex = currentIndex;

		coroutine = StartCoroutine(initVideo());
	}

    private void screenOpen() {
		LeanTween.move(activeScreen.GetComponent<RectTransform>(),Vector3.zero,TweenDuration).setEaseInOutQuad().setOnComplete(() => {
			pauseVideo = false;
		});
	}

	public void screenClose() {
		LeanTween.move(activeScreen.GetComponent<RectTransform>(),screenVectors[openScreenIndex],TweenDuration)
		.setEaseInQuad()
		.setOnComplete(() => {
			Destroy(activeScreen);
		});
	}

	IEnumerator initVideo() {
		//Debug.Log("********** Initializing Video");

		videoPlayer = gameObject.GetComponent<VideoPlayer>();
		audioSource =  gameObject.GetComponent<AudioSource>();
		//audioSource = gameObject.AddComponent<AudioSource>();

		//GameObject screen = screens[screens.Count-1];
		
		RawImage rawImage = activeScreen.transform.Find("vidScreen").transform.GetComponent<RawImage>();
		
		videoPlayer.playOnAwake = false;
		videoPlayer.waitForFirstFrame = true;
		audioSource.playOnAwake = false;
		audioSource.Pause();
		
		videoPlayer.source = VideoSource.VideoClip;
		
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		videoPlayer.EnableAudioTrack(0,true);
		videoPlayer.SetTargetAudioSource(0,audioSource);

		
		videoPlayer.clip = VideoClips[currentIndex];
		videoPlayer.Prepare();

		WaitForSeconds waitTime = new WaitForSeconds (1);
		
		//Debug.Log("********** Preparing Video");
		
		while (!videoPlayer.isPrepared) {
			yield return waitTime;
			break;
		}

		//Debug.Log("********** Done Preparing Video");

		rawImage.texture = videoPlayer.texture;
		
		videoPlayer.Play();
		audioSource.Play();

		while (videoPlayer.frame  < 2) {
			yield return null;
		}

		videoPlayer.Pause();
		Invoke("screenOpen",0.5f);

		while (pauseVideo) {
			yield return null;
		}

		videoPlayer.Play();
		audioSource.Play();

    	while (videoPlayer.isPlaying)
        {
            //Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            yield return null;
        }
		
		pauseVideo = true;
		allowTouch = true;
		StopCoroutine(coroutine);
		screenClose();
	}

	private void scrollbtnWrapper() {
		
		if (videoPlayer.isPlaying) {
 			pauseVideo = true;
			StopCoroutine(coroutine);
		}
		if(activeScreen != null) {
			screenClose();
		}

		animateBtnWrapper();
	}

	private void animateBtnWrapper() {
		
		float newVectorX = 0-(targetIndex*btnOffset),
			absNew = Mathf.Abs(newVectorX),
			oldVectorX = Mathf.Abs(btnWrapper.GetComponent<RectTransform>().localPosition.x),
			absOld = Mathf.Abs(oldVectorX);
		
		currentIndex = targetIndex;

		float distance = Mathf.Max(absNew, oldVectorX) - Mathf.Min(absNew, oldVectorX);
		float duration = getTransitionDuration(distance);

		Vector3 btnWrapperVector = new Vector3(newVectorX,0,0);
		
		LeanTween.move(btnWrapper.GetComponent<RectTransform>(),btnWrapperVector,duration).setEaseInOutQuad();
		
		animateButton(duration, "focus");
	}

	private void animateButton(float duration, string mode) {

		float shortTween = TweenDuration*.333f;
		float longTween = TweenDuration*2f;
		float delay = currentIndex == 0 ? Mathf.Max(duration-longTween, 0) : 0;
		
		GameObject newBtn = menuButtons[currentIndex];
		RectTransform newBtnRect = newBtn.GetComponent<RectTransform>();
		
		switch(mode) {
			case "focus":
				int ind = activeButtons.Count;
				while(ind > 0) {
					ind--;

					RectTransform oldBtnRect = activeButtons[ind].GetComponent<RectTransform>();
					
					LeanTween.scale(oldBtnRect,new Vector3(0.7f,0.7f,0.7f),duration)
						.setEaseInOutQuad();
					
					activeButtons.RemoveAt(ind);
				}
				
				activeButtons.Add(newBtn);

				LTSeq seq = LeanTween.sequence();
					seq.append(delay);
					seq.append(
						LeanTween.scale(newBtnRect,new Vector3(1f,1f,1f),duration)
								.setEaseInOutQuad()
								.setDelay(delay)
					);
					seq.append(() => {
						//enable button
						allowTouch = true;
					});
				break;
			case "click":
				LeanTween.scale(newBtnRect,new Vector3(0.9f,0.9f,1f),shortTween)
					.setEaseInOutQuad()
					.setLoopPingPong (1)
					.setOnComplete(() => {
						Invoke("screenOpen",0.5f);
					});
				break;
		}
	}

	private float getTransitionDuration(float distance) {
		//Debug.Log("***** getTransitionDuration | distance: "+distance+" | btnWidth: "+btnWidth);
		return Mathf.Abs(distance/btnOffset)*TweenDuration;
	}
    
	private void updateOrientation() {
		//Debug.Log("**** updateOrientation ****");
		
		//float aspectRatio = 9f/16f;// Aspect Ratio of Videos
		bool isLandscape = Screen.width > Screen.height;
		
		canvasWidth = Canvas.GetComponent<CanvasScaler>().referenceResolution.x;
		canvasHeight = Canvas.GetComponent<CanvasScaler>().referenceResolution.y;
		
		btnWidth = Mathf.Round(canvasWidth*btnSize);
		btnOffset = btnWidth+Mathf.Round((canvasWidth-btnWidth)/2)-Mathf.Round(btnWidth*0.7f/2);

		float videoOffsetX = canvasWidth+10;
		float videoOffsetY = canvasHeight+10;//Mathf.Ceil(canvasHeight-((canvasWidth*aspectRatio)/2+10));

		screenVectors = new Vector3[] {
			new Vector3(-videoOffsetX,0,0),
			new Vector3(0,videoOffsetY,0),
			new Vector3(videoOffsetX,0,0),
			new Vector3(0,-videoOffsetY,0),
			new Vector3(-videoOffsetX,0,0),
			new Vector3(0,videoOffsetY,0),
		};

		//Debug.Log("videoOffsetY: "+videoOffsetY+" | canvasHeight: "+canvasHeight+" | distance: "+distance+" | duration: "+duration);
	}

	protected virtual void onVuzixInputEvent( VINPUT_EVENT touchEvent ) {

		var newIndex = currentIndex;

		switch( touchEvent) {
			case VINPUT_EVENT.TAP_1FINGER:
				// basic input "click"
				if(allowTouch) {
					animateButton(0, "click");
					screenCreate();
				}

				break;
			case VINPUT_EVENT.TAP_2FINGER:
				// back or cancel. 
				break;
			case VINPUT_EVENT.SWIPE_FORWARD_1FINGER:
				// scroll right through menu. 

				newIndex = Mathf.Min(currentIndex+1, VideoClips.Length-1);

				if(newIndex != targetIndex) {
					targetIndex = newIndex;
					CancelInvoke();
					Invoke("scrollbtnWrapper",0.25f);
				}

				break;
			case VINPUT_EVENT.SWIPE_BACKWARD_1FINGER:
				// scroll left through menu.

				newIndex = Mathf.Max(0, targetIndex-1);

				if(newIndex != currentIndex) {
					targetIndex = newIndex;
					CancelInvoke();
					Invoke("scrollbtnWrapper",0.25f);
				}
				
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
		
		inputText.text = "touch: "+touchEvent;
	}
}
