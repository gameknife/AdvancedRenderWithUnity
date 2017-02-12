using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class CustomShaderGUI : ShaderGUI
{
    protected void DrawTitleBar(string displaystr, int indent = 0, bool dot = false, Color? dotcolor = null)
    {
        if (dot)
        {
            GUILayout.Space(2);
        }

        GUIStyle bigFont = new GUIStyle(GUI.skin.label);
        bigFont.font = EditorSkinHelper.Instance.sectionStyle.font;
        bigFont.fontSize = 12;

        var rect = EditorGUILayout.GetControlRect();
        rect = new Rect(rect.xMin + 0, rect.yMin, rect.width - 0, rect.height * 1.25f);
        var rectfont = new Rect(rect.xMin, rect.yMin + 2, rect.width, rect.height);
        EditorGUI.DrawRect(rect, new Color(0.0f, 0.0f, 0.0f, 0.5f * (30 - indent) / 30.0f));
        if (dot)
        {
            var hlrect = new Rect(rect.xMin + 2, rect.yMin + 2, (rect.height - 4) / 2, rect.height - 4);
            Color draw = dotcolor ?? new Color(0.8f, 0.7f, 0.1f, 1);
            EditorGUI.DrawRect(hlrect, draw);

            rectfont = new Rect(rectfont.xMin + 20, rectfont.yMin, rectfont.width - 20, rectfont.height);
        }
        EditorGUI.LabelField(rectfont, displaystr, bigFont);
        if (dot)
        {
            GUILayout.Space(5);
        }
    }

    protected void DrawNameLine(string displaystr, string descstr, int indent = 0, int heightOffset = 0)
    {
        GUIStyle bigFont = new GUIStyle(GUI.skin.label);
        bigFont.font = EditorSkinHelper.Instance.sectionStyle.font;
        bigFont.fontSize = 10;
        bigFont.richText = true;

        var rect = EditorGUILayout.GetControlRect();
        rect = new Rect(rect.xMin + indent, rect.yMin, rect.width - indent, rect.height);
        var rectBg = new Rect(rect.xMin, rect.yMin + rect.height, rect.width, rect.height + heightOffset + 2);
        var rectBgLine = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);
        var rectfont = new Rect(rect.xMin, rect.yMin + 2, rect.width, rect.height);
        EditorGUI.DrawRect(rectBg, new Color(0.0f, 0.0f, 0.0f, 0.4f * (30 - indent) / 30.0f));
        EditorGUI.DrawRect(rectBgLine, new Color(0.0f, 0.0f, 0.0f, 0.8f * (30 - indent) / 30.0f));

        if(descstr.Length > 0)
        {
            EditorGUI.LabelField(rectfont, "<color=#ffc600ff>" + displaystr + "</color>" + " | " + descstr, bigFont);
        }
        else
        {
            EditorGUI.LabelField(rectfont, "<color=#ffc600ff>" + displaystr + "</color>", bigFont);
        }
    }

    Dictionary<string, bool> marcos = new Dictionary<string, bool>();

    private Rect GetIndentRect(int indent = 20, int padding = 5)
    {
        Rect rect = EditorGUILayout.GetControlRect();
        rect = new Rect(rect.xMin + indent + padding, rect.yMin, rect.width - indent - padding * 2, rect.height);
        return rect;
    }

    private Rect GetIndentRectTex(int indent = 20, int padding = 5)
    {
        Rect rect = EditorGUILayout.GetControlRect();
        rect = new Rect(rect.xMin + indent + padding, rect.yMin, rect.width - indent - padding * 2, 80);
        return rect;
    }

    override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // get the current keywords from the material
        Material targetMat = materialEditor.target as Material;
        string[] keyWords = targetMat.shaderKeywords;

        foreach (string key in keyWords)
        {
            marcos[key] = true;
        }

        // render the shader properties using the default GUI
        foreach( var prop in properties )
        {
            if(prop.displayName.StartsWith("@"))
            {
                continue;
            }

            string[] wordbase = prop.displayName.Split('#');
            string label = wordbase[0];
            string description = "";
            if(wordbase.Length > 1)
            {
                description = wordbase[1];
            }

            string keyword = "";
            if (wordbase.Length > 2)
            {
                keyword = wordbase[2];
            }
            if (keyword != "")
            {
                if (!keyWords.Contains(keyword))
                {
                    continue;
                }
            }

            switch ( prop.type )
            {
                case MaterialProperty.PropType.Float:
                    {
                        if (prop.name.StartsWith("#Toggle#"))
                        {
                            string[] words = prop.name.Split('#');

                            var rect = EditorGUILayout.GetControlRect();
                            //EditorGUI.DrawRect(rect, new Color(0.0f, 0.0f, 0.0f, 0.5f));
                            if (!marcos.ContainsKey(words[2]))
                            {
                                marcos[words[2]] = false;
                            }
                            marcos[words[2]] = EditorGUI.Toggle(rect, label, marcos[words[2]]);
                        }
                        else if (prop.name.StartsWith("#Group#"))
                        {
                            DrawTitleBar(label, 10, true, new Color(0.1f, 0.7f, 0.9f));
                        }
                        else
                        {
                            DrawNameLine(label, description, 20);
                            materialEditor.FloatProperty(GetIndentRect(), prop, "");
                        }
                    }
                    
                    break;

                case MaterialProperty.PropType.Color:
                    {
                        DrawNameLine(label, description, 20);
                        materialEditor.ColorProperty(GetIndentRect(), prop, "");                
                    }          
                    break;

                case MaterialProperty.PropType.Range:
                    DrawNameLine(label, description, 20);
                    materialEditor.RangeProperty(GetIndentRect(), prop, "");
                    break;

                case MaterialProperty.PropType.Vector:
                    DrawNameLine(label, description, 20, 20);
                    materialEditor.VectorProperty(GetIndentRect(), prop, "");
                    GUILayout.Space(20);
                    break;

                case MaterialProperty.PropType.Texture:
                    DrawNameLine(label, description, 20, 50);
                    //materialEditor.TexturePropertySingleLine(new GUIContent(), prop);
                    materialEditor.TextureProperty(GetIndentRectTex(), prop, "", true);
                    GUILayout.Space(50);


                    break;
                default:
                    materialEditor.DefaultShaderProperty(prop, label);
                    break;
            }
            
        }

        var newkeywords = new List<string> { };
        foreach (var item in marcos)
        {
            if (item.Value)
            {
                newkeywords.Add(item.Key);
            }
        }

        var newkeywordarray = newkeywords.ToArray();

        if (!(keyWords.Length == newkeywordarray.Length && keyWords.Intersect(newkeywordarray).Count() == keyWords.Length))
        {
            targetMat.shaderKeywords = newkeywords.ToArray();
            EditorUtility.SetDirty(targetMat);
        }

    }
}