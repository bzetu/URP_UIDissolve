/*
	卡牌溶解(后期),支持拖拽同时溶解
	1.通过卷积算出边缘
	2.生成黑底白边的图作为溶解图
	3.通过有卡与无关两张颜色缓冲图进行溶解
	4.最后加透明度变化
	create by jiangcheng_m
*/
Shader "Custom/UIDissolve"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}  //有UI界面画面
		_ClipTex("ClipTex", 2D) = "white" {}  //没有UI界面画面
		_DissolveValue("DissolveValue", Range(0,1)) = 0  //溶解值 (0~1) 1完成消失


		_EdgeColor("Edge Color", Color) = (1, 1, 1, 1)  //边缘颜色
		_BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
	}



	SubShader
	{
		CGINCLUDE
		#include "UnityCG.cginc"
		fixed _Debug;
		sampler2D _MainTex;
		sampler2D _ClipTex;
		float4 _MainTex_ST;
		uniform half4 _MainTex_TexelSize; //一个像素大小
		fixed4 _EdgeColor;
		float _DissolveValue; //溶解值(0~1)
		float4 _BoundingBox;  //四个值 x:左上角.x   y:左上角.y   z:右下角.x   w:右下角.y
		//背景颜色
		fixed4 _BackgroundColor;

		struct a2v
		{
			float4 vertex : POSITION;
			half2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			half2 uv[9] : TEXCOORD0;
		};


		v2f vert(a2v  v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			half2 uv = v.texcoord;

			//求卷积核中每个位置的纹理坐标(uv坐标系原点在左下角)
			o.uv[0] = uv + _MainTex_TexelSize.xy * half2(-1, 1); //左上
			o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, 1);  //中上
			o.uv[2] = uv + _MainTex_TexelSize.xy * half2(1, 1);  //右上

			o.uv[3] = uv + _MainTex_TexelSize.xy * half2(-1, 0); //左中
			o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0, 0);  //中中
			o.uv[5] = uv + _MainTex_TexelSize.xy * half2(1, 0);  //右中

			o.uv[6] = uv + _MainTex_TexelSize.xy * half2(-1, -1); //左下
			o.uv[7] = uv + _MainTex_TexelSize.xy * half2(0, -1);  //中下
			o.uv[8] = uv + _MainTex_TexelSize.xy * half2(1, -1);  //右下

			return o;
		}

		//亮度值
		fixed luminance(fixed4 color)
		{
			return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
		}

		//边缘检测Sobel算子
		half Sobel(v2f i)
		{
			const half Gx[9] = { -1, -2, -1,
								0,  0,  0,
								1,  2,  1 };

			const half Gy[9] = { -1,  0,  1,
								-2,  0,  2,
								-1,  0,  1 };

			half texColor;
			half edgeX = 0;
			half edgeY = 0;
			for (int it = 0; it < 9; it++)
			{
				texColor = luminance(tex2D(_MainTex, i.uv[it]));
				edgeX += texColor * Gx[it];
				edgeY += texColor * Gy[it];
			}

			half edge = 1 - abs(edgeX) - abs(edgeY);
			return edge;
		}


		fixed4 frag(v2f i) : SV_Target
		{
			half2 uv = i.uv[4];
			fixed4 color = tex2D(_MainTex, uv);

			if (uv.x >= _BoundingBox.x && uv.x <= _BoundingBox.z && uv.y >= _BoundingBox.w && uv.y <= _BoundingBox.y)
			{
				if (_Debug == 1)
				{
					return fixed4(0, 0, 0, 1);
				}
				  
				//1.生成黑底白边的描边图作为溶解图
				half edge = Sobel(i);
				fixed4 dissolveColor = lerp(_EdgeColor, _BackgroundColor, edge);
				if (_Debug == 2)
				{
					return dissolveColor;
				}
					

				float diss = saturate(dissolveColor.r);
				fixed4 clip_color = tex2D(_ClipTex, uv);
				//2.用生成的溶解图溶解
				fixed4 diss_color = lerp(color, clip_color, step(diss, _DissolveValue)); //step(a, x)	Returns (a < x) ? 0 : 1
				float alpha = 1 - _DissolveValue;
				//3.增加透明度变化
				return alpha * diss_color + (1 - alpha) * clip_color; //透明度计算公式
			}
			return color;
		}
		ENDCG

		Tags{ "RenderType" = "Opaque" }
		Blend One OneMinusSrcAlpha
		Lighting Off
		ZTest Always
		ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag	
			ENDCG
		}
	}
	FallBack "Diffuse"
}

