/*{
  "DESCRIPTION": "Point lit, transparent lattice. Adding fake diffuse and specular lighting to the transparency formula. Kind of interesting, but not to be taken too seriously.",
  "CREDIT": "Converted from Shadertoy: Transparent Lattice",
  "CATEGORIES": [
    "Generators"
  ],
  "INPUTS": [
    {
      "NAME": "progress",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 5.0
    }
  ]
}
*/

/*
	
    Transparent Lattice
	-------------------

	Just a transparent lattice. Not much different to my other transparent examples, 
	except this one is point lit... In case it needs to be said, a lot of it is faked, 
	so is more of a novelty than anything else. 

	I wrote it some time ago, then forgot about it. I thought I'd put it up just in 
	case it's of any use to anyone. It runs reasonably fast, considering that the 
	lighting is calculated multiple times a pass, but could benefit from a little more 
	tweaking.

	Related shaders:

	Cloudy Spikeball - Duke
    https://www.shadertoy.com/view/MljXDw
    // Port from a demo by Las - Worth watching.
    // http://www.pouet.net/topic.php?which=7920&page=29&x=14&y=9

	Virtually the same thing, but with rounded cubes and less interesting lighting.
	Transparent Cube Field - Shane
	https://www.shadertoy.com/view/ll2SRy
	
*/

// Cheap vec3 to vec3 hash. Works well enough, but there are other ways.
vec3 hash33(vec3 p) {

    float n = sin(dot(p, vec3(7, 157, 113)));
    return fract(vec3(2097152, 262144, 32768) * n);
}

/*
// Rounded cube field, for comparison. It runs at full speed, believe it or not.
float map(vec3 p){
   
	// Creating the repeat cubes, with slightly convex faces. Standard,
    // flat faced cubes don't capture the light quite as well.
    
    // 3D space repetition.
    p = fract(p)-.5; // + o 
    
    // A bit of roundness. Used to give the cube faces a touch of convexity.
    float r = dot(p, p) - 0.21;
    
    // Max of abs(x), abs(y) and abs(z) minus a constant gives a cube.
    // Adding a little bit of "r," above, rounds off the surfaces a bit.
    p = abs(p); 
	return max(max(p.x, p.y), p.z)*.95 + r*0.25 - 0.21;
   
    
    // Alternative. Egg shapes... kind of.
    //float perturb = sin(p.x*10.)*sin(p.y*10.)*sin(p.z*10.);
	//p += hash33(floor(p))*.15;
	//return length(fract(p)-.5)-0.3 + perturb*0.05;
	
}
*/

/*
// A fake noise looking field. Pretty interesting.
float map(vec3 p){

   
	p = (cos(p*.315*2.5 + sin(p.zxy*.875*2.5)));	    // + (TIME * speed + 10.0 * progress)*.5
     
    float n = length(p);
    
    p = sin(p*6. + cos(p.yzx*6.));
    
    return n - 1. - abs(p.x*p.y*p.z)*.05;

    
}
*/

float map(vec3 p) {

    vec2 c;

    // SECTION 1
    //
    // Repeat field entity one, which is just some tubes repeated in all directions every 
    // two units, then combined with a smooth minimum function. Otherwise known as a lattice.
    p = abs(fract(p / 3.f) * 3.f - 1.5f);
    //c.x = sminP(length(p.xy),sminP(length(p.yz),length(p.xz), 0.25), 0.25)-0.75; // EQN 1
    //c.x = sqrt(min(dot(p.xy, p.xy),min(dot(p.yz, p.yz),dot(p.xz, p.xz))))-0.75; // EQN 2
    c.x = min(max(p.x, p.y), min(max(p.y, p.z), max(p.x, p.z))) - 0.75f; // EQN 3
    //p = abs(p); c.x = max(p.x,max(p.y,p.z)) - .5;

    // SECTION 2
    //
    // Repeat field entity two, which is just an abstract object repeated every half unit. 
    p = abs(fract(p * 4.f / 3.f) * .75f - 0.375f);
    c.y = min(p.x, min(p.y, p.z)); // EQN 1
    //c.y = min(max(p.x, p.y),min(max(p.y, p.z),max(p.x, p.z)))-0.125; //-0.175, etc. // EQN 2    
    //c.y = max(p.x,max(p.y,p.z)) - .4;

    // SECTION 3
    //
    // Combining the two entities above.
    //return length(c)-.1; // EQN 1
    //return max(c.x, c.y)-.05; // EQN 2
    return max(abs(c.x), abs(c.y)) * .75f + length(c) * .25f - .1f;
    //return max(abs(c.x), abs(c.y))*.75 + abs(c.x+c.y)*.25 - .1;
    //return max(abs(c.x), abs(c.y)) - .1;

}

