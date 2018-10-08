using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Vuforia;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using System.Net;

/// <summary>
/// This MonoBehaviour implements the Cloud Reco Event handling for this sample.
/// It registers itself at the CloudRecoBehaviour and is notified of new search results.
/// </summary>
public class SimpleCloudHandler : MonoBehaviour, ICloudRecoEventHandler
{
		
	public ImageTargetBehaviour ImageTargetTemplate;
	private ImageTargetBehaviour imageTargetBehaviour;
	private CloudRecoBehaviour mCloudRecoBehaviour;
    
    // Extended Tracking
    // protected TrackableBehaviour mTrackableBehaviour;
    // private DefaultTrackableEventHandler TrackerScript;
    private ImageTargetBehaviour _imageTargetTemplate;
    private bool _extendedTracking;


	private bool mIsScanning = false;
	public string mTargetMetadata = "";
	private string returnMeta;
	public string loadedData;
	private string jsonString;
	public string editorUrl;
	private GameObject newImageTarget;
	public GameObject ARCanvas;
	public GameObject FBShareButton;



	//Added
    //Variable "assetNames" to load all asset names in assetbundles
	static string[] assetNames;
	//Variable to Load all assets in assetbundles to "_assets"
	static UnityEngine.Object[] _assets;
	//Number of assets in AssetBundle
	int i=0;
    // Variable "asseBundle" to load asset bundle
	private AssetBundle asseBundle;
	// Variable "asset" to load prefab
	private AssetBundleRequest asset;
	//Variable "loadedAsset" to load prefab as gameobject
	private GameObject loadedAsset;	
    // AssetBundle Url
    private string url = "Load";
    private string url_loaded = "Unload";
    private bool loded;
    //download asset bundle using WWW
    private WWW www;
    
    // Bytes
    byte[] bytes;

	public GameObject[] clone;
	private int num;

	//Added
	// ImageTracker reference to avoid lookups
    private ObjectTracker mImageTracker;
	private GameObject mParentOfImageTargetTemplate;

	//Material Asset loading
	private AssetBundle matAssetBun;
	// url from meta
	private string matUrl = "Load";

	//Loading Screen Prefab
	//public GameObject[] loadingAssets;
	public GameObject LoadingPrefab;
	private LoaderScript loadingScript;

	//ART
	//to text to compare meta because stings cannot be compared

	private string _artist, _artistEmail;
	private bool _wLoad,_IsArt, _isScript;	
    
    
    // Manager UI Screens
    
    private GameObject ScanUIPrefab;


	// Use this for initialization
	void Start () {
		// register this event handler at the cloud reco behaviour
		/*mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();

		if (mCloudRecoBehaviour)
		{
			mCloudRecoBehaviour.RegisterEventHandler(this);
		}*/

		/* Added ===========================================================================*/

		mParentOfImageTargetTemplate = ImageTargetTemplate.gameObject;
		
		// register this event handler at the cloud reco behaviour
        CloudRecoBehaviour cloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        if (cloudRecoBehaviour)
        {
            cloudRecoBehaviour.RegisterEventHandler(this);
        }
 
        // remember cloudRecoBehaviour for later
        mCloudRecoBehaviour = cloudRecoBehaviour;

		/*================================================================================== */
		
		//LoadingPrefab.SetActive(false);
        
        loadSignInUPUI ();
		
	}

	/*private void Update()
	{
		
        

	}*/


