// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Main"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Rough("Rough", Range( 0 , 1)) = 0
		_HSV("HSV", Vector) = (1,1,1,0)
		_Lighten("Lighten", Range( 0 , 0.2)) = 0
		[Toggle]_UsePattern("Use Pattern", Float) = 0
		_Pattern("Pattern", 2D) = "white" {}
		_PatternBlend("Pattern Blend", Float) = 0
		_PatternScale("Pattern Scale", Float) = 1
		_PatternNormalStr("Pattern Normal Str", Float) = 0
		_DistortionScale("Distortion Scale", Float) = 0
		_DistortionStr("Distortion Str", Range( -1 , 1)) = 0
		_Gradient("Gradient", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		uniform sampler2D _Pattern;
		uniform float _DistortionScale;
		uniform float _DistortionStr;
		uniform float _PatternScale;
		uniform float _PatternNormalStr;
		uniform float _UsePattern;
		uniform sampler2D _Texture;
		uniform float4 _Texture_ST;
		uniform float3 _HSV;
		uniform float _Lighten;
		uniform float _PatternBlend;
		uniform float _Gradient;
		uniform float _Metallic;
		uniform float _Rough;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float3 PerturbNormal107_g2( float3 surf_pos, float3 surf_norm, float height, float scale )
		{
			// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
			float3 vSigmaS = ddx( surf_pos );
			float3 vSigmaT = ddy( surf_pos );
			float3 vN = surf_norm;
			float3 vR1 = cross( vSigmaT , vN );
			float3 vR2 = cross( vN , vSigmaS );
			float fDet = dot( vSigmaS , vR1 );
			float dBs = ddx( height );
			float dBt = ddy( height );
			float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
			return normalize ( abs( fDet ) * vN - vSurfGrad );
		}


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 surf_pos107_g2 = ase_worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 surf_norm107_g2 = ase_worldNormal;
			float simplePerlin2D153 = snoise( ase_worldPos.xy*_DistortionScale );
			simplePerlin2D153 = simplePerlin2D153*0.5 + 0.5;
			float temp_output_159_0 = ( simplePerlin2D153 * _DistortionStr );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float lerpResult57 = lerp( tex2D( _Pattern, ( (( ase_worldPos + temp_output_159_0 )).xy * _PatternScale ) ).r , tex2D( _Pattern, ( (( ase_worldPos + temp_output_159_0 )).xz * _PatternScale ) ).r , abs( ase_vertexNormal.y ));
			float lerpResult60 = lerp( lerpResult57 , tex2D( _Pattern, ( (( ase_worldPos + temp_output_159_0 )).yz * _PatternScale ) ).r , abs( ase_vertexNormal.x ));
			float height107_g2 = lerpResult60;
			float scale107_g2 = _PatternNormalStr;
			float3 localPerturbNormal107_g2 = PerturbNormal107_g2( surf_pos107_g2 , surf_norm107_g2 , height107_g2 , scale107_g2 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir42_g2 = mul( ase_worldToTangent, localPerturbNormal107_g2);
			o.Normal = worldToTangentDir42_g2;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 temp_cast_1 = (saturate( ase_vertex3Pos.y )).xxx;
			float3 temp_cast_2 = (ase_vertexNormal.y).xxx;
			float2 uv_Texture = i.uv_texcoord * _Texture_ST.xy + _Texture_ST.zw;
			float3 hsvTorgb2_g3 = RGBToHSV( tex2D( _Texture, uv_Texture ).rgb );
			float3 break8_g3 = _HSV;
			float3 hsvTorgb6_g3 = HSVToRGB( float3(( hsvTorgb2_g3.x * break8_g3.x ),( hsvTorgb2_g3.y * break8_g3.y ),( hsvTorgb2_g3.z * break8_g3.z )) );
			float3 blendOpSrc43 = temp_cast_2;
			float3 blendOpDest43 = hsvTorgb6_g3;
			float3 lerpBlendMode43 = lerp(blendOpDest43,	max( blendOpSrc43, blendOpDest43 ),_Lighten);
			float3 temp_output_43_0 = ( saturate( lerpBlendMode43 ));
			float3 temp_cast_4 = (lerpResult60).xxx;
			float3 blendOpSrc103 = temp_cast_4;
			float3 blendOpDest103 = temp_output_43_0;
			float3 lerpBlendMode103 = lerp(blendOpDest103,2.0f*blendOpDest103*blendOpSrc103 + blendOpDest103*blendOpDest103*(1.0f - 2.0f*blendOpSrc103),_PatternBlend);
			float3 blendOpSrc170 = temp_cast_1;
			float3 blendOpDest170 = (( _UsePattern )?( ( saturate( lerpBlendMode103 )) ):( temp_output_43_0 ));
			float3 lerpBlendMode170 = lerp(blendOpDest170,( blendOpSrc170 * blendOpDest170 ),_Gradient);
			o.Albedo = ( saturate( lerpBlendMode170 ));
			o.Metallic = _Metallic;
			o.Smoothness = ( 1.0 - _Rough );
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*
Сontacts of the author who created this shader:
 
https://vk.com/maximlm_3d
https://gamedev.ru/job/forum/?id=268969
https://www.artstation.com/lapshin

*/