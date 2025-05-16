/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/csyXzd by graygoose.  A sad field system.\n\nTODO: \n-Everything\n-Obstacles.\n-FIX laplacian",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iMouse",
            "TYPE": "point2D"
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
// shadertoy common
#define R RENDERSIZE
#define UV (U/R.xy)
#define A IMG_NORM_PIXEL(BufferA,mod(UV,1.0))
#define B IMG_NORM_PIXEL(BufferB,mod(UV,1.0))
#define C texture(, UV)
#define D texture(, UV)

// math common
vec2 wrap(in vec2 p, in vec2 res) {
    vec2 wrapped = mod(p, res);
    //vec2 stepWrap = step(wrapped, vec2(0.0));
    //wrapped += stepWrap * res;
    
    // Smooth interpolation
    //vec2 smoothed = mix(wrapped, p, smoothstep(0.0, 1.0, stepWrap));
    return wrapped;
}

vec4 lap(in sampler2D tex, in vec2 p, in vec2 res) {
    vec2 dt = 1. / res.xy;
    vec4 sum = -4.0 * texture(tex, p);
    sum += texture(tex, p + vec2(dt.x, 0.0));
    sum += texture(tex, p - vec2(dt.x, 0.0));
    sum += texture(tex, p + vec2(0.0, dt.y));
    sum += texture(tex, p - vec2(0.0, dt.y));
    return sum;
}


#define K 10.

void evolveWave(inout vec4 wave, in vec2 U) {
    vec4 L = lap(BufferA, UV, R.xy) - wave;
    wave -= K * vec4((wave.z + L.x),
                     (wave.x + L.y),
                     (wave.y + L.z),
                     (wave.w + L.w));
}

void addDynamics(inout vec4 wave, in vec2 U) {
    vec2 center = R.xy * 0.5;
    float distFromCenter = length(U - center) / length(center);
    wave *= 1.0 + 0.2 * sin(distFromCenter * 3.14159);

    float shiftFactor = 0.5;
    wave.x += shiftFactor * sin(wave.y * 2.0 * 3.14159);
    wave.y += shiftFactor * cos(wave.x * 2.0 * 3.14159);
    wave.z += shiftFactor * sin(wave.w * 2.0 * 3.14159);
    wave.w += shiftFactor * cos(wave.z * 2.0 * 3.14159);
}



vec4 toColor(in vec4 wave) {
    vec3 color = vec3(wave.x * 0.5 + 0.5, wave.y * 0.5 + 0.5, wave.z * 0.5 + 0.5);
    float combinedColor = color.r + color.g + color.b;

    if (combinedColor > 1.5) {
        return vec4(color, 1.0); 
    } else {
        return vec4(color.yzx, 1.0); 
    }
}


void main() {
	if (PASSINDEX == 0)	{
	    vec4 wave = A;
	
	    if (FRAMEINDEX < 1 || iMouse.z > 0.) {
	        wave = vec4(0.);
	        wave.zw = vec2(1.0);
	        wave.xy = vec2(-0.1 * cos(0.1 * length(gl_FragCoord.xy - 0.5 * R.xy)), 
	                       0.1 * sin(0.1 * length(gl_FragCoord.xy - 0.5 * R.xy)));
	    }
	
	    evolveWave(wave, gl_FragCoord.xy);
	    //addDynamics(wave, gl_FragCoord.xy);
	    gl_FragColor = normalize(wave);
	}
	else if (PASSINDEX == 1)	{
	}
	else if (PASSINDEX == 2)	{
	}
	else if (PASSINDEX == 3)	{
	}

}
