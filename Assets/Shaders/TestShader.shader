// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TestShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StencilTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "Queue"="Geometry+1"}

        Pass {
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _StencilTex;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                //fixed4 stencil = tex2D(_StencilTex, i.uv);
                //fixed4 black = fixed4(0,0,0,1);
                //if (stencil != black)
                //    return tex2D(_MainTex, i.uv);
                
                //return black;

                float check = tex2D(_StencilTex, i.uv);
                if (check > 0) {
                    return tex2D(_MainTex, i.uv);   
				}

                return tex2D(_StencilTex, i.uv);

            }
            ENDCG
        }
    }
}