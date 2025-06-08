/*{
    "CATEGORIES": [
        "Effect", "Displacement"
    ],
    "DESCRIPTION": "Created by ProjectileObjects. Applies analytical texture displacement using inputImage as the source with geometric patterns.",
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
    
    // Create geometric displacement patterns
    float sineDisplacement = sin(uv.y * frequency + TIME * timeFactor) * displacementStrength;
    float mirroredSine = sin((1.0 - uv.y) * frequency + TIME * timeFactor) * displacementStrength;
    float jaggedEdge = mod(uv.y * frequency, 1.0) < 0.5 ? displacementStrength : -displacementStrength;
    
    // Pentagram pattern (using a star-like perturbation based on UV coordinates)
    float starPattern = sin(5.0 * atan(uv.x - 0.5, uv.y - 0.5)) * displacementStrength;
    
    // Combine displacement effects
    float displacementX = sineDisplacement + starPattern;
    float displacementY = mirroredSine + jaggedEdge;
    
    // Apply displacement
    vec2 displacedUV = uv + vec2(displacementX, displacementY);
    
    // Sample the input image with displaced coordinates
    vec4 color = IMG_NORM_PIXEL(inputImage, displacedUV);
    
    // Output final color
    gl_FragColor = color;
}
