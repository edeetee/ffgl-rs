/*{
	"DESCRIPTION": "Polychora - A mesmerizing 4D geometry visualization",
	"CREDIT": "Original DE from Knighty's Fragmentarium frag, PostFX from jmpep",
	"CATEGORIES": ["Geometry", "Fractal"],
	"INPUTS": [
		{
			"NAME": "progress",
			"TYPE": "float",
			"DEFAULT": 0.0,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "rotSpeed",
			"TYPE": "float",
			"DEFAULT": 0.3,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "cameraDistance",
			"TYPE": "float",
			"DEFAULT": 12.0,
			"MIN": 8.0,
			"MAX": 20.0
		},
		{
			"NAME": "noise",
			"TYPE": "bool",
			"DEFAULT": true
		},
		{
			"NAME": "vertexRadius",
			"TYPE": "float",
			"DEFAULT": 0.05048,
			"MIN": 0.01,
			"MAX": 0.1
		},
		{
			"NAME": "segmentRadius",
			"TYPE": "float",
			"DEFAULT": 0.05476,
			"MIN": 0.01,
			"MAX": 0.1
		}
	]
}*/

// Original DE from Knighty's Fragmentarium frag.
// PostFX from jmpep.
// Whatever is left (not much) by Syntopia.

#define MaxSteps 30
#define MinimumDistance 0.0009
#define normalDistance     0.0002

#define PI 3.141592
#define Scale 3.0
#define FieldOfView 0.5
#define Jitter 0.06
#define FudgeFactor 1.0

#define Ambient 0.32184
#define Diffuse 0.5
#define LightDir vec3(1.0)
#define LightColor vec3(0.6,1.0,0.158824)
#define LightDir2 vec3(1.0,-1.0,1.0)
#define LightColor2 vec3(1.0,0.933333,1.0)
#define Offset vec3(0.92858,0.92858,0.32858)

#define BLACK_AND_WHITE 0
#define LINES_AND_FLICKER 0
#define BLOTCHES 0
#define GRAIN 0

// Return rotation matrix for rotating around vector v by angle
mat3 rotationMatrix3(vec3 v, float angle) {
    float c = cos(radians(angle));
    float s = sin(radians(angle));

    return mat3(c + (1.0f - c) * v.x * v.x, (1.0f - c) * v.x * v.y - s * v.z, (1.0f - c) * v.x * v.z + s * v.y, (1.0f - c) * v.x * v.y + s * v.z, c + (1.0f - c) * v.y * v.y, (1.0f - c) * v.y * v.z - s * v.x, (1.0f - c) * v.x * v.z - s * v.y, (1.0f - c) * v.y * v.z + s * v.x, c + (1.0f - c) * v.z * v.z);
}

vec2 rotate(vec2 v, float a) {
    return vec2(cos(a) * v.x + sin(a) * v.y, -sin(a) * v.x + cos(a) * v.y);
}

#define Type 5
float U;
float V;
float W;
float T = 1.0f;

float VRadius;
float SRadius;
vec3 RotVector = vec3(0.0f, 1.0f, 1.0f);
float RotAngle = 0.0f;

mat3 rot;
vec4 nc, nd, p;
void init() {
    U = 0.0f * cos(TIME) * 0.5f + 0.1f;
    V = 0.2f * sin(TIME * 0.1f) * 0.5f + 0.2f;
    W = 1.0f * cos(TIME * 1.2f) * 0.5f + 0.5f;

    VRadius = vertexRadius;
    SRadius = segmentRadius;

    float cospin = cos(PI / float(Type)), isinpin = 1.f / sin(PI / float(Type));
    float scospin = sqrt(2.f / 3.f - cospin * cospin), issinpin = 1.f / sqrt(3.f - 4.f * cospin * cospin);

    nc = 0.5f * vec4(0, -1, sqrt(3.f), 0.f);
    nd = vec4(-cospin, -0.5f, -0.5f / sqrt(3.f), scospin);

    vec4 pabc, pbdc, pcda, pdba;
    pabc = vec4(0.f, 0.f, 0.f, 1.f);
    pbdc = 0.5f * sqrt(3.f) * vec4(scospin, 0.f, 0.f, cospin);
    pcda = isinpin * vec4(0.f, 0.5f * sqrt(3.f) * scospin, 0.5f * scospin, 1.f / sqrt(3.f));
    pdba = issinpin * vec4(0.f, 0.f, 2.f * scospin, 1.f / sqrt(3.f));

    p = normalize(U * pabc + V * pbdc + W * pcda + T * pdba);

    rot = rotationMatrix3(normalize(RotVector), RotAngle);//in reality we need a 4D rotation
}