// Not big on accuracy, but lower on operations. Few distance function calls are important
// during volumetric passes.
vec3 calcNormal(in vec3 p, float d) {
    const vec2 e = vec2(0.01f, 0);
    return normalize(vec3(d - map(p - e.xyy), d - map(p - e.yxy), d - map(p - e.yyx)));
}

/*
// Tetrahedral normal, to save a couple of "map" calls. Courtesy of IQ. Unfortunately, still
// not fast enough in this particular instance.
vec3 calcNormal(in vec3 p){

    // Note the slightly increased sampling distance, to alleviate artifacts due to hit point inaccuracies.
    vec2 e = vec2(0.0025, -0.0025); 
    return normalize(e.xyy * map(p + e.xyy) + e.yyx * map(p + e.yyx) + e.yxy * map(p + e.yxy) + e.x * map(p + e.xxx));
}
*/

void mainImage(out vec4 fragColor, vec2 fragCoord) {

    // Screen coordinates.
    vec2 uv = (fragCoord.xy - RENDERSIZE.xy * .5f) / RENDERSIZE.y;

    // Unit direction ray. The last term is one of many ways to fish-lens the camera.
    // For a regular view, set "rd.z" to something like "0.5."
    vec3 rd = normalize(vec3(uv, (1.f - dot(uv, uv) * .5f) * .5f)); // Fish lens, for that 1337, but tryhardish, demo look. :)

    // There are a few ways to hide artifacts and inconsistencies. Making things go fast is one of them. :)
    // Ray origin, scene color, and surface postion vector.
    vec3 ro = vec3(0.f, 0.f, (TIME * speed + 10.0f * progress) * 1.5f), col = vec3(0), sp, sn, lp, ld, rnd;

    // Swivel the unit ray to look around the scene.
    // Compact 2D rotation matrix, courtesy of Shadertoy user, "Fabrice Neyret."
    vec2 a = sin(vec2(1.5707963f, 0) + (TIME * speed + 10.0f * progress) * 0.375f);
    rd.xz = mat2(a, -a.y, a.x) * rd.xz;
    rd.xy = mat2(a, -a.y, a.x) * rd.xy;

    lp = vec3(0, 1, 4);
    lp.xz = mat2(a, -a.y, a.x) * lp.xz;
    lp.xy = mat2(a, -a.y, a.x) * lp.xy;
    lp += ro;

    // Unit ray jitter is another way to hide artifacts. It can also trick the viewer into believing
    // something hard core, like global illumination, is happening. :)
    //rd *= 0.99 + hash33(rd)*0.02;

    // Some more randomization, to be used for color based jittering inside the loop.
    rnd = hash33(rd + 311.f);

	// Ray distance, bail out layer number, surface distance and normalized accumulated distance.
    // Note the slight jittering to begin with. It alleviates the subtle banding.
    float t = length(rnd) * .2f, layers = 0.f, d, aD;

	// Light variables.
    float lDist, s, l, fr;

    // Surface distance threshold. Smaller numbers gives a thinner membrane, but lessens detail... 
    // hard to explain. It's easier to check it out for yourself.
    float thD = .0125f; // + smoothstep(-0.2, 0.2, sin((TIME * speed + 10.0 * progress)*0.75 - 3.14159*0.4))*0.025;

    // Only a few iterations seemed to be enough. Obviously, more looks better, but is slower.
    for(float i = 0.f; i < 64.f; i++) {

        // Break conditions. Anything that can help you bail early usually increases frame rate.
        if(layers > 31.f || dot(col, vec3(.299f, .587f, .114f)) > 1.f || t > 16.f)
            break;

        // Current ray postion. Slightly redundant here, but sometimes you may wish to reuse
        // it during the accumulation stage.
        sp = ro + rd * t;

        d = map(sp); // Distance to nearest point on the noise surface.

        // If we get within a certain distance of the surface, accumulate some surface values.
        // Values further away have less influence on the total.
        //
        // aD - Accumulated distance. You could smoothly interpolate it, if you wanted.
        //
        // 1/.(1. + t*t*0.1) - Basic distance attenuation. Feel free to substitute your own.

         // Normalized distance from the surface threshold value to our current isosurface value.
        aD = (thD - abs(d) * 31.f / 32.f) / thD;
        //aD += dot(hash33(sp + 113.) - .5, vec3(.2)); // Extra jitter.

        // If we're within the surface threshold, accumulate some color.
        // Two "if" statements in a shader loop makes me nervous. I don't suspect there'll be any
        // problems, but if there are, let us know.
        if(aD > 0.f) { 

            // Add the accumulated surface distance value, along with some basic falloff using the 
            // camera to light distance, "lDist." There's a bit of color jitter there, too.

            sn = calcNormal(sp, d) * sign(d);
            ld = (lp - sp); //vec3(.5773)
            lDist = max(length(ld), .001f);
            ld /= lDist;
            s = pow(max(dot(reflect(-ld, sn), -rd), 0.f), 8.f);
            l = max(dot(ld, sn), 0.f);
            fr = pow(max(1.f - max(.0f, dot(-rd, sn)), 0.f), 5.f); // Faux Fresnel.

            //float c = dot(sin(sp*128. - cos(sp.yzx*64.)), vec3(.166))+.5;
            col += ((l + .02f + fr * .1f) + vec3(.5f, .7f, 1) * s) * aD / (1.f + lDist * 0.25f + lDist * lDist * 0.05f) * .2f;
            // Failed experiment with color jitter to take out more banding.
            //col += ((l + .05 + fract(rnd + i*27.)*.1) + vec3(.5, .7, 1)*s)*aD/(1. + lDist*0.25 + lDist*lDist*0.05)*.2;

            // The layer number is worth noting. Accumulating more layers gives a bit more glow.
            // Lower layer numbers allow a quicker bailout. A lot of it is guess work.
            layers++;

        }

        // Kind of weird the way this works. I think not allowing the ray to hone in properly is
        // the very thing that gives an even spread of values. The figures are based on a bit 
        // of knowledge versus trial and error. If you have a faster computer, feel free to tweak
        // them a bit.
        t += max(abs(d) * .75f, thD * .25f);

    }

    t = min(t, 16.f);

    col = mix(col, vec3(0), 1.f - exp(-0.025f * t * t));////1.-exp(-0.01*t*t) 1.-1./(1. + t*t*.1)

    // Mixing the greytone color with a firey orange vignette. There's no meaning
    // behind it. I just thought the artsy greyscale was a little too artsy.
    uv = abs(fragCoord.xy / RENDERSIZE.xy - .5f); // Wasteful, but the GPU can handle it.
    col = mix(col, pow(min(vec3(1, 1.2f, 1) * col.x, 1.f), vec3(2.5f, 1, 12)), min(dot(pow(uv, vec2(4.f)), vec2(1)) * 8.f, 1.f));

    //col = vec3(min(col.z*1.5, 1.), pow(col.z, 2.5), pow(col.z, 12.));

	// Mixing the vignette colors up a bit more.
    col = mix(col, col.zxy, dot(sin(rd * 5.f), vec3(.166f)) + 0.166f);

	// Presenting the color to the screen.
    fragColor = vec4(sqrt(clamp(col, 0.f, 1.f)), 1.0f);

}

void main() {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}