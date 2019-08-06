#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"

float4 _Tint;
sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _NormalMap;
float _BumpScale;

float _Metallic;
float _Smoothness;

struct VertexData {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;

	float3 tangent : TEXCOORD2;
	float3 binormal : TEXCOORD3;

	float3 worldPos : TEXCOORD4;
};

float GetSmoothness(Interpolators i) {
	float smoothness = 1;
	return smoothness * _Smoothness;
}

float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

Interpolators MyVertexProgram(VertexData v) {
	Interpolators i;
	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.normal = UnityObjectToWorldNormal(v.normal);

	#if defined(BINORMAL_PER_FRAGMENT)
		i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
	#endif

	i.uv = TRANSFORM_TEX(v.uv, _MainTex);

	return i;
}

UnityLight CreateLight(Interpolators i) {
	UnityLight light;
	light.dir = _WorldSpaceLightPos0.xyz;
	light.color = _LightColor0.rgb;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

float3 BoxProjection (
	float3 direction, float3 position, 
	float4 cubemapPosition, float3 boxMin, float3 boxMax) {
	
	#if UNITY_SPECCUBE_BOX_PROJECTION
	UNITY_BRANCH
		if(cubemapPosition.w > 0) {
			// scale direction vector from pos to intersection
			float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
			float scalar = min(min(factors.x, factors.y), factors.z);
			direction = direction * scalar + (position - cubemapPosition);
		}
	#endif
	return direction;
}

UnityIndirect CreateIndirectLight(Interpolators i, float3 viewDir) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(FORWARD_BASE_PASS)
		indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
		float3 reflectionDir = reflect(-viewDir, i.normal);
		// sample with reflection direction instead of normal
		Unity_GlossyEnvironmentData envData;
		envData.roughness = 1 - GetSmoothness(i);
		
		envData.reflUVW = BoxProjection(
			reflectionDir, i.worldPos,
			unity_SpecCube0_ProbePosition,
			unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
		float probe0 = Unity_GlossyEnvironment(
			UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
		envData.reflUVW = BoxProjection(
					reflectionDir, i.worldPos,
					unity_SpecCube1_ProbePosition,
					unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);

		indirectLight.specular = probe0;
	#endif
	
	return indirectLight;
}

// normal maps
void InitializeFragmentNormal(inout Interpolators i) {
	float3 tangentSpaceNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);

	i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * i.binormal + 
		tangentSpaceNormal.z * i.normal);
}

float3 GetAlbedo(Interpolators i) {
	float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Tint.rgb;
	return albedo;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET {
	
	InitializeFragmentNormal(i);

	i.normal = normalize(i.normal);
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	float3 specularTint;
	float oneMinusReflectivity;
	float3 albedo = DiffuseAndSpecularFromMetallic(
		GetAlbedo(i), _Metallic, specularTint, oneMinusReflectivity);

	// clear specular or diffuse
	#if !defined(_SPECULAR)
		specularTint = float3(0, 0, 0);
	#endif

	#if !defined(_DIFFUSE) 
		albedo = float3(0, 0, 0);
	#endif

	return UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, GetSmoothness(i),
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i, viewDir));
}
#endif