vec4 fold(vec4 pos) {
    for(int i = 0; i < Type * (Type - 2); i++) {
        pos.xy = abs(pos.xy);
        float t = -2.f * min(0.f, dot(pos, nc));
        pos += t * nc;
        t = -2.f * min(0.f, dot(pos, nd));
        pos += t * nd;
    }
    return pos;
}

float DD(float ca, float sa, float r) {
	//magic formula to convert from spherical distance to planar distance.
	//involves transforming from 3-plane to 3-sphere, getting the distance
	//on the sphere then going back to the 3-plane.
    return r - (2.f * r * ca - (1.f - r * r) * sa) / ((1.f - r * r) * ca + 2.f * r * sa + 1.f + r * r);
}

float dist2Vertex(vec4 z, float r) {
    float ca = dot(z, p), sa = 0.5f * length(p - z) * length(p + z);//sqrt(1.-ca*ca);//
    return DD(ca, sa, r) - VRadius;
}

float dist2Segment(vec4 z, vec4 n, float r) {
	//pmin is the orthogonal projection of z onto the plane defined by p and n
	//then pmin is projected onto the unit sphere
    float zn = dot(z, n), zp = dot(z, p), np = dot(n, p);
    float alpha = zp - zn * np, beta = zn - zp * np;
    vec4 pmin = normalize(alpha * p + min(0.f, beta) * n);
	//ca and sa are the cosine and sine of the angle between z and pmin. This is the spherical distance.
    float ca = dot(z, pmin), sa = 0.5f * length(pmin - z) * length(pmin + z);//sqrt(1.-ca*ca);//
    return DD(ca, sa, r) - SRadius;
}
//it is possible to compute the distance to a face just as for segments: pmin will be the orthogonal projection
// of z onto the 3-plane defined by p and two n's (na and nb, na and nc, na and and, nb and nd... and so on).
//that involves solving a system of 3 linear equations.
//it's not implemented here because it is better with transparency

float dist2Segments(vec4 z, float r) {
    float da = dist2Segment(z, vec4(1.f, 0.f, 0.f, 0.f), r);
    float db = dist2Segment(z, vec4(0.f, 1.f, 0.f, 0.f), r);
    float dc = dist2Segment(z, nc, r);
    float dd = dist2Segment(z, nd, r);

    return min(min(da, db), min(dc, dd));
}

float DE(vec3 pos) {
    float r = length(pos);
    vec4 z4 = vec4(2.f * pos, 1.f - r * r) * 1.f / (1.f + r * r);//Inverse stereographic projection of pos: z4 lies onto the unit 3-sphere centered at 0.
    z4.xyw = rot * z4.xyw;
    z4 = fold(z4);//fold it

    return min(dist2Vertex(z4, r), dist2Segments(z4, r));
}

vec3 lightDir;
vec3 lightDir2;

// Two light sources. No specular 
vec3 getLight(in vec3 color, in vec3 normal, in vec3 dir) {
    float diffuse = max(0.0f, dot(-normal, lightDir)); // Lambertian

    float diffuse2 = max(0.0f, dot(-normal, lightDir2)); // Lambertian

    return (diffuse * Diffuse) * (LightColor * color) +
        (diffuse2 * Diffuse) * (LightColor2 * color);
}

