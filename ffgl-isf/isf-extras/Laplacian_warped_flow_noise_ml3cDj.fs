/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/ml3cDj by Sergio_2357.  Laplacian warped flow noise inspired by nimitz's Dynamism (https://www.shadertoy.com/view/MtKSWW) and iq's warped fbm noise idea (https://www.shadertoy.com/view/lsl3RH).",
    "IMPORTED": {
    },
    "INPUTS": [
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


float random (in vec2 st) {
    return fract(sin(dot(st.xy,
                         vec2(12.9898,78.233)))*
        43758.5453123);
}

float lerp(float a, float b, float f) {
    return (1.0-f)*a + f*b;
}


const float ROTATION_SPEED = 0.7;
vec2 getGradient(vec2 p) {
    float deg = random(p)*8. + TIME*ROTATION_SPEED*(random(p)*.5+.5);
    vec2 grd = vec2(cos(deg), sin(deg));
    return grd;
}

float gradientNoise(vec2 ps) {
    vec2 pi = floor(ps);
    vec2 pf = fract(ps);
    
    vec2 u = pf * pf * (3.0 - 2.0 * pf);
    //vec2 u = pf;
    
    vec2 llp = pi;
    vec2 llv = getGradient(llp);
    vec2 hlp = pi + vec2(0.0, 1.0);
    vec2 hlv = getGradient(hlp);
    vec2 lrp = pi + vec2(1.0, 0.0);
    vec2 lrv = getGradient(lrp);
    vec2 hrp = pi + vec2(1.0, 1.0);
    vec2 hrv = getGradient(hrp);
    
    float ll = dot(llv, (ps-llp));
    float lr = dot(lrv, (ps-lrp));
    float hl = dot(hlv, (ps-hlp));
    float hr = dot(hrv, (ps-hrp));
    
    float l = lerp(ll, lr, u.x);
    float h = lerp(hl, hr, u.x);
    
    float v = lerp(l, h, u.y);
    
    
    v = v*0.5+0.5;
    return v;
}


float fbm(vec2 ps) {
    vec2 p = ps;
    float v = 0.0;
    float s = .7;
    for (int i = 0; i < 17; i++) {
        v += gradientNoise(p) * s;
        s *= 0.33;
        p *= 2.0;
    }
    return v;
}

vec2 v2fbm(vec2 ps) {
    float x = fbm(ps);
    float y = fbm(ps+vec2(5.0, 4.0));
    return vec2(x, y)*0.4;
}

float warpedFBM(vec2 ps) {
    return fbm(ps+v2fbm(ps+v2fbm(ps)));
}


vec2 gradient(vec2 uv) {
    
    vec2 pxuv = vec2(1.,1.) / RENDERSIZE.xy;
    
    float c = IMG_NORM_PIXEL(BufferA,mod(uv,1.0)).x;
    float r = IMG_NORM_PIXEL(BufferA,mod(uv+vec2(pxuv.x, 0.0),1.0)).x;
    float u = IMG_NORM_PIXEL(BufferA,mod(uv+vec2(0.0, pxuv.y),1.0)).x;
    
    vec2 grd = vec2(0.0, 0.0);
    
    grd.x = r-c;
    grd.y = u-c;
    
    grd = normalize(grd);
    
    return grd;
    
}

// div(normalize(grad(noise)) idea from nimitz https://www.shadertoy.com/view/MtKSWW
// fbm(p+fbm(p)) (warped noise) idea from iq https://www.shadertoy.com/view/lsl3RH
// plus flownoise in the fbm

float divergence(vec2 uv) {

    vec2 tx = 1. / RENDERSIZE.xy;

    vec4 uv_n =  (IMG_NORM_PIXEL(BufferB,mod(uv + vec2(0.0, tx.y),1.0))-0.5)*2.0;
    vec4 uv_e =  (IMG_NORM_PIXEL(BufferB,mod(uv + vec2(tx.x, 0.0),1.0))-0.5)*2.0;
    vec4 uv_s =  (IMG_NORM_PIXEL(BufferB,mod(uv + vec2(0.0, -tx.y),1.0))-0.5)*2.0;
    vec4 uv_w =  (IMG_NORM_PIXEL(BufferB,mod(uv + vec2(-tx.x, 0.0),1.0))-0.5)*2.0;

   	float div = uv_s.y - uv_n.y - uv_e.x + uv_w.x;

    return div;
}

void main() {
	if (PASSINDEX == 0)	{


	    vec2 uv = gl_FragCoord.xy/RENDERSIZE.y;
	    
	    uv *= 1.;
	    
	    vec3 col = vec3(warpedFBM(uv));
	    
	    gl_FragColor = vec4(col,1.0);
	}
	else if (PASSINDEX == 1)	{


	    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	    
	    vec2 grd = gradient(uv)*0.5+0.5;
	    
	    gl_FragColor = vec4(grd, 1.0, 1.0);
	}
	else if (PASSINDEX == 2)	{


	    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	    
	    float v = divergence(uv)*2.;
	    
	    vec3 col = (0.5 + 0.5*cos(5.*v*uv.xyx+vec3(0,2,4)));
	    
	    gl_FragColor = vec4(vec3(v)*col*1.5, 1.0);
	    
	    if (gl_FragCoord.x+2. >= RENDERSIZE.x || gl_FragCoord.y+2. >= RENDERSIZE.y) {
	        gl_FragColor = vec4(vec3(0.0), 1.0);
	    }
	    
	}

}
