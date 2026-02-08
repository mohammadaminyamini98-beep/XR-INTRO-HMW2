Shader "Custom/RT_UVRotate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Angle ("Angle (Radians)", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Angle;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv_image : TEXCOORD0; // Zoomed/Rotated for picture
                float2 uv_shape : TEXCOORD1; // Original 0-1 for circle shape
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Rotate Function
            float2 RotateUV(float2 uv, float angle)
            {
                float2 center = float2(0.5, 0.5);
                uv -= center;
                float s = sin(angle);
                float c = cos(angle);
                float2 r;
                r.x = uv.x * c - uv.y * s;
                r.y = uv.x * s + uv.y * c;
                return r + center;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // 1. Calculate Image UVs (Apply Tiling 0.7 from Material)
                o.uv_image = TRANSFORM_TEX(v.uv, _MainTex);
                
                // 2. Calculate Shape UVs (Force 0-1 range)
                o.uv_shape = v.uv; 

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // --- CIRCLE CUT LOGIC ---
                // Use the 'uv_shape' (0-1) so it ignores Tiling settings
                float dist = distance(i.uv_shape, float2(0.5, 0.5));
                if (dist > 0.5) discard; 
                // ------------------------

                // --- IMAGE ROTATION LOGIC ---
                // Use 'uv_image' (0.7 zoomed) to prevent glitches
                float2 uv = RotateUV(i.uv_image, _Angle);
                
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}