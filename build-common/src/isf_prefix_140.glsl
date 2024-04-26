#version 140

out vec4 fragColor;

#define gl_FragColor fragColor
#define isf_FragColor fragColor
#define IMG_NORM_PIXEL(sampler, coord) texture(sampler, coord)