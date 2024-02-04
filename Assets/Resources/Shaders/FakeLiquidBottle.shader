Shader "Unlit/FakeLiquid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [PerRendererData] _FillAmount("FillAmount", Vector) = (0,0,0,0) 
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
            float4 _Normal;

            float3 _FillAmount;
            float _WobbleX, _WobbleZ;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Important note
                // we need to transalte coordinates from local space to world space
                // but we don't need to apply world transformation, so we not using 'w'
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0));

                // get the dot product to understand on which side of the 'wobble' plane vertex is
                // we need to subtract the world pos to get the direction vector from vertex to _FillAmount
                o.fillPosition = dot(_Normal, _FillAmount - worldPos);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // we are cutting the all fragments where distance from fill point is negative
                clip(i.fillPosition.y);
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
