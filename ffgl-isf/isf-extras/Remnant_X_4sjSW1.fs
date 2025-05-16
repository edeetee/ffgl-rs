/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/4sjSW1 by Dave_Hoskins.  Binary subdivision finds the surface really well with this fractal. Two light sources with shadows, and near surface glows. MOUSE X TO TIME WARP",
    "IMPORTED": {
        "iChannel0": {
            "NAME": "iChannel0",
            "PATH": "f735bee5b64ef98879dc618b016ecf7939a5756040c2cde21ccb15e69a6e1cfb.png"
        }
    },
    "INPUTS": [
        {
            "NAME": "animate",
            "TYPE": "float"
        }
    ],
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferA"
        },
        {
        }
    ]
}

*/


// Remnant X
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// by David Hoskins.

#define TAU  6.28318530718

float n1 = 0.0;
float n2 = 0.0;
float n3 = 0.0;
float n4 = 0.0;
float fb_lp = 0.0;
float fb_hp = 0.0;
float hp = 0.0;
float p4=1.0e-24;

float gTime;
float beat;


#define N(a, b) if(t > a){x = a; n = b;}
#define K(a) if(t > a) x = a;
#define BRING_IN

#define _sample (1.0 / iSampleRate)

// Low pass resonant filter...
float Filter(float inp, float cut_lp, float res_lp)
{
	fb_lp 	= res_lp+res_lp/(1.0-cut_lp + 1e-20);
	n1 		= n1+cut_lp*(inp-n1+fb_lp*(n1-n2))+p4;
	n2		= n2+cut_lp*(n1-n2);
    return n2;
}

//----------------------------------------------------------------------------------
float Tract(float x, float f, float bandwidth)
{
    float ret = sin(TAU * f * x) * exp(-bandwidth * 2.14159265359 * x);
    return ret;
}

//----------------------------------------------------------------------------------
float Hash(float p)
{
	vec2 p2 = fract(vec2(p * 5.3983, p * 5.4427));
    p2 += dot(p2.yx, p2.xy + vec2(21.5351, 14.3137));
	return fract(p2.x * p2.y * 3.4337) * .5;
}

// Remnant X
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// by David Hoskins.
// Thanks to boxplorer and the folks at 'Fractalforums.com'
// HD Video:- https://www.youtube.com/watch?v=BjkK9fLXXo0

// #define STEREO

vec3 sunDir  = normalize( vec3(  0.35, 0.1,  0.3 ) );
const vec3 sunColour = vec3(1.0, .95, .8);


#define SCALE 2.8
#define MINRAD2 .25
float minRad2 = clamp(MINRAD2, 1.0e-9, 1.0);
#define scale (vec4(SCALE, SCALE, SCALE, abs(SCALE)) / minRad2)
float absScalem1 = abs(SCALE - 1.0);
float AbsScaleRaisedTo1mIters = pow(abs(SCALE), float(1-10));
vec3 surfaceColour1 = vec3(.8, .0, 0.);
vec3 surfaceColour2 = vec3(.4, .4, 0.5);
vec3 surfaceColour3 = vec3(.5, 0.3, 0.00);
vec3 fogCol = vec3(0.4, 0.4, 0.4);


//----------------------------------------------------------------------------------------
float Noise( in vec3 x )
{
    vec3 p = floor(x);
    vec3 f = fract(x);
	f = f*f*(3.0-2.0*f);
	
	vec2 uv = (p.xy+vec2(37.0,17.0)*p.z) + f.xy;
	vec2 rg = IMG_NORM_PIXEL(iChannel0,mod((uv+ 0.5)/256.0,1.0),-99.0).yx;
	return mix( rg.x, rg.y, f.z );
}

//----------------------------------------------------------------------------------------
float Map(vec3 pos) 
{
	
	vec4 p = vec4(pos,1);
	vec4 p0 = p;  // p.w is the distance estimate

	for (int i = 0; i < 9; i++)
	{
		p.xyz = clamp(p.xyz, -1.0, 1.0) * 2.0 - p.xyz;

		float r2 = dot(p.xyz, p.xyz);
		p *= clamp(max(minRad2/r2, minRad2), 0.0, 1.0);

		// scale, translate
		p = p*scale + p0;
	}
	return ((length(p.xyz) - absScalem1) / p.w - AbsScaleRaisedTo1mIters);
}

