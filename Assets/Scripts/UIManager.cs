using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DentedPixel;

public class UIManager : MonoBehaviour
{
	public GameObject Canvas,MainMenu,BackgroundImage;
	public float TweenDuration = 0.5f;
	public float ButtonWidth = 45;
	public VideoClip[] VideoClips;
	public List<GameObject> MenuButtons = new List<GameObject>();


	private List<GameObject> screens = new List<GameObject>();
	private VideoPlayer videoPlayer;
	private AudioSource audioSource;
	private Vector3[] screenVectors;
	private int screenIndex = 1;
	private bool pauseVideo = true;
	private float canvasWidth,canvasHeight,btnWrapperVectorX;
	private DeviceOrientation orientation;
	private Coroutine coroutine;


	public void Start () {
		orientation = Input.deviceOrientation;
		
		updateOrientation();

		int index = 0;
		foreach(GameObject button in MenuButtons) {
			float xPos = Mathf.Round((ButtonWidth*1.5f)*index);
			RectTransform btnRect = button.GetComponent<RectTransform>();
			btnRect.sizeDelta = new Vector2(ButtonWidth, ButtonWidth);

			button.transform.localPosition = new Vector3(xPos,0,0);
			index++;
		}
		/* RectTransform bkgRect = BackgroundImage.GetComponent<RectTransform>();
		Debug.Log("bkgRect.localScale: "+bkgRect.localScale);
		LeanTween.scale(bkgRect,bkgRect.localScale*1.1f,10f)
			.setLoopPingPong ()
			.setEaseInOutQuad(); */


		//Debug.Log("***** canvasResX: "+canvasResX+" | canvasResY: "+canvasResY+" | canvasWidth: "+canvasWidth);
		//Debug.Log("***** screenWidth: "+Screen.width+" | screenHeight: "+Screen.height);

		Invoke("screenSelect",2f);
	}

	public void Update() {
		if(orientation != Input.deviceOrientation) {
			updateOrientation();
		}
	}


	public void screenSelect() {

		if(screens.Count > 0) {
			screenClose();
			//delay = screens.Count > 0 ? TweenDuration : 0;
		}

		//delay = screenIndex == 1 ? 3f : 0;

		GameObject screen = new GameObject("Screen_0"+screenIndex,typeof(RectTransform));
		AspectRatioFitter ratioFitter = screen.AddComponent<AspectRatioFitter>();

		RawImage rawImage = screen.AddComponent<RawImage>();
			rawImage.color = new Color(255,255,255);
		
		RectTransform rectTransform = screen.GetComponent<RectTransform>();
		
		rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = new Vector2(.5f,.5f);

		ratioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
		ratioFitter.aspectRatio = 1.777f;

		screen.transform.localPosition = screenVectors[screenIndex-1];
		screen.transform.SetParent(Canvas.transform,false);
		screens.Add(screen);

		coroutine = StartCoroutine(initVideo());
		//Invoke("animateMainMenu",delay);
		
		//Invoke("playVideo",delay);
	}

    private void screenOpen() {
		LeanTween.move(screens[screens.Count-1].GetComponent<RectTransform>(),Vector3.zero,TweenDuration).setEaseInOutQuad().setOnComplete(() => {
			pauseVideo = false;
		});
	}

	public void screenClose() {
		LeanTween.move(screens[screens.Count-1].GetComponent<RectTransform>(),screenVectors[screenIndex-1],TweenDuration)
		.setEaseInQuad()
		.setOnComplete(() => {
			screenRemove();
			screenIndex++;
			
			if(screenIndex-1 < VideoClips.Length) {
				screenSelect();
			}
		});
	}

	private void screenRemove() {
		Destroy(screens[0]);
		screens.RemoveAt(0);
	}

