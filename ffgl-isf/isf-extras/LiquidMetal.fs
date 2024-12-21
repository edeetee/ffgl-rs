/*{
	"DESCRIPTION": "Your shader description",
	"CREDIT": "by you",
	"CATEGORIES": [
		"Your category"
	],
	"INPUTS": [
		{
			"NAME": "iChannel0",
			"TYPE": "image"
		},
		{
			"NAME": "SCALE",
			"TYPE": "float",
			"DEFAULT": 4.0,
			"MIN": 0.0,
			"MAX": 10.0
		},
		{
			"NAME": "WARP1",
			"TYPE": "float",
			"DEFAULT": 3.0,
			"MIN": 0.0,
			"MAX": 10.0
		},
		{
			"NAME": "WARP2",
			"TYPE": "float",
			"DEFAULT": 3.0,
			"MIN": 0.0,
			"MAX": 10.0
		},
		{
			"NAME": "JITTER",
			"TYPE": "bool",
			"DEFAULT": 1.0
		}
      	]
}*/

vec3 iResolution = vec3(RENDERSIZE, 1.f);
float iGlobalTime = TIME;

/*
	Bumped Sinusoidal Warp
	----------------------
	
	https://www.shadertoy.com/view/4l2XWK#

	Sinusoidal planar deformation, or the 2D sine warp effect to people 
	like me. The effect has been around for years, and there are
	countless examples on the net. IQ's "Sculpture III" is basically a 
	much more sophisticated, spherical variation.

    This particular version was modified from Fabrice's "Plop 2," which in 
	turn was a simplified version of Fantomas's "Plop." I simply reduced 
	the frequency and iteration count in order to make it less busy.

	I also threw in a texture, added point-lit bump mapping, speckles... 
	and that's pretty much it. As for why a metallic surface would be 
	defying	the laws of physics and moving like this is anyone's guess. :)

	By the way, I have a 3D version, similar to this, that I'll put up at 
	a later date.


	Related examples:

    Fantomas - Plop
    https://www.shadertoy.com/view/ltSSDV

    Fabrice - Plop 2
    https://www.shadertoy.com/view/MlSSDV

	IQ - Sculpture III (loosely related)
	https://www.shadertoy.com/view/XtjSDK

	Shane - Lit Sine Warp (far less code)
	https://www.shadertoy.com/view/Ml2XDV

*/

// Warp function. Variations have been around for years. This is
// almost the same as Fabrice's version:
// Fabrice - Plop 2
// https://www.shadertoy.com/view/MlSSDV
vec2 W(vec2 p) {

    p = (p + 3.f) * SCALE;

    float t = iGlobalTime / 2.f;

    // Layered, sinusoidal feedback, with time component.
    for(int i = 0; i < 5; i++) {
        p += cos(p.yx * 3.f + vec2(t, SCALE)) / WARP1;
        p += sin(p.yx + t + vec2(SCALE, 0)) / WARP2;
        p *= 1.3f;
    }

    // A bit of jitter to counter the high frequency sections.
    if(JITTER) {
        p += fract(sin(p + vec2(13, 7)) * 5e5f) * .03f - .015f;
    }
    return mod(p, 2.f) - 1.f; // Range: [vec2(-1), vec2(1)]

}

// Bump mapping function. Put whatever you want here. In this case, 
// we're returning the length of the sinusoidal warp function.
float bumpFunc(vec2 p) {

    return length(W(p)) * .7071f; // Range: [0, 1]

}

/*
// Standard ray-plane intersection.
vec3 rayPlane(vec3 p, vec3 o, vec3 n, vec3 rd) {
    
    float dn = dot(rd, n);

    float s = 1e8;
    
    if (abs(dn) > 0.0001) {
        s = dot(p-o, n) / dn;
        s += float(s < 0.0) * 1e8;
    }
    
    return o + s*rd;
}
*/

//vec3 smoothFract(vec3 x){ x = fract(x); return min(x, x*(1.-x)*12.); }

