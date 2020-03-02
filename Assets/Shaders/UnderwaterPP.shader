Shader "Custom/UnderwaterImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", float) = 1
        _NoiseFrequency ("Noise Frequency", float) = 1
        _NoiseSpeed ("Noise Speed", float) = 1
        _PixelOffset ("Pixel Offset", float) = 0.005

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
            #include "noiseSimplex.cginc"
            #define M_PI 3.1415926535897932384626433832795028841971

            uniform float _NoiseFrequency, _NoiseScale, _NoiseSpeed, _PixelOffset;

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

            sampler2D _MainTex;
            sampler2D _StencilTex;
            int _StencilTest;

            fixed4 frag (v2f i) : COLOR
            {
                float check = tex2D(_StencilTex, i.uv);
                if (_StencilTest == 3 && check == 0 || _StencilTest == 6 && check > 0) {
                    return tex2Dproj(_MainTex, i.screenPos);
                }

                float3 sp = float3(i.screenPos.x, i.screenPos.y, 0) * _NoiseFrequency;
                sp.z += _Time.x * _NoiseSpeed;
                float noise = _NoiseScale * ((snoise(sp) + 1) / 2);

                float4 noiseToDirection = float4(cos(noise*M_PI*2), sin(noise*M_PI*2), 0, 0);
                fixed4 col = tex2Dproj(_MainTex, i.screenPos + (normalize(noiseToDirection) * _PixelOffset));

                return col;
            }
            ENDCG
        }
    }
}
