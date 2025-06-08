/*{
    "CATEGORIES": ["Generators"],
    "CREDIT": "ProjectileObjects",
    "DESCRIPTION": "Moving Lines",
    "INPUTS": [
        {
            "NAME": "imageInput",
            "TYPE": "image"
        },
        {
            "DEFAULT": 2,
            "MAX": 100,
            "MIN": 1,
            "NAME": "lineWidth",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "MAX": 10,
            "MIN": 0.1,
            "NAME": "speed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 10,
            "MAX": 50,
            "MIN": 1,
            "NAME": "numLines",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.0,
            "MAX": 6.28319,
            "MIN": 0.0,
            "NAME": "rotation",
            "TYPE": "float"
        },
        {
    "DEFAULT": 0.5,
    "MAX": 100.0,
    "MIN": 0.0,
    "NAME": "feather",
    "TYPE": "float"
}
    ],
    "ISFVSN": "2"
}
*/

void main() {
    float spacing = RENDERSIZE.x / numLines;
    float doubleLineWidth = 2.0 * lineWidth;

    vec2 centeredCoord = gl_FragCoord.xy - RENDERSIZE.xy * 0.5;

    // Rotation transformation
    float cosR = cos(rotation);
    float sinR = sin(rotation);
    vec2 rotatedCoord = vec2(centeredCoord.x * cosR - centeredCoord.y * sinR, 
                             centeredCoord.x * sinR + centeredCoord.y * cosR);

    rotatedCoord += RENDERSIZE.xy * 0.5;

    float x = rotatedCoord.x;
    float s = spacing - mod(float(FRAMEINDEX) * speed, spacing);

    float distToLine = abs(mod(x + s, spacing) - lineWidth / 2.0);

    // Apply feathering while keeping lines thick
    float featheredAlpha = clamp(1.0 - (distToLine - (lineWidth / 2.0)) / feather, 0.0, 1.0);
    
    vec2 normalizedCoord = gl_FragCoord.xy / RENDERSIZE.xy;
    vec4 imageColor = IMG_NORM_PIXEL(imageInput, normalizedCoord);

    gl_FragColor = mix(imageColor, vec4(0.0, 0.0, 0.0, 0.0), featheredAlpha);
}