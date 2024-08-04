using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(Sprite))]
[CanEditMultipleObjects]
public class SpritePivotInlineEditor : Editor
{
    private Editor          defaultEditor;
    private bool            m_SetBorders                            = false;
    private SpriteAlignment m_PivotAlignment                        = SpriteAlignment.BottomCenter;
    private bool            m_TrimTransparentPixelsWhenSettingPivot = true;
    private Vector2         m_customPivot;
    
    public override void OnInspectorGUI()
    {
     
        m_TrimTransparentPixelsWhenSettingPivot = EditorGUILayout.Toggle("Trim Transparent Pixels when setting pivot", m_TrimTransparentPixelsWhenSettingPivot);
        m_SetBorders                            = EditorGUILayout.Toggle("Set Borders" ,                               m_SetBorders);
        m_PivotAlignment                        = (SpriteAlignment) EditorGUILayout.EnumPopup(m_PivotAlignment);

        if (m_PivotAlignment == SpriteAlignment.Custom)
        {
            m_customPivot = EditorGUILayout.Vector2Field("Custom Normalized Pivot",m_customPivot);
        }

        if (GUILayout.Button("Apply Custom Settings "))
        {
            ApplyCustomSettings();
        }
      
        MakeSureDefaultEditorExists();
        defaultEditor.OnInspectorGUI();
    }

    private void ApplyCustomSettings()
    {
       
        foreach (Sprite sprite in targets)
        {
            var importer   = (TextureImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
            ApplyNewSettings(sprite);
            importer.SaveAndReimport();
        }
    }
    
   
    
   void ApplyNewSettings(Sprite sprite)
   {
       var importer           = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.texture));
       var spriteDataProvider = new SpriteDataProviderFactories().GetSpriteEditorDataProviderFromObject(sprite);
       spriteDataProvider.InitSpriteEditorDataProvider();
       var spriteRect = spriteDataProvider.GetSpriteRects().First(x => x.name == sprite.name).rect;
       
       Undo.RegisterImporterUndo(importer.assetPath,"Change Pivot To Sprite"+sprite.name);
       
       var bounds = spriteRect;
     
       if (m_TrimTransparentPixelsWhenSettingPivot)
       {
           bounds = CalculateNonTransparentBounds(sprite,spriteRect);
       }
       
       if (spriteRect.max != bounds.max)
       {
           bounds.max += Vector2.one; // Pad Bounds by 1 pixel
       }
           
       if (m_SetBorders)
       {
           var leftMargin  = bounds.min;
           var rightMargin = spriteRect.max - bounds.max;
           importer.spriteBorder = new Vector4(leftMargin.x, leftMargin.y, rightMargin.x, rightMargin.y);
       }
       
       SetPivot(m_PivotAlignment,bounds, spriteRect,importer);
      
   }

   private Rect CalculateNonTransparentBounds(Sprite sprite, Rect spriteRect)
   {
       Texture2D texture         = sprite.texture;
          
       if (!texture.isReadable)
       {
           texture = new Texture2D(sprite.texture.width, sprite.texture.height, sprite.texture.format, texture.mipmapCount>1);
           Graphics.CopyTexture(sprite.texture,texture);
       }
           
       Vector2 Min = new(int.MaxValue, int.MaxValue);
       Vector2 Max = new(int.MinValue, int.MinValue);
    
       for (int x = 0; x < spriteRect.width; x++)
       {
           for(int y=0; y<spriteRect.height;y++)
           {
               var (textureCoordX,textureCoordY) = (spriteRect.x + x, spriteRect.y + y);
                   
               if (texture.GetPixel((int)textureCoordX, (int)textureCoordY).a > 0)
               {
                   Min.x = Mathf.Min(x, Min.x);
                   Max.x = Mathf.Max(x, Max.x);
                   Min.y = Mathf.Min(y, Min.y);
                   Max.y = Mathf.Max(y, Max.y);
               }
           }
       }
           
       if(texture!=sprite.texture) DestroyImmediate(texture); //If it's copied tempTexture
       return new Rect { min = Min, max =Max};
   }
    private void SetPivot(SpriteAlignment mPivotAlignment, Rect newBounds , Rect originalBounds, TextureImporter textureImporter)
    {
        Vector2 pixelCoordinates = newBounds.min + newBounds.size * mPivotAlignment switch
        {
            SpriteAlignment.Center       => new Vector2(0.5f, 0.5f),
            SpriteAlignment.TopLeft      => new Vector2(0,    1),
            SpriteAlignment.TopCenter    => new Vector2(0.5f, 1),
            SpriteAlignment.TopRight     => new Vector2(1,    1),
            SpriteAlignment.LeftCenter   => new Vector2(0,    0.5f),
            SpriteAlignment.RightCenter  => new Vector2(1,    0.5f),
            SpriteAlignment.BottomLeft   => new Vector2(0,    0),
            SpriteAlignment.BottomCenter => new Vector2(0.5f, 0),
            SpriteAlignment.BottomRight  => new Vector2(1,    0),
            SpriteAlignment.Custom       => m_customPivot
        };

        textureImporter.spritePivot = new Vector2(pixelCoordinates.x / originalBounds.width, pixelCoordinates.y / originalBounds.height);
    }

    private  void MakeSureDefaultEditorExists()
    {
        if(defaultEditor) return;
        var thisType             = GetType();
        var defaultInspectorType = AllInspectorsForTarget(target).First(x => x != thisType);
        defaultEditor = CreateEditor(targets, defaultInspectorType);
    }

    private  void OnDisable()
    {  
        if (defaultEditor!=null)
        {
            DestroyImmediate(defaultEditor);
            defaultEditor = null;
        }
    }
    private IEnumerable <Type> AllInspectorsForTarget(object target)
    {
        Type targetType = target.GetType();

        var all=  AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(type => type.IsDefined(typeof(CustomEditor), false))
                        .Where(type =>
                        {
                            var attr = type.GetCustomAttribute <CustomEditor>();
                            if (attr != null)
                            {
                                // Using reflection to access the internal m_InspectedType field
                                var field = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (field != null)
                                {
                                    var inspectedType = field.GetValue(attr) as Type;
                                    return inspectedType == targetType;
                                }
                            }

                            return false;
                        }).ToList();
        return all;

    }
   
}
