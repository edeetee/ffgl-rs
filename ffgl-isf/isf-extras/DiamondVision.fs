/*
{
  "CATEGORIES" : [
    "generator"
  ],
  "DESCRIPTION" : "",
  "INPUTS" : [
    {
      "NAME" : "Speed",
      "TYPE" : "float",
      "MAX" : 1,
      "DEFAULT" : 0.10000000000000001,
      "MIN" : -1,
      "LABEL" : "Speed"
    },
    {
      "NAME" : "GRAT",
      "TYPE" : "float",
      "DEFAULT" : 1,
      "LABEL" : "rotation one"
    },
    {
      "NAME" : "W",
      "TYPE" : "float",
      "MAX" : 2,
      "DEFAULT" : 1,
      "MIN" : -0.001,
      "LABEL" : "Ray tracing amount"
    },
    {
      "NAME" : "R",
      "TYPE" : "float",
      "MAX" : 1,
      "DEFAULT" : 1,
      "LABEL" : "Color HUE",
      "MIN" : -1
    },
    {
      "NAME" : "Ty",
      "TYPE" : "float",
      "MAX" : 0,
      "DEFAULT" : 0,
      "MIN" : -1,
      "LABEL" : "Shine"
    },
    {
      "NAME" : "Rot",
      "TYPE" : "float",
      "DEFAULT" : 1,
      "LABEL" : "Rotation of Torus"
    },
    {
      "NAME" : "Rat",
      "TYPE" : "float",
      "MAX" : 2,
      "DEFAULT" : 1,
      "MIN" : -1
    },
    {
      "NAME" : "Bat",
      "TYPE" : "float",
      "MAX" : 1.3,
      "DEFAULT" : 1,
      "MIN" : 0.5,
      "LABEL" : "Amount of Toruses"
    }
  ],
  "ISFVSN" : "2",
  "CREDIT" : "by mojovideotech"
}
*/

////////////////////////////////////////////////////////////////////
// DiamondVision  by mojovideotech
//
// based on :
// glslsandbox.com\/e#46411.0
//
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0
////////////////////////////////////////////////////////////////////

#ifdef GL_ES
precision highp float;
#endif

#define    	c30    	0.866025     	// cos 30
#define    	twpi   	6.2831853       // two pi, 2*pi
#define 	pi   	3.1415926 		// pi
#define		piphi	2.3999632		// pi*phi
#define 	sqpi 	1.7724538		// square root of pi
#define 	phi   	1.6180339*GRAT	// golden ratio
#define 	cucuphi	1.0549232		// cube root of cube root of phi
#define		epi		0.8652559		// e/pi
#define 	time2	TIME*Speed+50.0

vec2 rotate(in vec2 r, in float o) {
    return vec2(cos(o) * r.x * Rot + sin(o) * r.y, -sin(o) * r.x + cos(o) * r.y * Rat) * Bat;
}

float torus(in vec3 pos, in vec2 tor) {
    vec2 qt = abs(vec2(max(abs(pos.x), abs(pos.z)) - tor.x, pos.y));
    return max(qt.x, qt.y) - tor.y;
}

float trap(in vec3 tp) {
    return abs(min(torus(tp, vec2(epi, 0.125f)), max(abs(tp.z) - 0.125f, abs(tp.x) - 0.125f))) - 0.005f;
}

float map(in vec3 pm) {
    float c = dot(abs((pm.yz)), vec2(0.5f)) - 0.05f;
    vec3 m = abs(1.0f - mod(pm, 2.0f));
    m.yz = rotate(m.yz, sqrt(time2));
    float e = 9999.999f, f = 1.0f * W;
    for(float i = 0.0f; i < 4.0f; i++) {
        m.xz = rotate(m.xz, radians(i * 0.333f + time2));
        m.zy = rotate(m.yz, radians((i + i) * 0.667f + time2 * phi));
        m = abs(1.0f - mod(m + i / 3.0f, 2.0f));
        m *= abs(sqrt(m) * sqpi);
        f *= 0.5f;
        e = min(e, trap(m) * f);
    }
    return max(e, -c);
}

vec3 hsv(in float h, in float s, in float v) {
    return mix(vec3(1.0f), clamp((abs(fract(h + vec3(3.0f, 2.0f, 1.0f) / 3.0f) * 6.0f - 3.0f) - 1.0f), 0.0f, 1.0f), s) * v;
}

vec3 intersect(in vec3 rayOrigin, in vec3 rayDir) {
    float d = 1.0f, it = 0.0f;
    vec3 p = rayOrigin, col = vec3(0.0f);
    float md = phi + sin(time2 * 0.5f) * 0.25f;
    for(int i = 0; i < 50; i++) {
        if(d < 0.000999f)
            continue;
        d = map(p);
        p += d * rayDir;
        md = min(md, d);
        it++;
    }
    if(d < 0.001f) {
        float x = (it / 49.0f);
        float y = (d - 0.01f) / 0.01f / (49.0f);
        float z = (0.01f - d) / 0.01f / 49.0f;
        float q = 1.0f - x - y * 2.f + z;
        col = hsv(q * 0.2f + 0.5f * R, 1.0f - q * epi, q + Ty);
    }
    col += hsv(d, 1.0f, 1.0f) * md * 28.0f;
    return col;
}

void main() {
    vec2 ps = -1.0f + 2.0f * gl_FragCoord.xy / RENDERSIZE.xy;
    ps.x *= RENDERSIZE.x / RENDERSIZE.y;
    vec3 up = vec3(0, -1, 0);
    vec3 cd = vec3(1, 0, 0);
    vec3 co = vec3(time2 * 0.1f, 0, 0);
    vec3 uw = normalize(cross(up, co));
    vec3 vw = normalize(cross(cd, uw));
    vec3 rd = normalize(uw * ps.x + vw * ps.y + cd * (1.0f - length(ps) * phi));
    gl_FragColor = vec4(vec3(intersect(co, rd)), 1.0f);
}