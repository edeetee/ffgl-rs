/ *
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/4sjSW1 by Dave_Hoskins.  Binary subdivision finds the surface really well with this fractal. Two light sources with shadows, and near surface glows. MOUSE X TO TIME WARP",
    "INPUTS": [
        {
            "NAME": "animate",
            "TYPE": "float"
        },
        {
            "NAME": "progress",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 1.0
        }
    ],
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
        }
    ]
}
* e

// Remnant X
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// by David Hoskins.

#define TAU  6.28318530718
#define PI 3.14159
#define CONTRAST 1.08
#define SATURATION 1.5
#define BRIGHTNESS 1.5

float n1 = 0.0f;
float n2 = 0.0f;
float n3 = 0.0f;
float n4 = 0.0f;
float fb_lp = 0.0f;
float fb_hp = 0.0f;
float hp = 0.0f;
float p4 = 1.0e-24f;

float gTime;
float beat;

// Stable hash functions to replace iChannel0 texture
float hash1D(float p) {
p = fract(p * 0.1031f);
    p *= p + 33.33f;
    p *= p + p;
    return fract(p);
}

vec2 hash2D(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031f, 0.1030f, 0.0973f));
    p3 += dot(p3, p3.yxz + 33.33f);
    return fract((p3.xx + p3.yz) * p3.zy);
}

float hash3D(vec3 p) {
    p = fract(p * vec3(0.1031f, 0.1030f, 0.0973f));
    p += dot(p, p.yxz + 33.33f);
    return fract((p.x + p.y) * p.z);
}

// Stable noise function that doesn't require textures
float stableNoise(in vec3 x) {
    vec3 p = floor(x);
    vec3 f = fract(x);
    f = f * f * (3.0f - 2.0f * f);

    float n = p.x + p.y * 157.0f + 113.0f * p.z;

    return mix(mix(mix(hash1D(n + 0.0f), hash1D(n + 1.0f), f.x), mix(hash1D(n + 157.0f), hash1D(n + 158.0f), f.x), f.y), mix(mix(hash1D(n + 113.0f), hash1D(n + 114.0f), f.x), mix(hash1D(n + 270.0f), hash1D(n + 271.0f), f.x), f.y), f.z);
}

#define N(a, b) if(t > a){x = a; n = b;}
#define K(a) if(t > a) x = a;
#define BRING_IN

#define _sample (1.0 / iSampleRate)

// Low pass resonant filter...
float Filter(float inp, float cut_lp, float res_lp) {
    fb_lp = res_lp + res_lp / (1.0f - cut_lp + 1e-20f);
    n1 = n1 + cut_lp * (inp - n1 + fb_lp * (n1 - n2)) + p4;
    n2 = n2 + cut_lp * (n1 - n2);
    return n2;
}

//----------------------------------------------------------------------------------
float Tract(float x, float f, float bandwidth) {
    float ret = sin(TAU * f * x) * exp(-bandwidth * 2.14159265359f * x);
    return ret;
}

//----------------------------------------------------------------------------------
float Hash(float p) {
    vec2 p2 = fract(vec2(p * 5.3983f, p * 5.4427f));
    p2 += dot(p2.yx, p2.xy + vec2(21.5351f, 14.3137f));
    return fract(p2.x * p2.y * 3.4337f) * .5f;
}

// Remnant X
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// by David Hoskins.
// Thanks to boxplorer and the folks at 'Fractalforums.com'
// HD Video:- https://www.youtube.com/watch?v=BjkK9fLXXo0

vec3 sunDir = normalize(vec3(0.35f, 0.1f, 0.3f));
const vec3 sunColour = vec3(1.0f, .95f, .8f);

#define SCALE 2.8
#define MINRAD2 .25
float minRad2 = clamp(MINRAD2, 1.0e-9f, 1.0f);
#define scale (vec4(SCALE, SCALE, SCALE, abs(SCALE)) / minRad2)
float absScalem1 = abs(SCALE - 1.0f);
float AbsScaleRaisedTo1mIters = pow(abs(SCALE), float(1 - 10));
vec3 surfaceColour1 = vec3(.8f, .0f, 0.f);
vec3 surfaceColour2 = vec3(.4f, .4f, 0.5f);
vec3 surfaceColour3 = vec3(.5f, 0.3f, 0.00f);
vec3 fogCol = vec3(0.4f, 0.4f, 0.4f);

//----------------------------------------------------------------------------------------
float Noise(in vec3 x) {
    vec3 p = floor(x);
    vec3 f = fract(x);
    f = f * f * (3.0f - 2.0f * f);

    return stableNoise(x);
}

