/*{
    "DESCRIPTION": "Draws a circular line with a rainbow gradient, adjustable size, position, and rotation.",
    "CREDIT": "Cornelius // ProjectileObjects",
    "ISFVSN": "2.0",
    "CATEGORIES": [ "Generator" ],
    "INPUTS": [
        { "NAME": "circleSize", "TYPE": "float", "DEFAULT": 0.3, "MIN": 0.05, "MAX": 0.9 },
        { "NAME": "thickness", "TYPE": "float", "DEFAULT": 0.02, "MIN": 0.001, "MAX": 0.1 },
        { "NAME": "position", "TYPE": "point2D", "DEFAULT": [0.0, 0.0], "MIN": [-1.0, -1.0], "MAX": [1.0, 1.0] },
        { "NAME": "rotation", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 360.0 }
    ]
}*/

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    
    // Normalize position to range [-1,1] for intuitive placement
    vec2 normPos = position * 0.5 + 0.5;
    
    // Adjust UV coordinates based on position, and maintain aspect ratio
    vec2 aspect = vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    vec2 centeredUV = (uv - normPos) * aspect;

    // Compute radius and angle in polar coordinates
    float radius = length(centeredUV);
    float angle = atan(centeredUV.y, centeredUV.x);

    // Define PI explicitly
    const float PI = 3.14159265359;

    // Apply rotation (convert degrees to radians)
    float rotatedAngle = mod(angle + radians(rotation), 2.0 * PI) / (2.0 * PI);

    // Define outer and inner radius for the ring
    float outer = circleSize;
    float inner = outer - thickness;

    // Create a smooth circular band with anti-aliasing
    float ringMask = smoothstep(inner, inner + 0.002, radius) - smoothstep(outer, outer + 0.002, radius);

    // Generate a rainbow gradient using the rotated angle
    vec3 color = 0.5 + 0.5 * cos(6.2831 * rotatedAngle + vec3(0.0, 2.0, 4.0));

    // Ensure proper alpha blending (black background or transparent)
    gl_FragColor = vec4(color, ringMask);
}