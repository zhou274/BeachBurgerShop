using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using StarkSDKSpace;
using System.Collections.Generic;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;

public class PauseManager : MonoBehaviour {

	//***************************************************************************//
	// This class manages pause and unpause states.
	//***************************************************************************//
	public static bool  soundEnabled;
	public static bool  isPaused;
	private float savedTimeScale;
	public GameObject pausePlane;
    public string clickid;
    private StarkAdManager starkAdManager;

    enum Page {
		PLAY, PAUSE
	}
	private Page currentPage = Page.PLAY;


	void Awake (){		
		soundEnabled = true;
		isPaused = false;
		
		Time.timeScale = 1.0f;
		
		if(pausePlane)
	    	pausePlane.SetActive(false); 
	}


	void Update (){
		//touch control
		touchManager();
		
		//optional pause
		if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape)) {
			//PAUSE THE GAME
			switch (currentPage) {
	            case Page.PLAY: 
					PauseGame(); 
					break;
	            case Page.PAUSE: 
					UnPauseGame(); 
					break;
	            default: 
					currentPage = Page.PLAY;
					break;
	        }
		}

		//debug restart
		if(Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}


	void touchManager (){
		if(Input.GetMouseButtonUp(0)) {
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hitInfo)) {
				string objectHitName = hitInfo.transform.gameObject.name;
				switch(objectHitName) {
					case "PauseBtn":
					
						//pause is not allowed when game is finished
						if(MainGameController.gameIsFinished)
							return;
							
						switch (currentPage) {
				            case Page.PLAY: 
								PauseGame(); 
								break;
				            case Page.PAUSE: 
								UnPauseGame(); 
								break;
				            default: 
								currentPage = Page.PLAY;
								break;
				        }
						break;
					
					case "Btn-Resume":
						switch (currentPage) {
				            case Page.PLAY: 
								PauseGame(); 
								break;
				            case Page.PAUSE: 
								UnPauseGame(); 
								break;
				            default: 
								currentPage = Page.PLAY;
								break;
				        }
						break;
						
					case "Btn-Menu":
						UnPauseGame();
						SceneManager.LoadScene("Menu-c#");
						break;
						
					case "Btn-Restart":
						UnPauseGame();
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
						
					case "End-Menu":
						SceneManager.LoadScene("Menu-c#");
						break;

					case "End-Next":
						SceneManager.LoadScene("LevelSelection-c#");
						break;
						
					case "End-Restart":
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
					case "End-Continue":
                        MainGameController.instance.availableTime += 10;
                        MainGameController.gameIsFinished = false;
                        MainGameController.instance.endGamePlane.SetActive(false);
                        break;
                        
                }
			}
		}
	}
	public void Continue()
	{
        ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    MainGameController.instance.availableTime += 10;
                    MainGameController.gameIsFinished = false;
                    MainGameController.instance.endGamePlane.SetActive(false);

                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
    }
	public void Resume()
	{
        switch (currentPage)
        {
            case Page.PLAY:
                PauseGame();
                break;
            case Page.PAUSE:
                UnPauseGame();
                break;
            default:
                currentPage = Page.PLAY;
                break;
        }
    }

    public void RestartPause()
	{
        UnPauseGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetGame()
	{
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
	public void BackMenu()
	{
        UnPauseGame();
        SceneManager.LoadScene("Menu-c#");
    }
	public void nextLevel()
	{
        SceneManager.LoadScene("LevelSelection-c#");
    }
	void PauseGame (){
		print("Game in Paused...");
		isPaused = true;
		savedTimeScale = Time.timeScale;
	    Time.timeScale = 0;
	    AudioListener.volume = 0;
	    if(pausePlane)
	    	pausePlane.SetActive(true);
	    currentPage = Page.PAUSE;
	}


	void UnPauseGame (){
		print("Unpause");
	    isPaused = false;
	    Time.timeScale = savedTimeScale;
	    AudioListener.volume = 1.0f;
		if(pausePlane)
	    	pausePlane.SetActive(false);   
	    currentPage = Page.PLAY;
	}
    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }
}