	public void OnInitialized() {

		// get a reference to the Image Tracker, remember it
        mImageTracker = (ObjectTracker)TrackerManager.Instance.GetTracker<ObjectTracker>();
		Debug.Log ("Cloud Reco initialized");
	}
	public void OnInitError(TargetFinder.InitState initError) {
		switch (initError)
        {
            case TargetFinder.InitState.INIT_ERROR_NO_NETWORK_CONNECTION:
                Debug.Log ("Network Unavailable! Failed to initialize CloudReco because the device has no network connection.");
                break;
            case TargetFinder.InitState.INIT_ERROR_SERVICE_NOT_AVAILABLE:
                Debug.Log ("Service Unavailable! Failed to initialize CloudReco because the service is not available.");
                break;
        }
		Debug.Log ("Cloud Reco init error " + initError.ToString());
	}
	public void OnUpdateError(TargetFinder.UpdateState updateError) {
		switch (updateError)
        {
            case TargetFinder.UpdateState.UPDATE_ERROR_AUTHORIZATION_FAILED:
                Debug.Log ("Authorization Error! The cloud recognition service access keys are incorrect or have expired.");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_BAD_FRAME_QUALITY:
                Debug.Log ("Poor Camera Image! The camera does not have enough detail, please try again later");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_NO_NETWORK_CONNECTION:
                Debug.Log ("Network Unavailable! Please check your internet connection and try again.");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_PROJECT_SUSPENDED:
                Debug.Log ("Authorization Error! The cloud recognition service has been suspended.");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_REQUEST_TIMEOUT:
                Debug.Log ("Request Timeout! The network request has timed out, please check your internet connection and try again.");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_SERVICE_NOT_AVAILABLE:
                Debug.Log ("Service Unavailable! The service is unavailable, please try again later.");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_TIMESTAMP_OUT_OF_RANGE:
                Debug.Log ("Clock Sync Error! Please update the date and time and try again.");
                break;
            case TargetFinder.UpdateState.UPDATE_ERROR_UPDATE_SDK:
                Debug.Log ("Unsupported Version! The application is using an unsupported version of Vuforia.");
                break;
        }
		Debug.Log ("Cloud Reco update error " + updateError.ToString());
	}

	public void OnStateChanged(bool scanning) {
		mIsScanning = scanning;
		if (scanning)
		{
			// clear all known trackables
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			tracker.TargetFinder.ClearTrackables(false);

			if (mCloudRecoBehaviour.CloudRecoInitialized && !mCloudRecoBehaviour.CloudRecoEnabled)
		{
 			mCloudRecoBehaviour.CloudRecoEnabled = true;
		}
		}
	}

