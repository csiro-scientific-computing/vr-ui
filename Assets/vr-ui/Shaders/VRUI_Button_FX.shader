Shader "VRUI/Button_FX"
{
	Properties
	{
		//Colour Properties
		_IdlingColor("Idle Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_HoveringColor("Hovering Color", Color) = (1.0, 0.0, 1.0, 1.0)
		_ActivatingColor("Activating Color", Color) = (1.0, 0.0, 0.0, 1.0)

		//Icon Properties
		_IconTex("Icon", 2D) = "white" {}
		_IconScale("Icon Scale", Float) = 0.5
		_ButtonFalloff("Button Falloff", Float) = 0.5

		//Interaction States
		_InteractionState("Interaction State", Int) = 0
		_ToggleState("Toggle State", Int) = 0

		//Button Specific
		_ButtonType("Button Type", Int) = 0	//0 - Trigger, 1 - Toggle

		_InteractionThreshold("Interaction Threshold", Float) = 0.5

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

			float _InteractionThreshold;

			int _InteractionState;
			int _ToggleState;
			int _ButtonType;
			float _ButtonFalloff;

			float4 _CursorPos;

			float circle(float2 uv, float2 p, float r)
			{
				float len = length(uv - p) / r;
				return smoothstep(0.0, 1.0, len);
			}

			v2f vert(appdata v)
			{
				//Calculate cursor pos
				float4 cp = _CursorPos;

				//Create vector from vert to cursor pos.
				float4 p = v.vertex - float4(cp.x, 0.0, cp.z, 1.0);

				cp.x = 0.0;
				cp.z = 0.0;

				//Calculate falloff amount
				float falloff = pow(1.0 / length(p), _ButtonFalloff - 0.25) * 0.7;
				falloff = clamp(falloff, 0.0, 1.0);

				//Build Output Struct
				v2f o;
				o.falloff = falloff;
				o.uv = TRANSFORM_TEX(v.uv, _IconTex);

				//Store local pos with falloff offset
				o.localPos = v.vertex + (cp * falloff);// +float4(0.0, r * 22, 0.0, 1.0);

				//Store vertex in clip coords
				o.vertex = UnityObjectToClipPos(o.localPos);

				//Store vertex in world coords
				o.worldPos = UnityObjectToClipPos(o.localPos);

				//Apply Fog
				UNITY_TRANSFER_FOG(o,o.worldPos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//Set base color based on interaction state
				fixed4 col = (_InteractionState == 1) ? _HoveringColor : _IdlingColor;

				//If Button Trigger
				if (_ButtonType == 0)
				{
					col = (_InteractionState == 2) ? _ActivatingColor : col;
				}
				//If Button Toggle
				else if (_ButtonType == 1)
				{
					col = (_ToggleState == 1) ? _ActivatingColor : col;
					if (_InteractionState == 2)
						col = (_ToggleState == 1) ? _IdlingColor : _ActivatingColor;
				}

				//Calculate icon coordinate space
				float2 q = i.uv;
				q -= float2(0.5, 0.5);
				q /= _IconScale;
				q += float2(0.5, 0.5);

				//Sample texture
				float4 t = tex2D(_IconTex, q);
				t.a = (q.x > 1.0 || q.x < 0.0 || q.y > 1.0 || q.y < 0.0) ? 0 : t.a;
				col.xyz = lerp(col.xyz, t.xyz, t.a);

				//Falloff
				float falloff = sin(i.localPos.x * 10);
				float val = (1.0 - i.falloff) * 0.45 + 0.573;

				//Color circle at cursor pos only while hovering or activating
				if (_InteractionState > 0) {
					col *= val;
					col = lerp(col, float4(0.0, 0.0, 0.0, 1.0), (1.0 - circle(i.localPos.xz, _CursorPos.xz, 2.0))*0.25);
				}

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
