/*{
    "CREDIT": "Mad Team",
    "DESCRIPTION": "Dunes to Zebra Stripes.",
    "VSN": "1.0",
    "TAGS": "graphic",
    "INPUTS": [
        { "LABEL": "Scale", "NAME": "scale", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 4.0 },
        { "LABEL": "Speed", "NAME": "speed", "TYPE": "float", "MIN" : 0.0, "MAX" : 4.0, "DEFAULT": 1.0 },
        { "LABEL": "Reverse", "NAME": "reverse", "TYPE": "bool", "DEFAULT": false, "FLAGS": "button" },
        { "LABEL": "Foreground Color", "NAME": "foregroundColor", "TYPE": "color", "DEFAULT": [ 1.0, 1.0, 1.0, 1.0 ] },
        { "LABEL": "Background Color", "NAME": "backgroundColor", "TYPE": "color", "DEFAULT": [ 0.0, 0.0, 0.0, 1.0 ] },
        { "LABEL": "Brightness", "NAME": "brightness", "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0 },
        { "LABEL": "Contrast", "NAME": "contrast", "TYPE": "float", "MIN": 1.0, "MAX": 3.0, "DEFAULT": 1 }
    ]
}*/

vec2 hash(vec2 p) {
    p = vec2(dot(p, vec2(127.1, 311.7)),
             dot(p, vec2(269.5, 183.3)));
    return -1.0 + 2.0 * fract(sin(p + 20.0) * 53758.5453123);
}

float dunes_noise(in vec2 x) {
    vec2 p = floor(x);
    vec2 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    vec2 uv = (p + vec2(37.0, 17.0)) + f;
    vec2 rg = hash(uv / 256.0).yx;
    return 0.5 * mix(rg.x, rg.y, 0.5);
}

#define GABOR_BLOBS_NB 10
#define GABOR_BLOBS_SIZE 0.25

float rnd(int i, int j) {
    return dunes_noise(vec2(i, j));
}

float DuneStripes(vec2 uv, float d, float freq, float time) {
    float hv = 0.0;
    for (int i = 0; i < GABOR_BLOBS_NB; i++) {
        vec2 pos = vec2(rnd(i, 0), rnd(i, 1));
        vec2 dir = (0.15 + d) * vec2(rnd(i, 2), rnd(i, 3)) - d;
        hv += GABOR_BLOBS_SIZE * sin(dot(uv - pos, freq * dir) * 6.0 + time);
    }
    return hv;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    uv = vec2(0.5, 0.5) + (uv - vec2(0.5, 0.5)) * scale;
    float time = TIME * speed * (reverse ? -1.0 : 1.0);
    float h = DuneStripes(uv, -0.5, 10.0, -3.5 * time);
    vec3 color = mix(backgroundColor.rgb, foregroundColor.rgb, clamp(h, 0.0, 1.0));

    // Apply contrast
    color = mix(vec3(0.5), color, contrast);

    // Apply brightness
    color += vec3(brightness);

    gl_FragColor = vec4(color, 1.0);
}
