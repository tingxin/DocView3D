using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace HelperMethod{
	public static class Helper{

		static Dictionary<string,string> config =new Dictionary<string, string>();
		public static Dictionary<string,string> Config{ get{ return config;}}

		public static Material GetViewerMaterial(string filePath, int fileWidth, int fileHeight){

			//创建文件读取流
			FileStream fileStream = new FileStream (filePath, FileMode.Open, FileAccess.Read);
			fileStream.Seek (0, SeekOrigin.Begin);
			//创建文件长度缓冲区
			byte[] bytes = new byte[fileStream.Length];
			//读取文件
			fileStream.Read (bytes, 0, (int)fileStream.Length);
			//释放文件读取流
			fileStream.Close ();
			fileStream.Dispose ();
			fileStream = null;

			//创建Texture
			Texture2D texture = new Texture2D (fileWidth, fileHeight);
			texture.LoadImage (bytes);

			Material  mat = new Material(Shader.Find("Standard"));
			mat.SetTexture("_MainTex", texture);
			return mat;
		}

		public static bool ReadConfig(){
			string appFolder = System.IO.Directory.GetCurrentDirectory ();
			string settingPath = appFolder + "\\setting.cfg";
			if (File.Exists (settingPath)==false) {
				settingPath = appFolder + "/setting.cfg";
				if (File.Exists (settingPath) == false) {
					return false;
				}
			}

			try{
				string content = System.IO.File.ReadAllText(settingPath);

				string[] keyPairs = content.Split('\n');
				foreach(string keyString in keyPairs){
					string[] keyPair = keyString.Split(':');
					if(keyPair.Length==2){
						string trimKey=keyPair[0].Trim();
						if(trimKey.Length>0){
							Helper.Config.Add(trimKey, keyPair[1]);
						}
					}
				}
			}
			catch(Exception e){
				return false;
			}
			return true;
		}
	}
}
