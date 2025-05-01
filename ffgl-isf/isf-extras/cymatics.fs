/*{
    "CATEGORIES": [
        "Generator", 
        "Cymatics"
    ],
    "DESCRIPTION": "Cymatics pattern generator with wave interference",
    "INPUTS": [
        {
            "NAME": "progress",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 1.0,
            "LABEL": "Animation Progress"
        },
        {
            "NAME": "waveSpread",
            "TYPE": "float",
            "DEFAULT": 8.0,
            "MIN": 1.0,
            "MAX": 15.0,
            "LABEL": "Wave Spread"
        },
        {
            "NAME": "mouseX",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0.0,
            "MAX": 1.0,
            "LABEL": "Center Wave Period"
        },
        {
            "NAME": "zoom",
            "TYPE": "float",
            "DEFAULT": 0.27,
            "MIN": 0.05,
            "MAX": 1.0,
            "LABEL": "Zoom"
        }
    ]
}*/

// Noise pixel size
#define SIZE 1.0
// Lower - more flowing
#define FLUENCY 0.85
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898f, 78.233f))) * 43758.5453f);
}

#define S(X,Y,period) color += sin(length(UVcoords + vec2(X,Y)*d2)*zoom)-.2

void main() {
    vec2 UVcoords = gl_FragCoord.xy / RENDERSIZE;
    vec4 color = vec4(0.0f);

    vec2 id = ceil(UVcoords / SIZE);
    vec2 rid = vec2(rand(id), rand(id + RENDERSIZE.y));
    color = -vec4(0.1f / fract(rid.x + rid.y - TIME * FLUENCY) - 0.1f) * 15.0f;//sparke effect

    float d3 = RENDERSIZE.y * .5f, //number to move pic upwards
    d4 = RENDERSIZE.x * .5f, //number to move pic sideways
    d2 = waveSpread - 2.0f * sin(5.0f + TIME * 0.07f + progress * 6.28f), //number to move wave machines outwards
    d1 = .5f; // wave width

    UVcoords = .5f * (UVcoords - vec2(d4, d3)); //move pic around

    //function to make color concentric sinewaves like water drop waves radiating from a pt:   

    //make 5 wave machines where the color is added together on coordinates of pentagon:

    //these dots are arranged in a + arrangement, one origin for on xy axes. 
    //central wave machine on origin
    S(0.0f, 0.0f, mouseX * 0.002f);

    //4 other wave machines on axes
    S(0, 1.0f * d2, d1);
    S(0, -1.0f * d2, d1);
    S(-1.0f * d2, -0.0f, d1);
    S(1.0f * d2, 0.0f, d1);
    S(2.0f * d2, 2.0f * d2, d1);
    S(-2.0f * d2, -2.0f * d2, d1);
    S(2.0f * d2, -2.0f * d2, d1);
    S(-2.0f * d2, 2.0f * d2, d1);
    gl_FragColor = color;
}
