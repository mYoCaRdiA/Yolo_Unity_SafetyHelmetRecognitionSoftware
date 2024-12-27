Shader "Custom/BorderOnlyShader" {
      Properties
    {
        _BorderSize("Border Size", Float) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            float _BorderSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Border logic
                float alpha = step(_BorderSize, i.uv.x) * step(_BorderSize, i.uv.y) * step(i.uv.x, 1.0-_BorderSize) * step(i.uv.y, 1.0-_BorderSize);
                alpha = 1.0 - alpha;

                return fixed4(i.color.rgb, alpha * i.color.a);
            }
            ENDCG
        }
    }
}
