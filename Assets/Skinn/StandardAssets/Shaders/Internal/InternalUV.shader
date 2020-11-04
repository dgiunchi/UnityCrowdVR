﻿Shader "Hidden/Skinn/UV/Unlit"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);

				uv.x = fmod(uv.x, 1);
				uv.y = fmod(uv.y, 1);

                o.uv = uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {

                return fixed4(i.uv, 0, 0);
            }
            ENDCG
        }
    }
		FallBack "Diffuse"
}