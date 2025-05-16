/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/XcjBWW by hasse.  Tried to re-create a pretty artifact from a temporal reprojection algorithm.",
    "IMPORTED": {
    },
    "INPUTS": [
    ],
    "PASSES": [
        {
            "FLOAT": false,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
        }
    ]
}

*/


#define INV_UINT32_MAX 2.3283064365386963e-10f

uvec4 pcg4d(inout uvec4 seed)
{
    seed = seed * 1664525u + 1013904223u;
    seed += seed.yzxy * seed.wxyz;
    seed = (seed >> 16) ^ seed;
    seed += seed.yzxy * seed.wxyz;
    return seed;
}

void main() {
	if (PASSINDEX == 0)	{
	    uvec4 seed = uvec4(gl_FragCoord.x, gl_FragCoord.y, 0, 20)+uint(FRAMEINDEX);
	    vec4 random = vec4(pcg4d(seed))*INV_UINT32_MAX;
	
	    vec4 u = vec4(pcg4d(seed))*INV_UINT32_MAX;
	    vec2 uv = (gl_FragCoord.xy + u.xy*1.5f - 0.5f)/ RENDERSIZE.xy;
	    uv = uv * 2.0f - 1.0f;
	    uv *= 0.99f;
	    uv = uv * 0.5f + 0.5f;
	    vec4 prev = IMG_NORM_PIXEL(BufferA,mod(uv,1.0));
	    
	    if(random.w > 0.001f && dot(prev.xyz, prev.xyz) != 0.0)
	        random.xyz = prev.xyz;
	        
	    gl_FragColor = vec4(random.xyz,1.0);
	}
	else if (PASSINDEX == 1)	{
	    gl_FragColor = vec4(texelFetch(BufferA, ivec2(gl_FragCoord.xy), 0).xyz,1.0);
	}

}
