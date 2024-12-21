/*{
	"CREDIT": "by mojovideotech",
	"DESCRIPTION": "",
	"CATEGORIES": [
		"generator",
		"iterative",
		"fractal"
	],
  "INPUTS" : [
    {
        "NAME" :        "scale",
        "TYPE" :        "float",
        "DEFAULT" :     1.5,
        "MIN" :         0.5,
        "MAX" :         3.0
    },
    {
        "NAME" :        "rate",
        "TYPE" :        "float",
        "DEFAULT" :     3.0,
        "MIN" :         -5.0,
        "MAX" :         5.0
    },
    {
        "NAME" :        "loops",
        "TYPE" :        "float",
        "DEFAULT" :     20.0,
        "MIN" :         6.0,
        "MAX" :         40.0
    },
    {
        "NAME" :        "density",
        "TYPE" :        "float",
        "DEFAULT" :     1.5,
        "MIN" :         1.0,
        "MAX":          2.5
    },
    {
        "NAME" :        "depth",
        "TYPE" :        "float",
        "DEFAULT" :     12.0,
        "MIN" :         6.0,
        "MAX" :         20.0
    },
    {
        "NAME" :        "detail",
        "TYPE" :        "float",
        "DEFAULT" :     0.001,
        "MIN" :         0.0001,
        "MAX" :         0.1
    },
    {
        "NAME" :        "Xr",
        "TYPE" :        "float",
        "DEFAULT" :     -0.001,
        "MIN" :         -0.001,
        "MAX" :         0.001
    },
    {
        "NAME" :        "Yr",
        "TYPE" :        "float",
        "DEFAULT" :     0.003,
        "MIN" :         -0.001,
        "MAX" :         0.001
    },
    {
        "NAME" :        "Zr",
        "TYPE" :        "float",
        "DEFAULT" :     0.002,
        "MIN" :         -0.001,
        "MAX" :         0.001
    },
	{
		"NAME" : 		"R",
		"TYPE" : 		"float",
		"DEFAULT" : 	0.05,
		"MIN" : 		0.0,
		"MAX" : 		0.75
	},	
	{
		"NAME" : 		"G",
		"TYPE" : 		"float",
		"DEFAULT" : 	0.15,
		"MIN" : 		0.0,
		"MAX" : 		0.75
	},
	{
		"NAME" : 		"B",
		"TYPE" : 		"float",
		"DEFAULT" : 	0.667,
		"MIN" : 		0.0,
		"MAX" : 		0.75
	},
   	{
		"NAME" : 		"lightpos",
		"TYPE" : 		"point2D",
		"DEFAULT" :		[ 1.0, -20.0 ],
		"MAX" : 		[ 10.0, -5.0 ],
     	"MIN" : 		[ 5.0, -25.0 ]
	}


  ],
    "ISFVSN" : "2.0"
}
*/

////////////////////////////////////////////////////////////
// FractilianSpongeOfDoom   by mojovideotech
//
// based on 
// shadertoy.com\/MdKyRw  by wyatt
//
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0
////////////////////////////////////////////////////////////

vec4 light;
float T;
mat2 m, n, nn;

float map(vec3 p) {
    float t = 2.5f, d = length(p - light.xyz) - light.w;
    d = min(d, max(10.0f - p.z, 0.0f));
    for(int i = 0; i < 20; i++) {
        if(float(i) > depth)
            break;
        t = t * 0.66f;
        p.xy = m * p.xy;
        p.yz = n * p.yz;
        p.zx = nn * p.zx;
        p.xz = abs(p.xz) - t;
    }
    d = min(d, length(p) - density * t);
    return d;
}

vec3 norm(vec3 p) {
    vec2 e = vec2(detail, 0.0f);
    return normalize(vec3(map(p + e.xyy) - map(p - e.xyy), map(p + e.yxy) - map(p - e.yxy), map(p + e.yyx) - map(p - e.yyx)));
}

vec3 ray(vec3 r, vec3 d) {
    for(int i = 0; i < 40; i++) {
        if(float(i) > loops)
            break;
        r += d * map(r);
    }
    return r;
}

mat2 rot(float s) {
    return mat2(sin(s), cos(s), -cos(s), sin(s));
}

void main() {
    vec2 v = (gl_FragCoord.xy / RENDERSIZE.xy * 2.0f - 1.0f) * scale;
    v.x *= RENDERSIZE.x / RENDERSIZE.y;
    T = rate * TIME * 10.0f;
    m = rot(Xr * T);
    n = rot(Yr * T);
    nn = rot(Zr * T);
    vec3 r = vec3(0.0f, 0.0f, -15.0f + 2.0f * sin(0.01f * T));
    light = vec4(10.0f * sin(0.01f * T), lightpos.xy, 1.0f);
    vec3 d = normalize(vec3(v, 5.0f));
    vec3 p = ray(r, d);
    d = normalize(light.xyz - p);
    vec3 no = norm(p);
    vec3 col = vec3(R, G, B) + 0.25f;
    vec3 bounce = ray(p + 0.01f * d, d);
    col = mix(col, vec3(0.0f), dot(no, normalize(light.xyz - p)));
    if(length(bounce - light.xyz) > light.w + 0.1f)
        col *= 0.2f;
    gl_FragColor = vec4(col, 1.0f);
}
