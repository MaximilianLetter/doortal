// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PP/Snow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlendTex ("Image", 2D) = "" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}

		_BlendAmount("Blend Amount", float) = 0.5
		_EdgeSharpness("Edge Sharpness", float) = 1
		_Transparency("Transparency", float) = 0.2
		_Distortion("Distortion", float) = 0.1

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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _BlendTex;
			sampler2D _BumpMap;

			float _BlendAmount;
			float _EdgeSharpness;
			float _Transparency;
			float _Distortion;

			uniform fixed4 _ColorTint;

			sampler2D _StencilTex;
			int _StencilTest;

			half4 frag(v2f i) : COLOR
			{
				float check = tex2D(_StencilTex, i.uv);
				if (_StencilTest == 3 && check == 0 || _StencilTest == 6 && check > 0) {
					return tex2D(_MainTex, i.uv);
				}

				float4 blendColor = tex2D(_BlendTex, i.uv);

				blendColor.a = blendColor.a + (_BlendAmount * 2 - 1);
				blendColor.a = saturate(blendColor.a * _EdgeSharpness - (_EdgeSharpness - 1) * 0.5);

				//Distortion:
				half2 bump = UnpackNormal(tex2D(_BumpMap, i.uv)).rg;
				float4 mainColor = tex2D(_MainTex, i.uv + bump * blendColor.a * _Distortion);

				mainColor = float4((mainColor.rgb * (1 - _ColorTint.a) + _ColorTint.rgb * _ColorTint.a), 1);

				float4 overlayColor = blendColor;
				overlayColor.rgb = mainColor.rgb * (blendColor.rgb + 0.5) * (blendColor.rgb + 0.5); //double overlay

				blendColor = lerp(blendColor,overlayColor,_Transparency);

				return lerp(mainColor, blendColor, blendColor.a);
			}
			ENDCG
		}
	}
} 