//----------------------------------------------------------------------------------------
float Map(vec3 pos) {

    vec4 p = vec4(pos, 1);
    vec4 p0 = p;  // p.w is the distance estimate

    for(int i = 0; i < 9; i++) {
        p.xyz = clamp(p.xyz, -1.0f, 1.0f) * 2.0f - p.xyz;

        float r2 = dot(p.xyz, p.xyz);
        p *= clamp(max(minRad2 / r2, minRad2), 0.0f, 1.0f);

		// scale, translate
        p = p * scale + p0;
    }
    return ((length(p.xyz) - absScalem1) / p.w - AbsScaleRaisedTo1mIters);
}

//----------------------------------------------------------------------------------------
vec3 Colour(vec3 pos, float sphereR) {
    vec3 p = pos;
    vec3 p0 = p;
    float trap = 1.0f;

    for(int i = 0; i < 6; i++) {

        p.xyz = clamp(p.xyz, -1.0f, 1.0f) * 2.0f - p.xyz;
        float r2 = dot(p.xyz, p.xyz);
        p *= clamp(max(minRad2 / r2, minRad2), 0.0f, 1.0f);

        p = p * scale.xyz + p0.xyz;
        trap = min(trap, r2);
    }
	// |c.x|: log final distance (fractional iteration count)
	// |c.y|: spherical orbit trap at (0,0,0)
    vec2 c = clamp(vec2(0.3333f * log(dot(p, p)) - 1.0f, sqrt(trap)), 0.0f, 1.0f);

    float t = mod(length(pos) - gTime * 150.f, 16.0f);
    surfaceColour1 = mix(surfaceColour1, vec3(.4f, 3.0f, 5.f), pow(smoothstep(0.0f, .3f, t) * smoothstep(0.6f, .3f, t), 10.0f));
    return mix(mix(surfaceColour1, surfaceColour2, c.y), surfaceColour3, c.x);
}

//----------------------------------------------------------------------------------------
vec3 GetNormal(vec3 pos, float distance) {
distance *= 0.001f + .0001f;
vec2 eps = vec2(distance, 0.0f);
    vec3 nor = vec3(Map(pos + eps.xyy) - Map(pos - eps.xyy), Map(pos + eps.yxy) - Map(pos - eps.yxy), Map(pos + eps.yyx) - Map(pos - eps.yyx));
    return normalize(nor);
}

//----------------------------------------------------------------------------------------
float GetSky(vec3 pos) {
    pos *= 2.3f;
    float t = Noise(pos);
    t += Noise(pos * 2.1f) * .5f;
    t += Noise(pos * 4.3f) * .25f;
    t += Noise(pos * 7.9f) * .125f;
    return t;
}

//----------------------------------------------------------------------------------------
float BinarySubdivision(in vec3 rO, in vec3 rD, vec2 t) {
    float halfwayT;

    for(int i = 0; i < 6; i++) {

        halfwayT = dot(t, vec2(.5f));
        float d = Map(rO + halfwayT * rD); 
        //if (abs(d) < 0.001) break;
        t = mix(vec2(t.x, halfwayT), vec2(halfwayT, t.y), step(0.0005f, d));

    }

    return halfwayT;
}

//----------------------------------------------------------------------------------------
vec2 Scene(in vec3 rO, in vec3 rD, in vec2 fragCoord) {
    float t = .05f + 0.05f * hash1D(fragCoord.x + fragCoord.y * 113.0f);
    vec3 p = vec3(0.0f);
    float oldT = 0.0f;
    bool hit = false;
    float glow = 0.0f;
    vec2 dist;
    for(int j = 0; j < 100; j++) {
        if(t > 12.0f)
            break;
        p = rO + t * rD;

        float h = Map(p);

        if(h < 0.0005f) {
            dist = vec2(oldT, t);
            hit = true;
            break;
        }
        glow += clamp(.05f - h, 0.0f, .4f);
        oldT = t;
        t += h + t * 0.001f;
    }
    if(!hit)
        t = 1000.0f;
    else
        t = BinarySubdivision(rO, rD, dist);
    return vec2(t, clamp(glow * .25f, 0.0f, 1.0f));

}

//----------------------------------------------------------------------------------------
float Hash(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898f, 78.233f))) * 33758.5453f) - .5f;
} 

