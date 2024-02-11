Shader "Unlit/FakeLiquid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _BottleColor ("Bottle Color", Color) = (0,1,0,1)
        _DarkeningStrength ("Darkening Strength", Range(0, 5)) = 1
        _BottleThickness("Bottle Thickness", Range(0,1)) = 0.9
        _SplashStrength("Splash Strength", Float) = 5

        [PerRendererData] _FillAmount("Fill Amount", Vector) = (0,0,0,0) 
        [PerRendererData] _FlowDirection("Flow Direction", Vector) = (0,1,0,0) 
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
                float fillPosition : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float2 uv2 : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NoiseTexture;
            float4 _NoiseTexture_ST;

            float4 _FlowDirection;
            float3 _FillAmount;
            float _DarkeningLevel;
            float _DarkeningStrength;
            float _BottleThickness;
            float _SplashStrength;

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
                o.fillPosition = dot(_FlowDirection, _FillAmount - worldPos);

                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _NoiseTexture);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                //get the power of splash by dot of up vector and flow direction
                float splashDistortion = 1 - dot(_FlowDirection, float3(0,1,0));
                // adjusting power of splash and sampling from Perlin noise
                i.fillPosition =  i.fillPosition - tex2D(_NoiseTexture, i.uv2) * splashDistortion * _SplashStrength;

                // we are cutting the all fragments where distance from fill point is negative
                clip(i.fillPosition);

                float foam = step(i.fillPosition, 0.1);

                //get the top color from texture 
                float4 topColor = tex2D(_MainTex, 1) + 0.3;

                float mappingValue = 1 - i.fillPosition / _DarkeningStrength;
                float4 liquidColor = tex2D(_MainTex, mappingValue);

                float4 resultColor = (topColor + 0.4) * foam + liquidColor * (1 - foam);

                float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1 - dot(i.normal, viewDirection), 5);
                resultColor += fresnel;

                return facing > 0 ? resultColor : topColor;
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
                float fillPosition : TEXCOORD1;
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
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);

                float fresnel = pow(1 - dot(i.normal, viewDirection), 5);
                return  fresnel * _BottleColor ;
                // return 0;

            }
            ENDCG

        }
    }
}
