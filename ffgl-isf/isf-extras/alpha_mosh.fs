/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/mlGfDh by Akascape.  A try to replicate the datamosh effect, not exact but atleast works in shadertoy.",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        }
    ],
    "PASSES": [
        {
            "FLOAT": false,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
            "FLOAT": false,
            "PERSISTENT": true,
            "TARGET": "BufferB"
        },
        {
            "FLOAT": false,
            "PERSISTENT": true,
            "TARGET": "BufferC"
        },
        {
        }
    ]
}

*/

// Use Webcam for live preview

// Hash Source: https://www.shadertoy.com/view/4djSRW

float hash11(float p) {
    p = fract(p * .1031f);
    p *= p + 33.33f;
    p *= p + p;
    return fract(p);
}

float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * .1031f);
    p3 += dot(p3, p3.yzx + 33.33f);
    return fract((p3.x + p3.y) * p3.z);
}

// REALTIME DATAMOSH BY AKASCAPE 

uint RAND_STATE(vec2 Coord) {
    return ((uint(RENDERSIZE.x) * uint(Coord.y) + uint(Coord.x) + 1u) * uint(FRAMEINDEX));
}

void main() {
    if(PASSINDEX == 0) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

        gl_FragColor = IMG_NORM_PIXEL(inputImage, mod(uv, 1.0f));
    } else if(PASSINDEX == 1) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

        gl_FragColor.x = dot(IMG_NORM_PIXEL(BufferC, mod(uv, 1.0f)).rgb, vec3(0.3f));

        gl_FragColor.yzw = IMG_NORM_PIXEL(BufferB, mod(uv, 1.0f)).rgb;
    } else if(PASSINDEX == 2) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

        float time = TIME * 100.f;
        float index = floor(time);
        float chunk = 100.f;

        bool spawn = hash12(floor(uv * hash11(index + 80.f) * chunk) + index) > 0.9f;

        if(FRAMEINDEX < 1 || spawn) {
            gl_FragColor = IMG_NORM_PIXEL(BufferA, mod(uv, 1.0f));
        } else {
            gl_FragColor = IMG_NORM_PIXEL(BufferC, mod(uv, 1.0f));
        }
    } else if(PASSINDEX == 3) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

        float threshold = 0.1f * (1.0f - exp(-0.2f * TIME));
        vec4 delta = IMG_NORM_PIXEL(BufferB, mod(uv, 1.0f));

        if(abs(delta.x - delta.w) < threshold)
            gl_FragColor = vec4(0.0f);
        else
            gl_FragColor = IMG_NORM_PIXEL(BufferA, mod(uv, 1.0f));
    }

}
