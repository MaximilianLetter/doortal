Shader "Custom/PP/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelsX("Pixels X", float) = 64
        _PixelsY("Pixels Y", float) = 64
        _ColorRange("Color Range", float) = 32

        _ColorTint("Color Tint", Color) = (1,1,1,1)

        _StencilTex("Texture", 2D) = "white" {}

        // 3 == Equal, 6 == NotEqual
       [Enum(Equal, 3, NotEqual, 6)] _StencilTest("Stencil Test", int) = 6
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always
            Tags { "Queue" = "Geometry+1"}

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                uniform float _PixelsX, _PixelsY, _ColorRange;
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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _StencilTex;
            int _StencilTest;

            fixed4 frag(v2f i) : SV_Target
            {
                float check = tex2D(_StencilTex, i.uv);
                if (_StencilTest == 3 && check == 0 || _StencilTest == 6 && check > 0) {
                    return tex2D(_MainTex, i.uv);
                }

                float2 uv = i.uv;
                uv.x *= _PixelsX;
                uv.y *= _PixelsY;
                uv.x = round(uv.x) / _PixelsX;
                uv.y = round(uv.y) / _PixelsY;

                fixed4 col = tex2D(_MainTex, uv);
                col *= _ColorRange;
                col = round(col) / _ColorRange;

                col = half4((col.rgb * (1 - _ColorTint.a) + _ColorTint.rgb * _ColorTint.a), 1);

                return col;
            }
            ENDCG
        }
    }
}
