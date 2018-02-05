// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/lightingModelNoLightSimple"
{
	Properties {
        _Color ("Color", Color) = (1,1,1,1)
     	_ColorBright ("Light Bright Color", Color) = (1,1,1,1)
//      	_ColorDark ("Light Dark Color", Color) = (1,1,1,1)
      	//_LightPos("Light Position",Vector) = (0,0,0,0)
      	
		_Tile ("Tiling", Float) = 12
		_PTile1 ("PatternTiling", Float) = 12
		_PTile2 ("PatternTiling", Float) = 12
		_PTile3 ("PatternTiling", Float) = 12
		
		_MainTex ("Shading In Alpha, RGB Pattern", 2D) = "white" {}
		_MainTex2 ("Palette (RGB)", 2D) = "white" {}
		
		_CrossTex1("cross hatch", 2D) = "white" {}
		_Pattern1("pattern Red", 2D) = "white" {}
		_Pattern2("pattern Green", 2D) = "white" {}
		_Pattern3("pattern Blue", 2D) = "white" {}
		
		_ShowMask("Show Mask",float)=0 
		
    }
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			//#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
				
			sampler2D _MainTex;
			sampler2D _MainTex2;
			sampler2D _CrossTex1;
			
			sampler2D _Pattern1;
			sampler2D _Pattern2;
			sampler2D _Pattern3;
			
			//float4 _LightPos;
			
		    fixed4 _Color;
		    fixed4 _ColorBright;
//		    fixed4 _ColorDark;
		    
			fixed4 _DarkColor;
			fixed4 _MultColor;
			float _Tile;
			float _PTile1;
			float _PTile2;
			float _PTile3;
			float _ShowMask;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				
//				fixed3 mainTexture;
//				fixed3 colorTex;
//				fixed3 pattern;
//				fixed4 tile1;
//				float pos;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 vertex : SV_POSITION;
				
//				fixed3 mainTexture;
//				fixed3 colorTex;
//				fixed3 pattern;
//				fixed4 tile1;
//				float pos;
			};

			float4 _MainTex_ST;
			float dis(float3 a, float3 b){
				float A = b.x-a.x;
				float B = b.y-a.y;
				float C = b.z-a.z;
				return A*A+B*B+C*C;
			}
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
//				o.pos = max(0.0,min(1.0,1.0-(dis(mul (_Object2World, v.vertex).xyz,_LightPos.xyz)*_LightPos.w*.0001)));
				o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = v.uv2;//TRANSFORM_TEX(v.uv2, _MainTex);
//				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 f4(float a){
				return fixed4(a,a,a,a);
			}
			
			fixed4 f4(float a, float b){
				return fixed4(a,a,a,b);
			}
			
			fixed3 f3(float a){
				return float3(a,a,a);
			}
			
			fixed2 f2(float a){
				return float2(a,a);
			}
			
			fixed f4(fixed3 a, float b){
				return fixed4(a.x,a.y,a.z,b);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = f4(.5);
				
				//color matte
		    	float4 tp = tex2D (_MainTex, i.uv).rgba;
		    	
		    	float3 pat1 = tex2D (_Pattern1, i.uv*_PTile1).rgb;
		    	float3 pat2 = tex2D (_Pattern2, i.uv*_PTile2).rgb;
		    	float3 pat3 = tex2D (_Pattern3, i.uv*_PTile3).rgb;
		    	
		    	//palette color
		        fixed4 mainTexture = tex2D (_MainTex2, i.uv2);
		  
		        fixed4 pattern =
		        (
		        (fixed4(pat1.r,pat1.g,pat1.b,1.0)*tp.r)+
		        (fixed4(pat2.r,pat2.g,pat2.b,1.0)*tp.g)+
		        (fixed4(pat3.r,pat3.g,pat3.b,1.0)*tp.b)+
		        (1.0-(fixed4(tp.r,tp.r,tp.r,1.0)+fixed4(tp.g,tp.g,tp.g,1.0)+fixed4(tp.b,tp.b,tp.b,1.0))))*
		        mainTexture;
		              
		        float4 tile1 = tex2D (_CrossTex1, i.uv*_Tile).rgba;
        
				float4 c;
				c.rgb = pattern;
				c.a = tp.r;
				
//				float4 emit = lerp(float4(0,0,0,0),
//					   lerp(float4(tile1.r,tile1.r,tile1.r,1.0),
//					   lerp(float4(tile1.g,tile1.g,tile1.g,1.0),
//					   lerp(float4(tile1.b,tile1.b,tile1.b,1.0),
//					   lerp(float4(tile1.a,tile1.a,tile1.a,1.0), float4(1,1,1,1), 
//					   		clamp((_Color.a*2.0*c.a-0.75)*10.0, 0.0, 1.0)),
//					   		clamp((_Color.a*2.0*c.a-0.6)*10.0, 0.0, 1.0)),
//					   		clamp((_Color.a*2.0*c.a-0.45)*10.0, 0.0, 1.0)),
//					   		clamp((_Color.a*2.0*c.a-0.3)*10.0, 0.0, 1.0)),
//					   		clamp((_Color.a*2.0*c.a-0.15)*10.0, 0.0, 1.0));
//				
//				float dist = 1.0;//i.pos;
				
				float shadow = lerp(tile1.r,1.0,max(0.0,min(1.0,_Color.a*1.8-(.8-tp.r))));
				float4 L = lerp(_Color*float4(c.r,c.g,c.b,1.0),float4(c.r,c.g,c.b,1.0),shadow);
				fixed4 LColor = _ColorBright*_ColorBright.a*2.;//lerp(_ColorDark*_ColorDark.a*2.,_ColorBright*_ColorBright.a*2.,dist);		
					
				return tp*L*LColor;//*shadow;//lerp(L*LColor,tp,_ShowMask);
			}
			ENDCG
		}
	}
}
