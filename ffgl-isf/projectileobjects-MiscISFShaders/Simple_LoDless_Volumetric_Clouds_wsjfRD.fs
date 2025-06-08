/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/wsjfRD by 42yeah.  Dissecting iq's clouds - https://www.shadertoy.com/view/XslGRr and here's what I get by far. No fancy lods or textures. There aren't even lighting! But I am kinda satisfied with this, and will post a lighted up one next morning :P",
    "IMPORTED": {
    },
    "INPUTS": [
    ]
}

*/


// Created by 42yeah - 42yeah/2020
// Ripped off from inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

// Simple LoDless Volumetric clouds. It does NOT perform level of detail (LOD) for faster rendering

// A white noise function.
float rand(vec3 p) {
    return fract(sin(dot(p, vec3(12.345, 67.89, 412.12))) * 42123.45) * 2.0 - 1.0;
}

// A perlin noise function. Since we are not using textures, we am gonna sample 8 corners of a cube.
float perlin(vec3 p) {
    vec3 u = floor(p);
    vec3 v = fract(p);
    vec3 s = smoothstep(0.0, 1.0, v);
    
    float a = rand(u);
    float b = rand(u + vec3(1.0, 0.0, 0.0));
    float c = rand(u + vec3(0.0, 1.0, 0.0));
    float d = rand(u + vec3(1.0, 1.0, 0.0));
    float e = rand(u + vec3(0.0, 0.0, 1.0));
    float f = rand(u + vec3(1.0, 0.0, 1.0));
    float g = rand(u + vec3(0.0, 1.0, 1.0));
    float h = rand(u + vec3(1.0, 1.0, 1.0));
    
    return mix(mix(mix(a, b, s.x), mix(c, d, s.x), s.y),
               mix(mix(e, f, s.x), mix(g, h, s.x), s.y),
               s.z);
}

// The fbm function. iq unrolled the loop, so I am doing it too.
// If you wonder what fbm is, check this out: https://thebookofshaders.com/13/
float fbm(vec3 p) {
    vec3 off = vec3(0.0, 0.1, 1.0) * TIME;
    vec3 q = p - off;
    
    // fbm
    float f = 0.5 * perlin(q); q *= 2.0;
    f += 0.25 * perlin(q); q *= 2.0;
    f += 0.125 * perlin(q); q *= 2.0;
    f += 0.06250 * perlin(q); q *= 2.0;
    f += 0.03125 * perlin(q);
    return clamp(f - p.y, 0.0, 1.0);
}

// volmetric raymarching, which is kinda like the core algorithm.
// I ripped lighting calculations and other stuffs off, so this is bare bones raymarching
vec3 volumetricTrace(vec3 ro, vec3 rd) {
    // at first there's no depth
    float depth = 0.0;
    
    // and the color's black
    vec4 sumColor = vec4(0.0);
    
    // then we begin to march
    for (int i = 0; i < 100; i++) {
        vec3 p = ro + depth * rd;
        
        // and we get the cloud density at said position
        float density = fbm(p);
        // if there is an unignorable amount of density (the cloud is thick enough) then
        if (density > 1e-3) {
            // we estimate the color with density (the thicker, the whiter)
            vec4 color = vec4(mix(vec3(0.0), vec3(1.0), density), density);
            // and we multiply it by a factor so it makes the clouds softer
            color.w *= 0.4;
            color.rgb *= color.w;
            // sumColor.w will rise steadily, which stands for when the ray hits thick enough cloud,
            // its color won't change anymore
            sumColor += color * (1.0 - sumColor.a);
        }
        // we march forward
        depth += max(0.05, 0.02 * depth);
    }
    return clamp(sumColor.rgb, 0.0, 1.0);
}

void main() {

    // standard raymarching routine
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy * 2.0 - 1.0;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    uv.x *= aspect;
    vec3 ro = vec3(0.0, 1.0, 5.0);
    vec3 center = vec3(0.0);
    
    vec3 front = normalize(center - ro);
    vec3 right = normalize(cross(front, vec3(0.0, 1.0, 0.0)));
    vec3 up = normalize(cross(right, front));
    
    mat4 lookAt = mat4(
        vec4(right, 0.0),
        vec4(up, 0.0),
        vec4(front, 0.0),
        vec4(0.0, 0.0, 0.0, 1.0)
    );
    vec3 rd = normalize(vec3(lookAt * vec4(uv, 2.0, 1.0)));
    vec3 objColor = volumetricTrace(ro, rd);
    
    // gamma correction (yeah, that's definitely not needed here)
    objColor = pow(objColor, vec3(0.4545));
    gl_FragColor = vec4(objColor, 1.0);
}
