/*{
	"CREDIT": "by mojovideotech",
  "CATEGORIES" : [
    "Automatically Converted"
  ],
  "INPUTS" : [
  {
  	"NAME": "speedz",
  	"TYPE": "float"
  },
    {
      "NAME" : "mouse",
      "TYPE" : "point2D",
      "MAX" : [
        1,
        1
      ],
      "MIN" : [
        0,
        0
      ]
    },
    {
			"NAME": "formuparam2",
			"TYPE": "float",
			"DEFAULT": 1.0,
			"MIN": 0.0,
			"MAX": 1.0
	},
    {
			"NAME": "cloud",
			"TYPE": "float",
			"DEFAULT": 0.1,
			"MIN": 0.0,
			"MAX": 1.0
	},
    {
			"NAME": "iterations",
			"TYPE": "float",
			"DEFAULT": 5.0,
			"MIN": 1.0,
			"MAX": 20.0
	},
	{
			"NAME": "volsteps",
			"TYPE": "float",
			"DEFAULT": 4.0,
			"MIN": 1.0,
			"MAX": 20.0
	},
	{
			"NAME": "zoom",
			"TYPE": "float",
			"DEFAULT": 5.0,
			"MIN": 0.0,
			"MAX": 10.0
	},
	{
			"NAME": "tile",
			"TYPE": "float",
			"DEFAULT": 0.85,
			"MIN": 0.0,
			"MAX": 10.0
	}
  ],
  "DESCRIPTION" : "Automatically converted from http:\/\/glslsandbox.com\/e#28545.0"
}
*/

// BlackCherryCosmos by mojovideotech
// Automatically converted from glslsandbox.com/e#28545.0

#ifdef GL_ES
precision mediump float;
#endif

// #define iterations 10
//#define formuparam2 0.89

//#define volsteps 4
#define stepsize 0.390

//#define zoom 6.900
//#define tile   0.850
#define speed2  0.10

#define brightness 0.15
#define darkmatter 0.600
#define distfading 0.560
#define saturation 0.900

#define transverseSpeed zoom*2.0
// #define cloud 0.1067

float triangle(float x, float a) {

    float output2 = 2.0f * abs(2.0f * ((x / a) - floor((x / a) + 0.5f))) - 1.0f;
    return output2;
}

float field(in vec3 p) {

    float strength = 7.f + .03f * log(1.e-6f + fract(sin(TIME) * 4373.11f));
    float accum = 0.f;
    float prev = 0.f;
    float tw = 0.f;

    for(int i = 0; i < 6; ++i) {
        float mag = dot(p, p);
        p = abs(p) / mag + vec3(-.5f, -.8f + 0.1f * sin(TIME * 0.2f + 2.0f), -1.1f + 0.3f * cos(TIME * 0.15f));
        float w = exp(-float(i) / 7.f);
        accum += w * exp(-strength * pow(abs(mag - prev), 2.3f));
        tw += w;
        prev = mag;
    }
    return max(0.f, 5.f * accum / tw - .7f);
}

