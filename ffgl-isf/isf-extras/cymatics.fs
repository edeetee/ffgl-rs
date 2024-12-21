/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/ttXXD8 by henry.  cool 2d algorythm of 9 concentric waves added together same as wave tank, rendered in salt on reverberating square of metal. 9 concentric wave origins:\n@@@\n@@@\n@@@\nThe central concentric wave has fixed position,the other waves can be moved away",
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

// Noise pixel size
#define SIZE 1.0
// Lower - more flowing
#define FLUENCY 0.85
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898f, 78.233f))) * 43758.5453f);
}

void main() {

    vec2 id = ceil(gl_FragCoord.xy / SIZE);
    vec2 rid = vec2(rand(id), rand(id + RENDERSIZE.y));
    gl_FragColor = -vec4(0.1f / fract(rid.x + rid.y - TIME * FLUENCY) - 0.1f) * 15.0f;//sparke effect

    float d3 = RENDERSIZE.y * .5f,//number to move pic upwards
    d4 = RENDERSIZE.x * .5f,//number to move pic sideways
    d2 = 8.0f - 2.0f * sin(5.0f + TIME * .07f) + iMouse.y * 0.021f, //number to move 5 wave machines outwards
    d1 = .5f;
    ;// wave width
    gl_FragCoord.xy = .5f * (gl_FragCoord.xy - vec2(d4, d3)); //move pic around
    float zoom = 0.27f;

	//function to make gl_FragColor concentric sinewaves like water drop waves radiating from a pt:   
#define S(X, Y,period)   gl_FragColor += sin(length(gl_FragCoord.xy + vec2(X,Y)*d2)*zoom)-.2;

    //if (gl_FragColor.x<0.0)

    // gl_FragColor += sin(gl_FragCoord.x*100.0*TIME)/6.0;
    //  gl_FragColor += sin(gl_FragCoord.y*150.0*TIME)/6.0;  
    // sin(length()*p2)+v2
	//see end for full formula including angular coordinates as well as concentric
	//Tip: to remix the code, you can try mixing 3/4/8 
    //wave machines in different symmetries and vary their distance and amplitudes

//make 5 wave machines where the gl_FragColor is added t*d2ogether on coordinates of pentagon:

    //these dots are arranged in a + arrangement, one origin for on xy axes. 
    //central wave machine on origin
    S(0.0f, 0.0f, iMouse.x * 0.002f)

    //4 other wave machines on axes
    S(0, 1.0f * d2, d1) S(0, -1.0f * d2, d1) S(-1.0f * d2, -0.0f, d1) S(1.0f * d2, 0.0f, d1) S(2.0f * d2, 2.0f * d2, d1) S(-2.0f * d2, -2.0f * d2, d1) S(2.0f * d2, -2.0f * d2, d1) S(-2.0f * d2, 2.0f * d2, d1) }

//NOTE: original version had concentric wave forms in this fasion:

//float2 xy2 = IN.uv_MainTex + float2(-0.5, -0.5*d3 ) + float2(k1,j1)*d2; 
//position of the point

//float c2 = length(xy2);//polar coordinates (x axis becomes radial)

//ht+=  (sin(c2 * p2)  *v2) ;//angular coordinates (y becomes angle)
