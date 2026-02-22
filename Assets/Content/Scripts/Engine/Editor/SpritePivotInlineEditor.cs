using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEditor.U2D.Aseprite;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomEditor(typeof(Sprite))]
[CanEditMultipleObjects]
public class SpritePivotInlineEditor : Editor
{
    private Dictionary <string, VisualElement> m_UIElements;
    
    private Editor                             defaultEditor;
    private bool                               m_SetBorders                            = false;
    private SpriteAlignment                    m_PivotAlignment                        = SpriteAlignment.BottomCenter;
    private bool                               m_TrimTransparentPixelsWhenSettingPivot = true;
    private Vector2                            m_customPivot;
   
    private void OnEnable()
    {
        defaultEditor= CreateEditor(targets,Type.GetType("UnityEditor.SpriteInspector, UnityEditor"));
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new();
        
        root.Add(defaultEditor.CreateInspectorGUI());
        
        m_UIElements = new Dictionary <string, VisualElement> {
            ["Trim Toggle"]                           = new Toggle("Trim Transparent Pixels when setting pivot") { value            = m_TrimTransparentPixelsWhenSettingPivot },
            ["Border Toggle"]                         = new Toggle("Set Borders") { value                                           = m_SetBorders },
            ["Pivot DropDown"]                        = new EnumField("Pivot Alignment", m_PivotAlignment) { value                  = m_PivotAlignment },
            ["Custom Pivot"]                          = new Vector2Field("Custom Normalized Pivot") { value                         = m_customPivot },
            ["Apply Settings Button"]                 = new Button(ApplyCustomSettings) { text                                      = "Set Pivot In Current Sprite" },
            ["Copy Sprite Pivot To To Others Button"] = new Button(() => CopySpritePivotToOthersInSameSheet((Sprite)target)) { text = "Clone Pivots From Current To Others in Sheet" }
        };

        (m_UIElements["Trim Toggle"] as Toggle).RegisterValueChangedCallback(ev => m_TrimTransparentPixelsWhenSettingPivot = ev.newValue);
        (m_UIElements["Border Toggle"] as Toggle).RegisterValueChangedCallback(ev => m_SetBorders= ev.newValue);
        (m_UIElements["Pivot DropDown"] as EnumField).RegisterValueChangedCallback(ev =>
        {
            m_PivotAlignment    = (SpriteAlignment)ev.newValue;
            m_UIElements["Custom Pivot"].visible = m_PivotAlignment == SpriteAlignment.Custom;
        });
        
        foreach (VisualElement visualElement in m_UIElements.Values)
        {
            root.Add(visualElement);
        }
        
        return root;
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        return defaultEditor?.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    public override void DrawPreview(Rect previewArea)
    {
        defaultEditor?.DrawPreview(previewArea);
        
    }

    private void ApplyCustomSettings()
    {
        foreach (Sprite sprite in targets)
        {
            SetSpriteSettings(sprite);
        }
    }
    
   void SetSpriteSettings(Sprite sprite)
   {
       Manipulate(sprite, (selectedSprite, allSpriteRects) =>
       {
           //Do Calculations and Update Information
           var spriteRect = selectedSprite.rect;
           var bounds     = spriteRect;
     
           if (m_TrimTransparentPixelsWhenSettingPivot)
           {
               bounds= CalculateNonTransparentBounds(sprite,spriteRect);
           }
       
           if (spriteRect.max != bounds.max)
           {
               bounds.max               += Vector2.one; // Pad Bounds by 1 pixel
               selectedSprite.alignment =  SpriteAlignment.Custom;
           }

           else
           {
               selectedSprite.alignment = m_PivotAlignment;
           }
           
           if (m_SetBorders)
           {
               var leftMargin  = bounds.min;
               var rightMargin = spriteRect.max - bounds.max;
               selectedSprite.border = new Vector4(leftMargin.x, leftMargin.y, rightMargin.x, rightMargin.y);
           }
       
           SetPivot(selectedSprite,m_PivotAlignment,bounds, spriteRect);
       } );
   }
   
   private void CopySpritePivotToOthersInSameSheet(Sprite selectedSprite)
   {
      Manipulate(selectedSprite, (selectedSpriteRect, allSpriteRects) => 
      {
          foreach (SpriteRect otherSpriteRect in allSpriteRects.Where(x=>x!=selectedSpriteRect))
          {
              otherSpriteRect.alignment = selectedSpriteRect.alignment;
              otherSpriteRect.pivot     = selectedSpriteRect.pivot;
          }
      });
   }

  

   private Rect CalculateNonTransparentBounds(Sprite sprite, Rect spriteRect)
   {
       Texture2D texture         = sprite.texture;
          
       if (!texture.isReadable)
       {
           texture = new Texture2D(sprite.texture.width, sprite.texture.height, sprite.texture.format, texture.mipmapCount>1);
           Graphics.CopyTexture(sprite.texture,texture);
       }
           
       Vector2 min = new(int.MaxValue, int.MaxValue);
       Vector2 max = new(int.MinValue, int.MinValue);
    
       for (int x = 0; x < spriteRect.width; x++)
       {
           for(int y=0; y<spriteRect.height;y++)
           {
               var (textureCoordX,textureCoordY) = (spriteRect.x + x, spriteRect.y + y);
                   
               if (texture.GetPixel((int)textureCoordX, (int)textureCoordY).a > 0)
               {
                   min.x = Mathf.Min(textureCoordX, min.x);
                   max.x = Mathf.Max(textureCoordX, max.x);
                   min.y = Mathf.Min(textureCoordY, min.y);
                   max.y = Mathf.Max(textureCoordY, max.y);
               }
           }
       }

       return new Rect { min = min, max = max };
   }
    private void SetPivot(SpriteRect selectedSpriteRectData,SpriteAlignment mPivotAlignment, Rect newBounds , Rect originalBounds)
    {
        var standardNormalizedPivotPosition = mPivotAlignment switch
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
     
        var pivotPixelCoordinatesInClippedBounds          = Rect.NormalizedToPoint(newBounds,standardNormalizedPivotPosition);
        var pivotNormalizedCoordinatesRelativeToOldBounds = Rect.PointToNormalized(originalBounds,pivotPixelCoordinatesInClippedBounds);
        
        selectedSpriteRectData.pivot = pivotNormalizedCoordinatesRelativeToOldBounds;
    }
  
    private static void Manipulate(Sprite sprite, Action <SpriteRect,SpriteRect[]> action)
    {
        
        //Read Sprite Settings From Importer
        var importer           = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.texture));
        var spriteDataProvider = new SpriteDataProviderFactories().GetSpriteEditorDataProviderFromObject(sprite);
        spriteDataProvider.InitSpriteEditorDataProvider();
        var allSpriteRects = spriteDataProvider.GetSpriteRects();
        var spriteData     = allSpriteRects.First(x => x.name == sprite.name);
        
        //Get Ready To Change 
        Undo.RegisterImporterUndo(importer.assetPath,"Copy Pivot to other sprites");
        
        //Do Desired Action
        action.Invoke(spriteData,allSpriteRects);
        
        //Apply And Save
        spriteDataProvider.SetSpriteRects(allSpriteRects);
        spriteDataProvider.Apply();
        importer.SaveAndReimport();
        
    }
    
    private  void OnDisable()
    {  
        if (defaultEditor!=null)
        {
            defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance| BindingFlags.NonPublic)?.Invoke(defaultEditor, null);
            DestroyImmediate(defaultEditor);
            defaultEditor = null;
        }
    }
}