	IEnumerator initVideo() {
		//Debug.Log("********** Initializing Video | delay: "+delay);

		videoPlayer = gameObject.AddComponent<VideoPlayer>();
		audioSource = gameObject.AddComponent<AudioSource>();

		GameObject screen = screens[screens.Count-1];
		RawImage rawImage = screen.GetComponent<RawImage>();
		
		videoPlayer.playOnAwake = false;
		videoPlayer.waitForFirstFrame = true;
		audioSource.playOnAwake = false;
		audioSource.Pause();
		
		videoPlayer.source = VideoSource.VideoClip;
		
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		videoPlayer.EnableAudioTrack(0,true);
		videoPlayer.SetTargetAudioSource(0,audioSource);
    
		videoPlayer.clip = VideoClips[screenIndex-1];
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
		animateMainMenu();

		while (pauseVideo) {
			yield return null;
		}

		videoPlayer.Play();
		audioSource.Play();

    	while (videoPlayer.isPlaying)
        {
            ////Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            yield return null;
        }
		
		pauseVideo = true;
		StopCoroutine(coroutine);
		screenClose();
	}

	private void animateButton(float duration) {
		float shortTween = TweenDuration*.5f;
		float longTween = TweenDuration*2f;
		float delay = screenIndex == 1 ? Mathf.Max(duration-longTween, 0) : 0;

		if(screenIndex>1) {
			RectTransform oldBtnRect = MenuButtons[screenIndex-2].GetComponent<RectTransform>();
			LeanTween.scale(oldBtnRect,new Vector3(1f,1f,1f),longTween)
				.setEaseInOutQuad();
		};
		
		RectTransform newBtnRect = MenuButtons[screenIndex-1].GetComponent<RectTransform>();
		
		LTSeq seq = LeanTween.sequence();
			seq.append(delay);
			seq.append(
				LeanTween.scale(newBtnRect,new Vector3(1.4f,1.4f,1f),longTween)
						.setEaseInOutQuad()
						.setDelay(delay)
			);
			seq.append(0.75f);
			seq.append(
				LeanTween.scale(newBtnRect,new Vector3(1.3f,1.3f,1f),shortTween)
					.setEaseInOutQuad()
					.setLoopPingPong (1)
					.setOnComplete(() => {
						//Invoke("playVideo",0.25f);
						//coroutine = StartCoroutine(initVideo());
						Invoke("screenOpen",0.5f);
					})
			);
	}

	private void animateMainMenu() {
		
		float offset = Mathf.Round(ButtonWidth*1.5f);

		float newVectorX = 0-((screenIndex-1)*offset);
		float distance = btnWrapperVectorX - newVectorX;

		btnWrapperVectorX = newVectorX;

		Vector3 btnWrapperVector = new Vector3(btnWrapperVectorX,0,0);

		float duration = getTransitionDuration(distance);

		LeanTween.move(MainMenu.GetComponent<RectTransform>(),btnWrapperVector,duration).setEaseInOutQuad();
		
		animateButton(duration);
	}

	private float getTransitionDuration(float distance) {
		Debug.Log("***** getTransitionDuration | distance: "+distance+" | ButtonWidth: "+ButtonWidth);
		return (distance/(ButtonWidth))*TweenDuration;
	}
    
	private void updateOrientation() {
		//Debug.Log("**** updateOrientation ****");

		bool isLandscape = Screen.width > Screen.height;
		float canvasResX = Canvas.GetComponent<CanvasScaler>().referenceResolution.x;
		float canvasResY = Canvas.GetComponent<CanvasScaler>().referenceResolution.y;

		canvasWidth = canvasResX+ButtonWidth;
		canvasHeight = canvasResY+ButtonWidth;

		ButtonWidth = Mathf.Round(canvasResX*(ButtonWidth/100));

		//float stageWidth = isLandscape ? canvasWidth : canvasHeight;
		float stageHeight = isLandscape ? canvasHeight : canvasWidth;

		float modCanvasHeight = canvasWidth*(16/9);

		screenVectors = new Vector3[] {
			new Vector3(-canvasWidth,0,0),
			new Vector3(0,stageHeight,0),
			new Vector3(canvasWidth,0,0),
			new Vector3(0,-stageHeight,0),
			new Vector3(-canvasWidth,0,0),
			new Vector3(0,stageHeight,0),
		};

		btnWrapperVectorX = (canvasWidth/2)+ButtonWidth;
		MainMenu.transform.localPosition = new Vector3(btnWrapperVectorX,0,0);
	}

}
