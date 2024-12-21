/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/Msl3WH by Dave_Hoskins.  I couldn't resist playing around with the \"Cosmos\" shadertoy!\nThanks for the fractal, Kali!\nVideo of it:-\nhttp://www.youtube.com/watch?v=1eZeqKvI5_4\n",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iChannel0",
            "TYPE": "audio"
        }
    ]
}

*/

// 'Warp Speed' by David Hoskins 2013.
// I tried to find gaps and variation in the star cloud for a feeling of structure.
// Inspired by Kali: https://www.shadertoy.com/view/ltl3WS

void main() {

    float time = (TIME + 29.f) * 60.0f;
    float s = 0.0f, v = 0.0f;
    vec2 uv = (-RENDERSIZE.xy + 2.0f * gl_FragCoord.xy) / RENDERSIZE.y;
    float t = time * 0.005f;
    uv.x += sin(t) * .3f;
    float si = sin(t * 1.5f); // ...Squiffy rotation matrix!
    float co = cos(t);
    uv *= mat2(co, si, -si, co);
    vec3 col = vec3(0.0f);
    vec3 init = vec3(0.25f, 0.25f + sin(time * 0.001f) * .1f, time * 0.0008f);
    for(int r = 0; r < 100; r++) {
        vec3 p = init + s * vec3(uv, 0.143f);
        p.z = mod(p.z, 2.0f);
        for(int i = 0; i < 10; i++) p = abs(p * 2.04f) / dot(p, p) - 0.75f;
        v += length(p * p) * smoothstep(0.0f, 0.5f, 0.9f - s) * .002f;
		// Get a purple and cyan effect by biasing the RGB in different ways...
        col += vec3(v * 0.8f, 1.1f - s * 0.5f, .7f + v * 0.5f) * v * 0.013f;
        s += .01f;
    }
    gl_FragColor = vec4(col, 1.0f);
}
