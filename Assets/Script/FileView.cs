using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using HelperMethod;
using System;

public class FileView : NetworkBehaviour {

	public string FileFolder;
	public bool UseConfigFile = false;
	public int FileWidth;
	public int FileHeight;
	public GameObject PageViewer;
	public GameObject CatalogViewer;
	public GameObject NextPageViewer;
	public GameObject FilesCatalog;
	public GameObject ThumbnailPrefab;
	public GameObject FileViewer;

	public int AvailableCountInCatalog = 16;
	public float RotateValue = 5;


	private float catalogDiff = 0.01f;
	private float catalogViewXScale = 0.45f;
	private float catalogViewYScale = 0.062f;

	public int PageIndex { get; private set;}

	private ArrayList fileThumbnails=new ArrayList();
	private ArrayList fileMaterials = new ArrayList();

	private int willChangeIndex = 0;

	private bool isBusy = false;

	#region animation for turn page
	private int rotateDirection = 1;
	private float rotateSum = 0;
	#endregion

	#region animation for show file
	private FileThumbnail selectedFile;
	private bool needAnimationForShowFile =false;
	private bool showFileStatus = false;

	Vector3 moveForward=new Vector3(0,0,0.02f);
	Vector3 moveback=new Vector3(0,0,-0.02f);
	#endregion

