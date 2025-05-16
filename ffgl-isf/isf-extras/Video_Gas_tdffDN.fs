/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/tdffDN by michael0884.  This is cooler than the chain tbh",
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
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferC"
        },
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "BufferD"
        },
        {
        }
    ]
}

*/


float iFrameRate = 60.;
float frameRatio = RENDERSIZE.x/RENDERSIZE.y;
#define size RENDERSIZE.xy
#define pixel(a, p) texture(a, p/vec2(textureSize(a,0)))
#define texel(a, p) texelFetch(a, ivec2(p), 0)
#define ch0 inputImage
#define ch1 iChannel1
#define ch2 iChannel2
#define ch3 iChannel3
#define PI 3.14159265

#define dt 0.4
#define prop 0.5

ivec2 N;
int tot_n;

float hash11(float p)
{
    p = fract(p * 15.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p) - 0.5;
}

float hash12(vec2 p)
{
	vec3 p3  = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}


vec2 hash21(float p)
{
	vec3 p3 = fract(vec3(p) * vec3(.1031, .1030, .0973));
	p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx+p3.yz)*p3.zy);

}

vec2 hash22(vec2 p)
{
	vec3 p3 = fract(vec3(p.xyx) * vec3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx+33.33);
    return fract((p3.xx+p3.yz)*p3.zy);
}

const int k = 1664525;  

ivec4 hash( ivec4 x )
{
    x = ((x>>8)^x.wxyz)*k;
    x = ((x>>8)^x.wxyz)*k;
    x = ((x>>8)^x.wxyz)*k;
    x = ((x>>8)^x.wxyz)*k;
    return ivec4(x);
}

ivec2 i2xy(int id)
{
    return ivec2(id%N.x, id/N.x);
}

int xy2i(ivec2 p)
{
    return p.x + p.y*N.x;
}

ivec2 cross_distribution(int i)
{
    return (1<<(i/4)) * ivec2( ((i&2)/2)^1, (i&2)/2 ) * ( 2*(i%2) - 1 );
}

float sdSegment( in vec2 p, in vec2 a, in vec2 b )
{
    vec2 pa = p-a, ba = b-a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}

//particle buffer

int cid;

ivec4 get(int id)
{
    return ivec4(texel(ch0, i2xy(id)));
}

vec4 getParticle(int id)
{
    return texel(ch1, i2xy(id));
}

float F(float d)
{
    return (0.15*exp(-0.1*d) - 2.*exp(-0.2*d));
}

float imageV(vec2 p)
{
    return 1.-2.*texture(ch2, vec2(1., 1.)*p/size).x;
}

vec2 imageF(vec2 p)
{
    vec3 d = vec3(-1,0,1);
    return vec2(imageV(p+d.zy) - imageV(p+d.xy), imageV(p+d.yz) - imageV(p+d.yx));
}

vec2 Fv(vec4 p0, int pid)
{
    if(pid < 0 || pid >= tot_n || pid == cid) return vec2(0); 
   	vec4 p1 = getParticle(pid);
    float d= distance(p0.xy, p1.xy);
    vec2 dv = (p1.zw - p0.zw);
    float dotv = dot(normalize(p1.xy-p0.xy), normalize(dv)); //divergence correction
    vec2 antidivergence = 0.*dv*abs(dotv)*exp(-0.5*d);
    vec2 viscosity = 0.25*dv*exp(-0.1*d);
    vec2 pressure = normalize(p1.xy-p0.xy)*F(d);
    return viscosity + pressure + antidivergence;
}

float irad;

vec2 Fspring(vec4 p0, int pid)
{
    if(pid < 0 || pid >= tot_n || pid == cid) return vec2(0); 
   	vec4 p1 = getParticle(pid);
    vec2 interaction = normalize(p1.xy-p0.xy)*(distance(p1.xy,p0.xy)- 2.*PI*irad/float(tot_n) - 4.*tanh(0.1*TIME));
    return interaction;
}

//sorting closest 4 particles in axis directions that make a bounding box
//only in particle space, texture buffer not needed

ivec4 get(int id)
{
    return ivec4(floor(texel(ch0, i2xy(id))));
}

vec4 save(ivec4 v)
{
    return vec4(v);
}

