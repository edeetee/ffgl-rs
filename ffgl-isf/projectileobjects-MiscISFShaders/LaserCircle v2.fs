/*{
    "DESCRIPTION": "Animated circular line with customizable colors, movement, and rotation.",
    "CREDIT": "Cornelius // ProjectileObjects",
    "ISFVSN": "2.0",
    "CATEGORIES": [ "Generator" ],
    "INPUTS": [
        { "NAME": "circleSize", "TYPE": "float", "DEFAULT": 0.3, "MIN": 0.05, "MAX": 0.9 },
        { "NAME": "thickness", "TYPE": "float", "DEFAULT": 0.02, "MIN": 0.001, "MAX": 0.1 },
        { "NAME": "position", "TYPE": "point2D", "DEFAULT": [0.0, 0.0], "MIN": [-1.0, -1.0], "MAX": [1.0, 1.0] },
        { "NAME": "rotation", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 360.0 },
        { "NAME": "moveSpeed", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 5.0 },
        { "NAME": "moveAmount", "TYPE": "float", "DEFAULT": 0.05, "MIN": 0.0, "MAX": 0.3 },
        { "NAME": "rotationSpeed", "TYPE": "float", "DEFAULT": 30.0, "MIN": -180.0, "MAX": 180.0 },
        { "NAME": "color1", "TYPE": "color", "DEFAULT": [1.0, 0.0, 0.0, 1.0] },
        { "NAME": "color2", "TYPE": "color", "DEFAULT": [1.0, 0.5, 0.0, 1.0] },
        { "NAME": "color3", "TYPE": "color", "DEFAULT": [1.0, 1.0, 0.0, 1.0] },
        { "NAME": "color4", "TYPE": "color", "DEFAULT": [0.0, 1.0, 0.0, 1.0] },
        { "NAME": "color5", "TYPE": "color", "DEFAULT": [0.0, 0.0, 1.0, 1.0] },
        { "NAME": "color6", "TYPE": "color", "DEFAULT": [0.5, 0.0, 1.0, 1.0] }
    ]
}*/

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    
    // Normalize position so (0,0) is center, (-1,-1) bottom-left, (1,1) top-right
    vec2 normPos = position * 0.5 + 0.5;
    
    // Animate movement using sine wave
    float animatedSize = circleSize + moveAmount * sin(TIME * moveSpeed);

    // Adjust UV coordinates based on position, maintain aspect ratio
    vec2 aspect = vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    vec2 centeredUV = (uv - normPos) * aspect;

    // Compute radius and angle in polar coordinates
    float radius = length(centeredUV);
    float angle = atan(centeredUV.y, centeredUV.x);

    // Define PI explicitly
    const float PI = 3.14159265359;

    // Apply animated rotation (convert degrees to radians)
    float animatedRotation = radians(rotation + TIME * rotationSpeed);
    float rotatedAngle = mod(angle + animatedRotation, 2.0 * PI) / (2.0 * PI);

    // Define outer and inner radius for the ring
    float outer = animatedSize;
    float inner = outer - thickness;

    // Create a smooth circular band with anti-aliasing
    float ringMask = smoothstep(inner, inner + 0.002, radius) - smoothstep(outer, outer + 0.002, radius);

    // Store color inputs into an array
    vec3 colors[6];
    colors[0] = color1.rgb;
    colors[1] = color2.rgb;
    colors[2] = color3.rgb;
    colors[3] = color4.rgb;
    colors[4] = color5.rgb;
    colors[5] = color6.rgb;

    // Use rotated angle to smoothly blend between 6 colors
    float segment = rotatedAngle * 6.0;  // Divide circle into 6 segments
    float index = floor(segment);  // Get segment index as float
    float mixFactor = fract(segment); // Smooth interpolation factor

    // Ensure index wraps correctly using mod with floating point
    vec3 finalColor = mix(colors[int(mod(index, 6.0))], colors[int(mod(index + 1.0, 6.0))], mixFactor);

    // Ensure proper alpha blending (black background or transparent)
    gl_FragColor = vec4(finalColor, ringMask);
}