//----------------------------------------------------------------------------------------
vec3 Colour(vec3 pos, float sphereR) 
{
	vec3 p = pos;
	vec3 p0 = p;
	float trap = 1.0;
    
	for (int i = 0; i < 6; i++)
	{
        
		p.xyz = clamp(p.xyz, -1.0, 1.0) * 2.0 - p.xyz;
		float r2 = dot(p.xyz, p.xyz);
		p *= clamp(max(minRad2/r2, minRad2), 0.0, 1.0);

		p = p*scale.xyz + p0.xyz;
		trap = min(trap, r2);
	}
	// |c.x|: log final distance (fractional iteration count)
	// |c.y|: spherical orbit trap at (0,0,0)
	vec2 c = clamp(vec2( 0.3333*log(dot(p,p))-1.0, sqrt(trap) ), 0.0, 1.0);

    float t = mod(length(pos) - gTime*150., 16.0);
    surfaceColour1 = mix( surfaceColour1, vec3(.4, 3.0, 5.), pow(smoothstep(0.0, .3, t) * smoothstep(0.6, .3, t), 10.0));
	return mix(mix(surfaceColour1, surfaceColour2, c.y), surfaceColour3, c.x);
}


//----------------------------------------------------------------------------------------
vec3 GetNormal(vec3 pos, float distance)
{
    distance *= 0.001+.0001;
	vec2 eps = vec2(distance, 0.0);
	vec3 nor = vec3(
	    Map(pos+eps.xyy) - Map(pos-eps.xyy),
	    Map(pos+eps.yxy) - Map(pos-eps.yxy),
	    Map(pos+eps.yyx) - Map(pos-eps.yyx));
	return normalize(nor);
}

//----------------------------------------------------------------------------------------
float GetSky(vec3 pos)
{
    pos *= 2.3;
	float t = Noise(pos);
    t += Noise(pos * 2.1) * .5;
    t += Noise(pos * 4.3) * .25;
    t += Noise(pos * 7.9) * .125;
	return t;
}

//----------------------------------------------------------------------------------------
float BinarySubdivision(in vec3 rO, in vec3 rD, vec2 t)
{
    float halfwayT;
  
    for (int i = 0; i < 6; i++)
    {

        halfwayT = dot(t, vec2(.5));
        float d = Map(rO + halfwayT*rD); 
        //if (abs(d) < 0.001) break;
        t = mix(vec2(t.x, halfwayT), vec2(halfwayT, t.y), step(0.0005, d));

    }

	return halfwayT;
}

//----------------------------------------------------------------------------------------
vec2 Scene(in vec3 rO, in vec3 rD, in vec2 fragCoord)
{
	float t = .05 + 0.05 * IMG_NORM_PIXEL(iChannel0,mod(fragCoord.xy / IMG_SIZE(iChannel0).xy,1.0)).y;
	vec3 p = vec3(0.0);
    float oldT = 0.0;
    bool hit = false;
    float glow = 0.0;
    vec2 dist;
	for( int j=0; j < 100; j++ )
	{
		if (t > 12.0) break;
        p = rO + t*rD;
       
		float h = Map(p);
        
		if(h  <0.0005)
		{
            dist = vec2(oldT, t);
            hit = true;
            break;
        }
       	glow += clamp(.05-h, 0.0, .4);
        oldT = t;
      	t +=  h + t*0.001;
 	}
    if (!hit)
        t = 1000.0;
    else       t = BinarySubdivision(rO, rD, dist);
    return vec2(t, clamp(glow*.25, 0.0, 1.0));

}

//----------------------------------------------------------------------------------------
float Hash(vec2 p)
{
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 33758.5453)-.5;
} 

//----------------------------------------------------------------------------------------
vec3 PostEffects(vec3 rgb, vec2 xy)
{
	// Gamma first...
	

	// Then...
	#define CONTRAST 1.08
	#define SATURATION 1.5
	#define BRIGHTNESS 1.5
	rgb = mix(vec3(.5), mix(vec3(dot(vec3(.2125, .7154, .0721), rgb*BRIGHTNESS)), rgb*BRIGHTNESS, SATURATION), CONTRAST);
	// Noise...
	//rgb = clamp(rgb+Hash(xy*TIME)*.1, 0.0, 1.0);
	// Vignette...
	rgb *= .5 + 0.5*pow(20.0*xy.x*xy.y*(1.0-xy.x)*(1.0-xy.y), 0.2);	

    rgb = pow(rgb, vec3(0.47 ));
	return rgb;
}