//----------------------------------------------------------------------------------------
vec3 PostEffects(vec3 rgb, vec2 xy) {
	// Gamma first...

	// Then...
    rgb = mix(vec3(.5f), mix(vec3(dot(vec3(.2125f, .7154f, .0721f), rgb * BRIGHTNESS)), rgb * BRIGHTNESS, SATURATION), CONTRAST);
	// Noise...
	//rgb = clamp(rgb+Hash(xy*TIME)*.1, 0.0, 1.0);
	// Vignette...
    rgb *= .5f + 0.5f * pow(20.0f * xy.x * xy.y * (1.0f - xy.x) * (1.0f - xy.y), 0.2f);

    rgb = pow(rgb, vec3(0.47f));
    return rgb;
}

//----------------------------------------------------------------------------------------
float Shadow(in vec3 ro, in vec3 rd) {
    float res = 1.0f;
    float t = 0.05f;
    float h;

    for(int i = 0; i < 8; i++) {
        h = Map(ro + rd * t);
        res = min(6.0f * h / t, res);
        t += h;
    }
    return max(res, 0.0f);
}

//----------------------------------------------------------------------------------------
mat3 RotationMatrix(vec3 axis, float angle) {
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0f - c;

    return mat3(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
}

//----------------------------------------------------------------------------------------
vec3 LightSource(vec3 spotLight, vec3 dir, float dis) {
    float g = 0.0f;
    if(length(spotLight) < dis) {
        float a = max(dot(normalize(spotLight), dir), 0.0f);
        g = pow(a, 500.0f);
        g += pow(a, 5000.0f) * .2f;
    }

    return vec3(.6f) * g;
}

//----------------------------------------------------------------------------------------
vec3 CameraPath(float t) {
    vec3 p = vec3(-.78f + 3.f * sin(TAU * 8 * t), .05f + 2.5f * sin(TAU * t + 1.3f), .05f + 3.5f * cos(TAU * t));
    return p;
} 

//----------------------------------------------------------------------------------------
void main() {
    if(PASSINDEX == 0) {
    } else if(PASSINDEX == 1) {

        float aTime = progress + animate + TIME * 0.0001f;
        gTime = TIME * 0.01f;
        vec2 xy = gl_FragCoord.xy / RENDERSIZE.xy;
        vec2 uv = (-1.0f + 2.0f * xy) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0f);

        vec3 cameraPos = CameraPath(aTime);
        vec3 camTar = CameraPath(aTime + .01f);

        float roll = 13.0f * sin(aTime * TAU + .4f);
        vec3 cw = normalize(camTar - cameraPos);

        vec3 cp = vec3(sin(roll), cos(roll), 0.0f);
        vec3 cu = normalize(cross(cw, cp));

        vec3 cv = normalize(cross(cu, cw));
        cw = RotationMatrix(cv, sin(-aTime * TAU * 10) * .7f) * cw;
        vec3 dir = normalize(uv.x * cu + uv.y * cv + 1.3f * cw);

        vec3 spotLight = CameraPath(gTime + .03f) + vec3(sin(gTime * 18.4f), cos(gTime * 17.98f), sin(gTime * 22.53f)) * .2f;
        vec3 col = vec3(0.0f);
        vec3 sky = vec3(0.03f, .04f, .05f) * GetSky(dir);
        vec2 ret = Scene(cameraPos, dir, gl_FragCoord.xy);

        if(ret.x < 900.0f) {
            vec3 p = cameraPos + ret.x * dir;
            vec3 nor = GetNormal(p, ret.x);

            vec3 spot = spotLight - p;
            float atten = length(spot);

            spot /= atten;

            float shaSpot = Shadow(p, spot);
            float shaSun = Shadow(p, sunDir);

            float bri = max(dot(spot, nor), 0.0f) / pow(atten, 1.5f) * .25f;
            float briSun = max(dot(sunDir, nor), 0.0f) * .2f;

            col = Colour(p, ret.x);
            col = (col * bri * shaSpot) + (col * briSun * shaSun);

            vec3 ref = reflect(dir, nor);
            col += pow(max(dot(spot, ref), 0.0f), 10.0f) * 2.0f * shaSpot * bri;
            col += pow(max(dot(sunDir, ref), 0.0f), 10.0f) * 2.0f * shaSun * briSun;
        }

        col = mix(sky, col, min(exp(-ret.x + 1.5f), 1.0f));
        col += vec3(pow(abs(ret.y), 2.f)) * vec3(.02f, .04f, .1f);

        col += LightSource(spotLight - cameraPos, dir, ret.x);
        col = PostEffects(col, xy);

        gl_FragColor = vec4(col, 1.0f);
    }

}

//--------------------------------------------------------------------------
