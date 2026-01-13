/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/4sl3Dr by srtuss.  Some experiments with voronoi noise. I found many cool looking formulas, here is one of them. (Also try fullscreen!)\n*Now with colors.",
    "IMPORTED": {

    },
    "INPUTS": [
    ]
}

*/

// by srtuss, 2013

// rotate position around axis
vec2 rotate(vec2 p, float a) {
	return vec2(p.x * cos(a) - p.y * sin(a), p.x * sin(a) + p.y * cos(a));
}

// 1D random numbers
float rand(float n) {
	return fract(sin(n) * 43758.5453123f);
}

// 2D random numbers
vec2 rand2(in vec2 p) {
	return fract(vec2(sin(p.x * 591.32f + p.y * 154.077f), cos(p.x * 391.32f + p.y * 49.077f)));
}

vec3 rand3(in vec2 p) {
	return fract(vec3(sin(p.x * 591.32f + p.y * 154.077f), cos(p.x * 391.32f + p.y * 49.077f), sin(p.x * 910.34442f + p.y * -9999.077f)));
}

// 1D noise
float noise1(float p) {
	float fl = floor(p);
	float fc = fract(p);
	return mix(rand(fl), rand(fl + 1.0f), fc);
}

// voronoi distance noise, based on iq's articles
float voronoi(in vec2 x) {
	vec2 p = floor(x);
	vec2 f = fract(x);

	vec2 res = vec2(8.0f);
	for(int j = -1; j <= 1; j++) {
		for(int i = -1; i <= 1; i++) {
			vec2 b = vec2(i, j);
			vec2 r = vec2(b) - f + rand2(p + b);

			// chebyshev distance, one of many ways to do this
			float d = max(abs(r.x), abs(r.y));

			if(d < res.x) {
				res.y = res.x;
				res.x = d;
			} else if(d < res.y) {
				res.y = d;
			}
		}
	}
	return res.y - res.x;
}

void main() {

	float flicker = noise1(TIME * 2.0f) * 0.8f + 0.4f;
	vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	uv = (uv - 0.5f) * 2.0f;
	vec2 suv = uv;
	uv.x *= RENDERSIZE.x / RENDERSIZE.y;

	float v = 0.0f;

	// that looks highly interesting:
	//v = 1.0 - length(uv) * 1.3;

	// a bit of camera movement
	uv *= 0.6f + sin(TIME * 0.1f) * 0.4f;
	uv = rotate(uv, sin(TIME * 0.3f) * 1.0f);
	uv += TIME * 0.4f;

	// add some noise octaves
	float a = 0.6f, f = 1.0f;

	for(int i = 0; i < 3; i++) // 4 octaves also look nice, its getting a bit slow though
	{
		float v1 = voronoi(uv * f + 5.0f);
		float v2 = 0.0f;

		// make the moving electrons-effect for higher octaves
		if(i > 0) {
			// of course everything based on voronoi
			v2 = voronoi(uv * f * 0.5f + 50.0f + TIME);

			float va = 0.0f, vb = 0.0f;
			va = 1.0f - smoothstep(0.0f, 0.1f, v1);
			vb = 1.0f - smoothstep(0.0f, 0.08f, v2);
			v += a * pow(va * (0.5f + vb), 2.0f);
		}

		// make sharp edges
		v1 = 1.0f - smoothstep(0.0f, 0.3f, v1);

		// noise is used as intensity map
		v2 = a * (noise1(v1 * 5.5f + 0.1f));

		// octave 0's intensity changes a bit
		if(i == 0)
			v += v2 * flicker;
		else
			v += v2;

		f *= 3.0f;
		a *= 0.7f;
	}
	// slight vignetting
	v *= exp(-0.6f * length(suv)) * 1.2f;

	// use texture channel0 for color? why not.
	vec3 cexp = rand3(mod(uv * 0.001f, 1.0f)).xyz * 3.0f + rand3(mod(uv * 0.01f, 1.0f)).xyz;//vec3(1.0, 2.0, 4.0);
	cexp *= 1.4f;

	// old blueish color set
	//vec3 cexp = vec3(6.0, 4.0, 2.0);

	vec3 col = vec3(pow(v, cexp.x), pow(v, cexp.y), pow(v, cexp.z)) * 2.0f;

	gl_FragColor = vec4(col, 1.0f);
}
