/*{
    "CATEGORIES": [
        "Effect", "Displacement"
    ],
    "DESCRIPTION": "Created by ProjectileObjects. Applies analytical texture displacement using inputImage as the source.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "NAME": "displacementStrength",
            "LABEL": "Displacement Strength",
            "TYPE": "float",
            "DEFAULT": 0.05,
            "MIN": 0.0,
            "MAX": 0.2
        },
        {
            "NAME": "frequency",
            "LABEL": "Wave Frequency",
            "TYPE": "float",
            "DEFAULT": 10.0,
            "MIN": 1.0,
            "MAX": 50.0
        },
        {
            "NAME": "timeFactor",
            "LABEL": "Time Factor",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.0,
            "MAX": 5.0
        }
    ],
    "ISFVSN": "2"
}*/

void main() {
    // Normalize texture coordinates
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    
    // Compute displacement analytically using sine waves
    float displacementX = sin(uv.y * frequency + TIME * timeFactor) * displacementStrength;
    float displacementY = cos(uv.x * frequency + TIME * timeFactor) * displacementStrength;
    
    // Apply displacement
    vec2 displacedUV = uv + vec2(displacementX, displacementY);
    
    // Sample the input image with displaced coordinates
    vec4 color = IMG_NORM_PIXEL(inputImage, displacedUV);
    
    // Output final color
    gl_FragColor = color;
}
