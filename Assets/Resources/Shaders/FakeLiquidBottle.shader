Shader "Unlit/FakeLiquid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BottleColor ("Bottle Color", Color) = (0,1,0,1)
        _DarkeningStrength ("DarkeningStrength", Range(0, 5)) = 1
        _BottleThickness("BottleThickness", Range(0,1)) = 0.9
        [PerRendererData] _FillAmount("FillAmount", Vector) = (0,0,0,0) 
        [PerRendererData] _Normal("Normal", Vector) = (0,1,0,0) 
    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 fillPosition : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Normal;
            float3 _FillAmount;
            float _DarkeningLevel;
            float _DarkeningStrength;
            float _BottleThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex * float3(_BottleThickness, 1, _BottleThickness));

                // Important note
                // we need to transalte coordinates from local space to world space
                // but we don't need to apply world transformation, so we not using 'w'
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0));

                // get the dot product to understand on which side of the 'wobble' plane vertex is
                // we need to subtract the world pos to get the direction vector from vertex to _FillAmount
                o.fillPosition = dot(_Normal, _FillAmount - worldPos);

                o.normal = v.normal;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                // we are cutting the all fragments where distance from fill point is negative
                clip(i.fillPosition.y);

                //get the top color from texture 
                float4 topColor = tex2D(_MainTex, 1);

                float mappingValue = 1 - i.fillPosition.y / _DarkeningStrength;
                float4 liquidColor = tex2D(_MainTex, mappingValue);

                return facing > 0 ? liquidColor : topColor;
            }
            ENDCG
        }

        Pass
        {
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 fillPosition : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BottleColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            // Fresnel
            float FresnelSimplified(float NdotH)
            {
                return pow(1 - NdotH, 5);
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);

                // float result = FresnelSimplified(dot(i.normal, viewDirection));
                // return _BottleColor + result * _BottleColor ;
                return 0;

            }
            ENDCG

        }
    }
}
