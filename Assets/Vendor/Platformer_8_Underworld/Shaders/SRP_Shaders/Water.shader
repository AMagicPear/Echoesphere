// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		_ColorSurface("Color Surface", Color) = (0.0509804,0.6705883,1,1)
		_ColorDepth("Color Depth", Color) = (0.01568628,0.4196079,0.6313726,1)
		_Metallic("Metallic", Range( 0 , 1)) = 0.1
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.9
		_Depth("Depth", Float) = 5
		_DepthMultiply("Depth Multiply", Float) = 2
		_FoamWidth("Foam Width", Float) = 0.4
		_FoamScale("Foam Scale", Float) = 4
		_FoamDistortionSpeed("Foam Distortion Speed", Float) = 1.5
		_FoamDistortionScale("Foam Distortion Scale", Float) = 4
		_FoamDistortionAmount("Foam Distortion Amount", Range( 0 , 0.1)) = 0.03
		_FresnelBias("Fresnel Bias", Float) = 0
		_FresnelScale("Fresnel Scale", Range( 0 , 1)) = 0.1
		_FresnelPower("Fresnel Power", Float) = 5
		_RefractionDirection("Refraction Direction", Vector) = (0,1,0,0)
		_RefractionSpeed("Refraction Speed", Float) = 1
		_RefractionTillingNoise("Refraction Tilling Noise", Vector) = (0.3,1,0,0)
		_RefractionScale("Refraction Scale", Float) = 0.15
		_RefractionStr("Refraction Str", Range( 0 , 0.1)) = 0.04
		_WaveSmallSpeed("Wave Small Speed", Float) = 1.7
		_WaveSmallScale("Wave Small Scale", Float) = 1.6
		_WaveSmallPower("Wave Small Power", Float) = 4.4
		_WaveBigSpeed("Wave Big Speed", Float) = 1.5
		_WaveBigScale("Wave Big Scale", Float) = 0.9
		_WaveBigPower("Wave Big Power", Float) = 3.8
		_WavesMultiply("Waves Multiply", Float) = 3
		_WavesNormalStr("Waves Normal Str", Float) = 0
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 30
		_TessMaxDisp( "Max Displacement", Float ) = 25
		_DisplacementAmount("Displacement Amount", Float) = 0.4
		_ShoreWaveWidth("Shore Wave Width", Float) = 0.05
		_ShoreWidth("Shore Width", Float) = 1
		_ShoreWaveSpeed("Shore Wave Speed", Float) = -1.8
		_ShoreWaveScale("Shore Wave Scale", Float) = 2.4
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform float _WaveSmallScale;
		uniform float _WaveSmallSpeed;
		uniform float _WaveBigScale;
		uniform float _WaveBigSpeed;
		uniform float _DisplacementAmount;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WavesNormalStr;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _RefractionSpeed;
		uniform float2 _RefractionDirection;
		uniform float2 _RefractionTillingNoise;
		uniform float _RefractionScale;
		uniform float _RefractionStr;
		uniform float4 _ColorSurface;
		uniform float4 _ColorDepth;
		uniform float _Depth;
		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float _DepthMultiply;
		uniform float _ShoreWaveWidth;
		uniform float _ShoreWaveSpeed;
		uniform float _ShoreWaveScale;
		uniform float _ShoreWidth;
		uniform float _FoamWidth;
		uniform float _FoamDistortionSpeed;
		uniform float _FoamDistortionScale;
		uniform float _FoamDistortionAmount;
		uniform float _FoamScale;
		uniform float _WaveSmallPower;
		uniform float _WaveBigPower;
		uniform float _WavesMultiply;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform float _EdgeLength;
		uniform float _TessMaxDisp;


		float2 voronoihash130( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi130( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash130( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			
			 		}
			 	}
			}
			return F1;
		}


		float2 voronoihash109( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi109( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash109( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			
			 		}
			 	}
			}
			return F1;
		}


		float3 PerturbNormal107_g6( float3 surf_pos, float3 surf_norm, float height, float scale )
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


		float3 PerturbNormal107_g1( float3 surf_pos, float3 surf_norm, float height, float scale )
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


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength , _TessMaxDisp );
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float mulTime124 = _Time.y * _WaveSmallSpeed;
			float time130 = mulTime124;
			float2 voronoiSmoothId130 = 0;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 coords130 = (ase_worldPos).xz * _WaveSmallScale;
			float2 id130 = 0;
			float2 uv130 = 0;
			float voroi130 = voronoi130( coords130, time130, id130, uv130, 0, voronoiSmoothId130 );
			float mulTime112 = _Time.y * _WaveBigSpeed;
			float time109 = mulTime112;
			float2 voronoiSmoothId109 = 0;
			float2 coords109 = (ase_worldPos).xz * _WaveBigScale;
			float2 id109 = 0;
			float2 uv109 = 0;
			float voroi109 = voronoi109( coords109, time109, id109, uv109, 0, voronoiSmoothId109 );
			float temp_output_314_0 = max( voroi130 , voroi109 );
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth317 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_LOD( _CameraDepthTexture, float4( ase_screenPosNorm.xy, 0, 0 ) ));
			float distanceDepth317 = abs( ( screenDepth317 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 5.0 ) );
			float lerpResult319 = lerp( 0.0 , ( temp_output_314_0 * _DisplacementAmount ) , saturate( distanceDepth317 ));
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( lerpResult319 * ase_worldNormal.y ) * ase_vertexNormal );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 surf_pos107_g6 = ase_worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 surf_norm107_g6 = ase_worldNormal;
			float mulTime124 = _Time.y * _WaveSmallSpeed;
			float time130 = mulTime124;
			float2 voronoiSmoothId130 = 0;
			float2 coords130 = (ase_worldPos).xz * _WaveSmallScale;
			float2 id130 = 0;
			float2 uv130 = 0;
			float voroi130 = voronoi130( coords130, time130, id130, uv130, 0, voronoiSmoothId130 );
			float mulTime112 = _Time.y * _WaveBigSpeed;
			float time109 = mulTime112;
			float2 voronoiSmoothId109 = 0;
			float2 coords109 = (ase_worldPos).xz * _WaveBigScale;
			float2 id109 = 0;
			float2 uv109 = 0;
			float voroi109 = voronoi109( coords109, time109, id109, uv109, 0, voronoiSmoothId109 );
			float temp_output_314_0 = max( voroi130 , voroi109 );
			float height107_g6 = temp_output_314_0;
			float scale107_g6 = _WavesNormalStr;
			float3 localPerturbNormal107_g6 = PerturbNormal107_g6( surf_pos107_g6 , surf_norm107_g6 , height107_g6 , scale107_g6 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir42_g6 = mul( ase_worldToTangent, localPerturbNormal107_g6);
			o.Normal = worldToTangentDir42_g6;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float3 surf_pos107_g1 = ase_worldPos;
			float3 surf_norm107_g1 = ase_worldNormal;
			float2 panner322 = ( _Time.y * ( _RefractionSpeed * _RefractionDirection ) + ( (ase_worldPos).xz * _RefractionTillingNoise ));
			float simplePerlin2D275 = snoise( panner322*_RefractionScale );
			simplePerlin2D275 = simplePerlin2D275*0.5 + 0.5;
			float height107_g1 = simplePerlin2D275;
			float scale107_g1 = 1.0;
			float3 localPerturbNormal107_g1 = PerturbNormal107_g1( surf_pos107_g1 , surf_norm107_g1 , height107_g1 , scale107_g1 );
			float3 worldToTangentDir42_g1 = mul( ase_worldToTangent, localPerturbNormal107_g1);
			float4 screenColor27 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( float3( (ase_screenPosNorm).xy ,  0.0 ) + ( worldToTangentDir42_g1 * _RefractionStr ) ).xy);
			float4 Refraction77 = screenColor27;
			float eyeDepth182 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float temp_output_181_0 = saturate( ( ( eyeDepth182 - ase_screenPos.w ) / _Depth ) );
			float4 lerpResult277 = lerp( _ColorSurface , _ColorDepth , temp_output_181_0);
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV262 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode262 = ( _FresnelBias + _FresnelScale * pow( 1.0 - fresnelNdotV262, _FresnelPower ) );
			float4 lerpResult282 = lerp( Refraction77 , ( lerpResult277 + fresnelNode262 ) , saturate( ( temp_output_181_0 * _DepthMultiply ) ));
			float4 temp_cast_2 = (1.0).xxxx;
			float screenDepth247 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth247 = abs( ( screenDepth247 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) );
			float simplePerlin2D245 = snoise( (ase_worldPos).xz*_ShoreWaveScale );
			simplePerlin2D245 = simplePerlin2D245*0.5 + 0.5;
			float screenDepth249 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth249 = abs( ( screenDepth249 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) );
			float clampResult248 = clamp( ( sin( ( ( 1.0 - ( distanceDepth247 / _ShoreWaveWidth ) ) + ( _Time.y * _ShoreWaveSpeed ) ) ) + simplePerlin2D245 ) , 0.0 , ( 1.0 - ( distanceDepth249 / _ShoreWidth ) ) );
			float screenDepth97 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth97 = abs( ( screenDepth97 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FoamWidth ) );
			float2 temp_output_104_0 = (ase_worldPos).xz;
			float2 temp_cast_3 = (_FoamDistortionSpeed).xx;
			float2 panner105 = ( 0.2 * _Time.y * temp_cast_3 + temp_output_104_0);
			float simplePerlin2D290 = snoise( panner105*_FoamDistortionScale );
			simplePerlin2D290 = simplePerlin2D290*0.5 + 0.5;
			float simplePerlin2D100 = snoise( ( temp_output_104_0 + ( simplePerlin2D290 * _FoamDistortionAmount ) )*_FoamScale );
			simplePerlin2D100 = simplePerlin2D100*0.5 + 0.5;
			float4 lerpResult265 = lerp( lerpResult282 , temp_cast_2 , saturate( max( step( ( 1.0 - clampResult248 ) , 0.9 ) , saturate( step( distanceDepth97 , simplePerlin2D100 ) ) ) ));
			float4 temp_cast_4 = (1.0).xxxx;
			float4 lerpResult307 = lerp( lerpResult265 , temp_cast_4 , saturate( ( ( pow( voroi130 , _WaveSmallPower ) + pow( voroi109 , _WaveBigPower ) ) * _WavesMultiply ) ));
			o.Albedo = lerpResult307.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			float screenDepth81 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth81 = abs( ( screenDepth81 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 0.2 ) );
			o.Alpha = saturate( distanceDepth81 );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*
Сontacts of the author who created this shader:
 
https://vk.com/maximlm_3d
https://gamedev.ru/job/forum/?id=268969
https://www.artstation.com/lapshin

*/