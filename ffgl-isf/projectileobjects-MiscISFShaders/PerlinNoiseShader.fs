/*
{
    "DESCRIPTION": "Perlin Noise Shader with Image Distortion",
    "CREDIT": "Adapted from the book 'The Book of Shaders' by Patricio Gonzalez Vivo and Jen Lowe",
    "CATEGORIES": [
        "Generator"
    ],
    "INPUTS": [
        {
            "NAME": "scale",
            "LABEL": "Scale",
            "TYPE": "float",
            "DEFAULT": 0.0001,
            "MIN": 0.00001,
            "MAX": 1.0
        },
        {
            "NAME": "speed",
            "LABEL": "Speed",
            "TYPE": "float",
            "DEFAULT": 0.0005,
            "MIN": 0.000000001,
            "MAX": 0.005
        },
        {
            "NAME": "size",
            "LABEL": "Size",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": -100.01,
            "MAX": 100.0
        },
        {
            "NAME": "rotation",
            "LABEL": "Rotation",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 6.283185
        },
        {
            "NAME": "movementX",
            "LABEL": "Movement X",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": -1.0,
            "MAX": 1.0
        },
        {
            "NAME": "movementY",
            "LABEL": "Movement Y",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": -1.0,
            "MAX": 1.0
        },
        {
            "NAME": "randomMovement",
            "LABEL": "Random Movement",
            "TYPE": "bool",
            "DEFAULT": false
        }
    ]
}
*/

vec2 rotate(vec2 st, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return vec2(st.x * c - st.y * s, st.x * s + st.y * c);
}

void main() {
    // Get the normalized coordinates of the fragment
    vec2 st = isf_FragNormCoord.xy;
    
    // Apply scale to the coordinates
    st *= scale;
    
    // Apply speed to the coordinates with time
    st += TIME * speed;
    
    // Apply size to the coordinates
    st /= size;
    
    // Apply rotation
    st = rotate(st, rotation);
    
    // Apply movement
    if (randomMovement) {
        st += vec2(sin(TIME * speed), cos(TIME * speed)) * 0.1; // Random movement pattern
    } else {
        st += vec2(movementX, movementY) * TIME * speed;
    }
    
    // Calculate the noise value
    float noise = fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453);
    
    // Output the noise value as the color
    gl_FragColor = vec4(noise, noise, noise, 1.0);
}