// Finite difference normal
vec3 getNormal(in vec3 pos) {
    vec3 e = vec3(0.0f, normalDistance, 0.0f);

    return normalize(vec3(DE(pos + e.yxx) - DE(pos - e.yxx), DE(pos + e.xyx) - DE(pos - e.xyx), DE(pos + e.xxy) - DE(pos - e.xxy)));
}

// Solid color 
vec3 getColor(vec3 normal, vec3 pos) {
    return vec3(1.0f, 1.0f, 1.0f);
}

// Pseudo-random number
// From: lumina.sourceforge.net/Tutorials/Noise.html
float rand(vec2 co) {
    return fract(cos(dot(co, vec2(4.898f, 7.23f))) * 23421.631f);
}

float rand1(float co) {
    return fract(cos(94.898f * co) * 23421.631f);
}

vec4 rayMarch(in vec3 from, in vec3 dir, in vec2 fragCoord) {
	// Add some noise to prevent banding
    float totalDistance = Jitter * rand(fragCoord.xy + vec2(TIME));
    vec3 dir2 = dir;
    float distance;
    int steps = 0;
    vec3 pos;
    for(int i = 0; i <= MaxSteps; i++) {
        pos = from + totalDistance * dir;
        distance = DE(pos) * FudgeFactor;
        totalDistance += distance;
        if(distance < MinimumDistance)
            break;
        steps = i;
    }

	// 'AO' is based on number of steps.
	// Try to smooth the count, to combat banding.
    float smoothStep = float(steps);
    float ao = 1.0f - smoothStep / float(MaxSteps);

	// Since our distance field is not signed,
	// backstep when calc'ing normal
    vec3 normal = getNormal(pos - dir * normalDistance * 3.0f);
    vec3 bg = vec3(0.2f);
    if(steps == MaxSteps) {
        return vec4(bg, 1.0f);
    }
    vec3 color = getColor(normal, pos);
    vec3 light = getLight(color, normal, dir);

    color = mix(color * Ambient + light, bg, 1.0f - ao);
    return vec4(color, 1.0f);
}

#define FREQUENCY 10.0

vec2 uv;
float rnd(float c) {
    return rand(vec2(c, 1.0f));
}

float randomLine(float seed) {
    float b = 0.01f * rand1(seed);
    float a = rand1(seed + 1.0f);
    float c = rand1(seed + 2.0f) - 0.5f;
    float mu = rand1(seed + 3.0f);

    float l = 1.0f;

    if(mu > 0.2f)
        l = pow(abs(a * uv.x + b * uv.y + c), 1.0f / 8.0f);
    else
        l = 2.0f - pow(abs(a * uv.x + b * uv.y + c), 1.0f / 8.0f);

    return mix(0.5f, 1.0f, l);
}

// Generate some blotches.
float randomBlotch(float seed) {
    float x = rand1(seed);
    float y = rand1(seed + 1.0f);
    float s = 0.01f * rand1(seed + 2.0f);

    vec2 p = vec2(x, y) - uv;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;
    float a = atan(p.y, p.x);
    float v = 1.0f;
    float ss = s * s * (sin(6.2831f * a * x) * 0.1f + 1.0f);

    if(dot(p, p) < ss)
        v = 0.2f;
    else
        v = pow(dot(p, p) - ss, 1.0f / 16.0f);

    return mix(0.3f + 0.2f * (1.0f - (s / 0.02f)), 1.0f, v);
}

