using System;
using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [CustomEditor(typeof(ColorCorrectionLookup))]
    public class ColorCorrectionLookupEditor : Editor
    {
        private SerializedObject serObj;
        private Texture2D tempClutTex2D;

        void OnEnable()
        {
            serObj = new SerializedObject(target);
        }

        public override void OnInspectorGUI()
        {
            serObj.Update();

            var ccLookup = (ColorCorrectionLookup)target;

            EditorGUILayout.LabelField("Converts textures into color lookup volumes (for grading)", EditorStyles.miniLabel);

            tempClutTex2D = EditorGUILayout.ObjectField(" Based on", tempClutTex2D, typeof(Texture2D), false) as Texture2D;

            if (tempClutTex2D == null && !string.IsNullOrEmpty(ccLookup.basedOnTempTex))
            {
                Texture2D loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(ccLookup.basedOnTempTex);
                if (loadedTex != null) tempClutTex2D = loadedTex;
            }

            if (tempClutTex2D != null && ccLookup.basedOnTempTex != AssetDatabase.GetAssetPath(tempClutTex2D))
            {
                EditorGUILayout.Space();

                if (!ccLookup.ValidDimensions(tempClutTex2D))
                {
                    EditorGUILayout.HelpBox("Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.", MessageType.Warning);
                }
                else if (GUILayout.Button("Convert and Apply"))
                {
                    string path = AssetDatabase.GetAssetPath(tempClutTex2D);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (textureImporter != null)
                    {
                        bool doImport = false;

                        if (!textureImporter.isReadable) doImport = true;
                        if (textureImporter.mipmapEnabled) doImport = true;

                        // ✅ Kiểm tra và chỉnh compression trực tiếp từ TextureImporter
                        if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                        {
                            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                            doImport = true;
                        }

                        if (doImport)
                        {
                            textureImporter.isReadable = true;
                            textureImporter.mipmapEnabled = false;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }
                    }
                    ccLookup.Convert(tempClutTex2D, path);
                }
            }

            if (!string.IsNullOrEmpty(ccLookup.basedOnTempTex))
            {
                EditorGUILayout.HelpBox("Using: " + ccLookup.basedOnTempTex, MessageType.Info);

                Texture2D previewTex = AssetDatabase.LoadAssetAtPath<Texture2D>(ccLookup.basedOnTempTex);
                if (previewTex != null)
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    r = GUILayoutUtility.GetRect(r.width, 20);
                    r.x += r.width * 0.05f / 2.0f;
                    r.width *= 0.95f;
                    GUI.DrawTexture(r, previewTex, ScaleMode.ScaleToFit);
                    GUILayoutUtility.GetRect(r.width, 4);
                }
            }

            serObj.ApplyModifiedProperties();
        }
    }
}
