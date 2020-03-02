Shader "Custom/PlainColor" 
{
	SubShader
	{
		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			half4 frag(v2f_img i) : COLOR
			{
				return half4(1,1,1,1);
			}
			
			ENDCG
		}
	}
}