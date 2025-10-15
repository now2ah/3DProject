Shader "Custom/CustomOutlineShader"
{
    // === 1. Properties: Material Inspector에서 설정 가능한 변수들 ===
    Properties
    {
        [Header(Base Pass)]
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        
        [Header(Outline Pass)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // 아웃라인 색상
        _OutlineThickness ("Outline Thickness (World)", Range(0.0, 0.1)) = 0.01 // 아웃라인 두께
    }

    // === 2. SubShader ===
    SubShader
    {
        // 렌더링 태그: 불투명 오브젝트로 간주
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        // =======================================================
        // A. Pass 1: 아웃라인 렌더링 (Cull Front)
        // =======================================================
        Pass
        {
            // 앞면을 컬링하여 뒷면만 렌더링합니다. (모델을 부풀린 후, 뒷면만 아웃라인으로 남김)
            Cull Front 
            
            Name "OutlinePass" // 디버깅을 위한 Pass 이름

            HLSLPROGRAM
            #pragma vertex vert_outline 
            #pragma fragment frag_outline
            
            // URP 필수 라이브러리 포함 (UnityInput.hlsl에 UnityObjectToClipPos 포함)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            // Properties에서 정의한 변수 선언
            float4 _OutlineColor;
            float _OutlineThickness;

            // --- 1-1. 구조체 ---
            struct Attributes
            {
                float4 positionOS : POSITION; // Object Space 위치
                float3 normalOS : NORMAL;     // Object Space 법선
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // HClip Space 위치
            };

            // --- 1-2. Vertex Shader: 법선 방향으로 정점을 확장 ---
            Varyings vert_outline(Attributes IN)
            {
                Varyings OUT;
                
                // 1. 월드 공간 위치와 법선을 얻습니다.
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                // 2. 법선 방향으로 월드 공간 위치를 확장합니다.
                positionWS += normalWS * _OutlineThickness; 
                
                // 3. 확장된 월드 위치를 클립 공간으로 변환합니다.
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                
                return OUT;
            }

            // --- 1-3. Fragment Shader: 아웃라인 색상 반환 ---
            half4 frag_outline(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        } // End of Pass 1

        // =======================================================
        // B. Pass 2: 기본 모델 렌더링 (Cull Back)
        // =======================================================
        Pass
        {
            // URP 포워드 렌더링 파이프라인과 통합
            Tags { "LightMode" = "UniversalForward" } 
            // 일반 렌더링은 뒷면을 컬링합니다.
            Cull Back 
            
            Name "BasePass"

            HLSLPROGRAM
            #pragma vertex VS_Main
            #pragma fragment PS_Main
            
            // URP 필수 라이브러리
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Properties 변수 선언
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _BaseColor;

            // --- 2-1. 구조체 ---
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : NORMAL;
            };
            // ------------------

            // --- 2-2. Vertex Shader: 기본 변환 및 데이터 전달 ---
            Varyings VS_Main(Attributes IN)
            {
                Varyings OUT;
                
                // 클립 공간 변환
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz); // UnityObjectToClipPos 대체 함수
                
                // 월드 법선 계산
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                
                // 텍스처 좌표 계산
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex); 

                return OUT;
            }
            // ----------------------------------------------------

            // --- 2-3. Pixel Shader: 단순 조명 및 색상 출력 ---
            float4 PS_Main(Varyings IN) : SV_TARGET
            {
                // 1. 텍스처 및 기본 색상
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _BaseColor;

                // 2. 주광원 및 조명 계산
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(IN.worldNormal);
                
                // 램버트 확산 조명 (N dot L)
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                // 최종 조명 (환경광은 무시하고 단순 확산만 적용)
                float3 lighting = mainLight.color * NdotL;
                
                float3 finalRGB = baseColor.rgb * lighting;

                return float4(finalRGB, baseColor.a);
            }
            // -------------------------------------------------
            
            ENDHLSL
        } // End of Pass 2
    } // End of SubShader
    
    // 이 셰이더가 지원되지 않을 경우 표준 셰이더로 대체
    Fallback "Hidden/Universal Render Pipeline/FallbackError" 
}