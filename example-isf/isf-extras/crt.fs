/*
{
    "DESCRIPTION": "CRT TV Shader",
    "CREDIT": "Ryan Green / greenrhyno",
    "CATEGORIES": [
        "Stylize"
    ],
    "INPUTS": [
        {
            "NAME": "uIntensity",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.0,
            "MAX": 2.0
        },
        {
            "NAME": "uPixelSize",
            "TYPE": "color",
            "DEFAULT": [0.85, 0.96, 0.3, 0.15]
        },
        {
            "NAME": "inputImage",
            "TYPE": "image"
        }
    ],
    "ISFVSN": "2"
}
*/

// CRT TV Shader
// Ryan Green / greenrhyno
// 2023

// uniform float uIntensity;
// uniform vec4 uPixelSize; // pixelSize.xy + pixelSoftness.xy 

float roundedRect(in vec2 st, vec2 size, vec2 softness) {
    size = (vec2(1.f) - size) * .5f;
    float cornerRadius = .1f;

    float p = 1.f;
    p *= smoothstep(size.y, size.y + softness.y, st.y);
    p *= smoothstep(size.x, size.x + softness.x, st.x);
    p *= smoothstep(size.y, size.y + softness.y, 1.f - st.y);
    p *= smoothstep(size.x, size.x + softness.x, 1.f - st.x);
    return p;
}

vec3 pixelUnit(in vec2 st, in vec2 sampleCoord) {
    vec3 color = vec3(0.f);
    vec2 size = vec2(.85f, .96f);
    vec2 softness = vec2(.3f, .15f);

    vec3 texSample = IMG_NORM_PIXEL(inputImage, sampleCoord).rgb;

    color += texSample.r * vec3(1.f, 0.f, 0.f) * roundedRect(vec2(st.x * 3.f, st.y), size, softness);
    color += texSample.g * vec3(0.f, 1.f, 0.f) * roundedRect(vec2((st.x - .3333f) * 3.f, st.y), size, softness);
    color += texSample.b * vec3(0.f, 0.f, 1.f) * roundedRect(vec2((st.x - .6666f) * 3.f, st.y), size, softness);

    return color;
}

out vec4 fragColor;
void main() {
    vec2 sampleCoord = vUV.st * RENDERSIZE;
    sampleCoord.y -= step(1.f, mod(sampleCoord.x, 2.f)) * 0.5f;
    vec2 offsetUV = fract(sampleCoord);
    sampleCoord = (floor(sampleCoord) + 0.5f) / RENDERSIZE;

    vec3 color = pixelUnit(offsetUV, sampleCoord) * uIntensity;
    fragColor = vec4(color, 1.0f);
}