	// Here we handle a cloud target recognition event
	public void  OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult) {
		
		int len1 = clone.Length;

		Debug.Log("new Target Found");
        
        LoadLoadinganimation (); // run AR Loader Animation

		/***************************************************************************************************** 
		** Stop Loading animation
		***************************************************************************************************** */
/*
		//Loading Asset Prefab Set active on new target found
		LoadingPrefab.SetActive(true);
		//Start Loading animation
		foreach (GameObject _loadingassets in loadingAssets)
        {	
			//ERORRRR set obect to instance first and then use!!!!!!!!!!!
			// get reference Loading Script from the Loading Gameobjects
			LoaderScript loadingScrip = new LoaderScript();
			loadingScrip = _loadingassets.GetComponent<LoaderScript>() as LoaderScript;
			//Set Loading true (Important to read variable exactly from script as Case Sensitive)		
			loadingScrip.loading = true;
		}
*/
		/***************************************************************************************************** 
		** Stop Loading animation
		***************************************************************************************************** */
				

		/*for(i=0; i<num; i++)
            {
				Destroy(clone[i]);
			}*/
		//imageTargetBehaviour = new ImageTargetBehaviour();
		newImageTarget = Instantiate(ImageTargetTemplate.gameObject) as GameObject;
        _imageTargetTemplate = newImageTarget.GetComponent<ImageTargetBehaviour>();
        //TrackerScript = newImageTarget.GetComponent<DefaultTrackableEventHandler>();
        //mTrackableBehaviour = TrackerScript.mTrackableBehaviour;
		

		//ImageTargetBehaviour imageTargetBehaviour = mImageTracker.TargetFinder.EnableTracking(targetSearchResult, mParentOfImageTargetTemplate); 

		 // This code demonstrates how to reuse an ImageTargetBehaviour for new search results and modifying it according to the metadata
        // Depending on your application, it can make more sense to duplicate the ImageTargetBehaviour using Instantiate(), 
        // or to create a new ImageTargetBehaviour for each new result
 
        // Vuforia will return a new object with the right script automatically if you use
        // TargetFinder.EnableTracking(TargetSearchResult result, string gameObjectName)
         
        
         
        // enable the new result with the same ImageTargetBehaviour:
       // ImageTargetBehaviour imageTargetBehaviour = mImageTracker.TargetFinder.EnableTracking(targetSearchResult, mParentOfImageTargetTemplate);
		
	//	if (ImageTargetTemplate) {
			// enable the new result with the same ImageTargetBehaviour:
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			imageTargetBehaviour =	(ImageTargetBehaviour)tracker.TargetFinder.EnableTracking(targetSearchResult, newImageTarget); //mParentOfImageTargetTemplate
            UseExtendedTracking(false);
	//	}

		//Check if the metadata isn't null
		 if(targetSearchResult.MetaData == null)
        {
            return;
        }
		// do something with the target metadata
		mTargetMetadata = targetSearchResult.MetaData;		
		//mContentManager.TargetCreated(targetSearchResult.MetaData);
		/*if (imageTargetBehaviour != null)
    	{
    	    // stop the target finder
    	    mCloudRecoBehaviour.CloudRecoEnabled = false;
  		}*/

		/*if (imageTargetBehaviour != null)
        {
            // stop the target finder
            mCloudRecoBehaviour.CloudRecoEnabled = false;
             
            // Calls the TargetCreated Method of the SceneManager object to start loading
            // the BookData from the JSON
           
        }*/
		//url = ReadMetaUrl();
		Debug.Log("111111111111");
		//if(String.IsNullOrEmpty(url))
		//{
			url = ReadMetaUrl();
			Debug.Log("Downloading from yes yes worked:"+ url);
			Debug.Log("Dodhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh");
			Debug.Log("Read Metadata here");
			
			//if( _wLoad == true)
            if( _wLoad )
			{	
				Debug.Log("Downloading Assets here");
				StartCoroutine("DownloadBundle");
			}
            
            if( _isScript )
			{	
				Debug.Log("Downloading Script now..");
				StartCoroutine("DownloadBundleScript");
                
			}
            
		//}
		
		//Debug.Log("Downloading from yes yes worked:"+ matUrl);

		// use case statements
		
			else
			{
				
				Debug.Log("44444444444444");
				if(_IsArt == true)
				{
					Debug.Log("5555555555555555555");
					Debug.Log ("ART");
					Debug.Log (_artist);
					Debug.Log (_artistEmail);
                    DestroyLoadingAnimation (); // Close AR Loader Animation                    
				}
			
			}
	}


	IEnumerator DownloadBundle (){

		i = 0;
        
        //Check for Extended Tracking
        if(_extendedTracking)
        {
            UseExtendedTracking(true);
        }
        

		if(www != null)
		{
			www.Dispose();
       		www = null;
			asseBundle.Unload(false);
		}

		/***************************************************************************************************** 
		**
		** Load Materials Initially
		**
		***************************************************************************************************** */

		www = new WWW(matUrl);

		using (www)
        {
            yield return www;

			if (www.error != null)
			{
           		throw new Exception ("WWW download had an error: " + www.error);
			}

			matAssetBun = www.assetBundle;
			matAssetBun.LoadAllAssets();
		}
		/***************************************************************************************************** 
		** Loading Materials Completed
		***************************************************************************************************** */

		

        // if (bundleHolder.transform.childCount > 0)
        //     Destroy(bundleHolder.transform.GetChild(0).gameObject);

        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;
 
        // Update url_loaded to prevent downloading assets again for the reco 
        url_loaded = url;
        //download AssetBundle
        www = new WWW(url);
        // using (www = new WWW(url)) -- Or use this
        using (www)
        {
            
            //wait for download
            yield return www;
            
            DestroyLoadingAnimation (); // Close AR Loader Animation
            
		/***************************************************************************************************** 
		** Stop Loading animation
		***************************************************************************************************** */			
/*
			LoadingPrefab.SetActive(false);
			foreach (GameObject _loadingassets in loadingAssets)
			{	
				loadingScript = _loadingassets.GetComponent<LoaderScript>();		
				//loadingScript.loading = false;
			}
*/
		/***************************************************************************************************** 
		** Stop Loading animation
		***************************************************************************************************** */

            Debug.Log ("Loaded ");
            if (www.error != null)
            throw new Exception ("WWW download had an error: " + www.error);

            asseBundle = www.assetBundle;

             //load all assets
            _assets = www.assetBundle.LoadAllAssets();

            //load all asset names
            assetNames = www.assetBundle.GetAllAssetNames();
			num = _assets.Length;
            clone = new GameObject[num];   
            foreach (UnityEngine.Object eachAsset in _assets)
            {
                // request prefab to load
                asset = asseBundle.LoadAssetAsync<GameObject>(assetNames[i]);
                yield return asset;
                //GameObject yes = new GameObject(); 
                clone[i] = (GameObject)Instantiate(asset.asset,transform.position, transform.rotation);
				//clone[i] = yes;
				clone[i].transform.parent = newImageTarget.transform;
                Debug.Log(clone[i]);
				newImageTarget.transform.localScale += new Vector3(100, 100, 100);

				
				/*GameObject yes = asset.asset as GameObject;
				Rigidbody rig;
				Instantiate(yes);*/
				//clone[i] = (GameObject)Instantiate(yes);
				//clone[i].transform.parent = newImageTarget.transform;
				//GameObject yesins = (GameObject)Instantiate(yes);
				//rig = yesins.GetComponent<Rigidbody>();
				//yesins.transform.parent = newImageTarget.transform;

                /*Important to transform localscale before transform.parent */
                //clone[i].transform.parent = newImageTarget.transform;
                //Instantiate(asseBundle.LoadAsset(assetNames[i]));
                i += 1  ;
            }
			Templates();   
        }

		//asseBundle.Unload(false);
    }
    