ivec4 u; //ids
vec4 d; //distances
vec2 pos; //this particle position
int tid;

vec4 getParticle(int id)
{
    return texel(ch1, i2xy(id));
}

//insertion sort
void sort(int utemp)
{
    if(utemp == tid || utemp < 0) return;
     
   	vec4 part = getParticle(utemp);
    vec2 dx = part.xy - pos;
    float dtemp = length(dx);
    //sorting
    if(dx.x > abs(dx.y))
    {
        if(d.x > dtemp) 
        {
            d.x = dtemp;
        	u.x = utemp;
        }
    }
    else if(dx.x < -abs(dx.y))
    {
        if(d.y > dtemp) 
        {
            d.y = dtemp;
        	u.y = utemp;
        }
    }
    else if(dx.y > abs(dx.x))
    {
        if(d.z > dtemp) 
        {
            d.z = dtemp;
        	u.z = utemp;
        }
    }
    else if(d.w > dtemp) 
    {
        d.w = dtemp;
        u.w = utemp;
    }
}

void sortneighbor(int id)
{
    ivec4 nb = get(id);
    for(int j = 0; j < 4; j++)
    {
        sort(nb[j]);
    }
}

//4th order voronoi particle tracking 

ivec4 get(ivec2 p)
{
    return ivec4(floor(texel(ch0, p)));
}

vec4 save(ivec4 v)
{
    return vec4(v);
}

ivec4 u; //ids
vec4 d; //distances
vec2 pos; //pixel position

vec4 getParticle(int id)
{
    return texel(ch1, i2xy(id));
}

float particleDistance(int id, vec2 p)
{
    return distance(p, getParticle(id).xy);
}

//insertion sort
void sort(int utemp)
{
    if(utemp <0) return; 
   	float dtemp = particleDistance(utemp, pos);
    //sorting
    if(d.x > dtemp)
    {
        d = vec4(dtemp, d.xyz);
        u = ivec4(utemp, u.xyz);
    }
    else if(d.y > dtemp && dtemp > d.x)
    {
        d.yzw = vec3(dtemp, d.yz);
        u.yzw = ivec3(utemp, u.yz);
    }
    else if(d.z > dtemp && dtemp > d.y)
    {
        d.zw = vec2(dtemp, d.z);
        u.zw = ivec2(utemp, u.z);
    }
    else if(d.w > dtemp && dtemp > d.z)
    {
        d.w = dtemp;
        u.w = utemp;
    }
}

void sortpos(ivec2 p)
{
    ivec4 nb = get(p);
    for(int j = 0; j < 4; j++)
    {
        sort(nb[j]);
    }
}

// Fork of "Connected particle chain image" by michael0884. https://shadertoy.com/view/3dXfDN
// 2020-04-30 15:21:35

// Fork of "Large scale flocking" by michael0884. https://shadertoy.com/view/tsScRG
// 2020-04-30 07:24:31

ivec4 get(ivec2 p)
{
    return ivec4(floor(texel(ch0, p)));
}

ivec4 getb(int id)
{
    return ivec4(floor(texel(ch2, i2xy(id))));
}

vec4 getParticle(int id)
{
    return texel(ch1, i2xy(id));
}

vec3 imageC(vec2 p)
{
    return texture(ch3, vec2(1., 1.)*p/size).xyz;
}

float particleDistance(int id, vec2 p)
{
    return distance(p, getParticle(id).xy);
}

