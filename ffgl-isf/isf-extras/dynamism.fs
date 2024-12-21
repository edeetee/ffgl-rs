/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/MtKSWW by nimitz.  Buffer A is the fbm noise gradient generation\nBuffer B is the exact same thing\nBuffer C is the divergence calculator and coloring\nBuffer D is radial blur",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iMouse",
            "TYPE": "point2D"
        }
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
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferC"
        },
        {
        }
    ]
}

*/

// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

#define time TIME
#define time2 (time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5)
#define time3 (time*1. + ((1.0+sin(time*0.9 + sin(time*0.34+ cos(time*0.21)))))*1.5)
#define time4 (time*0.5 + ((1.0+sin(time*0.8 + sin(time*0.14+ cos(time*0.15)))))*1.2)

vec2 hash(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(.1031f, .1030f, .0973f));
    p3 += dot(p3.zxy, p3.yxz + 19.19f);
    return -1.0f + 2.0f * fract(vec2(p3.x * p3.y, p3.z * p3.x));
}

//2D Simplex noise from iq: https://www.shadertoy.com/view/Msf3WH
float noise(in vec2 p) {
    p *= 0.45f;
    const float K1 = 0.366025404f;
    const float K2 = 0.211324865f;

    vec2 i = floor(p + (p.x + p.y) * K1);

    vec2 a = p - i + (i.x + i.y) * K2;
    vec2 o = (a.x > a.y) ? vec2(1.0f, 0.0f) : vec2(0.0f, 1.0f);
    vec2 b = a - o + K2;
    vec2 c = a - 1.0f + 2.0f * K2;

    vec3 h = max(0.5f - vec3(dot(a, a), dot(b, b), dot(c, c)), 0.0f);

    vec3 n = h * h * h * h * vec3(dot(a, hash(i + 0.0f)), dot(b, hash(i + o)), dot(c, hash(i + 1.0f)));

    return dot(n, vec3(38.0f));

}

mat2 rot(in float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, s, -s, c);
}
float fbm(in vec2 p, in vec2 of) {
    p *= rot(time3 * 0.1f);
    p += of;
    float z = 2.f;
    float rz = 0.f;
    vec2 bp = p;
    for(float i = 1.f; i < 9.f; i++) {
        rz += noise(p * rot(float(i) * 2.3f) + time * 0.5f) / z;
        z *= 3.2f;
        p *= 2.0f;
    }
    return rz;
}

vec2 grdf(in vec2 p, in vec2 of) {
    vec2 ep = vec2(0.0f, 0.0005f);
    vec2 d = vec2(fbm(p - ep.yx, of) - fbm(p + ep.yx, of), fbm(p - ep.xy, of) - fbm(p + ep.xy, of));
    d /= length(d);
    return d;
}

// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

#define time TIME
#define time2 (time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5)
#define time3 (time*1. + ((1.0+sin(time*0.9 + sin(time*0.34+ cos(time*0.21)))))*1.5)
#define time4 (time*0.5 + ((1.0+sin(time*0.8 + sin(time*0.14+ cos(time*0.15)))))*1.2)

// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

#define time TIME

#define time2v (((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5)

//Divergence
vec2 div(vec2 p, sampler2D smp) {
    vec2 tx = 1.f / RENDERSIZE.xy;

    vec4 uv = textureLod(smp, p, -100.f);
    vec4 uv_n = textureLod(smp, p + vec2(0.0f, tx.y), -100.f);
    vec4 uv_e = textureLod(smp, p + vec2(tx.x, 0.0f), -100.f);
    vec4 uv_s = textureLod(smp, p + vec2(0.0f, -tx.y), -100.f);
    vec4 uv_w = textureLod(smp, p + vec2(-tx.x, 0.0f), -100.f);

    float div = uv_s.y - uv_n.y - uv_e.x + uv_w.x;
    float div2 = uv_s.w - uv_n.w - uv_e.z + uv_w.z;

    return vec2(div, div2) * 1.8f;
}

// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

/*
	Mostly about showing divergence based procedural noise, the rest is just me
	playing around to make it somewhat interesting to look at.

	I stumbled upon this new form of noise while playing with noise gradients
	and noise diverengence. First generate more or less standard fbm (with high decay)
	then compute the gradient of that noise (either numerically or analytically) and 
	then compute the divergence of the gradient and you get the noise you see here.

	As you can see it has a very "DLA" look to it. It is also very easy to animate as
	you can simply offset the noise fetches inside the initial fbm generation and produce
	good looking animated noise. I did some	testing and the paremeters can be modified 
	to result in a decent variety of output	noises, altough still somewhat similar than
	what is seen here.

	I have not tested it yet, but this method should extend to 3D without issues
	and should result in interesting volumes.

	This shader used to run at 60fps with webGL 1 but since webGL it seems
	capped at 30fps on my test computer.
*/

const vec2 center = vec2(0, 0);
const int samples = 15;
const float wCurveA = 1.f;
const float wCurveB = 1.f;
const float dspCurveA = 2.f;
const float dspCurveB = 1.f;

#define time TIME

float wcurve(float x, float a, float b) {
    float r = pow(a + b, a + b) / (pow(a, a) * pow(b, b));
    return r * pow(x, a) * pow(1.0f - x, b);
}

float hash21(in vec2 n) {
    return fract(sin(dot(n, vec2(12.9898f, 4.1414f))) * 43758.5453f);
}