/********************************************************************************************************************************

** Download Load Script
** Save in Application Data Path

********************************************************************************************************************************/
    
   IEnumerator DownloadBundleScript (){

		if(www != null)
		{
			www.Dispose();
       		www = null;
			asseBundle.Unload(false);
		}


        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;
 
        // Update url_loaded to prevent downloading assets again for the reco 
        url_loaded = url;
        //download AssetBundle
        www = new WWW(url);
        // using (www = new WWW(url)) -- Or use this
        using (www)
        {
            
            //wait for download
            yield return www;
            
            Debug.Log ("Script Loaded Here ");
            if (www.error != null)
            throw new Exception ("WWW download had an error: " + www.error);

            bytes = www.bytes;
            Debug.Log (bytes);
            File.WriteAllBytes(Application.streamingAssetsPath + "/" +  "JavaScript" + "/" + "ui_controller.javascript", bytes); 
            
            Debug.Log ("Script Name is ");
            DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath + "/" +  "JavaScript");
            FileInfo[] info = dir.GetFiles("*.*");
            
            foreach (FileInfo f in info)
            {
                Debug.Log(f.ToString());
            }
    
            Debug.Log ("This is the end of the list");
            DestroyLoadingAnimation (); 
            InitiateJavaScriptEngine();
        }   
    }      
    
    /*void DownloadBundleScript()
{
    WebClient client = new WebClient();
    client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler( DownloadFileCompleted );
    //client.DownloadFileAsync ((new Uri (url, Application.dataPath + "/" + "ui_controller.javascript")));
    Uri u = new UriBuilder(url).Uri;
    client.DownloadFileAsync (u, Application.dataPath + "/" + "ui_controller.javascript");
    //Debug.Log(Path.GetFileName(Application.dataPath));
    while (client.IsBusy) { }
    
    DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
    FileInfo[] info = dir.GetFiles("*.*");
 
    foreach (FileInfo f in info)
    {
        Debug.Log(f.ToString());
    }
}
void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
{
    if (e.Error == null)
    {
       DestroyLoadingAnimation (); 
    }
} */
    