void mainImage(out vec4 fragColor, in vec2 fragCoord) {

    // Screen coordinates.
    vec2 uv = (fragCoord - iResolution.xy * .5f) / iResolution.y;

    // PLANE ROTATION
    //
    // Rotating the canvas back and forth. I don't feel it adds value, in this case,
    // but feel free to uncomment it.
    //float th = sin(iGlobalTime*0.1)*sin(iGlobalTime*0.12)*4.;
    //float cs = cos(th), si = sin(th);
    //uv *= mat2(cs, -si, si, cs);

    // VECTOR SETUP - surface postion, ray origin, unit direction vector, and light postion.
    //
    // Setup: I find 2D bump mapping more intuitive to pretend I'm raytracing, then lighting a bump mapped plane 
    // situated at the origin. Others may disagree. :)  
    vec3 sp = vec3(uv, 0); // Surface posion. Hit point, if you prefer. Essentially, a screen at the origin.
    vec3 rd = normalize(vec3(uv, 1.f)); // Unit direction vector. From the origin to the screen plane.
    vec3 lp = vec3(cos(iGlobalTime) * 0.5f, sin(iGlobalTime) * 0.2f, -1.f); // Light position - Back from the screen.
    vec3 sn = vec3(0.f, 0.f, -1); // Plane normal. Z pointing toward the viewer.

/*
	// I deliberately left this block in to show that the above is a simplified version
	// of a raytraced plane. The "rayPlane" equation is commented out above.
	vec3 rd = normalize(vec3(uv, 1.));
	vec3 ro = vec3(0., 0., -1);

	// Plane normal.
	vec3 sn = normalize(vec3(cos(iGlobalTime)*0.25, sin(iGlobalTime)*0.25, -1));
    //vec3 sn = normalize(vec3(0., 0., -1));
	
	vec3 sp = rayPlane(vec3(0., 0., 0.), ro, sn, rd);
    vec3 lp = vec3(cos(iGlobalTime)*0.5, sin(iGlobalTime)*0.25, -1.); 
*/    

    // BUMP MAPPING - PERTURBING THE NORMAL
    //
    // Setting up the bump mapping variables. Normally, you'd amalgamate a lot of the following,
    // and roll it into a single function, but I wanted to show the workings.
    //
    // f - Function value
    // fx - Change in "f" in in the X-direction.
    // fy - Change in "f" in in the Y-direction.
    vec2 eps = vec2(4.f / iResolution.y, 0.f);

    float f = bumpFunc(sp.xy); // Sample value multiplied by the amplitude.
    float fx = bumpFunc(sp.xy - eps.xy); // Same for the nearby sample in the X-direction.
    float fy = bumpFunc(sp.xy - eps.yx); // Same for the nearby sample in the Y-direction.

 	// Controls how much the bump is accentuated.
    const float bumpFactor = 0.05f;

    // Using the above to determine the dx and dy function gradients.
    fx = (fx - f) / eps.x; // Change in X
    fy = (fy - f) / eps.x; // Change in Y.
    // Using the gradient vector, "vec3(fx, fy, 0)," to perturb the XY plane normal ",vec3(0, 0, -1)."
    // By the way, there's a redundant step I'm skipping in this particular case, on account of the 
    // normal only having a Z-component. Normally, though, you'd need the commented stuff below.
    //vec3 grad = vec3(fx, fy, 0);
    //grad -= sn*dot(sn, grad);
    //sn = normalize( sn + grad*bumpFactor ); 
    sn = normalize(sn + vec3(fx, fy, 0) * bumpFactor);           

    // LIGHTING
    //
	// Determine the light direction vector, calculate its distance, then normalize it.
    vec3 ld = lp - sp;
    float lDist = max(length(ld), 0.001f);
    ld /= lDist;

    // Light attenuation.    
    float atten = min(1.f / (0.25f + lDist * 0.5f + lDist * lDist * 0.05f), 1.f);
	//float atten = min(1./(lDist*lDist*1.), 1.);

    // Using the bump function, "f," to darken the crevices. Completely optional, but I
    // find it gives extra depth.
    atten *= f * .85f + .15f; // Or... f*f*.5 + .5; //  pow(f, .75); // etc.

	// Diffuse value.
    float diff = max(dot(sn, ld), 0.f);  
    // Enhancing the diffuse value a bit. Made up.
    diff = pow(diff, 2.f) * 0.66f + pow(diff, 4.f) * 0.34f; 
    // Specular highlighting.
    float spec = pow(max(dot(reflect(-ld, sn), -rd), 0.f), 8.f); 

    // TEXTURE COLOR
    //
	// Combining the surface postion with a fraction of the warped surface position to index 
    // into the texture. The result is a slightly warped texture, as a opposed to a completely 
    // warped one. By the way, the warp function is called above in the "bumpFunc" function,
    // so it's kind of wasteful doing it again here, but the function is kind of cheap, and
    // it's more readable this way.
    vec3 texCol = IMG_NORM_PIXEL(iChannel0, sp.xy + W(sp.xy) / 8.f).xyz;
    //
    /*
    // Textureless. Simple and elegant... so it clearly didn't come from me. Thanks Fabrice. :)
    vec3 texCol = smoothFract( W(sp.xy).xyy )*.1 + .4;
	*/

    // FINAL COLOR
    // Using the values above to produce the final color.   
    vec3 col = (texCol * (diff * vec3(1, .97f, .92f) * 1.3f + 0.5f) + vec3(1.f, 0.6f, .2f) * spec * 1.3f) * atten;

    // Done. 
    fragColor = vec4(min(col, 1.f), 1.f);
}

void main(void) {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}