void main() {

    vec2 uv2 = 2.f * gl_FragCoord.xy / RENDERSIZE.xy - 1.f;
    vec2 uvs = uv2 * RENDERSIZE.xy / max(RENDERSIZE.x, RENDERSIZE.y);

    float time2 = TIME;

    float speed = speed2;
        //speed = 0.005 * cos(time2*0.02 + 3.1415926/4.0);
    speed = 0.00005f * speedz * time2;

	//speed = 0.0;

    float formuparam = formuparam2;

	//get coords and direction

    vec2 uv = uvs;

	//mouse rotation
    float a_xz = 0.9f;
    float a_yz = -.6f;
    float a_xy = 0.9f + TIME * 0.04f;

    mat2 rot_xz = mat2(cos(a_xz), sin(a_xz), -sin(a_xz), cos(a_xz));

    mat2 rot_yz = mat2(cos(a_yz), sin(a_yz), -sin(a_yz), cos(a_yz));

    mat2 rot_xy = mat2(cos(a_xy), sin(a_xy), -sin(a_xy), cos(a_xy));

    float v2 = 1.0f;

    vec3 dir = vec3(uv * zoom, 1.f);

    vec3 from = vec3(0.0f, 0.0f, 0.0f);

    from.x -= 5.0f * (mouse.x - 0.5f);
    from.y -= 5.0f * (mouse.y - 0.5f);

    vec3 forward = vec3(0.f, 0.f, 1.f);

    from.x += transverseSpeed * (1.0f) * cos(0.01f * TIME) + 0.001f * TIME;
    from.y += transverseSpeed * (1.0f) * sin(0.01f * TIME) + 0.001f * TIME;

    from.z += 0.003f * TIME;

    dir.xy *= rot_xy;
    forward.xy *= rot_xy;

    dir.xz *= rot_xz;
    forward.xz *= rot_xz;

    dir.yz *= rot_yz;
    forward.yz *= rot_yz;

    from.xy *= -rot_xy;
    from.xz *= rot_xz;
    from.yz *= rot_yz;

	//zoom
    float zooom = (time2 - 3311.f) * speed;
    from += forward * zooom;
    float sampleShift = mod(zooom, stepsize);

    float zoffset = -sampleShift;
    sampleShift /= stepsize; // make from 0 to 1

	//volumetric rendering
    float s = 0.24f;
    float s3 = s + stepsize / 2.0f;
    vec3 v = vec3(0.f);
    float t3 = 0.0f;

    vec3 backCol2 = vec3(0.f);
    for(int r = 0; r < 20; r++) {
        if(r > int(volsteps)) {
            break;
        }
        vec3 p2 = from + (s + zoffset) * dir;// + vec3(0.,0.,zoffset);
        vec3 p3 = (from + (s3 + zoffset) * dir) * (1.9f / zoom);// + vec3(0.,0.,zoffset);

        p2 = abs(vec3(tile) - mod(p2, vec3(tile * 2.f))); // tiling fold
        p3 = abs(vec3(tile) - mod(p3, vec3(tile * 2.f))); // tiling fold

        t3 = field(p3);

        float pa, a = pa = 0.f;
        for(int i = 0; i < 20; i++) {
            if(i > int(iterations)) {
                break;
            }
            p2 = abs(p2) / dot(p2, p2) - formuparam; // the magic formula
			//p=abs(p)/max(dot(p,p),0.005)-formuparam; // another interesting way to reduce noise
            float D = abs(length(p2) - pa); // absolute sum of average change

            if(i > 2) {
                a += i > 7 ? min(12.f, D) : D;
            }
            pa = length(p2);
        }

		//float dm=max(0.,darkmatter-a*a*.001); //dark matter
        a *= a * a; // add contrast
		//if (r>3) fade*=1.-dm; // dark matter, don't render near
		// brightens stuff up a bit
        float s1 = s + zoffset;
		// need closed form expression for this, now that we shift samples
        float fade = pow(distfading, max(0.f, float(r) - sampleShift));

		//t3 += fade;

        v += fade;
	       		//backCol2 -= fade;

		// fade out samples as they approach the camera
        if(r == 0)
            fade *= (1.f - (sampleShift));
		// fade in samples as they approach from the distance
        if(r == int(volsteps) - 1)
            fade *= sampleShift;
        v += vec3(s1, s1 * s1, s1 * s1 * s1 * s1) * a * brightness * fade; // coloring based on distance

        backCol2 += mix(.4f, 1.f, v2) * vec3(1.8f * t3 * t3 * t3, 1.4f * t3 * t3, t3) * fade;

        s += stepsize;
        s3 += stepsize;

    }

    v = mix(vec3(length(v)), v, saturation); //color adjust

    vec4 forCol2 = vec4(v * .01f, 1.f);

    backCol2 *= cloud;

    backCol2.b *= 1.8f;

    backCol2.r *= 0.55f;

    backCol2.b = 0.5f * mix(backCol2.g, backCol2.b, 0.8f);
    backCol2.g = -0.5f;

    backCol2.bg = mix(backCol2.gb, backCol2.bg, 0.5f * (cos(TIME * 0.01f) + 1.0f));

    gl_FragColor = forCol2 + vec4(backCol2, 1.0f);

}