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

            float3 _FillAmount;
            float _WobbleX, _WobbleZ;

            // https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Rotate-About-Axis-Node.html
            float3 Unity_RotateAboutAxis_Degrees(float3 In, float3 Axis, float Rotation)
            {
                Rotation = radians(Rotation);
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);
                float3x3 rot_mat = 
                {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                float3 Out = mul(rot_mat,  In);
                return Out;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Important note
                // we need to transalte coordinates from local space to world space
                // but we don't need to apply world transformation, so we not using 'w'
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0));

                // rotate it around XY and XZ
                float3 worldPosX= Unity_RotateAboutAxis_Degrees(worldPos, float3(0,0,1),90);
                float3 worldPosZ = Unity_RotateAboutAxis_Degrees(worldPos, float3(1,0,0),90);
                // combine rotations with worldPos, based on sine wave from script
                float3 worldPosAdjusted = worldPos + worldPosX  * _WobbleX + worldPosZ * _WobbleZ;

                o.fillPosition = _FillAmount - worldPosAdjusted;

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
