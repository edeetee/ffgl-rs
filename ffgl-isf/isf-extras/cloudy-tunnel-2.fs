/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/XlSSzV by aiekick.  Variation more cloudy of my https://www.shadertoy.com/view/4lSXRK\nyou can modify cloudy precision with mouse y axis",
    "INPUTS": [
        {
            "LABEL": "offset",
            "NAME": "offset",
            "TYPE": "float"
        },
        {
            "NAME": "speed",
            "LABEL": "Animation Speed",
            "TYPE": "float",
            "MIN": 0.1,
            "MAX": 5.0,
            "DEFAULT": 1.0
        },
        {
            "NAME": "tunnelWidth",
            "LABEL": "Tunnel Width",
            "TYPE": "float",
            "MIN": 0.5,
            "MAX": 5.0,
            "DEFAULT": 1.0
        },
        {
            "NAME": "cloudVariety",
            "LABEL": "Cloud Variety",
            "TYPE": "float",
            "MIN": 0.1,
            "MAX": 5.0,
            "DEFAULT": 1.0
        },
        {
            "NAME": "progress",
            "LABEL": "Progress",
            "TYPE": "float",
            "MIN": 0.0,
            "MAX": 1.0,
            "DEFAULT": 0.0
        }
    ],
    "ISFVSN": "2"
}
*/

// Created by Stephane Cuillerdier - Aiekick/2015
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// Tuned via XShade (http://www.funparadigm.com/xshade/)

float t;

float cosPath(vec3 p, vec3 dec) {
	return dec.x * cos(p.z * dec.y + dec.z);
}
float sinPath(vec3 p, vec3 dec) {
	return dec.x * sin(p.z * dec.y + dec.z);
}

vec2 getCylinder(vec3 p, vec2 pos, float r, vec3 c, vec3 s) {
	return p.xy - pos - vec2(cosPath(p, c), sinPath(p, s));
}

/////////////////////////
// FROM Shader Cloudy spikeball from duke : https://www.shadertoy.com/view/MljXDw
// Hash function replacement for the original texture lookup
vec2 hash22(vec2 p) {
	p = vec2(dot(p, vec2(127.1f, 311.7f)), dot(p, vec2(269.5f, 183.3f)));
	return -1.0f + 2.0f * fract(sin(p) * 43758.5453123f);
}

float pn(in vec3 x) {
	vec3 p = floor(x);
	vec3 f = fract(x);
	f = f * f * (3.0f - 2.0f * f);

	// Hash-based random values instead of texture lookup
	vec2 uv = (p.xy + vec2(37.0f, 17.0f) * p.z);
	vec2 rg00 = hash22(uv) * 0.5f + 0.5f;
	vec2 rg10 = hash22(uv + vec2(1.0f, 0.0f)) * 0.5f + 0.5f;
	vec2 rg01 = hash22(uv + vec2(0.0f, 1.0f)) * 0.5f + 0.5f;
	vec2 rg11 = hash22(uv + vec2(1.0f, 1.0f)) * 0.5f + 0.5f;

	// Bilinear interpolation
	vec2 xy1 = mix(rg00, rg10, f.x);
	vec2 xy2 = mix(rg01, rg11, f.x);
	vec2 rg = mix(xy1, xy2, f.y);

	return -1.0f + 2.4f * mix(rg.x, rg.y, f.z);
}

float fpn(vec3 p) {
	p += t * 5.f;
	return pn(p * 0.02f) * 1.98f + pn(p * 0.02f) * 0.62f + pn(p * 0.09f) * 0.39f;
}
/////////////////////////

float map(vec3 p) {
	float pnNoise = fpn(p * 13.f) * .8f;
	float path = sinPath(p, vec3(6.2f, .33f, 0.f));
	float bottom = p.y + pnNoise;
	float cyl = 0.f;
	vec2 vecOld;
	for(float i = 0.f; i < 6.f; i++) {
		float x = 1.f * i;
		float y = .88f + 0.0102f * i;
		float z = -0.02f - 0.16f * i;
		float r = 4.4f + 2.45f * i * tunnelWidth;
		vec2 vec = getCylinder(p, vec2(path, 3.7f * i), r, vec3(x, y, z), vec3(z, x, y));
		cyl = r - min(length(vec), length(vecOld));
		vecOld = vec;
	}
	cyl += pnNoise;
	cyl = min(cyl, bottom);
	return cyl;
}

vec3 cam(vec2 uv, vec3 ro, vec3 cu, vec3 cv) {
	vec3 rov = normalize(cv - ro);
	vec3 u = normalize(cross(cu, rov));
	vec3 v = normalize(cross(rov, u));
	float fov = 3.f;
	vec3 rd = normalize(rov + fov * u * uv.x + fov * v * uv.y);
	return rd;
}

void main() {

	t = (TIME * speed + progress * 10.0f) * 2.5f;
	gl_FragColor = vec4(0, 0.15f, 0.32f, 1);
	vec2 si = RENDERSIZE.xy;
	vec2 uv = (2.f * gl_FragCoord.xy - si) / min(si.x, si.y);
	vec3 ro = vec3(0), p = ro;
	ro.y = sin(t * .2f) * 15.f + 15.f;
	ro.x = sin(t * .5f) * 5.f;
	ro.z = t * 5.f;
	vec3 rd = cam(uv, p, vec3(0, 1, 0), p + vec3(0, 0, 1));
	float s = 1.f, h = .15f, td = 0.f, d = 1.f, dd = 0.f, w;
	float var = 0.03f * cloudVariety;

	for(float i = 0.f; i < 200.f; i++) {
		if(s < 0.01f || d > 500.f || td > .95f)
			break;
		s = map(p) * (s > 0.001f ? var : .2f);
		if(s < h) {
			w = (1.f - td) * (h - s) * i / 200.f;
			gl_FragColor += w;
			td += w;
		}
		dd += 0.012f;
		td += 0.005f;
		s = max(s, 0.05f);
		d += s;
		p = ro + rd * d;
	}
	gl_FragColor.rgb = mix(gl_FragColor.rgb, vec3(0, 0.15f, 0.52f), 1.0f - exp(-0.001f * d * d)) / dd; // fog

	// vigneting from iq Shader Mike : https://www.shadertoy.com/view/MsXGWr
	vec2 q = gl_FragCoord.xy / si;
	gl_FragColor.rgb *= 0.5f + 0.5f * pow(16.0f * q.x * q.y * (1.0f - q.x) * (1.0f - q.y), 0.25f);
}
