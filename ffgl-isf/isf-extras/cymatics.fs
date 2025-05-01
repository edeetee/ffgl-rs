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
        },
        {
            "NAME": "waveCount",
            "TYPE": "long",
            "DEFAULT": 2,
            "MIN": 1,
            "MAX": 5,
            "LABEL": "Wave Grid Size"
        },
        {
            "NAME": "waveSpacing",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.5,
            "MAX": 2.0,
            "LABEL": "Wave Spacing"
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
    vec2 UVcoords = gl_FragCoord.xy;
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

    // Central wave machine on origin
    S(0.0f, 0.0f, mouseX * 0.002f);

    // Convert waveCount to integer grid size
    int gridSize = int(floor(waveCount));

    // Loop through wave machines in a grid pattern
    for(int x = -gridSize; x <= gridSize; x++) {
        for(int y = -gridSize; y <= gridSize; y++) {
            // Skip the center point (0,0) as we already rendered it
            if(x == 0 && y == 0)
                continue;

            // Apply wave spacing factor
            float xPos = float(x) * waveSpacing;
            float yPos = float(y) * waveSpacing;

            S(xPos, yPos, d1);
        }
    }

    gl_FragColor = color;
}
