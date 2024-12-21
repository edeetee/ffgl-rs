/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/XdlfWj by wizgrav.  Playing around with reaction diffusion from here https://www.shadertoy.com/view/XsG3z1",
    "IMPORTED": {
    },
    "INPUTS": [
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
        }
    ]
}

*/

// Reaction-diffusion pass.
//
// Here's a really short, non technical explanation:
//
// To begin, sprinkle the buffer with some initial noise on the first few frames (Sometimes, the 
// first frame gets skipped, so you do a few more).
//
// During the buffer loop pass, determine the reaction diffusion value using a combination of the 
// value stored in the buffer's "X" channel, and a the blurred value - stored in the "Y" channel 
// (You can see how that's done in the code below). Blur the value from the "X" channel (the old 
// reaction diffusion value) and store it in "Y", then store the new (reaction diffusion) value 
// in "X." Display either the "X" value  or "Y" buffer value in the "Image" tab, add some window 
// dressing, then repeat the process. Simple... Slightly confusing when I try to explain it, but 
// trust me, it's simple. :)
//
// Anyway, for a more sophisticated explanation, here are a couple of references below:
//
// Reaction-Diffusion by the Gray-Scott Model - http://www.karlsims.com/rd.html
// Reaction-Diffusion Tutorial - http://www.karlsims.com/rd.html

// Cheap vec2 to vec3 hash. Works well enough, but there are other ways.
vec3 hash33(in vec2 p) {
    float n = sin(dot(p, vec2(41, 289)));
    return fract(vec3(2097152, 262144, 32768) * n);
}

// Serves no other purpose than to save having to write this out all the time. I could write a 
// "define," but I'm pretty sure this'll be inlined.
vec4 tx(in vec2 p) {
    return IMG_NORM_PIXEL(BufferA, mod(p, 1.0f));
}

// Weighted blur function. Pretty standard.
float blur(in vec2 p) {

    // Used to move to adjoining pixels. - uv + vec2(-1, 1)*px, uv + vec2(1, 0)*px, etc.
    vec3 e = vec3(1, 0, -1);
    vec2 px = 1.f / RENDERSIZE.xy;

    // Weighted 3x3 blur, or a cheap and nasty Gaussian blur approximation.
    float res = 0.0f;
    // Four corners. Those receive the least weight.
    res += tx(p + e.xx * px).x + tx(p + e.xz * px).x + tx(p + e.zx * px).x + tx(p + e.zz * px).x;
    // Four sides, which are given a little more weight.
    res += (tx(p + e.xy * px).x + tx(p + e.yx * px).x + tx(p + e.yz * px).x + tx(p + e.zy * px).x) * 2.f;
	// The center pixel, which we're giving the most weight to, as you'd expect.
    res += tx(p + e.yy * px).x * 4.f;
    // Normalizing.
    return res / 16.f;

}

// The reaction diffusion loop.
// 
/*
	Reaction Diffusion - 2 Pass
	---------------------------

	Simple 2 pass reaction-diffusion, based off of "Flexi's" reaction-diffusion examples.
	It takes about ten seconds to reach an equilibrium of sorts, and in the order of a 
	minute longer for the colors to really settle in.

	I'm really thankful for the examples Flexi has been putting up lately. From what I 
	understand, he's used to showing his work to a lot more people on much bigger screens,
	so his code's pretty reliable. Reaction-diffusion examples are temperamental. Change 
	one figure by a minute fraction, and your image can disappear. That's why it was really 
	nice to have a working example to refer to. 
	
    Anyway, I've done things a little differently, but in essense, this is just a rehash 
	of Flexi's "Expansive Reaction-Diffusion" example. I've stripped this one down to the 
	basics, so hopefully, it'll be a little easier to take in than the multitab version.

	There are no outside textures, and everything is stored in the A-Buffer. I was 
	originally going to simplify things even more and do a plain old, greyscale version, 
	but figured I'd better at least try to pretty it up, so I added color and some very 
	basic highlighting. I'll put up a more sophisticated version at a later date.

	By the way, for anyone who doesn't want to be weighed down with extras, I've provided 
	a simpler "Image" tab version below.

	One more thing. Even though I consider it conceptually impossible, it wouldn't surprise
	me at all if someone, like Fabrice, produces a single pass, two tweet version. :)

	Based on:
	
	// Gorgeous, more sophisticated example:
	Expansive Reaction-Diffusion - Flexi
	https://www.shadertoy.com/view/4dcGW2

	// A different kind of diffusion example. Really cool.
	Gray-Scott diffusion - knighty
	https://www.shadertoy.com/view/MdVGRh

	
*/

