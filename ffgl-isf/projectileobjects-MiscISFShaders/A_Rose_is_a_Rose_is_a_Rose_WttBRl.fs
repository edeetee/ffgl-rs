/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/WttBRl by xavierseb.  Here's some roses for Valentine's",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iMouse",
            "TYPE": "point2D"
        }
    ]
}

*/


// A rose is a rose is a rose -Gertrude Stein
#define rot(a) mat2(cos(a), sin(a), -sin(a), cos(a))
#define PI2 6.2832

float mapLeaf(vec3 p, inout bool isbg) {	 // leaves
	p=mod(p+8.,vec3(16.))-8.; p.z-=5.; 
	p.xy *= rot(PI2/4.);
	p.xy =  vec2( length(p.xy)*sin(atan(p.y, p.x)), length(p.xy) ); p.y-=10.;
	isbg = true;
	return dot(abs(p), .5*normalize(vec3(1.5,abs(sin(atan(p.x,p.y)*80.)/30.+sin(min(p.y,12.)/8.)),26.))) - .16;  
}

float mapRose(vec3 p, inout bool isbg) { // roses
	p=mod(p+vec3(14.,14.,20.),vec3(28.,28.,40.))-vec3(14.,14.,20.); 
	if(p.z>-11.|| p.z<-33. || length(p.xy)>11.) return mapLeaf(p, isbg);
	p.xy *= rot(p.z/.8)/3.5;
	p.xy = vec2( length(p.xy)*sin(atan(p.y, p.x)), length(p.xy)-.25 );   
	p.xy *= rot(atan(p.y,p.x)); 
	p.z-=length(p.xy)/16.+2.8;
	p.yz = vec2( length(p.yz)*sin(mod(atan(p.z, p.y), PI2/3.) - PI2/3./2.), length(p.yz)-11.6 ); 			
	vec2 p1 = p.xy = vec2( length(p.xy)*sin(mod(atan(p.y, p.x), PI2/90.) - PI2/90./2.), length(p.xy) ); p.y-=8.9;
	p.yz *= rot(1.1);
	p.xy = vec2( length(p1)*sin(mod(atan(p1.y, p1.x), PI2/2.) - PI2/2./2.), length(p1)-3.5 )*(1.01-p.z)/2.9;
	p.z+=length(p.xy)/34.-2.8;			
	isbg=false;
	return dot(abs(p), normalize(vec3(11,2,4))) - .5;  
}

void main() {



	vec2 uv = (gl_FragCoord.xy -.5*RENDERSIZE.xy) /RENDERSIZE.y, m=iMouse.xy/RENDERSIZE.xy; 
	vec3 rd = normalize(vec3(uv, .25+ min(1.,m.y))), p = vec3(0,0,-30);
	gl_FragColor = vec4(0);
	bool isbg;
	p.xz *= rot(m.x*.5-.25);	p.yz *= rot(sin(TIME)/12.);
	
	for (int i=1; i<270; i++) {
		float d = mapRose(p,isbg);
		if (d < .001) {
			if(isbg) gl_FragColor = vec4(0,400./float(i*i),0,1);
			else 	 gl_FragColor = vec4(40./float(i),0,0,1);
			break;
		}
		p += rd * d;
	}
}
