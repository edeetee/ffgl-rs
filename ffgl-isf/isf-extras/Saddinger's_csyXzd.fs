/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/csyXzd by graygoose.  A sad field system.\n\nTODO: \n-Everything\n-Obstacles.\n-FIX laplacian",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "DEFAULT": 0.1,
            "NAME": "speed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "NAME": "displace",
            "TYPE": "float"
        },
        {
            "DEFAULT": 2,
            "NAME": "scale",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
        }
    ]
}
*/

float iFrameRate = 60.f;
float frameRatio = RENDERSIZE.x / RENDERSIZE.y;
// shadertoy common
#define R RENDERSIZE
#define UV (U/R.xy)
#define A IMG_THIS_NORM_PIXEL(BufferA)

// math common
vec2 wrap(in vec2 p, in vec2 res) {
    vec2 wrapped = mod(p, res);
    //vec2 stepWrap = step(wrapped, vec2(0.0));
    //wrapped += stepWrap * res;

    // Smooth interpolation
    //vec2 smoothed = mix(wrapped, p, smoothstep(0.0, 1.0, stepWrap));
    return wrapped;
}

vec4 lap(in sampler2D tex, in vec2 p, in vec2 res) {
    vec2 dt = 1.f / res.xy;
    vec4 sum = -4.0f * texture(tex, p);
    sum += texture(tex, p + vec2(dt.x, 0.0f));
    sum += texture(tex, p - vec2(dt.x, 0.0f));
    sum += texture(tex, p + vec2(0.0f, dt.y));
    sum += texture(tex, p - vec2(0.0f, dt.y));
    return sum;
}

#define K 100.

void evolveWave(inout vec4 wave, in vec2 U) {
    vec4 L = lap(BufferA, UV, R.xy) - wave;
    wave -= TIMEDELTA * K * speed * vec4((wave.z + L.x), (wave.x + L.y), (wave.y + L.z), (wave.w + L.w));
}

void addDynamics(inout vec4 wave, in vec2 U) {
    vec2 center = R.xy * 0.5f;
    float distFromCenter = length(U - center) / length(center);
    wave *= 1.0f + 0.2f * sin(distFromCenter * 3.14159f);

    float shiftFactor = displace * 0.5f;
    wave.x += shiftFactor * sin(wave.y * 2.0f * 3.14159f);
    wave.y += shiftFactor * cos(wave.x * 2.0f * 3.14159f);
    wave.z += shiftFactor * sin(wave.w * 2.0f * 3.14159f);
    wave.w += shiftFactor * cos(wave.z * 2.0f * 3.14159f);
}

vec4 toColor(in vec4 wave) {
    vec3 color = vec3(wave.x * 0.5f + 0.5f, wave.y * 0.5f + 0.5f, wave.z * 0.5f + 0.5f);
    float combinedColor = color.r + color.g + color.b;

    if(combinedColor > 1.5f) {
        return vec4(color, 1.0f);
    } else {
        return vec4(color.yzx, 1.0f);
    }
}

void main() {
    vec4 wave = A;

    if(FRAMEINDEX < 1) {
        wave = vec4(0.f);
        wave.zw = vec2(1.0f);
        wave.xy = vec2(-0.1f * cos(0.1f * length(gl_FragCoord.xy - 0.5f * R.xy)), 0.1f * sin(0.1f * length(gl_FragCoord.xy - 0.5f * R.xy)));
    }

    evolveWave(wave, gl_FragCoord.xy);
    addDynamics(wave, gl_FragCoord.xy);
    gl_FragColor = normalize(wave);

}
