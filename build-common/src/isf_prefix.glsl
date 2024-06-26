uniform int PASSINDEX = 0;
uniform vec2 res;
uniform int FRAMEINDEX = 0;
uniform float FRAMEDELTA = 0.0f;
uniform float TIME = 0.0f;

vec2 RENDERSIZE = res;
vec2 isf_FragCoord = gl_FragCoord.xy;
vec2 isf_FragNormCoord = isf_FragCoord.xy / RENDERSIZE;

///This one needs to be calculated by the preprocessor
// #define IMG_PIXEL(sampler,coord) texture(sampler,coord/textureSize(sampler, 0))

#define IMG_THIS_PIXEL(sampler) IMG_THIS_NORM_PIXEL(sampler)
#define IMG_THIS_NORM_PIXEL(sampler) IMG_NORM_PIXEL(sampler,isf_FragNormCoord)
