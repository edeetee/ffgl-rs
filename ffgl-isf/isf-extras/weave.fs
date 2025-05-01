/*{
    "CATEGORIES": [
        "Geometry",
        "Distortion"
    ],
    "DESCRIPTION": "Turbulent weave pattern with volume tracing",
    "INPUTS": [
        {
            "NAME": "progress",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 1.0,
            "LABEL": "Progress"
        },
        {
            "NAME": "speed",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.0,
            "MAX": 5.0,
            "LABEL": "Speed"
        },
        {
            "NAME": "focal",
            "TYPE": "float",
            "DEFAULT": 2.25,
            "MIN": 0.5,
            "MAX": 20.0,
            "LABEL": "Focal Length"
        },
        {
            "NAME": "q",
            "TYPE": "float",
            "DEFAULT": 5.0,
            "MIN": 1.0,
            "MAX": 10.0,
            "LABEL": "Pattern Scale"
        },
        {
            "NAME": "outerIter",
            "TYPE": "long",
            "DEFAULT": 99,
            "MIN": 10,
            "MAX": 200,
            "LABEL": "Quality"
        },
        {
            "NAME": "innerIterJump",
            "TYPE": "float",
            "DEFAULT": 3.0,
            "MIN": 1.5,
            "MAX": 5.0,
            "LABEL": "Detail"
        }
    ],
    "CREDIT": "chronos - adapted from shadertoy.com/view/W3SSRm"
}*/

/*
    ------------------------------------------
    |          The Weave  by chronos         |
    ------------------------------------------
    
    Little doodle exploring volume tracing turbulently distorted SDFs.

    Based on the glow volume marching approach as seen in "Ghosts" by xor
    https://www.shadertoy.com/view/tXlXDX
    
    ------------------------------------------------
    self link: https://www.shadertoy.com/view/W3SSRm
    ------------------------------------------------
*/

const float PI = 3.14159265f;
vec3 cmap(float x) {
    return pow(.5f + .5f * cos(PI * x + vec3(1, 2, 3)), vec3(2.5f));
}

void main() {
    vec2 uv = (2.f * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;
    float animationTime = progress * PI * 2 + TIME * speed;

    vec3 ro = vec3(0, 0, animationTime);
    vec3 rd = normalize(vec3(uv, -focal));
    vec3 color = vec3(0);
    float t = 0.f;
    for(int i = 0; i < outerIter; i++) {
        vec3 p = t * rd + ro;

        float T = t / q + animationTime;
        float c = cos(T), s = sin(T);
        p.xy = mat2(c, -s, s, c) * p.xy;

        for(float a = 1.f; a < exp2(9.f); a *= innerIterJump) {
            p += cos(p.yzx * a + animationTime) / a;
        }
        float d = 1.f / 50.f + abs((ro - p - vec3(0, 1, 0)).y - 1.f) / 10.f;
        color += cmap(t) * 2e-3f / d;
        t += d;
    }

    color *= color * color;
    color = 1.f - exp(-color);
    color = pow(color, vec3(1.f / 2.2f));
    gl_FragColor = vec4(color, 1);
}