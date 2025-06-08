/*{
    "CATEGORIES": [
        "Effect", "Eyeballs"
    ],
    "DESCRIPTION": "Two Eyeballs doing eyeball like things!? ProjectileObjects",
    "INPUTS": [
        {
            "NAME": "pupilPosition",
            "LABEL": "Pupil Position",
            "TYPE": "point2D",
            "DEFAULT": [0.0, 0.0],
            "MIN": [-2.5, -2.5],
            "MAX": [2.5, 2.5]
        },
        {
            "NAME": "irisSize",
            "LABEL": "Iris Size",
            "TYPE": "float",
            "DEFAULT": 0.15,
            "MIN": 0.05,
            "MAX": 0.3
        },
        {
            "NAME": "pupilSize",
            "LABEL": "Pupil Size",
            "TYPE": "float",
            "DEFAULT": 0.05,
            "MIN": 0.02,
            "MAX": 0.15
        },
        {
            "NAME": "eyeSpacing",
            "LABEL": "Eye Spacing",
            "TYPE": "float",
            "DEFAULT": 0.15,
            "MIN": 0.3,
            "MAX": 1.0
        },
        {
            "NAME": "eyeballColor",
            "LABEL": "Eyeball Color",
            "TYPE": "color",
            "DEFAULT": [1.0, 1.0, 1.0, 1.0]
        },
        {
            "NAME": "irisColor",
            "LABEL": "Iris Color",
            "TYPE": "color",
            "DEFAULT": [0.2, 0.3, 0.6, 1.0]
        },
        {
            "NAME": "pupilColor",
            "LABEL": "Pupil Color",
            "TYPE": "color",
            "DEFAULT": [0.0, 0.0, 0.0, 1.0]
        }
    ],
    "ISFVSN": "2"
}*/

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    
    // Define eye positions with adjustable spacing
    float halfSpacing = eyeSpacing * 0.5;
    vec2 leftEyeCenter = vec2(0.5 - halfSpacing, 0.5);
    vec2 rightEyeCenter = vec2(0.5 + halfSpacing, 0.5);
    
    // Pupil movement, mapped and clamped to stay within the iris
    vec2 pupilOffset = pupilPosition * 0.04; // Scale movement within iris
    vec2 leftPupilCenter = leftEyeCenter + pupilOffset;
    vec2 rightPupilCenter = rightEyeCenter + pupilOffset;
    
    float distToLeftEye = length(uv - leftEyeCenter);
    float distToRightEye = length(uv - rightEyeCenter);
    float distToLeftPupil = length(uv - leftPupilCenter);
    float distToRightPupil = length(uv - rightPupilCenter);
    
    vec3 color = eyeballColor.rgb; // Default to eyeball color
    float alpha = 0.0;
    
    // Draw left iris with smooth transition
    if (distToLeftEye < irisSize) {
        float irisBlend = smoothstep(irisSize, irisSize * 0.8, distToLeftEye);
        color = mix(irisColor.rgb, eyeballColor.rgb, irisBlend);
        alpha = 1.0;
    }
    
    // Draw right iris with smooth transition
    if (distToRightEye < irisSize) {
        float irisBlend = smoothstep(irisSize, irisSize * 0.8, distToRightEye);
        color = mix(irisColor.rgb, eyeballColor.rgb, irisBlend);
        alpha = 1.0;
    }
    
    // Draw left pupil
    if (distToLeftPupil < pupilSize) {
        color = pupilColor.rgb;
        alpha = 1.0;
    }
    
    // Draw right pupil
    if (distToRightPupil < pupilSize) {
        color = pupilColor.rgb;
        alpha = 1.0;
    }
    
    gl_FragColor = vec4(color, alpha);
}
