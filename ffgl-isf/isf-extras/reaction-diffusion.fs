/*{
    "CREDIT": "OPTIPHONIC",
    "DESCRIPTION": "Gray-Scott Reaction Diffusion with input",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 1,
            "MAX": 10,
            "MIN": 0,
            "NAME": "scale",
            "TYPE": "float"
        },
        {
            "NAME": "reset",
            "TYPE": "event"
        },
        {
            "DEFAULT": [
                1,
                0.5
            ],
            "MAX": [
                10,
                10
            ],
            "MIN": [
                0,
                0
            ],
            "NAME": "diffusionRate",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": 0.03611,
            "MAX": 0.1,
            "NAME": "killAdd",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.01,
            "MAX": 0.1,
            "NAME": "feedAdd",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.02,
            "MAX": 0.1,
            "NAME": "feedMult",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "MAX": 0.1,
            "NAME": "killMult",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.99,
            "MAX": 1,
            "NAME": "wet",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "lastFrame"
        },
        {
            "TARGET": "outputTarget"
        }
    ]
}
*/

const float VERT = 0.2f;
const float DIAG = 0.05f;
const float CENTER = -1.0f;

vec4 lap(sampler2D tex, vec2 uv, vec2 texelSize) {
    vec4 rg = vec4(0);

    rg += texture(tex, uv + vec2(-1, -1) * texelSize) * DIAG;
    rg += texture(tex, uv + vec2(-0, -1) * texelSize) * VERT;
    rg += texture(tex, uv + vec2(1, -1) * texelSize) * DIAG;
    rg += texture(tex, uv + vec2(-1, 0) * texelSize) * VERT;
    rg += texture(tex, uv + vec2(0, 0) * texelSize) * CENTER;
    rg += texture(tex, uv + vec2(1, 0) * texelSize) * VERT;
    rg += texture(tex, uv + vec2(-1, 1) * texelSize) * DIAG;
    rg += texture(tex, uv + vec2(0, 1) * texelSize) * VERT;
    rg += texture(tex, uv + vec2(1, 1) * texelSize) * DIAG;

    return rg;
}

float map(float value, float min1, float max1, float min2, float max2) {
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void main() {
    vec4 vUV = gl_FragCoord;
    vec4 inputRes = vUV;

    vec4 col = clamp(texture(lastFrame, vUV.xy / RENDERSIZE), 0.0f, 1.0f);
    vec4 inputCol = texture(inputImage, vUV.xy / RENDERSIZE);

    float inputVal = step(0.5f, inputCol.x);

    if(PASSINDEX == 1) {
        gl_FragColor = vec4(vec3(1.0f - abs(col.r - col.g)), 1.0f);
        // gl_FragColor = vec4(vec3(col.g), 1.0f);
        return;
    }

    vec4 lp = lap(lastFrame, vUV.xy / RENDERSIZE, scale / RENDERSIZE);

    if(reset || TIME < 0.5f || any(isnan(col)) || any(isinf(col))) {
        gl_FragColor = vec4(1.0f, inputVal, 0.0f, 1.0f);
        return;
    }

    float feed = inputCol.r * feedMult + feedAdd;
    float kill = inputCol.g * killMult + killAdd;

    float a = col.r;
    float b = col.g;

    float reaction = a * b * b;

    float a2 = a + (diffusionRate.x * lp.x - reaction + feed * (1 - a));
    float b2 = b + (diffusionRate.y * lp.y + reaction - (kill + feed) * b);

    gl_FragColor = vec4(a2, b2, 0.0f, 1.0f) + vec4(0.0f, inputVal * (1.0f - wet), 0.0f, 1.0f);
}
