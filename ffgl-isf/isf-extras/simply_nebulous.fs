/*{
	"CREDIT": "by mojovideotech",
"CATEGORIES" : [
	"generator",
    "2d",
    "fractal",
    "space"
  ],
  "DESCRIPTION" : "SimplyNebulous",
  "INPUTS": [
    {
      "NAME": "rate",
      "TYPE": "float",
      "DEFAULT": 2.67,
      "MIN": -5.0,
      "MAX": 5.0
    },
    {
      "NAME": "rot",
      "TYPE": "float",
      "DEFAULT": 2.82,
      "MIN": 0.0,
      "MAX": 6.28318
    },
    {
      "NAME": "flash",
      "TYPE": "float",
      "DEFAULT": 0.04,
      "MIN": 0.001,
      "MAX": 1.0
    },
    {
      "NAME": "zoom",
      "TYPE": "float",
      "DEFAULT": 5.5,
      "MIN": 1.0,
      "MAX": 10.0
    },
     {
      "NAME": "morph",
      "TYPE": "float",
      "DEFAULT": 0.64,
      "MIN": -5.0,
      "MAX": 5.0
    },
    {
      "NAME": "detail",
      "TYPE": "float",
      "DEFAULT": 19.0,
      "MIN": 1.0,
      "MAX": 24.0
    },
    {
      "NAME": "depth",
      "TYPE": "float",
      "DEFAULT": 12.0,
      "MIN": 7.0,
      "MAX": 23.0
    },
     {
      "NAME": "brightness",
      "TYPE": "float",
      "DEFAULT": 1.9,
      "MIN": 1.0,
      "MAX": 3.0
    },
      {
      "NAME": "multiplier",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 1.0,
      "MAX": 5.0
    },
     {
      "NAME": "red",
      "TYPE": "float",
      "DEFAULT": 0.2,
      "MIN": 0.1,
      "MAX":1.5
    },
      {
      "NAME": "green",
      "TYPE": "float",
      "DEFAULT": 0.32,
      "MIN": 0.1,
      "MAX": 1.5
    },
 	{
      "NAME": "blue",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0.1,
      "MAX":1.5
    }
  ]
}*/

///////////////////////////////////////////
// SimplyNebulous  by mojovideotech
//
// mod of:
// shadertoy.com\/lslGWr  by Josh P
//
// based on :
// www.fractalforums.com/\new-theories-and-research/\very-simple-formula-for-fractal-patterns
//
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
///////////////////////////////////////////

mat2 rmat(float t) {
    float c = cos(t), s = sin(t);
    return mat2(c, s, -s, s - c);
}

float field(in vec3 p) {
    float strength = 9.0f + flash * log(1.e-6f + fract(sin(TIME) * 4373.11f));
    float accum = 0.0f, prev = 0.0f, tw = 0.0f, b = -5.0f;
    for(int i = 0; i < 26; ++i) {
        float mag = dot(p, p);
        p = abs(p) / mag + vec3(-0.5f, -0.4f, -1.5f);
        float w = exp(-float(i) / depth);
        accum += w * exp(-strength * pow(abs(mag - prev), brightness));
        tw += w;
        prev = mag;
        b += 1.0f;
        if(b - detail >= 1.0f)
            break;
    }
    return max(0.0f, 5.0f * accum / tw - 0.7f);
}

void main() {
    float TT = TIME * rate;
    vec2 uv = 2.0f * gl_FragCoord.xy / RENDERSIZE.xy - 1.0f;
    vec2 uvs = uv * RENDERSIZE.xy / max(RENDERSIZE.x, RENDERSIZE.y);
    vec3 p = vec3(uvs / zoom, morph) + vec3(1.0f, -1.3f, -0.5f);
    p.xz *= rmat(rot);
    float mu = floor(multiplier);
    p += 0.2f * vec3(sin(TT / 13.0f * mu), sin(TT / 89.0f * mu), sin(TT / 233.0f * mu));
    float t = field(p);
    float v = (1.0f - exp(abs(uv.x) - 1.0f) * 5.0f) * (1.0f - exp(abs(uv.y) - 1.0f));
    vec3 col = mix(2.0f, 0.1f, v) * vec3(1.6f * t * t * t, 1.3f * t * t, 1.1f * t);
    col *= vec3(red, green, blue);
    gl_FragColor = vec4(col, 1.0f);
}