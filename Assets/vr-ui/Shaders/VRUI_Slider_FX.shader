Shader "VRUI/Slider_FX"
{
    Properties
    {
        _IdlingColor("Idle Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _HoveringColor("Hovering Color", Color) = (1.0, 0.0, 1.0, 1.0)
        _ActivatingColor("Activating Color", Color) = (1.0, 0.0, 0.0, 1.0)

        _IconScale("Icon Scale", Float) = 0.5
        _SurfaceScale("Surface Scale", Vector) = (1.0, 1.0, 0.0, 0.0)

        _ButtonFalloff("Button Falloff", Float) = 0.5

        _InteractionState("Interaction State", Int) = 0

        _SliderAmount("Slider Amount", Range(0.0, 1.0)) = 0.5

        _InteractionThreshold("Interaction Threshold", Float) = 0.5

        _IconTex("Icon Texture", 2D) = "white" {}
        _CursorPos("Cursor Pos", Vector) = (0.0, 0.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        //Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;

                float4 localPos : TEXCOORD1;
                float4 worldPos : TEXCOORD2;

                float falloff : TEXCOORD3;
            };

            sampler2D _IconTex;
            float4 _IconTex_ST;

            float4 _IdlingColor;
            float4 _HoveringColor;
            float4 _ActivatingColor;

            float _IconScale;
            float4 _SurfaceScale;

            float _InteractionThreshold;

            int _InteractionState;

            float _SliderAmount;

            float _ButtonFalloff;

            float4 _CursorPos;

            float circle(float2 uv, float2 p, float r)
            {
                float l = length(uv - p) / r;
                return smoothstep(0.0, 1.0, l);
            }

            v2f vert(appdata v)
            {
                //Calculate cursor pos
                float4 cp = _CursorPos;

                //Calculate falloff amount
                float4 p = v.vertex - float4(cp.x, 0.0, cp.z, 1.0);
                float2 res = _SurfaceScale.xy;
                p.x *= res.x;
                p.z *= res.y;
                cp.x = 0.0;
                cp.z = 0.0;

                //Calculate falloff
                float falloff = pow(1.0 / length(p), _ButtonFalloff - 0.25) * 0.7;
                falloff = clamp(falloff, 0.0, 1.0);

                //Build Output Struct
                v2f o;
                o.falloff = falloff;
                o.uv = TRANSFORM_TEX(v.uv, _IconTex);

                //Store local pos
                o.localPos = v.vertex + (cp * falloff);
                o.worldPos = mul(unity_ObjectToWorld, o.localPos);

                o.vertex = mul(UNITY_MATRIX_VP, o.worldPos);


                //Apply Fog
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Set base color based on interaction state
                fixed4 col = (_InteractionState == 1) ? _HoveringColor : _IdlingColor;

                //Color based on slider amount
                col = (_SliderAmount.x > i.uv.x) ? _ActivatingColor : col;

                //Calculate icon coordinate space
                float2 q = i.uv;
                q -= float2(0.5, 0.5);
                q /= _IconScale;
                float2 res = _SurfaceScale.xy;
                float ratio = res.x / res.y;
                q.x *= ratio;
                q += float2(0.5, 0.5);

                //Sample texture
                float4 t = tex2D(_IconTex, q);
                t.a = (q.x > 1.0 || q.x < 0.0 || q.y > 1.0 || q.y < 0.0) ? 0 : t.a;
                col.xyz = lerp(col.xyz, t.xyz, t.a);

                //Calculate falloff
                float falloff = sin(i.localPos.x * 10);
                float val = (1.0 - i.falloff) * 0.45 + 0.573;

                //Color circle at cursor position only while hovering or activating
                if (_InteractionState > 0) {
                    col *= val;

                    col = lerp(col, float4(0.0, 0.0, 0.0, 1.0), (1.0 - circle(i.localPos.xz * res, _CursorPos.xz * res, 2.0))*0.25);
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
