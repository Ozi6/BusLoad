Shader "Custom/HemisphereUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,0.6,0.2,1)
        _CutY ("Cut plane (object Y)", Float) = 0.0
        _RimPower ("Rim Power", Range(0.5,8)) = 2.0
        _RimIntensity ("Rim Intensity", Range(0,2)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Back
        ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _CutY;
            float _RimPower;
            float _RimIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 objPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.objPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (i.objPos.y < _CutY)
                    discard;

                float3 viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(i.objPos,1)).xyz);
                float rim = pow(1.0 - saturate(dot(normalize(i.worldNormal), viewDir)), _RimPower) * _RimIntensity;

                fixed4 col = _Color;
                col.rgb += rim;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
