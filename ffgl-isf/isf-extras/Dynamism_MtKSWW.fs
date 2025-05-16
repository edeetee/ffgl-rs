/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/MtKSWW by nimitz.  Buffer A is the fbm noise gradient generation\nBuffer B is the exact same thing\nBuffer C is the divergence calculator and coloring\nBuffer D is radial blur",
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
            "TARGET": "BufferC"
        },
        {
        }
    ]
}

*/


// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options


#define time TIME
#define time2 (time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5)
#define time3 (time*1. + ((1.0+sin(time*0.9 + sin(time*0.34+ cos(time*0.21)))))*1.5)
#define time4 (time*0.5 + ((1.0+sin(time*0.8 + sin(time*0.14+ cos(time*0.15)))))*1.2)

vec2 hash(vec2 p)
{
	vec3 p3 = fract(vec3(p.xyx) * vec3(.1031, .1030, .0973));
    p3 += dot(p3.zxy, p3.yxz+19.19);
    return -1.0 + 2.0*fract(vec2(p3.x * p3.y, p3.z*p3.x));
}

//2D Simplex noise from iq: https://www.shadertoy.com/view/Msf3WH
float noise(in vec2 p)
{
    p *= 0.45;
    const float K1 = 0.366025404;
    const float K2 = 0.211324865;

	vec2 i = floor( p + (p.x+p.y)*K1 );
	
    vec2 a = p - i + (i.x+i.y)*K2;
    vec2 o = (a.x>a.y) ? vec2(1.0,0.0) : vec2(0.0,1.0);
    vec2 b = a - o + K2;
	vec2 c = a - 1.0 + 2.0*K2;

    vec3 h = max( 0.5-vec3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );

	vec3 n = h*h*h*h*vec3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));

    return dot( n, vec3(38.0) );
	
}

mat2 rot(in float a){float c = cos(a), s = sin(a);return mat2(c,s,-s,c);}
float fbm(in vec2 p, in vec2 of)
{	
    p *= rot(time3*0.1);
    p += of;
	float z=2.;
	float rz = 0.;
	vec2 bp = p;
	for (float i= 1.;i <9.;i++ )
	{
        rz+= noise(p*rot(float(i)*2.3)+ time*0.5)/z;
		z *= 3.2;
		p *= 2.0;
	}
	return rz;
}

vec2 grdf(in vec2 p, in vec2 of)
{
    vec2 ep = vec2(0.0,0.0005);
    vec2 d = vec2(fbm(p - ep.yx, of) - fbm(p + ep.yx, of),
                fbm(p - ep.xy, of) - fbm(p + ep.xy, of));
    d /= length(d);
    return d;
}

// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

#define time TIME
#define time2 (time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5)
#define time3 (time*1. + ((1.0+sin(time*0.9 + sin(time*0.34+ cos(time*0.21)))))*1.5)
#define time4 (time*0.5 + ((1.0+sin(time*0.8 + sin(time*0.14+ cos(time*0.15)))))*1.2)



// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

#define time TIME

#define time2v (((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5)

//Divergence
vec2 div( vec2 p, sampler2D smp)
{
    vec2 tx = 1. / RENDERSIZE.xy;

    vec4 uv =    textureLod(smp, p, -100.);
    vec4 uv_n =  textureLod(smp, p + vec2(0.0, tx.y), -100.);
    vec4 uv_e =  textureLod(smp, p + vec2(tx.x, 0.0), -100.);
    vec4 uv_s =  textureLod(smp, p + vec2(0.0, -tx.y), -100.);
    vec4 uv_w =  textureLod(smp, p + vec2(-tx.x, 0.0), -100.);

   	float div = uv_s.y - uv_n.y - uv_e.x + uv_w.x;
    float div2 = uv_s.w - uv_n.w - uv_e.z + uv_w.z;
    
    return vec2(div, div2)*1.8;
}

// Dynamism by nimitz (twitter: @stormoid)
// https://www.shadertoy.com/view/MtKSWW
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// Contact the author for other licensing options

/*
	Mostly about showing divergence based procedural noise, the rest is just me
	playing around to make it somewhat interesting to look at.

	I stumbled upon this new form of noise while playing with noise gradients
	and noise diverengence. First generate more or less standard fbm (with high decay)
	then compute the gradient of that noise (either numerically or analytically) and 
	then compute the divergence of the gradient and you get the noise you see here.

	As you can see it has a very "DLA" look to it. It is also very easy to animate as
	you can simply offset the noise fetches inside the initial fbm generation and produce
	good looking animated noise. I did some	testing and the paremeters can be modified 
	to result in a decent variety of output	noises, altough still somewhat similar than
	what is seen here.

	I have not tested it yet, but this method should extend to 3D without issues
	and should result in interesting volumes.

	This shader used to run at 60fps with webGL 1 but since webGL it seems
	capped at 30fps on my test computer.
*/

const vec2 center = vec2(0,0);
const int samples = 15;
const float wCurveA = 1.;
const float wCurveB = 1.;
const float dspCurveA = 2.;
const float dspCurveB = 1.;

#define time TIME

float wcurve(float x, float a, float b)
{
    float r = pow(a + b,a + b)/(pow(a, a)*pow(b, b));
    return r*pow(x, a)*pow(1.0 - x, b);
}

float hash21(in vec2 n){ return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 43758.5453); }