	// Use this for initialization
	void Start () {
		
		float eachV = 1.0f / this.AvailableCountInCatalog*2;
		this.catalogViewYScale = eachV - this.catalogDiff;

		if (UseConfigFile) {
			if (Helper.ReadConfig ()) {
		
				if (Helper.Config.ContainsKey ("filefolder")) {
					this.FileFolder = Helper.Config ["filefolder"];
					this.RenderUI ();
				}
			}
		} else {
			this.RenderUI ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.UpArrow)) {
			
			this.CmdTrunPage (this.PageIndex + 1);
		} else if (Input.GetKeyUp (KeyCode.DownArrow)) {
			this.CmdTrunPage (this.PageIndex - 1);
		} else if (Input.GetKeyUp (KeyCode.LeftArrow)) {
			this.CmdDisplayFile (0, false);
		} else if (Input.GetKeyUp (KeyCode.RightArrow)) {
			this.CmdDisplayFile (0, true);
		}
	}

	void FixedUpdate(){
	
		this.TurnToPageAnimation ();

		this.CheckFileStatus ();

		this.ShowFileAnimation ();
	}

	#region public method
	public void CheckFileStatus(){

		if (this.showFileStatus) {
			if (this.selectedFile != null && this.selectedFile.IsSelected == false) {
				this.needAnimationForShowFile = true;
			}
		} else {

			foreach (FileThumbnail thumbnail in this.fileThumbnails) {
				if (thumbnail.IsSelected) {
				
					if (this.selectedFile == null || this.selectedFile != thumbnail) {
						this.RenderFile (thumbnail.FileFolder);
						this.selectedFile = thumbnail;	
					}
					this.needAnimationForShowFile = true;

				}
			}
		}
		if (this.needAnimationForShowFile && this.showFileStatus==false) {
			foreach (FileThumbnail thumbnail in this.fileThumbnails) {
				thumbnail.SetTextVisiable (false);
			}
		}
	}

	public void SetPageIndex(int index){
		this.willChangeIndex = this.GetValueInRange (index, 0, this.fileMaterials.Count - 1); 
		if (this.willChangeIndex != this.PageIndex && this.isBusy == false) {
			this.isBusy = true;
			if (this.willChangeIndex - this.PageIndex > 0) {
				this.rotateDirection = -1;
				Material mat = (Material)this.fileMaterials [this.PageIndex];
				this.NextPageViewer.GetComponent<Renderer> ().material = mat;

				Material nextMat = (Material)this.fileMaterials [this.willChangeIndex];
				this.PageViewer.GetComponent<Renderer> ().material = nextMat;

			} else {
				this.rotateDirection = 1;
				Material mat = (Material)this.fileMaterials [this.willChangeIndex];
				this.NextPageViewer.GetComponent<Renderer> ().material = mat;

			}
		}
	}

	public void DisplayFile(int index, bool show = true){
		if (show) {
			int cIndex = 0;
			foreach (FileThumbnail thumbnail in this.fileThumbnails) {
				if (cIndex == index) {
					thumbnail.IsSelected = true;
				}
				cIndex += 1;
			}
		} else {
			if (this.selectedFile != null) {
				this.selectedFile.IsSelected = false;
			}
		}
	}
	#endregion

	#region private method
	/// <summary>
	/// 以IO方式进行加载
	/// </summary>
	private void RenderUI()
	{
		double startTime = (double)Time.time;
		string[] folders = Directory.GetDirectories (this.FileFolder);
		int availableFolderIndex = 0;
		float thumbnailWidth = 1f / 5f;
		for (int folderIndex = 0; folderIndex < folders.Length; folderIndex++) {
			string folder = folders [folderIndex];
			FileAttributes attr = File.GetAttributes(folder);
			//detect whether its a directory or file
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {

				int yIndex = availableFolderIndex / 5;
				int xIndex = availableFolderIndex % 5;
				Vector3 pos = new Vector3 (0.4f - xIndex* thumbnailWidth, 0.3f - yIndex * (thumbnailWidth + 0.05f), 0.38f);
				print (pos);
				GameObject thumbnail = (GameObject)Instantiate (this.ThumbnailPrefab, Vector3.zero, Quaternion.identity);
				FileThumbnail fileThumbnail = thumbnail.GetComponent<FileThumbnail> ();
				fileThumbnail.FileFolder = folder;

				string[] files = Directory.GetFiles (folder);

				for (int index = 0; index < files.Length; index++) {
					string filePath = files [index];
					int lastIndex = filePath.LastIndexOf ('/');
					if (lastIndex < 0)
						lastIndex = this.FileFolder.LastIndexOf ('\\');

					string fileName = filePath.Substring (lastIndex + 1);
					if (fileName.StartsWith ("."))
						continue;
					if (filePath.ToLower ().EndsWith (".jpg") || filePath.ToLower ().EndsWith (".png")) {
						fileThumbnail.Thumbnail = filePath;
						break;
					}
				}
				thumbnail.transform.parent = this.FilesCatalog.transform;
				thumbnail.transform.localPosition = pos;
				this.fileThumbnails.Add (fileThumbnail);

				availableFolderIndex++;
			}
		}
		double costTime=(double)Time.time-startTime;
		Debug.Log("IO加载用时:" + costTime);
	}

	void RenderFile(string folderPath){
		ArrayList availableFiles = new ArrayList ();
		string[] files = Directory.GetFiles (folderPath);

		for (int index = 0; index < files.Length; index++) {
			string filePath = files [index];
			int lastIndex = filePath.LastIndexOf ('/');
			if (lastIndex < 0)
				lastIndex = filePath.LastIndexOf ('\\');

			string fileName = filePath.Substring (lastIndex + 1);
			if (fileName.StartsWith ("."))
				continue;
			if (filePath.ToLower ().EndsWith (".jpg") || filePath.ToLower ().EndsWith (".png")) {
				availableFiles.Add (filePath);
			}
		}
		this.PageIndex = 0; 
		this.willChangeIndex = this.PageIndex;	
		this.fileMaterials.Clear ();
		//clear childg
		for (var i = this.CatalogViewer.transform.childCount - 1; i >= 0; i--)
		{
			// objectA is not the attached GameObject, so you can do all your checks with it.
			GameObject objectA = this.CatalogViewer.transform.GetChild(i).gameObject;
			objectA.transform.parent = null;
			// Optionally destroy the objectA if not longer needed
		} 
		for (int index = 0; index < availableFiles.Count; index++) {					
			string filePath = availableFiles [index].ToString ();
			Material mat = Helper.GetViewerMaterial (filePath, this.FileWidth, this.FileHeight);
			this.fileMaterials.Add (mat);
			if (index == this.PageIndex) {
				this.PageViewer.GetComponent<Renderer> ().material = mat;
			}

			if (index < this.AvailableCountInCatalog) {
				GameObject catalogObj = GameObject.CreatePrimitive (PrimitiveType.Cube);  
				catalogObj.GetComponent<Renderer> ().material = mat;
				catalogObj.transform.parent = this.CatalogViewer.transform;
				catalogObj.transform.localScale = new Vector3 (this.catalogViewXScale, this.catalogViewYScale, 1.1f);
				int xIndex = index % 2;
				int yIndex = index / 2;
				float xpos = xIndex == 0 ? 0.25f : -0.25f;
				catalogObj.transform.localPosition = new Vector3 (xpos, 0.5f - (this.catalogViewYScale + this.catalogDiff) / 2 - yIndex * (this.catalogViewYScale + catalogDiff), 0);
			}
		}
	}

	void TurnToPageAnimation(){
		if (this.willChangeIndex != this.PageIndex) {
			
			float rotateValue = this.rotateDirection * this.RotateValue;
			Vector3 axis = new Vector3 (0, this.NextPageViewer.transform.localScale.y / 2, 1);
			this.NextPageViewer.transform.RotateAround (axis, Vector3.right, rotateValue);
			this.rotateSum += rotateValue;
			if (Mathf.Abs(Mathf.Abs(this.rotateSum) - 360)< this.RotateValue) {
				
				this.PageIndex = this.willChangeIndex;
				this.rotateSum = 0;

				Material mat = (Material)this.fileMaterials [this.willChangeIndex];
				this.PageViewer.GetComponent<Renderer> ().material = mat;
				this.isBusy = false;
			}
		}
	}


	void ShowFileAnimation(){
		if (this.needAnimationForShowFile == true) {
			if (this.showFileStatus == false) {
				this.FileViewer.transform.Translate (moveForward);

				if ((this.FileViewer.transform.position.z - 1.0f) > 0) {
					this.needAnimationForShowFile = false;
					this.showFileStatus = true;

				}
			} else {
				this.FileViewer.transform.Translate (moveback);

				if ((this.FileViewer.transform.position.z) < 0) {
					this.needAnimationForShowFile = false;
					this.showFileStatus = false;
					foreach (FileThumbnail thumbnail in this.fileThumbnails) {
						thumbnail.SetTextVisiable (true);
					}
				}
			}
		}
	}

	int GetValueInRange(int value, int min, int max){
		if (value < min)
			return min;
		if (value > max)
			return max;
		return value;
	}



	#endregion

	#region syncup data
	[Command]
	void CmdTrunPage(int index){
		RpcTrunPageLocal (index);
	}

	[ClientRpc]
	void RpcTrunPageLocal(int index)
	{
		this.SetPageIndex(index);
	}

	[Command]
	void CmdDisplayFile(int index, bool show){
		RpcDisplayFileLocal (index,show);
	}

	[ClientRpc]
	void RpcDisplayFileLocal(int index, bool show)
	{
		this.DisplayFile (index, show);
	}
	#endregion
}
