/*{
    "DESCRIPTION": "A ring of points",
	"ISFVSN": "2.0",
	"VSN": "2.0",
	"CATEGORIES": [
		"TEST-GLSL FX"
	],
	"INPUTS": [
		{
			"NAME": "inputPoints",
			"TYPE": "image"
		},
		{
			"NAME": "width",
			"TYPE": "float"
		},
	    {
			"NAME": "radius",
			"TYPE": "float"
		}
	]
}
*/

// Example Pixel Shader

// uniform float exampleUniform;

float sdCircle(in vec2 p, in float r) {
    return length(p) - r;
}

float opSmoothUnion(float d1, float d2, float k) {
    float h = clamp(0.5f + 0.5f * (d2 - d1) / k, 0.0f, 1.0f);
    return mix(d2, d1, h) - k * h * (1.0f - h);
}

float opSmoothSubtraction(float d1, float d2, float k) {
    float h = clamp(0.5f - 0.5f * (d2 + d1) / k, 0.0f, 1.0f);
    return mix(d2, -d1, h) + k * h * (1.0f - h);
}

float opSmoothIntersection(float d1, float d2, float k) {
    float h = clamp(0.5f - 0.5f * (d2 - d1) / k, 0.0f, 1.0f);
    return mix(d2, d1, h) + k * h * (1.0f - h);
}
 
// sc = sin/cos of aperture
vec3 sdgArc(in vec2 p, in vec2 sc, in float ra, in float rb) {
    vec2 q = p;
    float s = sign(p.x);
    p.x = abs(p.x);
    if(sc.y * p.x > sc.x * p.y) {
        vec2 w = p - ra * sc;
        float d = length(w);
        return vec3(d - rb, vec2(s * w.x, w.y) / d);
    } else {
        float l = length(q);
        float w = l - ra;
        return vec3(abs(w) - rb, sign(w) * q / l);
    }
}
 
float sdRing(in vec2 p) {
    return sdgArc(p, vec2(0.0f, 0.0f), radius, width).r;
}

vec4 points(int pointIndex) {
	return IMG_PIXEL(inputPoints, vec2(pointIndex,0.5f)*IMG_SIZE(inputPoints));
}

void main() {
	
    vec4 uv = isf_FragNormCoord;
 
    float dist = sdRing(uv.xy + points(0).xy);

    for(int i = 1; i < IMG_SIZE(inputPoints).x; i++) {
        dist = opSmoothUnion(dist, sdRing(uv.xy + points(i).xy), 0.01f);
    }
 
	// float val = 1-step(width/2, dist);

    fragColor = vec4(dist);
}
 