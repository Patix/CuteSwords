Shader "Voxel/WorldPosition"
{
    Properties
    {
        _Atlas ("Texture Atlas", 2D) = "white" {}
        _TileSize ("Tile Size (pixels)", Float) = 64
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        // Transparent blending and depth settings       
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Atlas;
            float4 _Atlas_TexelSize; // x=1/width, y=1/height, z=width, w=height
            float _TileSize; // configurable tile size in pixels
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR; // store tile indices per face
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float tileIndex : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);

                o.tileIndex = v.color.r * 255.0; // encode index in vertex color
                return o;
            }

            float2 AtlasUV(float2 uv, float index)
            {
                // Derive tiles across from texture size: TilesX = width/_TileSize, TilesY = height/_TileSize
                // Therefore, per-tile UV size is _TileSize/width and _TileSize/height
                float tileW = _TileSize / _Atlas_TexelSize.z;
                float tileH = _TileSize / _Atlas_TexelSize.w;

                float tilesX = _Atlas_TexelSize.z / _TileSize;
                float x = fmod(index, tilesX);
                float y = floor(index / tilesX);

                float2 minUV = float2(x * tileW, 1.0 - (y + 1) * tileH);
                float2 maxUV = minUV + float2(tileW, tileH);

                return lerp(minUV, maxUV, uv);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = abs(i.normal);
                float2 uv;

                // face projection like Minecraft
                if (n.y > n.x && n.y > n.z)
                    uv = i.worldPos.xz;
                else if (n.x > n.z)
                    uv = i.worldPos.zy;
                else
                    uv = i.worldPos.xy;
                
                uv = frac(uv); // repeat

                uv = AtlasUV(uv, i.tileIndex);

                fixed4 col = tex2D(_Atlas, uv);
                // Use texture alpha for transparency
                return col;
            }
            ENDCG
        }
    }
}
