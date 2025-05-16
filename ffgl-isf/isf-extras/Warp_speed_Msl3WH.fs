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



    float time = (TIME+29.) * 60.0;
    float s = 0.0, v = 0.0;
    vec2 uv = (-RENDERSIZE.xy + 2.0 * gl_FragCoord.xy ) / RENDERSIZE.y;
	float t = time*0.005;
	uv.x += sin(t) * .3;
	float si = sin(t*1.5); // ...Squiffy rotation matrix!
	float co = cos(t);
	uv *= mat2(co, si, -si, co);
	vec3 col = vec3(0.0);
	vec3 init = vec3(0.25, 0.25 + sin(time * 0.001) * .1, time * 0.0008);
	for (int r = 0; r < 100; r++) 
	{
		vec3 p = init + s * vec3(uv, 0.143);
		p.z = mod(p.z, 2.0);
		for (int i=0; i < 10; i++)	p = abs(p * 2.04) / dot(p, p) - 0.75;
		v += length(p * p) * smoothstep(0.0, 0.5, 0.9 - s) * .002;
		// Get a purple and cyan effect by biasing the RGB in different ways...
		col +=  vec3(v * 0.8, 1.1 - s * 0.5, .7 + v * 0.5) * v * 0.013;
		s += .01;
	}
	gl_FragColor = vec4(col, 1.0);
}
