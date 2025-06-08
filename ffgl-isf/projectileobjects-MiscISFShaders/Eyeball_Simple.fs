/*{
    "CATEGORIES": [
        "Effect", "Simple Eyeball by ProjectileObjects"
    ],
    "DESCRIPTION": "Generates an eyeball at the center of the frame with an adjustable pupil position and color controls.",
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
    vec2 center = vec2(0.5, 0.5);
    
    // Ensure pupil moves within the iris, mapped to -2.5 to 2.5 range
    vec2 pupilOffset = pupilPosition * 0.04; // Scaled to fit within the iris
    vec2 pupilCenter = center + pupilOffset;
    
    float distToCenter = length(uv - center);
    float distToPupil = length(uv - pupilCenter);
    
    vec3 color = eyeballColor.rgb; // Default to eyeball color
    float alpha = 1.0; // Ensure eyeball is always visible
    
    // Draw iris
    if (distToCenter < irisSize) {
        color = mix(eyeballColor.rgb, irisColor.rgb, smoothstep(irisSize - 0.02, irisSize, distToCenter));
    }
    
    // Draw pupil
    if (distToPupil < pupilSize) {
        color = pupilColor.rgb; // Use adjustable pupil color
    }
    
    // Make background transparent
    if (distToCenter >= irisSize) {
        alpha = 0.0;
    }
    
    gl_FragColor = vec4(color, alpha);
}