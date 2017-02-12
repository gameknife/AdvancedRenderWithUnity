using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorSkinHelper {

	static EditorSkinHelper s_instance = null;
	static public EditorSkinHelper Instance {
		get {
			if(s_instance == null)
			{
				s_instance = new EditorSkinHelper();
			}
			return s_instance;
		}
	}

	public GUISkin proSkin = null;

	public GUIStyle sectionStyle = null;
	public GUIStyle groupStyle = null;
	public GUIStyle titleStyle = null;
	public EditorSkinHelper() {
		proSkin = AssetDatabase.LoadAssetAtPath("Assets/Util/GUISkins/DarkSkin.guiskin", typeof(GUISkin)) as GUISkin;
		sectionStyle = proSkin.FindStyle("Section Label");
		groupStyle = proSkin.FindStyle("Group Label");
		titleStyle = proSkin.FindStyle("Prop Title");
	}
}