/********************************************************************************************************************************
********************************************************************************************************************************/    

 

	public string ReadMetaUrl()
	{
		returnMeta = mTargetMetadata;
		//Debug.Log ("Meta Data:" + returnMeta);
		MetaDataFields loadedData = JsonUtility.FromJson<MetaDataFields>(returnMeta);
		//Debug.Log(loadedData.Editor);
		// Debug.Log(loadedData.Android);
		// Debug.Log(loadedData.iOS);
		// PLatform Specific runtime
		    _wLoad = loadedData.W_Download;
			_IsArt = loadedData.Art;
			_artist = loadedData.Artist;
			_artistEmail = loadedData.Artist_Email;
			string a = loadedData.Editor;
			string b = loadedData.Android;
			string c = loadedData.iOS;
			string d = loadedData.EditorMatUrl;
			string e = loadedData.AndroidMatUrl;
			string f = loadedData.iOSMatUrl;
            _extendedTracking = loadedData.ExtendedTracking;
            _isScript = loadedData.Is_Script;

			Debug.Log("here here here here hereh here hereh ereh ereh ere");
			Debug.Log(_wLoad);
			Debug.Log(_IsArt);
			Debug.Log(_artist);
			Debug.Log(_artistEmail);
			Debug.Log(a);
			Debug.Log(b);
			Debug.Log(c);
			Debug.Log(d);
			Debug.Log(e);
			Debug.Log(f);
			Debug.Log("here here here here hereh here hereh ereh ereh ere");

			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				matUrl = loadedData.iOSMatUrl;
				return loadedData.iOS;
			}

			else if (Application.platform == RuntimePlatform.Android)
			{
				matUrl = loadedData.AndroidMatUrl;
				return loadedData.Android;
			}

			else //(Application.platform == RuntimePlatform.Android)
			{
				matUrl = loadedData.EditorMatUrl;		
				editorUrl = loadedData.Editor;
				return loadedData.Editor;
			}

			
		//return returnMeta;	
	}
    
    
        // Starts/stops extended tracking for a given trackable
    public void UseExtendedTracking(bool enabled) 
    {    
        if (enabled)
        { 
            _imageTargetTemplate.ImageTarget.StartExtendedTracking();
        }
        else
        {
            _imageTargetTemplate.ImageTarget.StopExtendedTracking();        
        }
        return;
    }


	void Templates()
	{
		GameObject _ARCanvas = new GameObject();
		GameObject _FBButton = new GameObject();
		
		_ARCanvas = (GameObject)Instantiate(ARCanvas,transform.position, transform.rotation);
		_ARCanvas.transform.parent = newImageTarget.transform;

		_FBButton = (GameObject)Instantiate(FBShareButton,transform.position, transform.rotation);
		_FBButton.transform.parent = _ARCanvas.transform;
	}
    
    
        public void loadSignInUPUI ()
    {
        GameObject LogUI = Resources.Load ("UIElements/SignInandSignUp") as GameObject;
        LogUI = Instantiate (LogUI);
        Debug.Log("Instantiated login scree NOWWWWWWWWWW!!!!!!!!!!!!!!!!!!!!");
    }
    
    
     public void LoadLoadinganimation ()
    {
        GameObject LoadingAnimation = Resources.Load ("UIElements/ARLoader/ARLoaderPrefab") as GameObject;
        LoadingAnimation = Instantiate (LoadingAnimation);
        //Debug.Log("Instantiated login scree NOWWWWWWWWWW!!!!!!!!!!!!!!!!!!!!");
    }
    
    public void InitiateJavaScriptEngine()
    {
        GameObject JsEngine = Resources.Load ("JavaScript/_JSEngine") as GameObject;
        GameObject JsController = Resources.Load ("JavaScript/Controller") as GameObject;
        JsEngine = Instantiate (JsEngine);
        JsController = Instantiate (JsController);
    }
    
    
    public void DestroyLoadingAnimation ()
    {
        Destroy (GameObject.FindWithTag("ARLoaderAnimation"));
    }
    


	[System.Serializable]
	public class MetaDataFields
	{
		public bool  W_Download;
        public bool Is_Script;
		public string Editor;
		public string Android;
		public string iOS;
		public string EditorMatUrl;
		public string AndroidMatUrl;
		public string iOSMatUrl;
        public bool ExtendedTracking; //Add this to the meta vuforia as it is new!!!!!!!!!!!!
		public bool Art;
		public string Artist;
		public string Artist_Email;
	}
}

/********************** workikng on **********************************
*
** Templates: 1. Facebook
** Resize Objects
** Position
*
********************************************************************/
