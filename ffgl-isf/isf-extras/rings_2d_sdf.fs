/*
{
    "DESCRIPTION": "A ring of points"
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

#define NUM_POINTS 12

// uniform vec2[NUM_POINTS] points;

float width = 0.2f;
float radius = 0.5f;

float sdRing(in vec2 p) {
    return sdgArc(p, vec2(0.0f, 0.0f), radius, width).r;
}

int tris = 3;
vec2 genPointTris(int index) {
    float angle = 2.0f * 3.1415926f * float(index) / float(tris);
    return vec2(cos(angle), sin(angle)) * 0.5f;
}

out vec4 fragColor;
void main() {

    vec4 uv = texture(sTD2DInputs[0], vUV.st);

    float dist = sdRing(uv.xy + genPointTris(0));

    for(int i = 1; i < NUM_POINTS; i++) {
        dist = opSmoothUnion(dist, sdRing((uv.xy + genPointTris(i))), 0.01f);
    }

	// float val = 1-step(width/2, dist);

    fragColor = TDOutputSwizzle(vec4(dist));
}
