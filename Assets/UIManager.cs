using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DentedPixel;

public class UIManager : MonoBehaviour
{
	public GameObject Canvas,MainMenu,BtnWrapper;
	public float TweenDuration = 0.5f;
	public int ButtonWidth = 512;
	public VideoClip[] VideoClips;
	public List<GameObject> Buttons = new List<GameObject>();



	private List<GameObject> menus = new List<GameObject>();
	private VideoPlayer videoPlayer;
	private AudioSource audioSource;
	private Vector3[] screenVectors;
	private int menuIndex = 1;
	private float delay = 0,canvasWidth,canvasHeight,btnWrapperVectorX;
	private Coroutine coroutine;



	public void Start () {
		canvasWidth = Canvas.GetComponent<CanvasScaler>().referenceResolution.x;
		canvasHeight = Canvas.GetComponent<CanvasScaler>().referenceResolution.y;

		screenVectors = new Vector3[] {
			new Vector3(-canvasWidth,0,0),
			new Vector3(0,canvasHeight,0),
			new Vector3(canvasWidth,0,0),
			new Vector3(0,-canvasHeight,0),
			new Vector3(-canvasWidth,0,0),
			new Vector3(0,canvasHeight,0),
		};

		btnWrapperVectorX = (canvasWidth/2)+ButtonWidth;

		int index = 0;

		foreach(GameObject button in Buttons) {
			float xPos = Mathf.Round((ButtonWidth*1.5f)*index);

			button.transform.localPosition = new Vector3(xPos,0,0);
			index++;
		}

		Invoke("selectScreen",2f);
	}

	public void selectScreen() {

		if(menus.Count > 0) {
			closeScreen();
			//delay = menus.Count > 0 ? TweenDuration : 0;
		}

		delay = menuIndex == 1 ? 3f : 0;

		GameObject menu = new GameObject("Menu_0"+menuIndex,typeof(RectTransform));
	
		RawImage rawImage = menu.AddComponent<RawImage>();
			rawImage.color = new Color(255,255,255);
		
		RectTransform rectTransform = menu.GetComponent<RectTransform>();
		
		rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = new Vector2(.5f,.5f);

		menu.transform.localPosition = screenVectors[menuIndex-1];
		menu.transform.SetParent(Canvas.transform,false);
		menus.Add(menu);


		Invoke("animateButtonWrapper",delay);
		
		//Invoke("playVideo",delay);
	}

	private void animateButton(float duration) {
		
		float shortTween = TweenDuration*.25f;
		float longTween = TweenDuration*2f;
		float delay = menuIndex == 1 ? duration-longTween : 0;

		if(menuIndex>1) {
			LeanTween.scale( Buttons[menuIndex-2].GetComponent<RectTransform>(),new Vector3(1f,1f,1f),longTween).setEaseInOutQuad();
		};
		
		
		LTSeq seq = LeanTween.sequence();
			seq.append(delay);
			seq.append(
				LeanTween.scale( Buttons[menuIndex-1].GetComponent<RectTransform>(),new Vector3(1.3f,1.3f,1f),longTween)
					.setEaseInOutQuad()
					.setDelay(delay)
			);
			seq.append(0.75f);
			seq.append(
				LeanTween.scale( Buttons[menuIndex-1].GetComponent<RectTransform>(),new Vector3(1.2f,1.2f,1f),shortTween)
					.setEaseInOutQuad()
					.setLoopPingPong (1)
					.setOnComplete(() => {
						//Invoke("playVideo",0.25f);
						coroutine = StartCoroutine(initVideo());
					})
			);
	}

	private void animateButtonWrapper() {
		
		float offset = Mathf.Round(ButtonWidth*1.5f);

		float newVectorX = 0-((menuIndex-1)*offset);
		float distance = btnWrapperVectorX - newVectorX;

		btnWrapperVectorX = newVectorX;

		Vector3 btnWrapperVector = new Vector3(btnWrapperVectorX,0,0);

		float duration = getTransitionDuration(distance);

		LeanTween.move( BtnWrapper.GetComponent<RectTransform>(),btnWrapperVector,duration).setEaseInOutQuad();
		
		animateButton(duration);
	}

    private void openScreen() {
		LeanTween.move( menus[menus.Count-1].GetComponent<RectTransform>(),Vector3.zero,TweenDuration).setEaseInOutQuad();
	}

	public void closeScreen() {
		LeanTween.move( menus[menus.Count-1].GetComponent<RectTransform>(),screenVectors[menuIndex-1],TweenDuration)
		.setEaseInQuad()
		.setOnComplete(() => {
			removePanel();
			menuIndex++;
			Debug.Log("***** menuIndex: "+menuIndex+" | VideoClips.Length: "+VideoClips.Length);
			if(menuIndex-1 < VideoClips.Length) {
				selectScreen();
			}
		});
	}

	private void removePanel() {
		Destroy(menus[0]);
		menus.RemoveAt(0);
	}

	IEnumerator initVideo() {
		//Debug.Log("********** Initializing Video");
		videoPlayer = gameObject.AddComponent<VideoPlayer>();
		audioSource = gameObject.AddComponent<AudioSource>();

		GameObject menu = menus[menus.Count-1];
		RawImage rawImage = menu.GetComponent<RawImage>();
		
		videoPlayer.playOnAwake = false;
		audioSource.playOnAwake = false;
		audioSource.Pause();
		
		videoPlayer.source = VideoSource.VideoClip;
		
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		videoPlayer.EnableAudioTrack(0,true);
		videoPlayer.SetTargetAudioSource(0,audioSource);
    
		videoPlayer.clip = VideoClips[menuIndex-1];
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
		
		openScreen();
		
		//Debug.Log("********** Playing Video");
        while (videoPlayer.isPlaying)
        {
            ////Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            yield return null;
        }
        Debug.Log("********** Done Playing Video");
		StopCoroutine(coroutine);
		closeScreen();
	}

	private float getTransitionDuration(float distance) {
		return (distance/(ButtonWidth))*TweenDuration;
	}
    
}
