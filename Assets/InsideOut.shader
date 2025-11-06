// InsideOut Shader
// Este shader dibuja las caras internas de un objeto en lugar de las externas.
// Perfecto para skyboxes o estudios interiores.

Shader "Custom/InsideOut"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // La línea mágica: "Cull Front" le dice a la GPU que ignore
        // las caras frontales y dibuje solo las traseras.
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}