// Ultra simple version, minus the window dressing.
void main() {
    if(PASSINDEX == 0) {

        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy; // Screen coordinates. Range: [0, 1]
        vec2 pw = 1.f / RENDERSIZE.xy; // Relative pixel width. Used for neighboring pixels, etc.

	    // The blurred pixel. This is the result that's used in the "Image" tab. It's also reused
	    // in the next frame in the reaction diffusion process (see below).
        float avgReactDiff = blur(uv);

		// The noise value. Because the result is blurred, we can get away with plain old static noise.
	    // However, smooth noise, and various kinds of noise textures will work, too.
        vec3 noise = hash33(uv + vec2(53, 43) * TIME) * .6f + .2f;

	    // Used to move to adjoining pixels. - uv + vec2(-1, 1)*px, uv + vec2(1, 0)*px, etc.
        vec3 e = vec3(1, 0, -1);

	    // Gradient epsilon value. The "1.5" figure was trial and error, but was based on the 3x3 blur radius.
        vec2 pwr = pw * 1.5f; 

	    // Use the blurred pixels (stored in the Y-Channel) to obtain the gradient. I haven't put too much 
	    // thought into this, but the gradient of a pixel on a blurred pixel grid (average neighbors), would 
	    // be analogous to a Laplacian operator on a 2D discreet grid. Laplacians tend to be used to describe 
	    // chemical flow, so... Sounds good, anyway. :)
	    //
	    // Seriously, though, take a look at the formula for the reacion-diffusion process, and you'll see
	    // that the following few lines are simply putting it into effect.

	    // Gradient of the blurred pixels from the previous frame.
        vec2 lap = vec2(tx(uv + e.xy * pwr).y - tx(uv - e.xy * pwr).y, tx(uv + e.yx * pwr).y - tx(uv - e.yx * pwr).y);//

	    // Add some diffusive expansion, scaled down to the order of a pixel width.
        uv = uv + lap * pw * 3.0f; 

	    // Stochastic decay. Ie: A differention equation, influenced by noise.
	    // You need the decay, otherwise things would keep increasing, which in this case means a white screen.
        float newReactDiff = tx(uv).x + (noise.z - 0.5f) * 0.0025f - 0.002f; 

	    // Reaction-diffusion.
        newReactDiff += dot(tx(uv + (noise.xy - 0.5f) * pw).xy, vec2(1, -1)) * 0.145f; 

	    // Storing the reaction diffusion value in the X channel, and avgReactDiff (the blurred pixel value) 
	    // in the Y channel. However, for the first few frames, we add some noise. Normally, one frame would 
	    // be enough, but for some weird reason, it doesn't always get stored on the very first frame.
        float ifr = min(0.97f, float(FRAMEINDEX) / 100.0f);
        gl_FragColor.xy = mix(IMG_NORM_PIXEL(inputImage, mod(uv, 1.0f)).xz, clamp(vec2(newReactDiff, avgReactDiff / .98f), 0.f, 1.f), ifr);

    } else if(PASSINDEX == 1) {

    // The screen coordinates.
        vec2 uv = isf_FragNormCoord;

    // Read in the blurred pixel value. There's no rule that says you can't read in the
    // value in the "X" channel, but blurred stuff is easier to bump, that's all.
        float c = 1.f - IMG_NORM_PIXEL(BufferA, mod(uv, 1.0f)).y; 
    // Reading in the same at a slightly offsetted position. The difference between
    // "c2" and "c" is used to provide the highlighting.
        float c2 = 1.f - IMG_NORM_PIXEL(BufferA, mod(uv + .5f / RENDERSIZE.xy, 1.0f)).y;

    // Color the pixel by mixing two colors in a sinusoidal kind of pattern.
    //
        float pattern = -cos(uv.x * 0.75f * 3.14159f - 0.9f) * cos(uv.y * 1.5f * 3.14159f - 0.75f) * 0.5f + 0.5f;
    //
    // Blue and gold, for an abstract sky over a... wheat field look. Very artsy. :)
        vec3 col = vec3(c * 1.5f, pow(c, 2.25f), pow(c, 6.f));
        col = mix(col, col.zyx, clamp(pattern - .2f, 0.f, 1.f));

    // Extra color variations.
    //vec3 col = mix(vec3(c*1.2, pow(c, 8.), pow(c, 2.)), vec3(c*1.3, pow(c, 2.), pow(c, 10.)), pattern );
	//vec3 col = mix(vec3(c*1.3, c*c, pow(c, 10.)), vec3(c*c*c, c*sqrt(c), c), pattern );

    // Adding the highlighting. Not as nice as bump mapping, but still pretty effective.
        col += vec3(.6f, .85f, 1.f) * max(c2 * c2 - c * c, 0.f) * 12.f;

    // Apply a vignette and increase the brightness for that fake spotlight effect.
        col *= pow(16.0f * uv.x * uv.y * (1.0f - uv.x) * (1.0f - uv.y), .125f) * 1.15f;

    // Fade in for the first few seconds.
        col *= smoothstep(0.f, 1.f, TIME / 2.f);

    // Done.
        isf_FragColor = vec4(min(col, 1.f), 1.f);

    }

}
