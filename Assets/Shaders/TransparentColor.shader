// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Custom/TransparentColor" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)

    // 3 == Equal, 6 == NotEqual
	[Enum(Equal, 3, NotEqual, 6)] _StencilTest ("Stencil Test", int) = 6
}

SubShader {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Stencil {
		Ref 1
		Comp [_StencilTest]
	}

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(0)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = _Color;
                return col;
            }
        ENDCG
    }
}

}
