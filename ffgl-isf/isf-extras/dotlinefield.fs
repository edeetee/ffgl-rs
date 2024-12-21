/*{
	"CREDIT": "by UaraLab forked from mojovideotech Inner Dimensional Matrix",
	"CATEGORIES" : [ "generator"
  ],
  "DESCRIPTION" : "",
  "INPUTS" : [
  	{
     	"NAME" :		"quantity",
     	"TYPE" : 		"float",
    	"DEFAULT" :		42.0,
     	"MIN" : 		0.0,
      	"MAX" :			100.0
	},
	{
     	"NAME" :		"fade",
     	"TYPE" : 		"float",
    	"DEFAULT" :		0.0,
     	"MIN" : 		0.0,
      	"MAX" :			0.9999999
	},
	{
     	"NAME" :		"seed1",
     	"TYPE" : 		"float",
    	"DEFAULT" :		155,
     	"MIN" : 		34,
      	"MAX" :			233
	},
    {
      	"NAME" :		"seed2",
      	"TYPE" :		"float",
      	"DEFAULT" :		649,
      	"MIN" : 		89,
      	"MAX" :			987	
	},
	{
		"NAME" : 		"scale",
		"TYPE" : 		"float",
		"DEFAULT" : 	1.3,
		"MIN" : 		0.25,
		"MAX" : 		2.0
	},
	{
		"NAME" : 		"rate",
		"TYPE" : 		"float",
		"DEFAULT" : 	5.9,
		"MIN" : 		0.0,
		"MAX" : 		10.0
	},
	{
		"NAME" : 		"zoom",
		"TYPE" : 		"float",
		"DEFAULT" : 	0.22,
		"MIN" : 		-1.0,
		"MAX" : 		1.0
	},
	{
		"NAME" : 		"line",
		"TYPE" : 		"float",
		"DEFAULT" : 	0.37,
		"MIN" : 		0.0,
		"MAX" : 		0.5
	},
	{
		"NAME" : 		"flash",
		"TYPE" : 		"float",
		"DEFAULT" : 	7.5,
		"MIN" : 		0.5,
		"MAX" : 		10.0
	},
	{
   		"NAME" : 		"mirror",
     	"TYPE" : 		"bool",
     	"DEFAULT" : 	false
   	},
   	{
   		"NAME" : 		"color",
     	"TYPE" : 		"bool",
     	"DEFAULT" : 	false
   	},
   	{
		"NAME" : 		"cycle",
		"TYPE" : 		"float",
		"DEFAULT" : 	1.0,
		"MIN" : 		0.05,
		"MAX" : 		20.0
	},
	{
			"NAME": "mathOp",
			"TYPE": "long",
			"VALUES": [
				0,
				1,
				2
			],
			"LABELS": [
				"pow",
				"dot",
				"fract"
			],
			"DEFAULT": 0
		}
  ]
}
*/

////////////////////////////////////////////////////////////////////
// InnerDimensionalMatrix  by mojovideotech
//
// based on :
// The Universe Within - by Martijn Steinrucken aka BigWings 2018
// shadertoy.com/\lscczl
// glslsandbox.com\/e#47584.1
//
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0
////////////////////////////////////////////////////////////////////

#ifdef GL_ES
precision highp float;
#endif

#define S(a, b, t) smoothstep(a, b, t)

float N1(float n) {
    return fract(sin(n) * 43758.5453123f);
}

float N11(float p) {
    float fl = floor(p);
    float fc = fract(p);
    return mix(N1(fl), N1(fl + 1.0f), fc);
}

float N21(vec2 p) {
    return fract(sin(p.x * floor(seed1) + p.y * floor(seed2)) * floor(seed2 + seed1));
}

vec2 N22(vec2 p) {
    return vec2(N21(p), N21(p + floor(seed2)));
}

float L(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float t = clamp(dot(pa, ba) / dot(ba, ba), 0.0f, 1.0f);
    float d = length(pa - ba * t);
    float m = S(0.02f, 0.0f, d);
    d = length(a - b);
    float f = S(1.0f, fade, d);
    m *= f;
    m += m * S(0.05f, 0.06f, abs(d - 0.5f)) * 0.0f;
    return m;
}

vec2 GetPos(vec2 p, vec2 o) {
    p += o;
    vec2 n = N22(p) * TIME * rate;
    p = sin(n) * line;
    return o + p;
}

float G(vec2 uv) {
    vec2 id = floor(uv);
    uv = fract(uv) - 0.5f;
    vec2 g = GetPos(id, vec2(0));
    float m = 0.0f;
    for(float y = -1.0f; y <= 2.0f; y++) {
        for(float x = -1.0f; x <= 2.0f; x++) {
            vec2 offs = vec2(x, y);
            vec2 p = GetPos(id, offs);
            m += L(uv, g, p);
            vec2 a = p - uv;
            float f = 0.003f / dot(a, a);
            if(mathOp == 0) {
                f *= pow(sin(N21(id + offs) * 6.2831f + (flash * TIME)) * 0.4f + 0.6f, flash);
            } else if(mathOp == 1) {
                f *= dot(sin(N21(id + offs) * 6.2831f + (flash * TIME)) * 0.4f + 0.6f, flash);
            } else if(mathOp == 2) {
                f *= dot(fract(N21(id + offs) * 6.2831f + (flash * TIME)) * 0.4f + 0.6f, flash);
            }
            m += f;
        }
    }
    m += L(uv, GetPos(id, vec2(-1, 0)), GetPos(id, vec2(0, -1)));
    m += L(uv, GetPos(id, vec2(0, -1)), GetPos(id, vec2(1, 0)));
    m += L(uv, GetPos(id, vec2(1, 0)), GetPos(id, vec2(0, 1)));
    m += L(uv, GetPos(id, vec2(0, 1)), GetPos(id, vec2(-1, 0)));
    return m;
}

void main() {
    vec2 uv = (2.25f - scale) * (gl_FragCoord.xy - 0.5f * RENDERSIZE.xy) / RENDERSIZE.y;
    if(mirror) {
        if(uv.x < 0.0f)
            uv.x = abs(uv.x);
    }
    float m = 0.0f;
    vec3 col;
    for(float i = 0.0f; i < 1.0f; i += 0.2f) {
        float z = fract(i + TIME * zoom);
        float s = mix(quantity, 0.5f, z);
        float f = S(0.0f, 0.4f, z) * S(1.0f, 0.8f, z);
        m += G(uv * s + (N11(i) * 100.0f) * i) * f;
    }
    if(color) {
        col = 0.5f + sin(vec3(1.0f, 0.5f, 0.75f) * TIME * cycle) * 0.5f;
    } else
        col = vec3(1.0f);
    col *= m;
    gl_FragColor = vec4(col, 1.0f);
}