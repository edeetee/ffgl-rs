/*{
    "DESCRIPTION": "Animated wave with customizable shape, colors, movement, and rotation.",
    "CREDIT": "Cornelius // ProjectileObjects",
    "ISFVSN": "2.0",
    "CATEGORIES": [ "Generator" ],
    "INPUTS": [
        { "NAME": "amplitude", "TYPE": "float", "DEFAULT": 0.2, "MIN": 0.05, "MAX": 0.5 },
        { "NAME": "wavelength", "TYPE": "float", "DEFAULT": 4.0, "MIN": 1.0, "MAX": 20.0 },
        { "NAME": "randomSeed", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 10.0 },
        { "NAME": "position", "TYPE": "point2D", "DEFAULT": [0.0, 0.0], "MIN": [-1.0, -1.0], "MAX": [1.0, 1.0] },
        { "NAME": "rotation", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 360.0 },
        { "NAME": "moveSpeed", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 5.0 },
        { "NAME": "reverseWave", "TYPE": "bool", "DEFAULT": false },
        { "NAME": "moveAmount", "TYPE": "float", "DEFAULT": 0.05, "MIN": 0.0, "MAX": 0.3 },
        { "NAME": "rotationSpeed", "TYPE": "float", "DEFAULT": 30.0, "MIN": 0.0, "MAX": 180.0 },
        { "NAME": "reverseRotation", "TYPE": "bool", "DEFAULT": false },
        { "NAME": "color1", "TYPE": "color", "DEFAULT": [1.0, 0.0, 0.0, 1.0] },
        { "NAME": "color2", "TYPE": "color", "DEFAULT": [1.0, 0.5, 0.0, 1.0] },
        { "NAME": "color3", "TYPE": "color", "DEFAULT": [1.0, 1.0, 0.0, 1.0] },
        { "NAME": "color4", "TYPE": "color", "DEFAULT": [0.0, 1.0, 0.0, 1.0] },
        { "NAME": "color5", "TYPE": "color", "DEFAULT": [0.0, 0.0, 1.0, 1.0] },
        { "NAME": "color6", "TYPE": "color", "DEFAULT": [0.5, 0.0, 1.0, 1.0] }
    ]
}*/

float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    
    // Normalize position so (0,0) is center, (-1,-1) bottom-left, (1,1) top-right
    vec2 normPos = position * 0.5 + 0.5;
    
    // Adjust UV coordinates based on position
    vec2 centeredUV = (uv - normPos) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);

    // Define PI explicitly
    const float PI = 3.14159265359;

    // Reverse wave movement if toggle is enabled
    float directionMultiplier = reverseWave ? -1.0 : 1.0;
    float animatedX = centeredUV.x * wavelength + TIME * moveSpeed * directionMultiplier;

    // Apply randomization to the wave shape based on `randomSeed`
    float randomFactor = random(vec2(floor(animatedX * randomSeed), randomSeed)) * 0.5;
    float wave = sin(animatedX + randomFactor) * amplitude;

    // Define the thickness of the wave line
    float waveMask = smoothstep(0.01, 0.0, abs(centeredUV.y - wave));

    // Use wave position to determine color blend
    float rotatedAngle = mod(centeredUV.x + radians(rotation + TIME * rotationSpeed * (reverseRotation ? -1.0 : 1.0)), 2.0 * PI) / (2.0 * PI);
    float segment = rotatedAngle * 6.0; // Divide into 6 segments
    float index = floor(segment);
    float mixFactor = fract(segment);

    // Store colors in an array
    vec3 colors[6];
    colors[0] = color1.rgb;
    colors[1] = color2.rgb;
    colors[2] = color3.rgb;
    colors[3] = color4.rgb;
    colors[4] = color5.rgb;
    colors[5] = color6.rgb;

    // Smooth transition between colors
    vec3 finalColor = mix(colors[int(mod(index, 6.0))], colors[int(mod(index + 1.0, 6.0))], mixFactor);

    // Blend wave with a solid black background
    vec3 outputColor = mix(vec3(0.0), finalColor, waveMask);
    gl_FragColor = vec4(outputColor, 1.0);
}