void main() {
	if (PASSINDEX == 0)	{


	    ivec2 p = ivec2(gl_FragCoord.xy);
	    N = ivec2(prop*RENDERSIZE.xy);
	    tot_n = N.x*N.y;
	    if(p.x < N.x && p.y < N.y)
	    {
	        irad = 0.3*size.y;
	        gl_FragCoord.xy = floor(gl_FragCoord.xy);
	        //this pixel value
	        gl_FragColor = texel(ch1, gl_FragCoord.xy);
	        int id = xy2i(p);
	        cid = id;
	        
	        //this pixel value
	        if(FRAMEINDEX<10)
	        {
	            float t = 2.*PI*float(id)/float(tot_n);
	            gl_FragColor.xy = size*hash22(3.14159*gl_FragCoord.xy);
				gl_FragColor.zw = 1.*(hash22(3.14159*gl_FragCoord.xy) - 0.5);
	      		return;
	        }
	        
	        //neighbors
	   		ivec4 cp = get(id);
	   	  
	        vec2 F = Fv(gl_FragColor, cp.x) +
	            	 Fv(gl_FragColor, cp.y) +
	            	 Fv(gl_FragColor, cp.z) +
	                 Fv(gl_FragColor, cp.w) +
	            	 -20.*imageF(gl_FragColor.xy);
	        
	        if(iMouse.z > 0.) 
	        {
	            float d = distance(iMouse.xy, gl_FragColor.xy);
	            F += 2.*normalize(iMouse.xy - gl_FragColor.xy)/(sqrt(d)+2.);
	        }
	        
	        gl_FragColor.zw = 15.*tanh((F*dt + gl_FragColor.zw)/15.) ;
	        gl_FragColor.xy += gl_FragColor.zw*dt;
	        
	        //border conditions
	        if(size.x - gl_FragColor.x < 2.) gl_FragColor.z = -abs(gl_FragColor.z);
	        if(gl_FragColor.x < 2.) gl_FragColor.z = abs(gl_FragColor.z);
	        if(size.y - gl_FragColor.y < 2.) gl_FragColor.w = -abs(gl_FragColor.w);
	        if(gl_FragColor.y < 2.) gl_FragColor.w = abs(gl_FragColor.w);
	 
	        
	    }
	    else discard;
	}
	else if (PASSINDEX == 1)	{
  

	    ivec2 p = ivec2(gl_FragCoord.xy);
	    N = ivec2(prop*RENDERSIZE.xy);
	    tot_n = N.x*N.y;
	    if(p.x > N.x || p.y > N.y) discard;
	    
	    int id = xy2i(p);
	     
	    u = ivec4(-1); d = vec4(1e10); 
	   
	    tid = id;
	    pos = getParticle(id).xy;
	    
	    sortneighbor(id); 
	    
	    for(int i = 0; i < 8; i++)
	    {
	        sort(hash(ivec4(p, FRAMEINDEX, i)).x%tot_n); //random sort    
	    }
	    
	    ivec4 nb = get(id);
	    for(int i = 0; i < 4; i++)
	    {
	        sortneighbor(nb[i]); 
	    }
	    
	    if( any(lessThan(u, ivec4(-1))) || any(greaterThan(u, ivec4(tot_n))))
	    {
	        u = ivec4(0);
	    }
	    
	    
	    gl_FragColor = save(u);
	}
	else if (PASSINDEX == 2)	{


	    pos = gl_FragCoord.xy;
	     N = ivec2(prop*RENDERSIZE.xy);
	    tot_n = N.x*N.y;
	    ivec2 p = ivec2(pos);
	     
	    u = ivec4(-1); d = vec4(1e10); 
	   
	    //jump flood sorting 
	    sortpos(p); //resort this position
	    for(int i = 0; i < 8; i++)
	    {
	        sortpos(p+cross_distribution(i)); 
	    }
	    
	    for(int i = 0; i < 4; i++)
	    {
	        sort(hash(ivec4(p, FRAMEINDEX, i)).x%tot_n); //random sort    
	    }
	    
	    if( any(lessThan(u, ivec4(-1))) || any(greaterThan(u, ivec4(tot_n))) )
	    {
	        u = ivec4(0);
	    }
	    
	    gl_FragColor = save(u);
	}
	else if (PASSINDEX == 3)	{


	    gl_FragColor = texture(ch0, gl_FragCoord.xy/size);
	}
	else if (PASSINDEX == 4)	{


	     N = ivec2(prop*RENDERSIZE.xy);
	    tot_n = N.x*N.y;
	    ivec4 nb = get(ivec2(gl_FragCoord.xy));
	 	vec4 p0 = getParticle(nb.x);
	   
	    gl_FragColor = vec4(0.,0,0,1);
	    for(int i = 0; i < 4; i++)
	    {
	       vec4 p0 = getParticle(nb[i]);
	    	gl_FragColor.xyz += 0.3*(0.85+0.25*imageC(p0.xy))
	            			//*sin(vec3(1,2,3)*length(p0.zw))
	            			*exp(-0.15*distance(p0.xy, gl_FragCoord.xy));
	    }
	}

}
