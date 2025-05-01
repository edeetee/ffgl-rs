/*{
    "DESCRIPTION": "A chill Menger tunnel with fractal details",
    "CREDIT": "Converted to ISF",
    "CATEGORIES": ["Tunnel", "Fractal"],
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
            "DEFAULT": 0.6,
            "MIN": 0.0,
            "MAX": 2.0,
            "LABEL": "Speed"
        },
        {
            "NAME": "colorShift",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 1.0,
            "LABEL": "Color Shift"
        },
        {
            "NAME": "intensity",
            "TYPE": "float",
            "DEFAULT": 1.5,
            "MIN": 0.5,
            "MAX": 3.0,
            "LABEL": "Intensity"
        }
    ]
}*/

/*
    Just a chill Menger tunnel. No stereo, but I can add
    that if anybody wants it.
    
    you're not crazy, it does move a little bit.
*/

#define T ((TIME*speed) + progress*20.0)

#define W 2.
#define H 1.
#define L 180.

#define P(z) (vec3(tanh(cos((z) * .13) * .45) * 8., tanh(cos((z) * .15) * .4) * 16., (z)))
#define rot(a) mat2(cos(a), -sin(a), sin(a), cos(a))
#define N normalize
#define inf 9e9
#define TONEMAP(x) ((x) / ((x) + 0.155) * 1.019)

// Modified from Shane's Menger function
#define MENGERLAYER(scale, minmax, hole) s /= (scale), p = abs(fract(q / s) * s - s * .5f), d = minmax(d, min(max(p.x, p.y), min(max(p.y, p.z), max(p.x, p.z))) - s / (hole))

float fractal(in vec3 q) {
    vec3 p;
    float d = inf, s = 4.f;

    MENGERLAYER(1.f, min, 3.f);
    MENGERLAYER(3.f, max, 3.5f);
    MENGERLAYER(4.f, max, 3.5f);
    return d;
}

float tunnel(vec3 p) {
    vec2 t = P(p.z).xy;
    float s;

    s = min(length(p.xy - t.xy), min(length(p.xy - t.y), length(p.xy - t.x)));

    return 1.f - s;
}

float map(vec3 p) {
    return max(tunnel(p), fractal(p));
}

float AO(in vec3 pos, in vec3 nor) {
    float sca = 2.0f, occ = 0.0f;
    for(int i = 0; i < 5; i++) {

        float hr = 0.01f + float(i) * 0.5f / 4.0f;
        float dd = map(nor * hr + pos);
        occ += (hr - dd) * sca;
        sca *= 0.7f;
    }
    return clamp(1.0f - occ, 0.0f, 1.0f);
}

void main() {
    vec4 o;
    float s = .002f, d = 0.f, i = 0.f, a;
    vec3 r = vec3(RENDERSIZE, 1.0f);

    vec2 u = gl_FragCoord.xy;
    u = (u - r.xy / 2.f) / r.y;

    vec3 p = P(T), ro = p, e = vec3(.007f, 0, 0), Z = normalize(P(T + 1.f) - p), X = normalize(vec3(Z.z, 0, -Z)), D = vec3(rot(tanh(sin(p.z * .1f) * 1.5f) * 2.f) * u, 1) * mat3(-X, cross(X, Z), Z);

    o = vec4(0.0f);

    while(i++ < L && s > .001f)
        p = ro + D * d, p += cos(.3f * T + p.yzx) * .2f, d += (s = map(p) * .65f);

    float f = mod(p.z, 30.f);

    // Apply color shift based on parameter
    f = mod(f + colorShift * 30.0f, 30.0f);

    if(f > 20.f)
        o.rgb = vec3(.25f, .125f, .35f);
    else if(f > 10.f)
        o.rgb = vec3(.5f, .3f, .1f);
    else
        o.rgb = vec3(.1f, .3f, .5f);

    vec3 lights = abs(o.rgb /
        dot(cos(.25f * TIME + p * .2f), vec3(.01f)));

    r = N(map(p) - vec3(map(p - e.xyy), map(p - e.yxy), map(p - e.yyx)));

    o *= max(dot(r, normalize(ro - p)), .05f);
    o *= AO(p, r);
    o.rgb = mix(o.rgb, lights, .001f);
    o.rgb = pow(TONEMAP(o.rgb * vec3(1.5f, 1.f, .7f) * intensity * exp(-d / 2.f)) - dot(u, u) * .1f, vec3(.45f));

    o.a = 1.0f; // Set alpha
    gl_FragColor = o;
}