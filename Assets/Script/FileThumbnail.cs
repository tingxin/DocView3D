using UnityEngine;
using System.Collections;
using HelperMethod;

public class FileThumbnail : MonoBehaviour {

	public GameObject TextUI;
	public GameObject ImageUI;

	public int FileWidth;
	public int FileHeight;
	public int TextLength =15;
	public string FileFolder{ get; set;}
	public string Thumbnail{ get; set;}

	public bool IsSelected = false;

	private string text;
	// Use this for initialization
	void Start () {
		if (string.IsNullOrEmpty (this.FileFolder) == false) {
			
			int lastIndex = this.FileFolder.LastIndexOf ('\\');
			if (lastIndex < 0)
				lastIndex = this.FileFolder.LastIndexOf ('/');

			this.text = this.FileFolder.Substring (lastIndex + 1);
			if (this.text.Length > this.TextLength) {
				this.TextUI.GetComponent<TextMesh> ().text = this.text.Substring (0, this.TextLength-1) + "...";
			
			} else {
				this.TextUI.GetComponent<TextMesh> ().text = this.text;
			}

			Material mat = Helper.GetViewerMaterial (this.Thumbnail, this.FileWidth, this.FileHeight);
			this.ImageUI.GetComponent<Renderer> ().material = mat;
		}

	}

	public void SetTextVisiable(bool show){
		if (show) {
			if (this.text.Length > this.TextLength) {
				this.TextUI.GetComponent<TextMesh> ().text = this.text.Substring (0, this.TextLength - 1) + "...";

			} else {
				this.TextUI.GetComponent<TextMesh> ().text = this.text;
			}
		} else {
			this.TextUI.GetComponent<TextMesh> ().text = string.Empty;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
