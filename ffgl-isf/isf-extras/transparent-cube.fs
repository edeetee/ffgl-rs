/*{
  "DESCRIPTION": "Some simple code to produce a relatively cheap, transparent cube field.",
  "CREDIT": "Converted from Shadertoy: Transparent Cube Field",
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

    Transparent Cube Field
    ----------------------
	
	Obviously, this isn't what I'd consider genuine transparency, because there's no
	ray bending, and so forth. However, it attempts to give that feel whilst maintaining 
	framerate. I've tried to keep things simple and keep the code to a minimum. It needs 
	some God rays, but that'd just add to the complexity.

	The inspiration to do this came from a discussion with Fabrice Neyret, after viewing
	his various cube examples. He's a pretty clever guy, so he'll probably know how to do 
	this ten times faster with ten times more efficiency. :)

	It doesn't look much like it, but Duke's port of Las's "Cloudy Spikeball" also provided 
	inspiration.

	By the way, I deliberately made it blurry, and added more jitter than necessary in the
	pursuit of demo art. :) However, you can tweak the figures and dull down the jitter	to 
	produce a reasonably clean looking, transparent scene... of vacuum filled objects. :)

	// Related shaders:

	Crowded Cubes 2 - FabriceNeyret2
	https://www.shadertoy.com/view/ltBSRy

	Cloudy Spikeball - Duke
    https://www.shadertoy.com/view/MljXDw
    // Port from a demo by Las - Worth watching.
    // http://www.pouet.net/prod.php?which=56866
    // http://www.pouet.net/topic.php?which=7920&page=29&x=14&y=9

    // Here's a more interesting, cleaner version.
    Transparent Lattice - Shane
    https://www.shadertoy.com/view/Xd3SDs
	
*/

// Cheap vec3 to vec3 hash. Works well enough, but there are other ways.
vec3 hash33(vec3 p) {

    float n = sin(dot(p, vec3(7, 157, 113)));
    return fract(vec3(2097152, 262144, 32768) * n);
}

float map(vec3 p) {

	// Creating the repeat cubes, with slightly convex faces. Standard,
    // flat faced cubes don't capture the light quite as well.

    // Cube center offset, to create a bit of disorder, which breaks the
    // space up a little.
    vec3 o = hash33(floor(p)) * .2f; 

    // 3D space repetition.
    p = fract(p + o) - .5f; 

    // A bit of roundness. Used to give the cube faces a touch of convexity.
    float r = dot(p, p) - 0.21f;

    // Max of abs(x), abs(y) and abs(z) minus a constant gives a cube.
    // Adding a little bit of "r," above, rounds off the surfaces a bit.
    p = abs(p);
    return max(max(p.x, p.y), p.z) * .95f + r * .05f - .21f;

    // Alternative. Egg shapes... kind of.
    //float perturb = sin(p.x*10.)*sin(p.y*10.)*sin(p.z*10.);
	//p += hash33(floor(p))*.2;
	//return length(fract(p)-.5) - .25 + perturb*.05;

}

void mainImage(out vec4 fragColor, vec2 fragCoord) {

    // Screen coordinates.
    vec2 uv = (fragCoord - RENDERSIZE.xy * .5f) / RENDERSIZE.y;

    // Unit direction ray. The last term is one of many ways to fish-lens the camera.
    // For a regular view, set "rd.z" to something like "0.5."
    vec3 rd = normalize(vec3(uv, (1.f - dot(uv, uv) * .5f) * .5f)); // Fish lens, for that 1337, but tryhardish, demo look. :)

    // There are a few ways to hide artifacts and inconsistencies. Making things go fast is one of them. :)
    // Ray origin, scene color, and surface postion vector.
    vec3 ro = vec3(0, 0, (TIME * speed + 10.0f * progress) * 3.f), col = vec3(0), sp;

    // Swivel the unit ray to look around the scene.
    float cs = cos((TIME * speed + 10.0f * progress) * .375f), si = sin((TIME * speed + 10.0f * progress) * .375f);
    rd.xz = mat2(cs, si, -si, cs) * rd.xz;
    rd.xy = mat2(cs, si, -si, cs) * rd.xy;

    // Unit ray jitter is another way to hide artifacts. It can also trick the viewer into believing
    // something hard core, like global illumination, is happening. :)
    rd *= 0.985f + hash33(rd) * .03f;

	// Ray distance, bail out layer number, surface distance and normalized accumulated distance.
    float t = 0.f, layers = 0.f, d, aD;

    // Surface distance threshold. Smaller numbers give a sharper object. I deliberately
    // wanted some blur, so bumped it up slightly.
    float thD = .035f; // + smoothstep(-0.2, 0.2, sin((TIME * speed + 10.0 * progress)*0.75 - 3.14159*0.4))*0.025;

    // Only a few iterations seemed to be enough. Obviously, more looks better, but is slower.
    for(int i = 0; i < 56; i++) {

        // Break conditions. Anything that can help you bail early usually increases frame rate.
        if(layers > 15.f || col.x > 1.f || t > 10.f)
            break;

        // Current ray postion. Slightly redundant here, but sometimes you may wish to reuse
        // it during the accumulation stage.
        sp = ro + rd * t;

        d = map(sp); // Distance to nearest point in the cube field.

        // If we get within a certain distance of the surface, accumulate some surface values.
        // Values further away have less influence on the total.
        //
        // aD - Accumulated distance. I interpolated aD on a whim (see below), because it seemed 
        // to look nicer.
        //
        // 1/.(1. + t*t*.25) - Basic distance attenuation. Feel free to substitute your own.

         // Normalized distance from the surface threshold value to our current isosurface value.
        aD = (thD - abs(d) * 15.f / 16.f) / thD;

        // If we're within the surface threshold, accumulate some color.
        // Two "if" statements in a shader loop makes me nervous. I don't suspect there'll be any
        // problems, but if there are, let us know.
        if(aD > 0.f) { 
            // Smoothly interpolate the accumulated surface distance value, then apply some
            // basic falloff (fog, if you prefer) using the camera to surface distance, "t."
            col += aD * aD * (3.f - 2.f * aD) / (1.f + t * t * .25f) * .2f;
            layers++;
        }

        // Kind of weird the way this works. I think not allowing the ray to hone in properly is
        // the very thing that gives an even spread of values. The figures are based on a bit of 
        // knowledge versus trial and error. If you have a faster computer, feel free to tweak
        // them a bit.
        t += max(abs(d) * .7f, thD * 1.5f);

    }

    // I'm virtually positive "col" doesn't drop below zero, but just to be safe...
    col = max(col, 0.f);

    // Mixing the greytone color and some firey orange with a sinusoidal pattern that
    // was completely made up on the spot.
    col = mix(col, pow(col.x * vec3(1.5f, 1, 1), vec3(1, 2.5f, 12)), dot(sin(rd.yzx * 8.f + sin(rd.zxy * 8.f)), vec3(.1666f)) + .4f);

	// Doing the same again, but this time mixing in some green. I might have gone overboard
    // applying this step. Commenting it out probably looks more sophisticated.
    col = mix(col, vec3(col.x * col.x * .85f, col.x, col.x * col.x * .3f), dot(sin(rd.yzx * 4.f + sin(rd.zxy * 4.f)), vec3(.1666f)) + .25f);

	// Presenting the color to the screen -- Note that there isn't any gamma correction. That
    // was a style choice.
    fragColor = vec4(max(col, 0.f), 1);

}

void main() {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}