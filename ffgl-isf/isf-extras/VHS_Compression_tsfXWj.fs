/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/tsfXWj by mpalko.  Simulates the resolution and format of VHS tapes (with resolution presets for both NTSC and PAL) without any noise/distortion effects.\n\nUse the mouse to swipe back and forth with the effect enabled/disabled.\n\nPreview video can be subbed out in Buffer A.",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iMouse",
            "TYPE": "point2D"
        },
        {
            "NAME": "inputImage",
            "TYPE": "image"
        }
    ],
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferB"
        },
        {
        }
    ]
}

*/


float iFrameRate = 60.;
float frameRatio = RENDERSIZE.x/RENDERSIZE.y;
#define lerp mix

#define NTSC 0
#define PAL 1

// Effect params
#define VIDEO_STANDARD PAL

#if VIDEO_STANDARD == NTSC
    const vec2 maxResLuminance = vec2(333.0, 480.0);
    const vec2 maxResChroma = vec2(40.0, 480.0);
#elif VIDEO_STANDARD == PAL
    const vec2 maxResLuminance = vec2(335.0, 576.0);
    const vec2 maxResChroma = vec2(40.0, 240.0);
#endif

const vec2 blurAmount = vec2(0.2, 0.2);

// End effect params

#define VIDEO_TEXTURE inputImage



mat3 rgb2yiq = mat3(0.299, 0.596, 0.211,
                        0.587, -0.274, -0.523,
                        0.114, -0.322, 0.312);

mat3 yiq2rgb = mat3(1, 1, 1,
                        0.956, -0.272, -1.106,
                        0.621, -0.647, 1.703);

// from http://www.java-gaming.org/index.php?topic=35123.0
vec4 cubic(float v)
{
    vec4 n = vec4(1.0, 2.0, 3.0, 4.0) - v;
    vec4 s = n * n * n;
    float x = s.x;
    float y = s.y - 4.0 * s.x;
    float z = s.z - 4.0 * s.y + 6.0 * s.x;
    float w = 6.0 - x - y - z;
    return vec4(x, y, z, w) * (1.0/6.0);
}

vec4 textureBicubic(sampler2D sampler, vec2 texCoords)
{

    vec2 texSize = vec2(textureSize(sampler, 0));
    vec2 invTexSize = vec2(1.0) / texSize;

    texCoords = texCoords * texSize - 0.5;


    vec2 fxy = fract(texCoords);
    texCoords -= fxy;

    vec4 xcubic = cubic(fxy.x);
    vec4 ycubic = cubic(fxy.y);

    vec4 c = texCoords.xxyy + vec2 (-0.5, +1.5).xyxy;

    vec4 s = vec4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
    vec4 offset = c + vec4 (xcubic.yw, ycubic.yw) / s;

    offset *= invTexSize.xxyy;

    vec4 sample0 = texture(sampler, offset.xz);
    vec4 sample1 = texture(sampler, offset.yz);
    vec4 sample2 = texture(sampler, offset.xw);
    vec4 sample3 = texture(sampler, offset.yw);

    float sx = s.x / (s.x + s.y);
    float sy = s.z / (s.z + s.w);

    return mix(
       mix(sample3, sample2, sx), mix(sample1, sample0, sx)
    , sy);
}

// Copies input video to buffer, for a common input source in subsequent shaders
// Downsample buffer A and convert to YIQ color space


vec3 downsampleVideo(vec2 uv, vec2 pixelSize, ivec2 samples)
{
    //return texture(VIDEO_TEXTURE, uv).rgb * rgb2yiq;
    
    vec2 uvStart = uv - pixelSize / 2.0;
    vec2 uvEnd = uv + pixelSize;
    
    vec3 result = vec3(0.0, 0.0, 0.0);
    for (int i_u = 0; i_u < samples.x; i_u++)
    {
        float u = lerp(uvStart.x, uvEnd.x, float(i_u) / float(samples.x));
        
        for (int i_v = 0; i_v < samples.y; i_v++)
        {
            float v = lerp(uvStart.y, uvEnd.y, float(i_v) / float(samples.y));
            
            result += texture(VIDEO_TEXTURE, vec2(u, v)).rgb;
        }
    }    
    
    return (result / float(samples.x * samples.y)) * rgb2yiq;
}

vec3 downsampleVideo(vec2 fragCoord, vec2 downsampledRes)
{
   
    if (fragCoord.x > downsampledRes.x || fragCoord.y > downsampledRes.y)
    {
        return vec3(0.0);
    }
    
    vec2 uv = fragCoord / downsampledRes;
    vec2 pixelSize = 1.0 / downsampledRes;
    ivec2 samples = ivec2(8, 3);
    
    pixelSize *= 1.0 + blurAmount; // Slight box blur to avoid aliasing
    
    return downsampleVideo(uv, pixelSize, samples);
}



vec2 rotate(vec2 v, float a)
{
	float s = sin(a);
	float c = cos(a);
	mat2 m = mat2(c, -s, s, c);
	return m * v;
}

void main() {
	if (PASSINDEX == 0)	{


	    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	    gl_FragColor = IMG_NORM_PIXEL(inputImage,mod(uv,1.0));
	}
	else if (PASSINDEX == 1)	{


	    vec2 resLuminance = min(maxResLuminance, vec2(RENDERSIZE));
	    vec2 resChroma = min(maxResChroma, vec2(RENDERSIZE));    
	    
	    float luminance = downsampleVideo(gl_FragCoord.xy, resLuminance).r;
	    vec2 chroma = downsampleVideo(gl_FragCoord.xy, resChroma).gb;
	    
	    gl_FragColor = vec4(luminance, chroma, 1);
	}
	else if (PASSINDEX == 2)	{


	    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	    vec2 mouseNormalized = iMouse.xy / RENDERSIZE.xy;
	    
	    vec2 resLuminance = min(maxResLuminance, vec2(RENDERSIZE));
	    vec2 resChroma = min(maxResChroma, vec2(RENDERSIZE));
	    
	    vec2 uvLuminance = uv * (resLuminance / vec2(RENDERSIZE));
	    vec2 uvChroma = uv * (resChroma / vec2(RENDERSIZE));
	    
	    vec3 result;
	    
	    if (uv.x > mouseNormalized.x)
	    {
	        float luminance = textureBicubic(BufferB, uvLuminance).x;
	        vec2 chroma = textureBicubic(BufferB, uvChroma).yz;
	        result = vec3(luminance, chroma) * yiq2rgb;
	    }
	    else
	    {
	        result = IMG_NORM_PIXEL(BufferA,mod(uv,1.0)).rgb;
	    }
	    
	    gl_FragColor = vec4(result, 1);
	}

}
