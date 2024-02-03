Shader "Unlit/FakeLiquid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillAmount ("FillAmount", Range(-1,1)) = 1
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 fillPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FillAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Important note
                // we need to transalte coordinates from local space to world space
                // but we don't need to apply world transformation, so we not using 'w'
                // UPD: this trick needs an actual low point to work properly when the pivot is off center
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0));
                o.fillPosition =  worldPos - _FillAmount;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // for now: because the origin point in my model is in the center
                // we use negative value 
                clip(- i.fillPosition.y);
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