void main() {
    if(PASSINDEX == 0) {

        vec2 p = gl_FragCoord.xy / RENDERSIZE.xy - 0.5f;
        p.x *= RENDERSIZE.x / RENDERSIZE.y;
        p *= 1.75f;

        float t1 = mod(time2 * 0.35f, 4.f);
        float t2 = mod(time2 * 0.35f + 1.f, 4.f);

        vec2 p1 = p * (4.0f - t1);
        vec2 p2 = p * (4.0f - t2);

        vec2 fld = grdf(p1, vec2(time4 * 0.2f, time * 0.0f));
        vec2 fld2 = grdf(p2, vec2(time4 * 0.2f, time * 0.0f) + 2.2f);

        gl_FragColor = vec4(fld, fld2);
    } else if(PASSINDEX == 1) {

        vec2 p = gl_FragCoord.xy / RENDERSIZE.xy - 0.5f;
        p.x *= RENDERSIZE.x / RENDERSIZE.y;
        p *= 1.75f;

        float t3 = mod(time2 * 0.35f + 2.f, 4.f);
        float t4 = mod(time2 * 0.35f + 3.f, 4.f);

        vec2 p3 = p * (4.0f - t3);
        vec2 p4 = p * (4.0f - t4);

        vec2 fld = grdf(p3, vec2(time4 * 0.2f, time * 0.0f) + 4.5f);
        vec2 fld2 = grdf(p4, vec2(time4 * 0.2f, time * 0.0f) + 7.3f);

        gl_FragColor = vec4(fld, fld2);
    } else if(PASSINDEX == 2) {

        vec2 p = gl_FragCoord.xy / RENDERSIZE.xy;

        vec2 dv = div(p, BufferA);
        vec2 dv2 = div(p, BufferB);

        dv = pow(abs(dv), vec2(.5f)) * sign(dv);
        dv = clamp(dv, 0.f, 4.f);
        dv2 = pow(abs(dv2), vec2(.5f)) * sign(dv2);
        dv2 = clamp(dv2, 0.f, 4.f);

        float t1 = mod(time2 * 0.35f, 4.f);
        float t2 = mod(time2 * 0.35f + 1.f, 4.f);
        float t3 = mod(time2 * 0.35f + 2.f, 4.f);
        float t4 = mod(time2 * 0.35f + 3.f, 4.f);

        const float ws = 1.1f;
        const float wof = 1.8f;

	    //derivative of the "depth"
	    //time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5
        float x = time;
        float drvT = 1.5f * cos(x + sin(0.4f * x + cos(0.1f * x))) * (cos(0.4f * x + cos(0.1f * x)) * (0.4f - 0.1f * sin(0.1f * x)) + 1.0f) + 2.1f;

        float ofsc = 0.8f + drvT * 0.07f;
        float t1w = clamp(t1 * ws + wof, 0.f, 10.f);
        float t2w = clamp(t2 * ws + wof, 0.f, 10.f);
        float t3w = clamp(t3 * ws + wof, 0.f, 10.f);
        float t4w = clamp(t4 * ws + wof, 0.f, 10.f);

        vec3 col = vec3(0);

        col += sqrt(t1) * vec3(0.28f, 0.19f, 0.15f) * exp2(dv.x * t1w - t1w * ofsc);
        col += sqrt(t2) * vec3(0.1f, 0.13f, 0.23f) * exp2(dv.y * t2w - t2w * ofsc);
        col += sqrt(t3) * vec3(0.27f, 0.07f, 0.07f) * exp2(dv2.x * t3w - t3w * ofsc);
        col += sqrt(t4) * vec3(0.1f, 0.18f, 0.25f) * exp2(dv2.y * t4w - t4w * ofsc);

        col = pow(col, vec3(.6f)) * 1.2f;
        col *= smoothstep(0.f, 1.f, col);

        col *= pow(16.0f * p.x * p.y * (1.0f - p.x) * (1.0f - p.y), 0.4f);

        gl_FragColor = vec4(col, 1.0f);
    } else if(PASSINDEX == 3) {

        vec2 p = gl_FragCoord.xy / RENDERSIZE.xy;
        vec2 mo = iMouse.xy / RENDERSIZE.xy;

        vec2 center = mo;
        center = vec2(0.5f, 0.5f);

        vec3 col = vec3(0.0f);
        vec2 tc = center - p;

        float w = 1.0f;
        float tw = 1.f;

        float rnd = (hash21(p) - 0.5f) * 0.75f;

	    //derivative of the "depth"
	    //time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5
        float x = time;
        float drvT = 1.5f * cos(x + sin(0.4f * x + cos(0.1f * x))) * (cos(0.4f * x + cos(0.1f * x)) * (0.4f - 0.1f * sin(0.1f * x)) + 1.0f) + 2.1f;

        float strength = 0.01f + drvT * 0.01f;

        for(int i = 0; i < samples; i++) {
            float sr = float(i) / float(samples);
            float sr2 = (float(i) + rnd) / float(samples);
            float weight = wcurve(sr2, wCurveA, wCurveB);
            float displ = wcurve(sr2, dspCurveA, dspCurveB);
            col += IMG_NORM_PIXEL(BufferC, mod(p + (tc * sr2 * strength * displ), 1.0f)).rgb * weight;
            tw += .9f * weight;
        }
        col /= tw;

        gl_FragColor = vec4(col, 1.0f);
    }

}
