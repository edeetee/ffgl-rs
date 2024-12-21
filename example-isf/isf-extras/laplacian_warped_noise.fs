/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/ml3cDj by Sergio_2357.  Laplacian warped flow noise inspired by nimitz's Dynamism (https://www.shadertoy.com/view/MtKSWW) and iq's warped fbm noise idea (https://www.shadertoy.com/view/lsl3RH).",
    "IMPORTED": {
    },
    "INPUTS": [
    ],
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferB"
        },
        {
        }
    ]
}

*/

float random(in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898f, 78.233f))) *
        43758.5453123f);
}

float lerp(float a, float b, float f) {
    return (1.0f - f) * a + f * b;
}

const float ROTATION_SPEED = 0.7f;
vec2 getGradient(vec2 p) {
    float deg = random(p) * 8.f + TIME * ROTATION_SPEED * (random(p) * .5f + .5f);
    vec2 grd = vec2(cos(deg), sin(deg));
    return grd;
}

float gradientNoise(vec2 ps) {
    vec2 pi = floor(ps);
    vec2 pf = fract(ps);

    vec2 u = pf * pf * (3.0f - 2.0f * pf);
    //vec2 u = pf;

    vec2 llp = pi;
    vec2 llv = getGradient(llp);
    vec2 hlp = pi + vec2(0.0f, 1.0f);
    vec2 hlv = getGradient(hlp);
    vec2 lrp = pi + vec2(1.0f, 0.0f);
    vec2 lrv = getGradient(lrp);
    vec2 hrp = pi + vec2(1.0f, 1.0f);
    vec2 hrv = getGradient(hrp);

    float ll = dot(llv, (ps - llp));
    float lr = dot(lrv, (ps - lrp));
    float hl = dot(hlv, (ps - hlp));
    float hr = dot(hrv, (ps - hrp));

    float l = lerp(ll, lr, u.x);
    float h = lerp(hl, hr, u.x);

    float v = lerp(l, h, u.y);

    v = v * 0.5f + 0.5f;
    return v;
}

float fbm(vec2 ps) {
    vec2 p = ps;
    float v = 0.0f;
    float s = .7f;
    for(int i = 0; i < 17; i++) {
        v += gradientNoise(p) * s;
        s *= 0.33f;
        p *= 2.0f;
    }
    return v;
}

vec2 v2fbm(vec2 ps) {
    float x = fbm(ps);
    float y = fbm(ps + vec2(5.0f, 4.0f));
    return vec2(x, y) * 0.4f;
}

float warpedFBM(vec2 ps) {
    return fbm(ps + v2fbm(ps + v2fbm(ps)));
}

vec2 gradient(vec2 uv) {

    vec2 pxuv = vec2(1.f, 1.f) / RENDERSIZE.xy;

    float c = IMG_NORM_PIXEL(BufferA, mod(uv, 1.0f)).x;
    float r = IMG_NORM_PIXEL(BufferA, mod(uv + vec2(pxuv.x, 0.0f), 1.0f)).x;
    float u = IMG_NORM_PIXEL(BufferA, mod(uv + vec2(0.0f, pxuv.y), 1.0f)).x;

    vec2 grd = vec2(0.0f, 0.0f);

    grd.x = r - c;
    grd.y = u - c;

    grd = normalize(grd);

    return grd;

}

// div(normalize(grad(noise)) idea from nimitz https://www.shadertoy.com/view/MtKSWW
// fbm(p+fbm(p)) (warped noise) idea from iq https://www.shadertoy.com/view/lsl3RH
// plus flownoise in the fbm

float divergence(vec2 uv) {

    vec2 tx = 1.f / RENDERSIZE.xy;

    vec4 uv_n = (IMG_NORM_PIXEL(BufferB, mod(uv + vec2(0.0f, tx.y), 1.0f)) - 0.5f) * 2.0f;
    vec4 uv_e = (IMG_NORM_PIXEL(BufferB, mod(uv + vec2(tx.x, 0.0f), 1.0f)) - 0.5f) * 2.0f;
    vec4 uv_s = (IMG_NORM_PIXEL(BufferB, mod(uv + vec2(0.0f, -tx.y), 1.0f)) - 0.5f) * 2.0f;
    vec4 uv_w = (IMG_NORM_PIXEL(BufferB, mod(uv + vec2(-tx.x, 0.0f), 1.0f)) - 0.5f) * 2.0f;

    float div = uv_s.y - uv_n.y - uv_e.x + uv_w.x;

    return div;
}

void main() {
    if(PASSINDEX == 0) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.y;

        uv *= 1.f;

        vec3 col = vec3(warpedFBM(uv));

        gl_FragColor = vec4(col, 1.0f);
    } else if(PASSINDEX == 1) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

        vec2 grd = gradient(uv) * 0.5f + 0.5f;

        gl_FragColor = vec4(grd, 1.0f, 1.0f);
    } else if(PASSINDEX == 2) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

        float v = divergence(uv) * 2.f;

        vec3 col = (0.5f + 0.5f * cos(5.f * v * uv.xyx + vec3(0, 2, 4)));

        gl_FragColor = vec4(vec3(v) * col * 1.5f, 1.0f);

        if(gl_FragCoord.x + 2.f >= RENDERSIZE.x || gl_FragCoord.y + 2.f >= RENDERSIZE.y) {
            gl_FragColor = vec4(vec3(0.0f), 1.0f);
        }

    }

}