//----------------------------------------------------------------------------------------
float Shadow( in vec3 ro, in vec3 rd)
{
	float res = 1.0;
    float t = 0.05;
	float h;
	
    for (int i = 0; i < 8; i++)
	{
		h = Map( ro + rd*t );
		res = min(6.0*h / t, res);
		t += h;
	}
    return max(res, 0.0);
}

//----------------------------------------------------------------------------------------
mat3 RotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat3(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c);
}

//----------------------------------------------------------------------------------------
vec3 LightSource(vec3 spotLight, vec3 dir, float dis)
{
    float g = 0.0;
    if (length(spotLight) < dis)
    {
        float a = max(dot(normalize(spotLight), dir), 0.0);
		g = pow(a, 500.0);
        g +=  pow(a, 5000.0)*.2;
    }
   
    return vec3(.6) * g;
}

#define PI 3.14159

//----------------------------------------------------------------------------------------
vec3 CameraPath( float t )
{
    vec3 p = vec3(-.78 + 3. * sin(TAU*8*t),.05+2.5 * sin(TAU*t+1.3),.05 + 3.5 * cos(TAU*t) );
	return p;
} 
    
//----------------------------------------------------------------------------------------
void main() {
	if (PASSINDEX == 0)	{
	}
	else if (PASSINDEX == 1)	{

		float aTime = animate+TIME*0.0001;
		gTime = TIME*0.01;
	    vec2 xy = gl_FragCoord.xy / RENDERSIZE.xy;
		vec2 uv = (-1.0 + 2.0 * xy) * vec2(RENDERSIZE.x/RENDERSIZE.y, 1.0);
		
		
		#ifdef STEREO
		float isRed = mod(gl_FragCoord.x + mod(gl_FragCoord.y, 2.0),2.0);
		#endif
	
		vec3 cameraPos	= CameraPath(aTime);
	    vec3 camTar		= CameraPath(aTime + .01);
	
		float roll = 13.0*sin(aTime*TAU+.4);
		vec3 cw = normalize(camTar-cameraPos);
	
		vec3 cp = vec3(sin(roll), cos(roll),0.0);
		vec3 cu = normalize(cross(cw,cp));
	
		vec3 cv = normalize(cross(cu,cw));
	    cw = RotationMatrix(cv, sin(-aTime*TAU*10)*.7) * cw;
		vec3 dir = normalize(uv.x*cu + uv.y*cv + 1.3*cw);
	
		#ifdef STEREO
		cameraPos += .008*cu*isRed; // move camera to the right
		#endif
	
	    vec3 spotLight = CameraPath(gTime + .03) + vec3(sin(gTime*18.4), cos(gTime*17.98), sin(gTime * 22.53))*.2;
		vec3 col = vec3(0.0);
	    vec3 sky = vec3(0.03, .04, .05) * GetSky(dir);
		vec2 ret = Scene(cameraPos, dir,gl_FragCoord.xy);
	    
	    if (ret.x < 900.0)
	    {
			vec3 p = cameraPos + ret.x*dir; 
			vec3 nor = GetNormal(p, ret.x);
	        
	       	vec3 spot = spotLight - p;
			float atten = length(spot);
	
	        spot /= atten;
	        
	        float shaSpot = Shadow(p, spot);
	        float shaSun = Shadow(p, sunDir);
	        
	       	float bri = max(dot(spot, nor), 0.0) / pow(atten, 1.5) * .25;
	        float briSun = max(dot(sunDir, nor), 0.0) * .2;
	        
	       col = Colour(p, ret.x);
	       col = (col * bri * shaSpot) + (col * briSun* shaSun);
	        
	       vec3 ref = reflect(dir, nor);
	       col += pow(max(dot(spot,  ref), 0.0), 10.0) * 2.0 * shaSpot * bri;
	       col += pow(max(dot(sunDir, ref), 0.0), 10.0) * 2.0 * shaSun * briSun;
	    }
	    
	    col = mix(sky, col, min(exp(-ret.x+1.5), 1.0));
	    col += vec3(pow(abs(ret.y), 2.)) * vec3(.02, .04, .1);
	
	    col += LightSource(spotLight-cameraPos, dir, ret.x);
		col = PostEffects(col, xy);	
	
		
		#ifdef STEREO	
		col *= vec3( isRed, 1.0-isRed, 1.0-isRed );	
		#endif
		
		gl_FragColor=vec4(col,1.0);
	}

}

//--------------------------------------------------------------------------
