/*{
  "CREDIT": "by mojovideotech",
  "DESCRIPTION": "",
  "CATEGORIES": [
  		"generator",
  		"2d"
  	],
  "INPUTS": [
    {
      "MAX": [
        300,
        200
      ],
      "MIN": [
        10,
        6
      ],
      "DEFAULT": [
        100,
        50
      ],
      "NAME": "grid",
      "TYPE": "point2D"
    },
    {
      "NAME": "density",
      "TYPE": "float",
      "DEFAULT": 1000,
      "MIN": -900,
      "MAX": 1800
    },
    {
      "NAME": "rate",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": -3,
      "MAX": 3
    },
    {
      "NAME": "seed1",
      "TYPE": "float",
      "DEFAULT": 55,
      "MIN": 8,
      "MAX": 233
    },
    {
      "NAME": "seed2",
      "TYPE": "float",
      "DEFAULT": 89,
      "MIN": 55,
      "MAX": 987
    },
    {
      "NAME": "seed3",
      "TYPE": "float",
      "DEFAULT": 514229,
      "MIN": 75025,
      "MAX": 3524578
    },
    {
      "NAME": "offset1",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -100,
      "MAX": 100
    },
    {
      "NAME": "offset2",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -100,
      "MAX": 100
    }
  ]
}*/

///////////////////////////////////////////
// BitStreamer  by mojovideotech
//
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0
//
// based on :
// www.patriciogonzalezvivo.com/2015/thebookofshaders/10/ikeda-03.frag
//
// from :
// thebookofshaders.com  by Patricio Gonzalez Vivo
///////////////////////////////////////////

float ranf(in float x) {
    return fract(sin(x) * 1e4f);
}

float rant(in vec2 st) {
    return fract(sin(dot(st.xy, vec2(seed1, seed2))) * seed3);
}

float pattern(vec2 st, vec2 v, float t) {
    vec2 p = floor(st + v);
    return step(t, rant(100.f + p * .000001f) + ranf(p.x) * 0.5f);
}

void main() {
    vec2 st = gl_FragCoord.xy / RENDERSIZE.xy;
    st.x *= RENDERSIZE.x / RENDERSIZE.y;
    st *= grid;

    vec2 ipos = floor(st);
    vec2 fpos = fract(st);
    vec2 vel = vec2(TIME * rate * max(grid.x, grid.y));
    vel *= vec2(-1.f, 0.0f) * ranf(1.0f + ipos.y);
    vec2 off1 = vec2(offset1, 0.f);
    vec2 off2 = vec2(offset2, 0.f);
    vec3 color = vec3(0.f);
    color.r = pattern(st + off1, vel, 0.5f + density / RENDERSIZE.x);
    color.g = pattern(st, vel, 0.5f + density / RENDERSIZE.x);
    color.b = pattern(st - off2, vel, 0.5f + density / RENDERSIZE.x);
    color *= step(0.2f, fpos.y);

    gl_FragColor = vec4(color, 1.0f);
}