vec3 degrade(vec3 image) {
    // Only apply effects if noise is enabled
    if(!noise)
        return image;

    // Set frequency of global effect to 20 variations per second
    float t = float(int(TIME * FREQUENCY));

    // Get some image movement
    vec2 suv = uv + 0.002f * vec2(rand1(t), rand1(t + 23.0f));

    // #if BLACK_AND_WHITE = 1
    // // Pass it to B/W
    // float luma = dot(vec3(0.2126f, 0.7152f, 0.0722f), image);
    // vec3 oldImage = luma * vec3(0.7f, 0.7f, 0.7f);
    // #else
    vec3 oldImage = image;
    // #endif

    // Create a time-varyting vignetting effect
    float vI = 16.0f * (uv.x * (1.0f - uv.x) * uv.y * (1.0f - uv.y));
    vI *= mix(0.7f, 1.0f, rand1(t + 0.5f));

    // Add additive flicker
    vI += 1.0f + 0.4f * rand1(t + 8.f);

    // Add a fixed vignetting (independent of the flicker)
    vI *= pow(16.0f * uv.x * (1.0f - uv.x) * uv.y * (1.0f - uv.y), 0.4f);

    // Add some random lines (and some multiplicative flicker. Oh well.)
    // #if LINES_AND_FLICKER = 1
    // int l = int(8.0f * rand(t + 7.0f));

    // if(0 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(0));
    // if(1 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(1));
    // if(2 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(2));
    // if(3 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(3));
    // if(4 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(4));
    // if(5 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(5));
    // if(6 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(6));
    // if(7 < l)
    //     vI *= randomLine(t + 6.0f + 17.f * float(7));

    // #endif

    // Add some random blotches.
    // #if BLOTCHES = 1
    // int s = int(max(8.0f * rand(t + 18.0f) - 2.0f, 0.0f));

    // if(0 < s)
    //     vI *= randomBlotch(t + 6.0f + 19.f * float(0));
    // if(1 < s)
    //     vI *= randomBlotch(t + 6.0f + 19.f * float(1));
    // if(2 < s)
    //     vI *= randomBlotch(t + 6.0f + 19.f * float(2));
    // if(3 < s)
    //     vI *= randomBlotch(t + 6.0f + 19.f * float(3));
    // if(4 < s)
    //     vI *= randomBlotch(t + 6.0f + 19.f * float(4));
    // if(5 < s)
    //     vI *= randomBlotch(t + 6.0f + 19.f * float(5));

    // #endif

    vec3 outv = oldImage * vI;

    // Add some grain (thanks, Jose!)
    // #if GRAIN = 1
    outv *= (1.0f + (rand(uv + t * .01f) - .2f) * .15f);        
    // #endif
    return outv;
}

void main() {
    uv = gl_FragCoord.xy / RENDERSIZE.xy;

    init();

    // Camera position //(eye), and camera target
    vec3 camPos = (cameraDistance + 2.0f * sin(TIME * 0.6f)) * vec3(cos(TIME * rotSpeed), 0.0f, sin(TIME * rotSpeed));
    vec3 target = vec3(0.0f, 0.0f, 0.0f);
    vec3 camUp = vec3(0.0f, 1.0f, 0.0f);

    // Calculate orthonormal camera reference system
    vec3 camDir = normalize(target - camPos); // direction for center ray
    camUp = normalize(camUp - dot(camDir, camUp) * camDir); // orthogonalize
    vec3 camRight = normalize(cross(camDir, camUp));

    lightDir = -normalize(camPos + 7.5f * camUp);
    lightDir2 = -normalize(camPos - 6.5f * camRight);

    vec2 coord = -1.0f + 2.0f * gl_FragCoord.xy / RENDERSIZE.xy;
    float vignette = 0.4f + (1.0f - coord.x * coord.x) * (1.0f - coord.y * coord.y);
    coord.x *= RENDERSIZE.x / RENDERSIZE.y;

    // Get direction for this pixel
    vec3 rayDir = normalize(camDir + (coord.x * camRight + coord.y * camUp) * FieldOfView);

    vec3 col = rayMarch(camPos, rayDir, gl_FragCoord.xy).xyz;   

    // vignetting 
    // col *= clamp(vignette,0.0,1.0);
    col = degrade(col);
    gl_FragColor = vec4(col, 1.0f);
}