void main() {
	if (PASSINDEX == 0)	{


	    vec2 p = gl_FragCoord.xy / RENDERSIZE.xy-0.5;
		p.x *= RENDERSIZE.x/RENDERSIZE.y;
	    p *= 1.75;
	    
	    float t1 = mod(time2*0.35,4.);
	    float t2 = mod(time2*0.35 + 1.,4.);
	    
	    vec2 p1 = p*(4.0-t1);
	    vec2 p2 = p*(4.0-t2);
	    
	    vec2 fld = grdf(p1, vec2(time4*0.2,time*0.0));
	    vec2 fld2 = grdf(p2, vec2(time4*0.2,time*0.0) + 2.2);
	    
	    gl_FragColor = vec4(fld, fld2);
	}
	else if (PASSINDEX == 1)	{


	    vec2 p = gl_FragCoord.xy / RENDERSIZE.xy-0.5;
		p.x *= RENDERSIZE.x/RENDERSIZE.y;
	    p *= 1.75;
	    
	    float t3 = mod(time2*0.35 + 2.,4.);
	    float t4 = mod(time2*0.35 + 3.,4.);
	    
	    vec2 p3 = p*(4.0-t3);
	    vec2 p4 = p*(4.0-t4);
	    
	    vec2 fld = grdf(p3, vec2(time4*0.2,time*0.0) + 4.5);
	    vec2 fld2 = grdf(p4, vec2(time4*0.2,time*0.0) + 7.3);
	    
	    gl_FragColor = vec4(fld, fld2);
	}
	else if (PASSINDEX == 2)	{


		vec2 p = gl_FragCoord.xy / RENDERSIZE.xy;
	    
	    vec2 dv = div(p, BufferA);
	    vec2 dv2 = div(p, BufferB);
	    
	    dv = pow(abs(dv), vec2(.5))*sign(dv);
	    dv = clamp(dv,0.,4.);
	    dv2 = pow(abs(dv2), vec2(.5))*sign(dv2);
	    dv2 = clamp(dv2,0.,4.);
	    
	    float t1 = mod(time2*0.35,4.);
	    float t2 = mod(time2*0.35 + 1.,4.);
	    float t3 = mod(time2*0.35 + 2.,4.);
	    float t4 = mod(time2*0.35 + 3.,4.);
	    
	    const float ws = 1.1;
	    const float wof = 1.8;
	    
	    //derivative of the "depth"
	    //time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5
	    float x = time;
	    float drvT = 1.5 * cos(x + sin(0.4*x + cos(0.1*x)))*(cos(0.4*x + cos(0.1*x)) * (0.4 - 0.1*sin(0.1*x)) + 1.0) + 2.1;
	    
	    float ofsc = 0.8 + drvT*0.07;
	    float t1w = clamp(t1*ws + wof,0.,10.);
	    float t2w = clamp(t2*ws + wof,0.,10.);
	    float t3w = clamp(t3*ws + wof,0.,10.);
	    float t4w = clamp(t4*ws + wof,0.,10.);
	    
	    vec3 col = vec3(0);
	    
	    col += sqrt(t1)*vec3(0.28,0.19,0.15)*exp2(dv.x*t1w-t1w*ofsc);
	    col += sqrt(t2)*vec3(0.1,0.13,0.23)*exp2(dv.y*t2w-t2w*ofsc);
	    col += sqrt(t3)*vec3(0.27,0.07,0.07)*exp2(dv2.x*t3w-t3w*ofsc);
	    col += sqrt(t4)*vec3(0.1,0.18,0.25)*exp2(dv2.y*t4w-t4w*ofsc);
	    
	    col = pow(col, vec3(.6))*1.2;
	    col *= smoothstep(0.,1.,col);
	    
	    col *= pow(16.0*p.x*p.y*(1.0 - p.x)*(1.0 - p.y), 0.4);
	    
		gl_FragColor = vec4(col,1.0);
	}
	else if (PASSINDEX == 3)	{


	    vec2 p = gl_FragCoord.xy/RENDERSIZE.xy;
	    vec2 mo = iMouse.xy/RENDERSIZE.xy;
		
	    vec2 center= mo;
	    center = vec2(0.5,0.5);
	    
	    vec3  col = vec3(0.0);
	    vec2 tc = center - p;
	    
	    float w = 1.0;
	    float tw = 1.;
	    
	    float rnd = (hash21(p)-0.5)*0.75;
	    
	    //derivative of the "depth"
	    //time*2.1 + ((1.0+sin(time + sin(time*0.4+ cos(time*0.1)))))*1.5
	    float x = time;
	    float drvT = 1.5 * cos(x + sin(0.4*x + cos(0.1*x)))*(cos(0.4*x + cos(0.1*x)) * (0.4 - 0.1*sin(0.1*x)) + 1.0) + 2.1;
	    
	    
	    float strength = 0.01 + drvT*0.01;
	    
	    for(int i=0; i<samples; i++)
	    {
	        float sr = float(i)/float(samples);
	        float sr2 = (float(i) + rnd)/float(samples);
	        float weight = wcurve(sr2, wCurveA, wCurveB);
	        float displ = wcurve(sr2, dspCurveA, dspCurveB);
	        col += IMG_NORM_PIXEL(BufferC,mod(p + (tc*sr2*strength*displ),1.0)).rgb*weight;
	        tw += .9*weight;
	    }
	    col /= tw;
	
		gl_FragColor = vec4( col, 1.0 );
	}

}
