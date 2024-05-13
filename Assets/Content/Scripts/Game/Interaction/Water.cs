using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Interaction
{
    public class Water : MonoBehaviour
    {
        [SerializeField] private float                                           WaterHeight;
        [SerializeField] private Material                                        m_waterMaterial;

        public  Collider2D                                      waterCollider;
        private Dictionary <Rigidbody2D, WaterInteractedObject> Interacted = new();

        private void Awake()
        {
            waterCollider = GetComponent <Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.attachedRigidbody) Interacted.Add(other.attachedRigidbody, new WaterInteractedObject(other.attachedRigidbody, this));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.attachedRigidbody && Interacted.TryGetValue(other.attachedRigidbody, out var sunkObject))
            {
                sunkObject.Destroy();
                Interacted.Remove(other.attachedRigidbody);
            }
          
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.attachedRigidbody && Interacted.TryGetValue(other.attachedRigidbody, out var sunkObject))
            {
                sunkObject.Update();
            }
        }

        public class WaterInteractedObject
        {
            public  Water                 Water;
            private Rigidbody2D           rigidbody2D;
            private RenderData[]          spriteRenderersData;
            private float                 MinYBoundOfRenderer => spriteRenderersData.Min(x => x.spriteRenderer.bounds.min.y);
            private float                 SinkPosition        => Math.Clamp(MinYBoundOfRenderer + Water.WaterHeight, Water.waterCollider.bounds.min.y, Water.waterCollider.bounds.max.y);
            private bool                  IsInWaterBounds     => MinYBoundOfRenderer >= Water.waterCollider.bounds.min.y;

            public WaterInteractedObject(Rigidbody2D rigidbody2D , Water water)
            {
                this.rigidbody2D   = rigidbody2D;
                this.Water         = water;
                SaveData();
                foreach (var renderData in spriteRenderersData)
                {
                    renderData.SetWaterShaded(water.m_waterMaterial);
                }
            }

            public void Update()
            {
                var sinkPosition = SinkPosition;
                if (!IsInWaterBounds)
                {
                    Destroy();
                    return;
                }
                
                foreach (var renderData in spriteRenderersData)
                {
                    renderData.waterPropertyBlock.SetFloat("_ClipUnderY", sinkPosition);
                    renderData.waterPropertyBlock.SetFloat("_Rotation",   -rigidbody2D.transform.eulerAngles.z);
                    renderData.spriteRenderer.SetPropertyBlock(renderData.waterPropertyBlock);
                    
                }
            }

            public void Destroy()
            {
                foreach (var renderData in spriteRenderersData)
                {
                    renderData.RestoreOriginal();
                }
            }

            void SaveData()
            {
                var spriteRenderers = rigidbody2D.transform.GetComponentsInChildren <SpriteRenderer>(true);
                if (spriteRenderersData == null || spriteRenderersData.Length < spriteRenderers.Length)
                    spriteRenderersData = new RenderData[spriteRenderers.Length];

                for (var i = 0; i < spriteRenderersData.Length; i++)
                {
                    spriteRenderersData[i] = new RenderData(spriteRenderers[i]);
                }
            }

            private class RenderData
            {
                public SpriteRenderer        spriteRenderer;
                public Material              originalMaterial;
                public MaterialPropertyBlock originalPropertyBlock;
                public MaterialPropertyBlock waterPropertyBlock;
                public RenderData(SpriteRenderer spriteRenderer) => SaveOriginal(spriteRenderer);

                private void SaveOriginal(SpriteRenderer spriteRenderer)
                {
                    this.spriteRenderer = spriteRenderer;
                    originalMaterial    = spriteRenderer.sharedMaterial;

                    if (spriteRenderer.HasPropertyBlock())
                    {
                        originalPropertyBlock = new MaterialPropertyBlock();
                        waterPropertyBlock    = new MaterialPropertyBlock();
                        this.spriteRenderer.GetPropertyBlock(waterPropertyBlock);
                        this.spriteRenderer.GetPropertyBlock(originalPropertyBlock);
                    }
                }

                public void RestoreOriginal()
                {
                    spriteRenderer.sharedMaterial = originalMaterial;
                    if (originalPropertyBlock != null)
                        spriteRenderer.SetPropertyBlock(originalPropertyBlock);
                }

                public void SetWaterShaded(Material waterMaterial)
                {
                    spriteRenderer.sharedMaterial = waterMaterial;
                    spriteRenderer.SetPropertyBlock(waterPropertyBlock);
                }
            }
        }
    }
}