Shader "Custom/StencilTextureTesting"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ColorTint("Color Tint", Color) = (1,1,1,1)

        //_BufferTex ("Texture", 2D) = "white" {}
        _StencilTex ("Texture", 2D) = "white" {}

        // 3 == Equal, 6 == NotEqual
	    [Enum(Equal, 3, NotEqual, 6)] _StencilTest ("Stencil Test", int) = 6
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Tags { "Queue"="Geometry+1"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform fixed4 _ColorTint;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            //sampler2D _BufferTex;
            sampler2D _MainTex;
            sampler2D _StencilTex;
            int _StencilTest;

            fixed4 frag (v2f i) : COLOR
            {
                float check = tex2D(_StencilTex, i.uv);
                if (_StencilTest == 3 && check == 0 || _StencilTest == 6 && check > 0) {
                    return tex2Dproj(_MainTex, i.screenPos);
                }

                fixed4 col = tex2Dproj(_MainTex, i.screenPos);

                col = half4((col.rgb * (1 - _ColorTint.a) + _ColorTint.rgb * _ColorTint.a), 1);

                return col;
            }
            ENDCG
        }
    }
}
